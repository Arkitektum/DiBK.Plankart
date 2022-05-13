using System.Threading.Tasks;
using DiBK.Plankart.Application.Models.Map;

namespace DiBK.Plankart.Application.Services;

public interface IUnitOfWork
{
    IRepository<CesiumIonAsset> AssetRepository { get; }
    void Commit();
    Task<int> CommitAsync();
    void Dispose();
    void RejectChanges();
}