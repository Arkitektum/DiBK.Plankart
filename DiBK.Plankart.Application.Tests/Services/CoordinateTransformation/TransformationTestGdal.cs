using System;
using DiBK.Plankart.Application.Models.Map;
using MaxRev.Gdal.Core;
using OSGeo.OSR;
using Xunit;

namespace DiBK.Plankart.Application.Tests.Services;

public class TransformationTestGdal
{
    [Fact]
    public void TransformWithGdalTest()
    {
        GdalBase.ConfigureAll();

        var x = 299416.02;
        var y = 6695529.56;

        var sourceCoordinateSystem = new SpatialReference(string.Empty);
        var targetCoordinateSystem = new SpatialReference(string.Empty);

        sourceCoordinateSystem.ImportFromEPSG(5972);
        targetCoordinateSystem.ImportFromEPSG(Epsg.CesiumCoordinateSystemCode);

        var transformation = new OSGeo.OSR.CoordinateTransformation(sourceCoordinateSystem, targetCoordinateSystem);
        if (transformation == null)
            throw new ArgumentException("Invalid projection WKT");

        var projected = new double[3];
        transformation.TransformPoint(projected, x, y, 0.0);
        var px = projected[0];
        var py = projected[1];

        const double ex = 60.34645969;
        const double ey = 5.36455553;
            
        Assert.Equal(ex, px, 8);
        Assert.Equal(ey, py, 8);

    }
}