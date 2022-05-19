using System.Threading.Tasks;

namespace DiBK.Plankart.Application.Services;

public interface IUnitOfWork
{
    CesiumIonTerrainResourceRepository TerrainResourceRepository { get; }
    void Commit();
    Task<int> CommitAsync();
    void Dispose();
    void RejectChanges();
}