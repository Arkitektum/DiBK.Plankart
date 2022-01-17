using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace DiBK.Plankart.Application.Services
{
    public interface IMultipartRequestService
    {
        Task<IFormFile> GetFileFromMultipart();
    }
}
