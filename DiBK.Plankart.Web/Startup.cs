using Arkitektum.XmlSchemaValidator.Config;
using DiBK.Plankart.Application.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OSGeo.OGR;
using System;
using System.IO;

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
            services.AddTransient<IPlankartService, PlankartService>();

            services.AddXmlSchemaValidator(options =>
            {
                options.AddSchema(
                    "Plankart",
                    "http://skjema.geonorge.no/SOSI/produktspesifikasjon/Reguleringsplanforslag/5.0",
                    "http://skjema.geonorge.no/SOSITEST/produktspesifikasjon/Reguleringsplanforslag/5.0/reguleringsplanforslag-5.0_rev20210827.xsd"
                );

                options.CacheFilesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "DiBK.Plankart/XSD");
                options.CacheDurationDays = 30;
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Ogr.RegisterAll();
            Ogr.UseExceptions();

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseCors(options => options
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowAnyOrigin());

            app.UseXmlSchemaValidator();

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
