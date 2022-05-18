using System.Linq;
using System.Threading.Tasks;
using DiBK.Plankart.Application.Models.Map;
using Microsoft.EntityFrameworkCore;

namespace DiBK.Plankart.Application.Services;

public class UnitOfWork : IUnitOfWork
{
    private readonly CesiumIonResourceDbContext _dbContext;

    #region Repositories

    public IRepository<CesiumIonAsset> AssetRepository => new GenericRepository<CesiumIonAsset>(_dbContext);

    #endregion

    public UnitOfWork(CesiumIonResourceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Commit()
    {
        _dbContext.SaveChanges();
    }

    public async Task<int> CommitAsync()
    {
        return await _dbContext.SaveChangesAsync();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    public void RejectChanges()
    {
        foreach (var entry in _dbContext.ChangeTracker.Entries().Where(e => e.State != EntityState.Unchanged))
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.State = EntityState.Detached;
                    break;
                case EntityState.Modified:
                case EntityState.Deleted:
                    entry.Reload();
                    break;
            }
        }
    }
}