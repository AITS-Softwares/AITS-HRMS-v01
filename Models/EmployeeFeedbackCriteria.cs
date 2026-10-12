namespace Erp_Aits.Models
{
    public class EmployeeFeedbackCriteria
    {
        public int Id { get; set; }

        // This will store ERPNext document name.
        public string CriteriaCode { get; set; } = string.Empty;

        // ERPNext actual field: criteria
        public string Criteria { get; set; } = string.Empty;
    }
}