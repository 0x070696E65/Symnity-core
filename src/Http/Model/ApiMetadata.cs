using System;
using System.Numerics;
using Newtonsoft.Json.Linq;
using Symnity.Core.Format;
using Symnity.Model;
using Symnity.Model.Accounts;
using Symnity.Model.Metadatas;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Symnity.Http.Model
{
    [Serializable]
    public class ApiMetadata : MonoBehaviour
    {
        public static async UniTask<Metadata> CreateMetadataFromApi(MetadataSearchCriteria searchCriteria)
        {
            try
            {
                var param = "/metadata?sourceAddress=" + searchCriteria.SourceAddress + "&scopedMetadataKey=" +
                            searchCriteria.ScopedMetadataKey;
                param += searchCriteria.MetadataType == MetadataType.Account
                    ? "&targetAddress=" + searchCriteria.Id
                    : "&targetId=" + searchCriteria.Id;

                var metadataRootData = await HttpUtiles.GetDataFromApi(param);
                if (metadataRootData["data"] == null) throw new Exception("metaRootData is null");
                if (metadataRootData["data"][0] == null) throw new Exception("metaDataFirst is null");
                var metaDataFirst = metadataRootData["data"][0].ToString().Replace("\r", "").Replace("\n", "");
                var jsonMetaData = JObject.Parse(metaDataFirst);
                if (jsonMetaData["metadataEntry"] == null) throw new Exception("metadataEntry is null");
                var jsonMetaDataEntry = jsonMetaData["metadataEntry"];
                if (jsonMetaDataEntry == null) throw new Exception("metadataEntry is null");
                if (jsonMetaDataEntry["targetId"] == null) throw new Exception("targetId is null");
                if (jsonMetaDataEntry["version"] == null) throw new Exception("version is null");
                if (jsonMetaDataEntry["compositeHash"] == null) throw new Exception("compositeHash is null");
                if (jsonMetaDataEntry["sourceAddress"] == null) throw new Exception("sourceAddress is null");
                if (jsonMetaDataEntry["targetAddress"] == null) throw new Exception("version is null");
                if (jsonMetaDataEntry["scopedMetadataKey"] == null) throw new Exception("scopedMetadataKey is null");
                if (jsonMetaDataEntry["metadataType"] == null) throw new Exception("metadataType is null");
                if (jsonMetaDataEntry["value"] == null) throw new Exception("value is null");
                if (jsonMetaData["id"] == null) throw new Exception("id is null");

                var id = searchCriteria.MetadataType == MetadataType.Account
                    ? null
                    : new Id(long.Parse(jsonMetaDataEntry["targetId"].ToString(),
                        System.Globalization.NumberStyles.AllowHexSpecifier));

                var metaDataEntry = new MetadataEntry(
                    int.Parse(jsonMetaDataEntry["version"].ToString()),
                    jsonMetaDataEntry["compositeHash"].ToString(),
                    Address.CreateFromRawAddress(
                        RawAddress.AddressToString(ConvertUtils.GetBytes(jsonMetaDataEntry["sourceAddress"].ToString()))),
                    Address.CreateFromRawAddress(
                        RawAddress.AddressToString(ConvertUtils.GetBytes(jsonMetaDataEntry["targetAddress"].ToString()))),
                    new BigInteger(long.Parse(jsonMetaDataEntry["scopedMetadataKey"].ToString(),
                        System.Globalization.NumberStyles.AllowHexSpecifier)),
                    (MetadataType) Enum.ToObject(typeof(MetadataType), byte.Parse(jsonMetaDataEntry["metadataType"].ToString())),
                    jsonMetaDataEntry["value"].ToString(),
                    id
                );

                return new Metadata(
                    jsonMetaData["id"].ToString(),
                    metaDataEntry
                );
            }
            catch (Exception e)
            {
                throw new Exception("Error From CreateMetadataFromApi " + e.Message);
            }
        }
    }
}