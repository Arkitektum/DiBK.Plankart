using System.Threading.Tasks;
using DiBK.Plankart.Application.Models.Map;

namespace DiBK.Plankart.Application.Services;

public interface ICesiumIonTerrainAssetManager
{
    Task<int> GetCesiumTerrainAssetId(TerrainLocation terrainLocation);
}