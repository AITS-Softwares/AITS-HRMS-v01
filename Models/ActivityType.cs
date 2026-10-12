namespace Erp_Aits.Models
{
    public class ActivityType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal? DefaultBillingRate { get; set; }
        public decimal? DefaultCostingRate { get; set; }
        public bool Disabled { get; set; }
    }
}
