using Microsoft.EntityFrameworkCore;
using DiBK.Plankart.Application.Models.Map;
using Microsoft.Data.SqlClient;

namespace DiBK.Plankart.Application.Services;

public class CesiumIonResourceDbContext : DbContext
{
    private readonly string _connectionString;
    public DbSet<CesiumIonTerrainResource> TerrainResources { get; set; }

    public CesiumIonResourceDbContext(DbContextOptions<CesiumIonResourceDbContext> options) : base(options)
    {
    }

    public CesiumIonResourceDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
            return;

        var connection = new SqlConnection(_connectionString);
        //connection.AccessToken = 


        optionsBuilder.UseSqlServer(connection);
        // ?? "Server=tcp:crxtn05u7r.database.windows.net,1433;Initial Catalog=sqldb-cesiumTerrainResources-test;Persist Security Info=False;User ID=sweco-db;Password=HellandtunetDB13;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
    }
}