using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json.Linq;
using Symnity.Core.Format;
using Symnity.Model.Accounts;
using Symnity.Model.Mosaics;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Symnity.Http.Model
{
    [Serializable]
    public class ApiAccount : MonoBehaviour
    {
        public static async UniTask<AccountInfo> CreateAccountFromApi(string address)
        {
            try
            {
                var param = "/accounts/" + address;
                var accountRootData = await HttpUtiles.GetDataFromApi(param);
                if (accountRootData["account"] == null) throw new Exception("account is null");
                var accountData = accountRootData["account"].ToString().Replace("\r", "").Replace("\n", "");
                var jsonAccountData = JObject.Parse(@accountData);
                if (jsonAccountData["mosaics"] == null) throw new Exception("mosaics is null");
                var mosaics = new List<Mosaic>();
                jsonAccountData["mosaics"].ToList().ForEach(mosaic =>
                {
                    mosaics.Add(new Mosaic(new MosaicId(mosaic["id"].ToString()),
                        long.Parse(mosaic["amount"].ToString())));
                });
                if (accountRootData["id"] == null) throw new Exception("id is null");
                if (jsonAccountData["version"] == null) throw new Exception("version is null");
                if (jsonAccountData["address"] == null) throw new Exception("address is null");
                if (jsonAccountData["addressHeight"] == null) throw new Exception("addressHeight is null");
                if (jsonAccountData["publicKey"] == null) throw new Exception("publicKey is null");
                if (jsonAccountData["publicKeyHeight"] == null) throw new Exception("publicKeyHeight is null");
                if (jsonAccountData["importance"] == null) throw new Exception("importance is null");
                if (jsonAccountData["importanceHeight"] == null) throw new Exception("importanceHeight is null");
                if (jsonAccountData["accountType"] == null) throw new Exception("accountType is null");
                return new AccountInfo(
                    accountRootData["id"].ToString(),
                    int.Parse(jsonAccountData["version"].ToString()),
                    Address.CreateFromRawAddress(
                        RawAddress.AddressToString(ConvertUtils.GetBytes(jsonAccountData["address"].ToString()))),
                    BigInteger.Parse(jsonAccountData["addressHeight"].ToString()),
                    jsonAccountData["publicKey"].ToString(),
                    BigInteger.Parse(jsonAccountData["publicKeyHeight"].ToString()),
                    BigInteger.Parse(jsonAccountData["importance"].ToString()),
                    BigInteger.Parse(jsonAccountData["importanceHeight"].ToString()),
                    mosaics,
                    (AccountType) Enum.ToObject(typeof(AccountType),
                        int.Parse(jsonAccountData["accountType"].ToString()))
                );
            }
            catch (Exception e)
            {
                throw new Exception("Error From CreateAccountFromApi " + e.Message);
            }
        }
    }
}