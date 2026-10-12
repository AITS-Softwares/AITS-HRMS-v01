namespace Erp_Aits.Models
{
    public class ProjectProfitabilityRow
    {
        public string Customer { get; set; }
        public string Project { get; set; }
        public decimal TotalHours { get; set; }
        public decimal BillingAmount { get; set; }
        public decimal CostingAmount { get; set; }
        public decimal Profit { get; set; }
    }
}
