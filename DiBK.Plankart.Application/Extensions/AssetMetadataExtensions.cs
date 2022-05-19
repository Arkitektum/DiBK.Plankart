using System;
using Arkitektum.Cesium.Ion.RestApiSharp.Models;
using Arkitektum.Cesium.Ion.RestApiSharp.Util;

namespace DiBK.Plankart.Application.Extensions
{
    public static class AssetMetadataExtensions
    {
        public static bool IsCorrupt(this AssetMetadata asset)
        {
            return asset.Status is AssetStatus.DATA_ERROR or AssetStatus.ERROR ||
                   asset.Status != AssetStatus.COMPLETE && asset.DateAdded.CompareTo(DateTime.Now.AddDays(-1)) < 0;
        }
    }
}
