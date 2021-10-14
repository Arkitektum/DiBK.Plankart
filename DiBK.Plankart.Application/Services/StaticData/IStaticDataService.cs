using Reguleringsplanforslag.Rules.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiBK.Plankart.Application.Services
{
    public interface IStaticDataService
    {
        Task<List<Arealformål>> GetArealformål();
        Task<List<FeltnavnArealformål>> GetFeltnavnArealformål();
        Task<List<GeonorgeCodeListValue>> GetHensynskategori();
    }
}
