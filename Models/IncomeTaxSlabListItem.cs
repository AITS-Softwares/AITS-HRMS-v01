namespace Erp_Aits.Models
{
    public class IncomeTaxSlabListItem
    {
        public string Name { get; set; } = "";
        public string EffectiveFrom { get; set; } = "";
        public string Company { get; set; } = "";
        public string Currency { get; set; } = "";
        public bool Disabled { get; set; }
        public string Modified { get; set; } = "";
    }
}
