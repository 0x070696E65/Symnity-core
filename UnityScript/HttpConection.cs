using System;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Symnity.UnityScript
{
    public class HttpConection
    {
        public static async UniTask<string> Announce(string nodeUrl, string payload)
        {
            try
            { 
                var url = nodeUrl + "/transactions";
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
    }
}