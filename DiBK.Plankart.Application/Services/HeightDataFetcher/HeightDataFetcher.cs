using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DiBK.Plankart.Application.Utils;

namespace DiBK.Plankart.Application.Services;

public class HeightDataFetcher : IHeightDataFetcher
{
    private const string Url = "https://func-ftp-plankartvisning-fetchheightdata.azurewebsites.net/api/FetchHeightData?bBox={0}&epsg={1}";
    private const string LocalUrl = "http://localhost:7071/api/FetchHeightData?bBox={0}&epsg={1}";

    public async Task<byte[]> FetchAsByteArrayAsync(string boundingBox, int epsgCode)
    {
        var result = await GetAsyncWrapper(boundingBox, epsgCode);

        return await result.Content.ReadAsByteArrayAsync();
    }

    public async Task<Stream> FetchAsStreamAsync(string boundingBox, int epsgCode)
    {
        var result = await GetAsyncWrapper(boundingBox, epsgCode);

        return await result.Content.ReadAsStreamAsync();
    }

    public async Task<Stream> FetchHighestResolutionAsStreamAsync(string boundingBox, int epsgCode)
    {
        var boundingBoxAsDoubleArray = boundingBox.Split(',').Select(c => double.Parse(c, ApplicationConfig.DoubleFormatInfo));
        var splitBoundingBox = SplitBoundingBoxIntoMaximum1000By1000Squares(boundingBoxAsDoubleArray.ToList());

        var listOfResults = splitBoundingBox.Select(bBox => FetchAsStreamAsync(bBox, epsgCode));

        var results = await Task.WhenAll(listOfResults);

        var stream = new MemoryStream();

        foreach (var result in results)
        {
            await result.CopyToAsync(stream);
        }

        stream.Position = 0;

        return stream;
    }

    
    private static async Task<HttpResponseMessage> GetAsyncWrapper(string bBox, int epsgCode)
    {
        using var client = new HttpClient();
#if DEBUG
        return await client.GetAsync(string.Format(LocalUrl, bBox, epsgCode));
#endif
        return await client.GetAsync(string.Format(Url, bBox, epsgCode));
    }

    private static IEnumerable<string> SplitBoundingBoxIntoMaximum1000By1000Squares(IReadOnlyList<double> boundingBox)
    {
        var xSplitPer1000 = CreateSortedAscendingListWithStepNoGreaterThan1000(boundingBox[0], boundingBox[2]);
        var ySplitPer1000 = CreateSortedAscendingListWithStepNoGreaterThan1000(boundingBox[1], boundingBox[3]);

        for (var i = 0; i < xSplitPer1000.Count-1; i++)
        {
            for (var j = 0; j < ySplitPer1000.Count-1; j++)
            {
                yield return "[" +
                             xSplitPer1000[i].ToString(ApplicationConfig.DoubleFormatInfo) + "," +
                             ySplitPer1000[j].ToString(ApplicationConfig.DoubleFormatInfo) + "," +
                             xSplitPer1000[i+1].ToString(ApplicationConfig.DoubleFormatInfo) + "," +
                             ySplitPer1000[j+1].ToString(ApplicationConfig.DoubleFormatInfo) +
                             "]";
            }
        }
    }

    private static List<double> CreateSortedAscendingListWithStepNoGreaterThan1000(double minVal, double maxVal)
    {
        var list = new List<double>();

        while (minVal < maxVal)
        {
            list.Add(minVal);
            minVal += 1000;
        }

        list.Add(maxVal);

        return list;
    }
}