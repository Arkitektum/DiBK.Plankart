using System;
using Azure.Identity;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace DiBK.Plankart
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder<Startup>(args)
                .ConfigureAppConfiguration((_, configBuilder) =>
                {
                    var config = configBuilder.Build();
                    configBuilder.AddAzureKeyVault(new Uri(config["KeyVault:VaultUri"]), new DefaultAzureCredential());
                });
    }
}
