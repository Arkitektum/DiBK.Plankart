using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DiBK.Plankart.Application.Services;

public class UnitOfWork : IUnitOfWork
{
    private readonly CesiumIonResourceDbContext _context;

    #region Repositories

    private CesiumIonTerrainResourceRepository _cesiumIonTerrainResourceRepository;
    public CesiumIonTerrainResourceRepository TerrainResourceRepository => _cesiumIonTerrainResourceRepository ??= new CesiumIonTerrainResourceRepository(_context);

    #endregion

    public UnitOfWork(CesiumIonResourceDbContext context)
    {
        _context = context;
    }

    public void Commit()
    {
        _context.SaveChanges();
    }

    public async Task<int> CommitAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    public void RejectChanges()
    {
        foreach (var entry in _context.ChangeTracker.Entries().Where(e => e.State != EntityState.Unchanged))
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