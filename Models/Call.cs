namespace Erp_Aits.Models
{
    public class Call
    {
        public int Id { get; set; }
        public string CallType { get; set; } = "Incoming";
        public string ContactPerson { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public DateTime CallDate { get; set; } = DateTime.Now;
        public string Duration { get; set; } = "5 min";
        public string Purpose { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string Status { get; set; } = "Completed";
        public string AssignedTo { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int? LeadId { get; set; }
        public int? OpportunityId { get; set; }
    }
}