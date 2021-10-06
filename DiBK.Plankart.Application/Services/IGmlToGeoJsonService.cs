using DiBK.Plankart.Application.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiBK.Plankart.Application.Services
{
    public interface IGmlToGeoJsonService
    {
        Task<GeoJsonDocument> CreateGeoJsonDocument(IFormFile gmlFile, Dictionary<string, string> geometryFieldMappings = null);
    }
}
