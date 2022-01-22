using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

namespace Symnity.Http
{
    public class HttpUtiles : MonoBehaviour
    {
        public static async UniTask<JObject> GetDataFromApi(string param)
        {
            try
            {
                var url = ConstantValue.Node + param;
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
        

        public static async Task Announce(string payload)
        {
            try
            {
                var json = "{ \"payload\" : \"" + payload + "\"}";
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var client = new HttpClient();
                var accept = new MediaTypeWithQualityHeaderValue("application/json");
                client.DefaultRequestHeaders.Accept.Add(accept);
                var response = await client.PutAsync(ConstantValue.Node, content);
                Console.WriteLine(response.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.WriteLine("End");
        }
    }
}