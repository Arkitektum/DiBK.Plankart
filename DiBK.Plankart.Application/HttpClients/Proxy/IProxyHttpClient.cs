using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DiBK.Plankart.Application.HttpClients.Proxy
{
    public interface IProxyHttpClient
    {
        Task<FileContentResult> GetAsync(string url);
    }
}
