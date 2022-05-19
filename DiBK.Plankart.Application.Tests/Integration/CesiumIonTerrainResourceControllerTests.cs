using System;
using System.Collections.Generic;
using System.Linq;
using Arkitektum.Cesium.Ion.RestApiSharp.Models;
using DiBK.Plankart.Application.Models.Map;
using Xunit;

namespace DiBK.Plankart.Application.Tests.Integration
{
    public class CesiumIonTerrainResourceControllerTests
    {
        [Fact]
        public void ShouldDeleteDataExceeding4Gb()
        {
            (var assets, var resources) = Generate5GbsOfCesiumIonAssets();

            var cleanedAssets = MakeSureMax4GbIsUsed(resources, assets);

            Assert.InRange(cleanedAssets.Aggregate(0L, (i, metadata) => i + metadata.Bytes), 0, 4 * Math.Pow(1024, 3));
        }


        private static (List<AssetMetadata>, List<CesiumIonTerrainResource>) Generate5GbsOfCesiumIonAssets()
        {
            var assets = new List<AssetMetadata>();
            var resources = new List<CesiumIonTerrainResource>();
            var totalBytes = 0L;

            var id = 3000;

            while (totalBytes < 5*Math.Pow(1024,3))
            {
                var added = DateTime.Now.AddDays(Random.Shared.Next(-600,0));
                var south = Random.Shared.NextInt64(0, 9900);
                var west = Random.Shared.NextInt64(0, 9900);
                var north = Random.Shared.NextInt64(south, 10000);
                var east = Random.Shared.NextInt64(west, 10000);
                var size = ((north - south) * (east - west) * 3);

                totalBytes += size;

                assets.Add(new AssetMetadata
                {
                    Id = id,
                    DateAdded = added,
                    Bytes = size
                });
                resources.Add(new CesiumIonTerrainResource
                {
                    CesiumIonAssetId = id++,
                    Added = added,
                    South = south,
                    West = west,
                    East = east,
                    North = north,
                    LastAccessed = added.AddDays(Random.Shared.Next((DateTime.Now - added).Days)),
                    NumberOfUsages = Random.Shared.Next(50)
                });
            }

            return (assets, resources);
        }

        private static IEnumerable<AssetMetadata>? MakeSureMax4GbIsUsed(
            List<CesiumIonTerrainResource> terrainResources,
            List<AssetMetadata> assets)
        {
            var assetCopy = new List<AssetMetadata>(assets);
            var totalBytes = assets.Aggregate(0L, (i, asset) => i + asset.Bytes);
            var totalGigaBytes = totalBytes / Math.Pow(1024, 3);

            if (totalGigaBytes < 4)
                return null;

            terrainResources.Sort((a1, a2) => a1.PriorityScore.CompareTo(a2.PriorityScore));
            var assetIndex = 0;
            while (totalGigaBytes > 4)
            {
                var assetToDeleteFromDatabase = terrainResources[assetIndex];

                var assetToRemove = assets.FirstOrDefault(a => a.Id == assetToDeleteFromDatabase.CesiumIonAssetId);
                if (assetCopy.Remove(assetToRemove))
                    totalGigaBytes -= assetToRemove.Bytes / Math.Pow(1024, 3);

                assetIndex++;
            }

            return assetCopy;
        }
    }
}
