using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Symnity.Core.Format;
using Symnity.Model.Accounts;
using Symnity.Model.BlockChain;
using Symnity.Model.Messages;
using Symnity.Model.Mosaics;
using Symnity.Model.Network;
using Symnity.Model.Transactions;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Symnity.UnityScript
{
    [Serializable]
    public class SampleTransactions : MonoBehaviour
    {
        private static NetworkType _networkType = NetworkType.TEST_NET;

        private const string Node = "https://test.hideyoshi-node.net:3001";
        private const int EpochAdjustment = 1637848847;
        private const string GenerationHash = "7FCCD304802016BEBBCD342A332F91FF1F3BB5E902988B352697BE245F48E836";
        private const string PrivateKey1 = "C4A87F3B600EBE31D6DBA29FD3E68396F34D362B0C2252825B506006B4BBC45C";
        private const string PrivateKey2 = "BBD394D0EE4E10650D5BF15D1389580C6A6C044481E52022A98CD288A2EB679D";
        private const string PrivateKey3 = "E2A2348F784BAA529E12D2E1B7FFFC9FDD76ABD1C3F649CA82231E24A0C84F94";

        // シンプルなメッセージ送信トランザクション
        public static SignedTransaction TransferTransaction()
        {
            Debug.Log("TransferTransaction start");
            var deadLine = Deadline.Create(EpochAdjustment);
            var senderAccount = Account.CreateFromPrivateKey(PrivateKey1, _networkType);
            var receiverAccount = Account.CreateFromPrivateKey(PrivateKey2, _networkType);
            var message = PlainMessage.Create("Hello Symbol from NEM!");
            var mosaicId = new MosaicId("3A8416DB2D53B6C8");
            var mosaic = new Mosaic(mosaicId, 1000000);
            var transferTransaction = Model.Transactions.TransferTransaction.Create(
                deadLine,
                receiverAccount.Address,
                new List<Mosaic> {mosaic},
                message,
                NetworkType.TEST_NET,
                100000
            );
            var signedTx = senderAccount.Sign(transferTransaction, GenerationHash);
            return signedTx;
        }

        // Revocableモザイク発行トランザクション
        public static void DefineRevocableMosaicTransaction()
        {
            var mosaicCreatorAccount = Account.CreateFromPrivateKey(PrivateKey1, _networkType);

            //revocable モザイク発行
            const bool isSupplyMutable = true;
            const bool isTransferable = true;
            const bool isRestrictable = true;
            const bool isRevocable = true;

            var deadLine = Deadline.Create(EpochAdjustment);
            var nonce = MosaicNonce.CreateRandom(4);
            //var nonce = new MosaicNonce(1234);

            var mosaicDefTx = MosaicDefinitionTransaction.Create(
                deadLine,
                nonce,
                MosaicId.CreateFromNonce(nonce, mosaicCreatorAccount.Address),
                MosaicFlags.Create(isSupplyMutable, isTransferable, isRestrictable, isRevocable),
                0,
                new BlockDuration(0),
                _networkType
            );

            //モザイク変更
            var mosaicChangeTx = MosaicSupplyChangeTransaction.Create(
                deadLine,
                mosaicDefTx.mosaicId,
                MosaicSupplyChangeAction.Increase,
                1000000,
                _networkType
            );

            var aggregateTransaction = AggregateTransaction.CreateComplete(
                deadLine,
                new List<Transaction>
                {
                    Transaction.ToAggregate(mosaicDefTx, mosaicCreatorAccount.GetPublicAccount()),
                    Transaction.ToAggregate(mosaicChangeTx, mosaicCreatorAccount.GetPublicAccount())
                },
                _networkType,
                new List<AggregateTransactionCosignature>(),
                1000000);

            var signedTx = mosaicCreatorAccount.Sign(
                aggregateTransaction,
                GenerationHash
            );

            Console.WriteLine(signedTx.Hash);
            Console.WriteLine(signedTx.Payload);
            Console.WriteLine(Node + "/transactions/confirmed/" + signedTx.Hash);
            Announce(signedTx.Payload).Forget();
        }

        // ゲーム開始時にプレイヤーアカウントにキャラクターアカウント付与（メタデータ初期）
        public static SignedTransaction MultisigAndMetadataAggregateTransaction()
        {
            var adminAccount = Account.CreateFromPrivateKey(PrivateKey1, _networkType);
            var playerMasterAccount = Account.CreateFromPrivateKey(PrivateKey2, _networkType);
            var characterAccount = Account.GenerateNewAccount(_networkType);
            Debug.Log(characterAccount.Address.Plain());

            var deadLine = Deadline.Create(EpochAdjustment);

            var keyLife = KeyGenerator.GenerateUInt64Key("Life");
            var keyAttack = KeyGenerator.GenerateUInt64Key("Attack");

            const string metaDataLife = "100";
            const string metaDataPower = "10";

            var playerChildAccountModificationTransaction = MultisigAccountModificationTransaction.Create(
                deadLine,
                1,
                1,
                new List<UnresolvedAddress>
                {
                    playerMasterAccount.Address
                },
                new List<UnresolvedAddress>(),
                _networkType
            );

            var playerChildAccountMetadataLifeTransactionL = AccountMetadataTransaction.Create(
                deadLine,
                characterAccount.Address,
                keyLife,
                (short) metaDataLife.Length,
                metaDataLife,
                _networkType
            );

            var playerChildAccountMetadataPowerTransactionL = AccountMetadataTransaction.Create(
                deadLine,
                characterAccount.Address,
                keyAttack,
                (short) metaDataPower.Length,
                metaDataPower,
                _networkType
            );
            

            var aggregateTransaction = AggregateTransaction.CreateComplete(
                deadLine,
                new List<Transaction>
                {
                    Transaction.ToAggregate(playerChildAccountModificationTransaction, characterAccount.GetPublicAccount()),
                    Transaction.ToAggregate(playerChildAccountMetadataLifeTransactionL,
                        adminAccount.GetPublicAccount()),
                    Transaction.ToAggregate(playerChildAccountMetadataPowerTransactionL,
                        adminAccount.GetPublicAccount()),
                },
                _networkType,
                new List<AggregateTransactionCosignature>(),
                100000);
            
            var signedTransactionNotComplete = adminAccount.Sign(
                aggregateTransaction,
                GenerationHash
            );

            var cosignedTransactionCharacter =
                CosignatureTransaction.SignTransactionPayload(
                    characterAccount,
                    signedTransactionNotComplete.Payload,
                    GenerationHash
                );

            var cosignedTransactionGame =
                CosignatureTransaction.SignTransactionPayload(
                    playerMasterAccount,
                    signedTransactionNotComplete.Payload,
                    GenerationHash
                );

            var cosignatureSignedTransactions = new[]
            {
                new CosignatureSignedTransaction(
                    cosignedTransactionGame.ParentHash,
                    cosignedTransactionGame.Signature,
                    cosignedTransactionGame.SignerPublicKey
                ),
                new CosignatureSignedTransaction(
                    cosignedTransactionCharacter.ParentHash,
                    cosignedTransactionCharacter.Signature,
                    cosignedTransactionCharacter.SignerPublicKey
                )
            };

            var signedTransactionComplete =
                adminAccount.SignTransactionGivenSignatures(
                    aggregateTransaction,
                    cosignatureSignedTransactions,
                    GenerationHash
                );
            return signedTransactionComplete;
        }

        // Revocableモザイク没収トランザクション
        public static void MosaicRevocationTransaction()
        {
            var revokerAccount = Account.CreateFromPrivateKey(PrivateKey1, _networkType);
            var revokedAccount = Account.CreateFromPrivateKey(PrivateKey1, _networkType);

            var deadLine = Deadline.Create(EpochAdjustment);
            var revTx = MosaicSupplyRevocationTransaction.Create(
                deadLine,
                revokedAccount.Address,
                new Mosaic(new MosaicId("65DBB4CC472A5734"), 3),
                _networkType,
                2000000
            );

            var signedTx = revokerAccount.Sign(revTx, GenerationHash);

            Console.WriteLine(signedTx.Hash);
            Console.WriteLine(signedTx.Payload);
            Console.WriteLine(Node + "/transactions/confirmed/" + signedTx.Hash);
            Announce(signedTx.Payload).Forget();
        }

        public static async UniTask<string> Announce(string payload)
        {
            try
            {
                const string url = Node + "/transactions";
                var myData = Encoding.UTF8.GetBytes("{ \"payload\" : \"" + payload + "\"}");
                var webRequest = UnityWebRequest.Put(url, myData);
                webRequest.SetRequestHeader("Content-Type", "application/json");
                await webRequest.SendWebRequest();
                if (webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    //エラー確認
                    throw new Exception(webRequest.error);
                }

                webRequest.Dispose();
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }

            return "Upload complete!";
        }

        // アカウントデータ取得
        /*var accountData = ApiAccount.CreateAccountFromApi("TAIVS4GFLTZQVJGHCQD232Y3L5BSP2F27XRDBFQ");
        var mosaicId = new MosaicId("65DBB4CC472A5734");
        Console.WriteLine(accountData.address.Plain());
        var mosaic = accountData.mosaics.Where(mosaic=>mosaic.Id.GetId() == mosaicId.GetId());
        Console.WriteLine(mosaic.ToList()[0].Amount);*/

        // トランザクションデータ取得
        /*var transactionData = ApiTransaction.CreateTransferTransactionFromApi("97E74C42E4DB83684011B4D29ADA6A5EDF03A87173D6635A8EA7B97CA6988088");
        Console.WriteLine(transactionData.RecipientAddress.Plain());
        Console.WriteLine(transactionData.Message.Payload);
        Console.WriteLine(transactionData.MaxFee);*/

        // メタデータ取得
        /*var metadataSearchCriteria = new ApiMetadata.MetadataSearchCriteria(
            MetadataType.Account,
            "TAIVS4GFLTZQVJGHCQD232Y3L5BSP2F27XRDBFQ",
            "19670280EC3E4E7D",
            "TCOHSBNTWYNFUWP2PLGSGDK6EWE4BC5TFZNQBLI"
        );
        var metadataData = ApiMetadata.CreateMetadataFromApi(metadataSearchCriteria);
        Console.WriteLine(ConvertUtils.HexToChar(metadataData.metadataEntry.value));*/

        // マルチシグデータ取得
        /*var multisigData = ApiMultisig.CreateAccountFromApi("TAIVS4GFLTZQVJGHCQD232Y3L5BSP2F27XRDBFQ");
        multisigData.multisigAddresses.ForEach(multisigAddress=>
        {
            Console.WriteLine(RawAddress.AddressToString(ConvertUtils.GetBytes(multisigAddress)));
        });*/
    }
}