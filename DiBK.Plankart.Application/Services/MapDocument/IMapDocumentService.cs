using DiBK.Plankart.Application.Models.Map;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace DiBK.Plankart.Application.Services
{
    public interface IMapDocumentService
    {
        Task<MapDocument> CreateMapDocument(IFormFile file);
    }
}
