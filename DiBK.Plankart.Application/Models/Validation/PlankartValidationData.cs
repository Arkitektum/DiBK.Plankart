using DiBK.RuleValidator.Extensions;
using Reguleringsplanforslag.Rules;
using Reguleringsplanforslag.Rules.Models;
using SOSI.Produktspesifikasjon.Reguleringsplanforslag.Oversendelse;
using SOSI.Produktspesifikasjon.Reguleringsplanforslag.Planbestemmelser;
using System;
using System.Collections.Generic;

namespace DiBK.Plankart.Application.Models.Validation
{
    public class PlankartValidationData : IRpfValidationData
    {
        public List<GmlDocument> Plankart2D { get; set; } = new();
        public GmlDocument Plankart3D => null;
        public ValidationDataElement<ReguleringsplanbestemmelserType> Planbestemmelser => null;
        public ValidationDataElement<OversendelseReguleringsplanforslagType> Oversendelse => null;
        public Kodelister Kodelister { get; set; }
        public List<Attachment> Attachments => null;

        private PlankartValidationData(
            GmlDocument plankart,
            Kodelister kodelister)
        {
            Plankart2D.Add(plankart);
            Kodelister = kodelister;
        }

        public static IRpfValidationData Create(
            GmlDocument plankart,
            Kodelister kodelister)
        {
            return new PlankartValidationData(plankart, kodelister);
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

            Plankart2D.ForEach(surface => surface.Dispose());

            if (Plankart3D != null)
                Plankart3D.Dispose();
        }
    }
}
