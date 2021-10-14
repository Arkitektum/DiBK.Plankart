using DiBK.RuleValidator.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Reguleringsplanforslag.Rules.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using static DiBK.Plankart.Application.Services.StaticDataConfig;

namespace DiBK.Plankart.Application.Services
{
    public class StaticDataService : IStaticDataService
    {
        private static readonly JsonSerializerSettings _jsonSerializerSettings = new()
        {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private readonly StaticDataConfig _config;
        private readonly ILogger<StaticDataService> _logger;
        public HttpClient Client { get; }

        public StaticDataService(
            HttpClient client,
            IOptions<StaticDataConfig> options,
            ILogger<StaticDataService> logger)
        {
            Client = client;
            _config = options.Value;
            _logger = logger;
        }

        public async Task<List<Arealformål>> GetArealformål()
        {
            return await GetData(_config.Arealformål, stream =>
            {
                return XDocument.Load(stream)
                    .GetElements("//*:dictionaryEntry/*:Definition")
                    .Select(element => new Arealformål(element.GetValue<int>("*:identifier"), element.GetValue("*:name")))
                    .OrderBy(arealformål => arealformål.Kode)
                    .ToList();
            });
        }

        public async Task<List<FeltnavnArealformål>> GetFeltnavnArealformål()
        {
            return await GetData(_config.FeltnavnArealformål, stream =>
            {
                return XDocument.Load(stream)
                    .GetElements("//*:dictionaryEntry/*:Definition")
                    .Select(element => new FeltnavnArealformål(element.GetValue<int>("*:identifier"), element.GetValue("*:name"), element.GetValue("*:description")))
                    .OrderBy(feltnavn => feltnavn.Arealformål)
                    .ToList();
            });
        }

        public async Task<List<GeonorgeCodeListValue>> GetHensynskategori()
        {
            return await GetData(_config.Hensynskategori, stream =>
            {
                return XDocument.Load(stream)
                    .GetElements("//*:containeditems/*:Registeritem")
                    .Select(element => new GeonorgeCodeListValue(element.GetValue("*:label"), element.GetValue("*:description"), element.GetValue("*:codevalue")))
                    .OrderBy(feltnavn => feltnavn.Codevalue)
                    .ToList();
            });
        }

        private async Task<T> GetData<T>(DataSource source, Func<Stream, T> resolver) where T : class
        {
            var filePath = GetFilePath(source.FileName);
            var data = await LoadDataFromDisk<T>(filePath, source.CacheDays);

            if (data != null)
                return data;

            try
            {
                using var response = await Client.GetAsync(source.Url);
                response.EnsureSuccessStatusCode();
                using var stream = await response.Content.ReadAsStreamAsync();

                data = resolver.Invoke(stream);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Kunne ikke laste ned data fra {source.Url}!");
                return null;
            }

            await SaveDataToDisk(filePath, data);

            return data;
        }

        private static async Task SaveDataToDisk(string filePath, object data)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            await File.WriteAllTextAsync(filePath, JsonConvert.SerializeObject(data, _jsonSerializerSettings));
        }

        private static async Task<T> LoadDataFromDisk<T>(string filePath, int cacheDurationDays) where T : class
        {
            if (!File.Exists(filePath))
                return null;

            var sinceLastUpdate = DateTime.Now.Subtract(File.GetLastWriteTime(filePath));

            if (sinceLastUpdate.TotalDays >= cacheDurationDays)
                return null;

            return JsonConvert.DeserializeObject<T>(await File.ReadAllTextAsync(filePath));
        }

        private static string GetFilePath(string fileName)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "DiBK.Plankart/StaticData", fileName);
        }
    }
}
