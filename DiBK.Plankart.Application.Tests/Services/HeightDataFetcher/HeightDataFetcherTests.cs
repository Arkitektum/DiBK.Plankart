using DiBK.Plankart.Application.Services;
using Xunit;

namespace DiBK.Plankart.Application.Tests.Services
{
    public class HeightDataFetcherTests
    {
        [Fact]
        public void ShouldFetchHeightData()
        {
            var result = new HeightDataFetcher().FetchAsByteArrayAsync("299395.95,6695452.54,299591.23,6695862.01", 25832).Result;

            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }
    }
}
