using System.Threading.Tasks;
using DiBK.Plankart.Application.Models.Map;

namespace DiBK.Plankart.Application.Services;

public interface ITerrainResourceService
{
    public Task<CesiumIonTerrainResource> CreateTerrainResourceAsync(TerrainLocation terrainLocation);
}