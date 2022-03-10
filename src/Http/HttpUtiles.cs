using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

namespace Symnity.Http
{
    public class HttpUtiles : MonoBehaviour
    {
        public static async UniTask<JObject> GetDataFromApi(string node, string param)
        {
            try
            {
                var url = node + param;
                var webRequest = UnityWebRequest.Get(url);
                await webRequest.SendWebRequest();
                
                if (webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    //エラー確認
                    throw new Exception(webRequest.error);
                }
                else
                {
                    //結果確認
                    return  JObject.Parse(webRequest.downloadHandler.text);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error From GetDataFromApi" + e.Message);
            }
        }
        
        public static async UniTask<string> GetDataFromApiString(string node, string param)
        {
            try
            {
                var url = node + param;
                var webRequest = UnityWebRequest.Get(url);
                await webRequest.SendWebRequest();
                
                if (webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    //エラー確認
                    throw new Exception(webRequest.error);
                }
                else
                {
                    //結果確認
                    return  webRequest.downloadHandler.text;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error From GetDataFromApi" + e.Message);
            }
        }
    }
}