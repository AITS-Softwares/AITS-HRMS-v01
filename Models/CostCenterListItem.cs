namespace Erp_Aits.Models
{
    public class CostCenterListItem
    {
        public string Name { get; set; } = "";
        public string CostCenterName { get; set; } = "";
        public string CostCenterNumber { get; set; } = "";
        public string ParentCostCenter { get; set; } = "";
        public string Modified { get; set; } = "";
        public bool Disabled { get; set; }
    }
}
