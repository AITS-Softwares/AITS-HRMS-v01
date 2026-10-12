using System;

namespace Erp_Aits.Models
{
    public class JobOpening
    {
        public int Id { get; set; }
        public string JobOpeningTemplate { get; set; }
        public string JobTitle { get; set; }
        public string Designation { get; set; }
        public string Status { get; set; } = "Open";
        public DateTime PostedOn { get; set; } = DateTime.Now;
        public DateTime? ClosesOn { get; set; }
        public string Company { get; set; }
        public string EmploymentType { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
        public bool PublishOnWebsite { get; set; }
        public string Description { get; set; }
        public string Currency { get; set; } = "INR";
        public string SalaryPaidPer { get; set; } = "Month";
        public decimal? LowerRange { get; set; }
        public decimal? UpperRange { get; set; }
        public bool PublishSalaryRange { get; set; }
    }
}
