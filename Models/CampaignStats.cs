using System;

namespace Erp_Aits.Models
{
    public class CampaignStats
    {
        public int Total { get; set; }
        public int Opens { get; set; }
        public int Attachments { get; set; }
        public int Clicks { get; set; }
        public int Failed { get; set; }

        public double OpenRate => Total > 0 ? Math.Round((double)Opens / Total * 100, 2) : 0;
        public double ClickRate => Total > 0 ? Math.Round((double)Clicks / Total * 100, 2) : 0;
    }
}