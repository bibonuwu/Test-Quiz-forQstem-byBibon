using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HIGHT4
{
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> PatchAsync(this HttpClient client, string requestUri, HttpContent content)
        {
            var request = new HttpRequestMessage
            {
                Method = new HttpMethod("PATCH"),
                RequestUri = new System.Uri(requestUri),
                Content = content
            };
            return await client.SendAsync(request);
        }
    }
}
