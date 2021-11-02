using DiBK.Plankart.Application.Models.Validation;
using DiBK.Plankart.Application.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OSGeo.OGR;
using System.Globalization;

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

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "DiBK.Plankart", Version = "v1" });
            });

            services.AddTransient<IGmlToGeoJsonService, GmlToGeoJsonService>();
            services.AddTransient<IMapDocumentService, MapDocumentService>();
            services.AddHttpClient<IValidationService, ValidationService>();

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
            );

            app.UseSwagger();

            app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "DiBK.Plankart v1"));

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
