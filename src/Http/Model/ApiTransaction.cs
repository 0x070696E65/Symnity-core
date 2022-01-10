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
    [Serializable]
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
            var message = new Message(MessageType.PlainMessage, ConvertUtils.HexToChar(jsonTransactionData["message"].ToString()));
            return new TransferTransaction(
                (NetworkType) Enum.ToObject(typeof(NetworkType), byte.Parse(jsonTransactionData["network"].ToString())),
                byte.Parse(jsonTransactionData["version"].ToString()),
                Deadline.CreateFromAdjustedValue(uint.Parse(jsonTransactionData["deadline"].ToString())),
                long.Parse(jsonTransactionData["maxFee"].ToString()),
                Address.CreateFromRawAddress(
                    RawAddress.AddressToString(
                        ConvertUtils.GetBytes(jsonTransactionData["recipientAddress"].ToString()))),
                mosaics,
                message
            );
        }
    }
}