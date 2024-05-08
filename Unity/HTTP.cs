using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;  
using UnityEngine;

namespace myHTTP{
public class HTTP : MonoBehaviour
{

    public HttpClient client = new HttpClient();

        void Update()
        {
            
        }
        // 发送GET请求
        public async Task<string> GetAsync(string uri) {
            HttpResponseMessage response = await client.GetAsync(uri);
            string content = await response.Content.ReadAsStringAsync();
            return content;
        }

        // 发送POST请求，使用Newtonsoft.Json序列化数据
        async Task<string> PostAsync<T>(string uri, T data) {
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(uri, content);
            string responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;
        }

    }
}