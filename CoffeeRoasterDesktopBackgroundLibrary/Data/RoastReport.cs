﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CoffeeRoasterDesktopBackgroundLibrary.Data
{
    public class RoastReport
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; set; }
        public int BatchNumber { get; set; }
        public string RoastType { get; set; }
        public string BeanVariety { get; set; }
        public int BatchWeightRaw { get; set; }
        public int AmbientTemperatureCelcius { get; set; }
        public int HumidityPercentage { get; set; }
        public int RecordedRR { get; set; }
        public int FirstCrackSeconds { get; set; }
        public int RoastTime { get; set; }
        public int BatchWeightRoaster { get; set; }
        public string PhotoOneFileLocation { get; set; }
        public string PhotoTwoFileLocation { get; set; }
        public string PhotoThreeFileLocation { get; set; }
        public string PhotoFourFileLocation { get; set; }

        [StringLength(500, ErrorMessage = "The {0} value cannot exceed {1} characters. ")]
        public string Notes { get; set; }
    }
}