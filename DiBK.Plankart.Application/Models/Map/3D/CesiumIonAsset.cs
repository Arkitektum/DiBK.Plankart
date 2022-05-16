﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using DiBK.Plankart.Application.Utils;

namespace DiBK.Plankart.Application.Models.Map
{
    [Table("Assets")]
    public class CesiumIonAsset
    {
        [Key]
        public int Id { get; set; }
        public int CesiumIonAssetId { get; set; }
        public int EpsgCode { get; set; }
        public double South { get; set; }
        public double West { get; set; }
        public double North { get; set; }
        public double East { get; set; }
        public DateTime Added { get; set; }
        public DateTime LastAccessed { get; set; }
        public int NumberOfUsages { get; set; }
        public int PriorityScore => CalculatePriorityScore();

        public bool Intersects(CesiumIonAsset asset)
        {
            if (asset.North < South)
                return false;
            if (asset.East < West)
                return false;
            if (asset.West > East)
                return false;
            if (asset.South > North)
                return false;

            return true;
        }

        public bool Encloses(string envelope)
        {
            var envelopeAsDouble = envelope.Split(',').Select(e => double.Parse(e, ApplicationConfig.DoubleFormatInfo)).ToArray();
            
            if (South > envelopeAsDouble[0])
                return false;
            if (West > envelopeAsDouble[1])
                return false;
            if (North < envelopeAsDouble[2])
                return false;
            if (East < envelopeAsDouble[3])
                return false;

            return true;
        }

        public bool EnclosesWithMargin(CesiumIonAsset asset, double margin)
        {
            if (South - margin > asset.South)
                return false;
            if (West - margin > asset.West)
                return false;
            if (North + margin < asset.North)
                return false;
            if (East + margin < asset.East)
                return false;

            return true;
        }

        private int CalculatePriorityScore()
        {
            return NumberOfUsages - (DateTime.Now.Day - LastAccessed.Day) + (int)((North - South) * (East - West))/1000;
        }
    }
}
