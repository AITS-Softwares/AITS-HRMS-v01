
using System;
using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class EnergyPointSettings
    {
        public int Id { get; set; }
        public bool Enabled { get; set; }
        public string PointAllocationPeriodicity { get; set; } = "Weekly";
        public DateTime? LastPointAllocationDate { get; set; }

        public List<EnergyPointReviewLevel> ReviewLevels { get; set; } = new List<EnergyPointReviewLevel>();
    }

    public class EnergyPointReviewLevel
    {
        public int Id { get; set; }
        public int EnergyPointSettingsId { get; set; }
        public string LevelName { get; set; }
        public string Role { get; set; }
        public int ReviewPoints { get; set; }
    }
}