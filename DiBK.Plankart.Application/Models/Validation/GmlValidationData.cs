using DiBK.RuleValidator.Extensions;
using DiBK.RuleValidator.Rules.Gml;
using System;
using System.Collections.Generic;

namespace DiBK.Plankart.Application.Models.Validation
{
    public class GmlValidationData : IGmlValidationData
    {
        public List<GmlDocument> Surfaces { get; } = new();
        public List<GmlDocument> Solids { get; } = new();

        private GmlValidationData(GmlDocument surface)
        {
            Surfaces.Add(surface);
        }

        public static IGmlValidationData Create(GmlDocument surface)
        {
            return new GmlValidationData(surface);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                return;

            Surfaces.ForEach(surface => surface.Dispose());
            Solids.ForEach(solid => solid.Dispose());
        }
    }
}
