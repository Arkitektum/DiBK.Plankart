using Microsoft.EntityFrameworkCore;
using DiBK.Plankart.Application.Models.Map;

namespace DiBK.Plankart.Application.Services;

public class CesiumIonResourceDbContext : DbContext
{
    public DbSet<CesiumIonTerrainResource> TerrainResources { get; set; }

    public CesiumIonResourceDbContext(DbContextOptions<CesiumIonResourceDbContext> options) : base(options)
    {
    }
}