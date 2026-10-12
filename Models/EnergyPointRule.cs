namespace Erp_Aits.Models
{
    public class EnergyPointRule
    {
        public int Id { get; set; }
        public string ErpNextId { get; set; }
        public bool Enabled { get; set; } = true;
        public string RuleName { get; set; }
        public string ReferenceDocumentType { get; set; }
        public string ForDocumentEvent { get; set; } = "Custom";
        public string FieldToCheck { get; set; }
        public int Points { get; set; }
        public bool ApplyOnlyOnce { get; set; }
        public bool AllotPointsToAssignedUsers { get; set; }
        public string UserField { get; set; }
        public string MultiplierField { get; set; }
        public int? MaxPoints { get; set; }
        public string Condition { get; set; }
        public string Status { get; set; } = "Active";
    }

    public class EnergyPointRuleListRow
    {
        public int Id { get; set; }
        public string ErpNextId { get; set; }
        public string RuleName { get; set; }
        public string ReferenceDocumentType { get; set; }
        public bool Enabled { get; set; }
        public string Status { get; set; }
    }
}