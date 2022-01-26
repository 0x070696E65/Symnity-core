using System;
using System.Collections.Generic;
using System.Text;
using Symnity.Core.Format;
using Symnity.Model.Accounts;
using Symnity.Model.BlockChain;
using Symnity.Model.Messages;
using Symnity.Model.Mosaics;
using Symnity.Model.Network;
using Symnity.Model.Transactions;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Symnity.Core.Crypto;
using Symnity.Http;
using Symnity.Http.Model;
using Symnity.Model.Lock;
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
        private const string PrivateKey1 = "";
        private const string PrivateKey2 = "";
        private const string PrivateKey3 = "";

        // シンプルなメッセージ送信トランザクション
        public static SignedTransaction SimpleTransferTransaction()
        {
            var deadLine = Deadline.Create(EpochAdjustment);
            var senderAccount = Account.CreateFromPrivateKey(PrivateKey1, _networkType);
            var receiverAccount = Account.CreateFromPrivateKey(PrivateKey2, _networkType);
            var message = PlainMessage.Create("Hello Symbol from NEM!");
            var mosaicId = new MosaicId("3A8416DB2D53B6C8");
            var mosaic = new Mosaic(mosaicId, 1000000);
            var transferTransaction = TransferTransaction.Create(
                deadLine,
                receiverAccount.Address,
                new List<Mosaic> {mosaic},
                message,
                _networkType,
                100000
            );
            var signedTx = senderAccount.Sign(transferTransaction, GenerationHash);
            return signedTx;
        }

        public class Order
        {
            public string BuyMosaicId;
            public int BuyMosaicAmount;
            public string SellMosaicId;
            public int SellMosaicAmount;
            public Order(string buyMosaicId, int buyMosaicAmount, string sellMosaicId,int sellMosaicAmount)
            {
                BuyMosaicId = buyMosaicId;
                BuyMosaicAmount = buyMosaicAmount;
                SellMosaicId = sellMosaicId;
                SellMosaicAmount = sellMosaicAmount;
            }
        }
        
        

        // 購入意思トランザクション
        public async static UniTask<SignedTransaction> ExchangeLikeBuyTransaction(string sellPublicKey, string buyPrivateKey, string exchangePrivateKey, string hash)
        {
            var deadLine = Deadline.Create(EpochAdjustment);
            //var deadLine = Deadline.CreateFromAdjustedValue(EpochAdjustment);
            var sellPublicAccount = PublicAccount.CreateFromPublicKey(sellPublicKey, _networkType);
            var buyAccount = Account.CreateFromPrivateKey(buyPrivateKey, _networkType);
            var exchangeAccount = Account.CreateFromPrivateKey(exchangePrivateKey, _networkType);
            
            var param = "/transactions/confirmed/" + hash;
            var transactionRootData = await HttpUtiles.GetDataFromApi(Node, param);
            var jToken = transactionRootData.SelectToken("transaction.transactions" , true);

            object message = null;
            object secret = null;
            object orderObj = null;
            if (jToken is {Type: JTokenType.Array})
            {
                var jArr = (JArray)jToken;
                var messageToken = jArr[1].SelectToken("transaction.message");
                var secretToken = jArr[0].SelectToken("transaction.secret");
                var orderToken = jArr[2].SelectToken("transaction.message");
                message = ((JValue)messageToken)?.Value;
                secret = ((JValue)secretToken)?.Value;
                orderObj = ((JValue)orderToken)?.Value;
            }

            if (message != null && secret != null && orderObj != null)
            {
                var proof = "";
                var secretText = "";
                var decodedMessage = Message.DecodeHex(message.ToString())[1..];
                var encryptedMessage = new EncryptedMessage(decodedMessage, exchangeAccount.GetPublicAccount());
                proof = exchangeAccount.DecryptMessage(encryptedMessage, sellPublicAccount).Payload;
                secretText = secret.ToString();
                var orderJsonHex = orderObj.ToString();
                var order = JsonUtility.FromJson<Order>(ConvertUtils.HexToChar(orderJsonHex));
                Debug.Log(order.SellMosaicId);
                Debug.Log(order.SellMosaicAmount);
                Debug.Log(order.BuyMosaicId);
                Debug.Log(order.BuyMosaicAmount);
                Debug.Log(proof);
                Debug.Log(secretText);

                var proofTx = SecretProofTransaction.Create(
                    deadLine,
                    LockHashAlgorithm.Op_Sha3_256,
                    proof,
                    secretText,
                    exchangeAccount.Address,
                    _networkType
                );
                
                var sellMosaics = new List<Mosaic> {new Mosaic(new MosaicId(order.BuyMosaicId), order.BuyMosaicAmount)};
                var returnTx = TransferTransaction.Create(
                    deadLine,
                    sellPublicAccount.Address,
                    sellMosaics,
                    PlainMessage.Create(""),
                    _networkType);

                var feeMosaics = new List<Mosaic> {new Mosaic(new MosaicId("3A8416DB2D53B6C8"), 1000000)};
                var feeTx = TransferTransaction.Create(
                    deadLine,
                    exchangeAccount.Address,
                    feeMosaics,
                    PlainMessage.Create("payment fee"),
                    _networkType
                );

                var buyMosaics = new List<Mosaic> {new (new MosaicId(order.SellMosaicId), order.SellMosaicAmount)};
                var lastTx = TransferTransaction.Create(
                    deadLine,
                    buyAccount.Address,
                    buyMosaics,
                    PlainMessage.Create(""),
                    _networkType
                );

                var aggTx = AggregateTransaction.CreateComplete(
                    deadLine,
                    new List<Transaction>
                    {
                        returnTx.ToAggregate(buyAccount.GetPublicAccount()),
                        feeTx.ToAggregate(buyAccount.GetPublicAccount()),
                        proofTx.ToAggregate(exchangeAccount.GetPublicAccount()),
                        lastTx.ToAggregate(exchangeAccount.GetPublicAccount())
                    },
                    _networkType,
                    new List<AggregateTransactionCosignature>()
                ).SetMaxFeeForAggregate(100, 1);

                var signedTransactionNotComplete = buyAccount.Sign(
                    aggTx,
                    GenerationHash
                );

                var cosignedTransaction = CosignatureTransaction.SignTransactionPayload(
                    exchangeAccount,
                    signedTransactionNotComplete.Payload,
                    GenerationHash
                );

                var cosignatureSignedTransactions = new[]
                {
                    new CosignatureSignedTransaction(
                        cosignedTransaction.ParentHash,
                        cosignedTransaction.Signature,
                        cosignedTransaction.SignerPublicKey
                    )
                };


                var signedTransactionComplete = buyAccount.SignTransactionGivenSignatures(
                    aggTx,
                    cosignatureSignedTransactions,
                    GenerationHash
                );
                Debug.Log(signedTransactionComplete.Hash);
                return signedTransactionComplete;
            }
            else
            {
                throw new Exception("Some Data is missing");
            }
        }


        // 疑似取引所購入意思表示トランザクション
        public static SignedTransaction ExchangeLikeSellTransaction(string sellPrivateKey, string exchangePublicKey, string buyMosaicId, int buyAmount, string sellMosaicId, int sellAmount, int durationHour)
        {
            var deadLine = Deadline.Create(EpochAdjustment);
            //var deadLine = Deadline.CreateFromAdjustedValue(EpochAdjustment);
            var sellAccount = Account.CreateFromPrivateKey(sellPrivateKey, _networkType);
            var exchangePublicAccount = PublicAccount.CreateFromPublicKey(exchangePublicKey, _networkType);

            var random = Crypto.RandomBytes(20);
            var proof = ConvertUtils.ToHex(random);
            Debug.Log("proof: "+proof);
            var hasher = SHA3Hasher.CreateHasher(32);
            var array = new byte[32];
            hasher.Hasher.BlockUpdate(random, 0, random.Length);
            hasher.Hasher.DoFinal(array, 0);
            var secret = ConvertUtils.ToHex(array).ToUpper();
            Debug.Log("secret: "+secret);

            var order = new Order(buyMosaicId, buyAmount, sellMosaicId, sellAmount);
            var json = JsonUtility.ToJson(order);
            Debug.Log("order: "+json);
            
            var coinLockTx = SecretLockTransaction.Create(
                deadLine,
                new Mosaic(new MosaicId(sellMosaicId), sellAmount),
                new BlockDuration((durationHour * 3600) / 30), // assuming one block every 30 seconds
                LockHashAlgorithm.Op_Sha3_256,
                secret,
                exchangePublicAccount.Address,
                _networkType
            );

            var proofTx = TransferTransaction.Create(
                deadLine,
                exchangePublicAccount.Address,
                new List<Mosaic> { },
                sellAccount.EncryptMessage(proof, exchangePublicAccount),
                _networkType
            );
            var mosaic = new Mosaic(new MosaicId("3A8416DB2D53B6C8"), 1000000);
            var infoTx = TransferTransaction.Create(
                deadLine,
                exchangePublicAccount.Address,
                new List<Mosaic> {mosaic},
                PlainMessage.Create(json),
                _networkType
            );

            var aggTx = AggregateTransaction.CreateComplete(
                deadLine,
                new List<Symnity.Model.Transactions.Transaction>
                {
                    coinLockTx.ToAggregate(sellAccount.GetPublicAccount()),
                    proofTx.ToAggregate(sellAccount.GetPublicAccount()),
                    infoTx.ToAggregate(sellAccount.GetPublicAccount())
                },
                _networkType,
                new List<AggregateTransactionCosignature>()
            ).SetMaxFeeForAggregate(100, 0);

            var aggTxSigned = sellAccount.Sign(
                aggTx,
                GenerationHash
            );
            Debug.Log(aggTxSigned.Hash);
            return aggTxSigned;
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
                mosaicDefTx.MosaicId,
                MosaicSupplyChangeAction.Increase,
                1000000,
                _networkType
            );

            var aggregateTransaction = AggregateTransaction.CreateComplete(
                deadLine,
                new List<Transaction>
                {
                    mosaicDefTx.ToAggregate(mosaicCreatorAccount.GetPublicAccount()),
                    mosaicChangeTx.ToAggregate(mosaicCreatorAccount.GetPublicAccount())
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
                    playerChildAccountModificationTransaction.ToAggregate(characterAccount.GetPublicAccount()),
                    playerChildAccountMetadataLifeTransactionL.ToAggregate(adminAccount.GetPublicAccount()),
                    playerChildAccountMetadataPowerTransactionL.ToAggregate(adminAccount.GetPublicAccount()),
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