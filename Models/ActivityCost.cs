namespace Erp_Aits.Models
{
    public class ActivityCost
    {
        public int Id { get; set; }
        public string ErpNextId { get; set; }
        public string ActivityType { get; set; }
        public string Employee { get; set; }
        public decimal BillingRate { get; set; }
        public decimal CostingRate { get; set; }
    }
}