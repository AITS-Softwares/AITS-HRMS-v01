using System;

namespace Erp_Aits.Models
{
    public class EmployeeReferral
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime ReferralDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Pending";
        public string ForDesignation { get; set; }
        public string Email { get; set; }
        public string CurrentEmployer { get; set; }
        public string ContactNo { get; set; }
        public string CurrentJobTitle { get; set; }
        public string ResumeLink { get; set; }
        public string ResumeAttachment { get; set; }
        public string ResumeAttachmentName { get; set; }
        public string Referrer { get; set; }
        public bool IsApplicableForReferralBonus { get; set; } = true;
        public string QualifiedReason { get; set; }
        public string WorkReferences { get; set; }
    }
}
