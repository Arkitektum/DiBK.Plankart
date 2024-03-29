﻿using System;
using System.Collections.Generic;

namespace DiBK.Plankart.Application.Models.Validation
{
    public class ValidationReport
    {
        public string CorrelationId { get; set; }
        public int Errors { get; set; }
        public int Warnings { get; set; }
        public List<ValidationRule> Rules { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<string> Files { get; set; }
        public double TimeUsed => Math.Round(EndTime.Subtract(StartTime).TotalSeconds, 2);
    }
}
