using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Symnity.Http.Model
{
    [Serializable]
    public class ApiMultisig : MonoBehaviour
    {
        public static async UniTask<ApiMultisigInfo> CreateAccountFromApi(string address)
        {
            var param = "/account/" + address + "/multisig";
            var multisigData = await HttpUtiles.GetDataFromApi(param);
            if (multisigData["multisig"] == null) throw new Exception("multisigData is null");
            var multisigInfo = JObject.Parse(multisigData["multisig"].ToString().Replace("\r", "").Replace("\n", ""));
            var cosignatoryAddresses = new List<string>();
            if (multisigInfo["cosignatoryAddresses"] == null) throw new Exception("cosignatoryAddresses is null");
            if (multisigInfo["multisigAddresses"] == null) throw new Exception("multisigAddresses is null");
            if (multisigInfo["version"] == null) throw new Exception("version is null");
            if (multisigInfo["accountAddress"] == null) throw new Exception("accountAddress is null");
            if (multisigInfo["minApproval"] == null) throw new Exception("minApproval is null");
            if (multisigInfo["minRemoval"] == null) throw new Exception("minRemoval is null");
            multisigInfo["cosignatoryAddresses"].ToList().ForEach(cosignatoryAddress =>
            {
                cosignatoryAddresses.Add(cosignatoryAddress.ToString());
            });
            var multisigAddresses = new List<string>();
            multisigInfo["multisigAddresses"].ToList().ForEach(multisigAddress =>
            {
                multisigAddresses.Add(multisigAddress.ToString());
            });
            return new ApiMultisigInfo(
                int.Parse(multisigInfo["version"].ToString()),
                multisigInfo["accountAddress"].ToString(),
                int.Parse(multisigInfo["minApproval"].ToString()),
                int.Parse(multisigInfo["minRemoval"].ToString()),
                cosignatoryAddresses,
                multisigAddresses
            );
        }

        public class ApiMultisigInfo
        {
            public int version { get; set; }
            public string accountAddress { get; set; }
            public int minApproval { get; set; }
            public int minRemoval { get; set; }
            public List<string> cosignatoryAddresses { get; set; }
            public List<string> multisigAddresses { get; set; }

            public ApiMultisigInfo(
                int version,
                string accountAddress,
                int minApproval,
                int minRemoval,
                List<string> cosignatoryAddresses,
                List<string> multisigAddresses
            )
            {
                this.version = version;
                this.accountAddress = accountAddress;
                this.minApproval = minApproval;
                this.minRemoval = minRemoval;
                this.cosignatoryAddresses = cosignatoryAddresses;
                this.multisigAddresses = multisigAddresses;
            }
        }
    }
}