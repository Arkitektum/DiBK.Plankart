using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

namespace DiBK.Plankart.Application.HttpClients.Proxy
{
    public class ProxyHttpClient : IProxyHttpClient
    {
        private readonly HttpClient _client;

        public ProxyHttpClient(
            HttpClient client)
        {
            _client = client;
        }

        public async Task<FileContentResult> GetAsync(string url)
        {
            using var response = await _client.GetAsync(url);
            var fileContents = await response.Content.ReadAsByteArrayAsync();

            return new FileContentResult(fileContents, response.Content.Headers.ContentType.ToString());
        }
    }
}
