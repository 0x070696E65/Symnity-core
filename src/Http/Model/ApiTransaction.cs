using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Symnity.Core.Format;
using Symnity.Model.Accounts;
using Symnity.Model.Messages;
using Symnity.Model.Mosaics;
using Symnity.Model.Network;
using Symnity.Model.Transactions;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Symnity.Http.Model
{
    public class ApiTransaction : MonoBehaviour
    {
        public static async UniTask<TransferTransaction> CreateTransferTransactionFromApi(string hash)
        {
            var param = "/transactions/confirmed/" + hash;
            Debug.Log(param);
            var transactionRootData = await HttpUtiles.GetDataFromApi(param);
            if (transactionRootData["transaction"] == null) throw new Exception("transaction is null");
            var transactionData = transactionRootData["transaction"].ToString().Replace("\r", "").Replace("\n", "");
            var jsonTransactionData = JObject.Parse(transactionData);
            if (jsonTransactionData["mosaics"] == null) throw new Exception("mosaics is null");
            if (jsonTransactionData["message"] == null) throw new Exception("message is null");
            if (jsonTransactionData["network"] == null) throw new Exception("network is null");
            if (jsonTransactionData["version"] == null) throw new Exception("version is null");
            if (jsonTransactionData["deadline"] == null) throw new Exception("deadline is null");
            if (jsonTransactionData["maxFee"] == null) throw new Exception("maxFee is null");
            if (jsonTransactionData["recipientAddress"] == null) throw new Exception("recipientAddress is null");
            var mosaics = new List<Mosaic>();

            jsonTransactionData["mosaics"].ToList().ForEach(mosaic =>
            {
                mosaics.Add(new Mosaic(new MosaicId(mosaic["id"].ToString()),
                    long.Parse(mosaic["amount"].ToString())));
            });
            var message = new Message(MessageType.PlainMessage,
                ConvertUtils.HexToChar(jsonTransactionData["message"].ToString()));
            return new TransferTransaction(
                (NetworkType) Enum.ToObject(typeof(NetworkType), byte.Parse(jsonTransactionData["network"].ToString())),
                byte.Parse(jsonTransactionData["version"].ToString()),
                Deadline.CreateFromAdjustedValue(long.Parse(jsonTransactionData["deadline"].ToString())),
                long.Parse(jsonTransactionData["maxFee"].ToString()),
                Address.CreateFromRawAddress(
                    RawAddress.AddressToString(
                        ConvertUtils.GetBytes(jsonTransactionData["recipientAddress"].ToString()))),
                mosaics,
                message
            );
        }

        public class TransactionQueryParameters
        {
            public string address;
            public string recipientAddress;
            public string signerPublicKey;
            public string height;
            public string fromHeight;
            public string toHeight;
            public string fromTransferAmount;
            public string toTransferAmount;
            public string type;
            public string embedded;
            public string transferMosaicId;
            public string pageSize;
            public string pageNumber;
            public string offset;
            public string order;

            public TransactionQueryParameters(
                string address = null,
                string recipientAddress = null,
                string signerPublicKey = null,
                string height = null,
                string fromHeight = null,
                string toHeight = null,
                string fromTransferAmount = null,
                string toTransferAmount = null,
                string type = null,
                string embedded = null,
                string transferMosaicId = null,
                string pageSize = null,
                string pageNumber = null,
                string offset = null,
                string order = null
            )
            {
                this.address = address;
                this.recipientAddress = address;
                this.signerPublicKey = address;
                this.height = address;
                this.fromHeight = address;
                this.toHeight = address;
                this.fromTransferAmount = address;
                this.toTransferAmount = address;
                this.type = address;
                this.embedded = address;
                this.transferMosaicId = address;
                this.pageSize = address;
                this.pageNumber = address;
                this.offset = address;
                this.order = address;
            }
        }

        public static async UniTask<List<TransferTransaction>> CreateTransferTransactionsFromApi(
            TransactionQueryParameters query)
        {
            var param = "?";
            if (query.address != null) param += "&address=" + query.address;
            if (query.recipientAddress != null) param += "&recipientAddress=" + query.recipientAddress;
            if (query.signerPublicKey != null) param += "&signerPublicKey=" + query.signerPublicKey;
            if (query.height != null) param += "&height=" + query.height;
            if (query.fromHeight != null) param += "&fromHeight=" + query.fromHeight;
            if (query.fromTransferAmount != null) param += "&fromTransferAmount=" + query.fromTransferAmount;
            if (query.toTransferAmount != null) param += "&toTransferAmount=" + query.toTransferAmount;
            if (query.type != null) param += "&type=" + query.type;
            if (query.embedded != null) param += "&embedded=" + query.embedded;
            if (query.transferMosaicId != null) param += "&transferMosaicId=" + query.transferMosaicId;
            if (query.pageSize != null) param += "&pageSize=" + query.pageSize;
            if (query.pageNumber != null) param += "&pageNumber=" + query.pageNumber;
            if (query.offset != null) param += "&offset=" + query.offset;
            if (query.order != null) param += "&order=" + query.order;

            var url = "/transactions/confirmed" + param;
            Debug.Log(url);
            var transactionRootData = await HttpUtiles.GetDataFromApi(url);
            var root = JsonUtility.FromJson<Root>(transactionRootData.ToString());
            if (root.data.Count == 0) return null;

            var transactionList = new List<TransferTransaction>();
            root.data.ForEach(
                transaction =>
                {
                    var mosaics = new List<Mosaic>();
                    transaction.transaction.mosaics.ToList().ForEach(mosaic =>
                    {
                        mosaics.Add(new Mosaic(new MosaicId(mosaic.id.ToString()),
                            long.Parse(mosaic.amount.ToString())));
                    });
                    Debug.Log(transaction.transaction.recipientAddress);
                    Debug.Log(Address.CreateFromRawAddress(
                        RawAddress.AddressToString(
                            ConvertUtils.GetBytes(transaction.transaction.recipientAddress))));

                    Debug.Log(transaction.transaction.message);
                    var tx = new TransferTransaction(
                        (NetworkType) Enum.ToObject(typeof(NetworkType), (byte) transaction.transaction.network),
                        (byte) transaction.transaction.version,
                        Deadline.CreateFromAdjustedValue(long.Parse(transaction.transaction.deadline)),
                        long.Parse(transaction.transaction.maxFee),
                        Address.CreateFromRawAddress(
                            RawAddress.AddressToString(
                                ConvertUtils.GetBytes(transaction.transaction.recipientAddress))),
                        mosaics,
                        new Message(MessageType.PlainMessage, ConvertUtils.HexToChar(transaction.transaction.message))
                    );
                    transactionList.Add(tx);
                });
            return transactionList;
        }

        [Serializable]
        public class Meta
        {
            public string height;
            public string hash;
            public string merkleComponentHash;
            public int index;
            public string timestamp;
            public int feeMultiplier;
        }

        [Serializable]
        public class Transaction
        {
            public int size;
            public string signature;
            public string signerPublicKey;
            public int version;
            public int network;
            public int type;
            public string maxFee;
            public string deadline;
            public string recipientAddress;
            public string message;
            public List<ApiMosaic> mosaics;
        }

        [Serializable]
        public class ApiMosaic
        {
            public string id;
            public int amount;

            public ApiMosaic(string id, int amount)
            {
                this.id = id;
                this.amount = amount;
            }
        }

        [Serializable]
        public class Datum
        {
            public Meta meta;
            public Transaction transaction;
            public string id;
        }

        [Serializable]
        public class Pagination
        {
            public int pageNumber;
            public int pageSize;
        }

        [Serializable]
        public class Root
        {
            public List<Datum> data;
            public Pagination pagination;
        }
    }
}
