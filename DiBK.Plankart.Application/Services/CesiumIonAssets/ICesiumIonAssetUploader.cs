using System.IO;
using System.Threading.Tasks;

namespace DiBK.Plankart.Application.Services;

public interface ICesiumIonAssetUploader
{
    public Task<int?> UploadTerrainModelAsync(string assetName, FileStream assetFileStream);
}