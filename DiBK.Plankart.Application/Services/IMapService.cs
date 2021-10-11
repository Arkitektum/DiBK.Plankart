using DiBK.Plankart.Application.Models;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace DiBK.Plankart.Application.Services
{
    public interface IMapService
    {
        Task<MapDocument> CreateMapDocument(IFormFile file);
    }
}
