using System.IO;
using System.Threading.Tasks;

namespace DiBK.Plankart.Application.Services;

public interface ICesiumIonResourceUploader
{
    public Task<int?> UploadTerrainModelAsync(string assetName, FileStream assetFileStream);
}