using System;
using System.IO;
using DiBK.Plankart.Application.Services;
using Xunit;

namespace DiBK.Plankart.Application.Tests.Services;

public class HeightDataFetcherTests
{
    [Fact]
    public void ShouldFetchHeightData()
    {
        var result = new HeightDataFetcher().FetchAsByteArrayAsync("299395.95,6695452.54,299591.23,6695862.01", 25832).Result;

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void ShouldFetchHighResHeightData()
    {
        var filename = Guid.NewGuid() + ".zip";
        using var writeFileStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
        using var result = new HeightDataFetcher().FetchHighestResolutionAsStreamAsync("298395.95,6694452.54,300591.23,6696862.01", 25832).Result;

        result.Position = 0;
        result.CopyToAsync(writeFileStream);
        writeFileStream.Flush();

    }
}