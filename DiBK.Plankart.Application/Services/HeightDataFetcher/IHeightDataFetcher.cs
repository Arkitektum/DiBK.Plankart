using System.IO;
using System.Threading.Tasks;

namespace DiBK.Plankart.Application.Services;

public interface IHeightDataFetcher
{
    public Task<byte[]> FetchAsByteArrayAsync(string boundingBox, int epsgCode);

    public Task<Stream> FetchAsStreamAsync(string boundingBox, int epsgCode);

    public Task<Stream> FetchHighestResolutionAsStreamAsync(string boundingBox, int epsgCode);
}