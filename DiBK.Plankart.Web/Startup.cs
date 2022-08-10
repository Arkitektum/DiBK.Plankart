using DiBK.Plankart.Application.HttpClients.Proxy;
using DiBK.Plankart.Application.Models.Validation;
using DiBK.Plankart.Application.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OSGeo.OGR;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace DiBK.Plankart
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddDbContext<CesiumIonResourceDbContext>(options =>
                options.UseSqlServer(Configuration["ConnectionStrings:CesiumTerrainResourcesTest"]));

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "DiBK.Plankart", Version = "v1" });
            });

            services.AddHttpContextAccessor();
            services.AddResponseCaching();

            services.AddTransient<IGmlToGeoJsonService, GmlToGeoJsonService>();
            services.AddTransient<IGmlToCzmlService, GmlToCzmlService>();
            services.AddTransient<IMapDocumentService, MapDocumentService>();
            services.AddTransient<IMultipartRequestService, MultipartRequestService>();
            services.AddTransient<ICesiumIonAssetUploader, CesiumIonAssetUploader>();
            services.AddTransient<IHeightDataFetcher, HeightDataFetcher>();
            services.AddTransient<ITerrainResourceService, TerrainResourceService>();
            services.AddTransient<ICesiumIonAssetService, CesiumIonAssetService>();
            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddHttpClient<IValidationService, ValidationService>();
            services.AddHttpClient<IProxyHttpClient, ProxyHttpClient>();

            services.Configure<ValidationSettings>(Configuration.GetSection(ValidationSettings.SectionName));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var cultureInfo = new CultureInfo("nb-NO");
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            Ogr.RegisterAll();
            Ogr.UseExceptions();

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseCors(options => options
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
            );

            app.Use(async (context, next) => {
                context.Request.EnableBuffering();
                await next();
            });

            app.UseSwagger();

            app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "DiBK.Plankart v1"));

            app.UseHttpsRedirection();

            app.UseResponseCaching();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
