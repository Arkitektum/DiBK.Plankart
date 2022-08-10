using DiBK.Plankart.Application.Models.Map;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace DiBK.Plankart.Application.Services
{
    public interface IMapDocumentService
    {
        Task<MapDocument> CreateMapDocumentAsync(IFormFile file);
        Task<MapDocument3D> UpdateWith3dData(IFormFile file);
    }
}
