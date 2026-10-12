using System;
using System.ComponentModel.DataAnnotations;

namespace Erp_Aits.Models
{
    public class Campaign
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Campaign Name is required")]
        [StringLength(200)]
        public string? CampaignName { get; set; }

        [Required(ErrorMessage = "Scheduled Time is required")]
        public DateTime? ScheduledTime { get; set; }

        [Required(ErrorMessage = "Channel is required")]
        [StringLength(20)]
        public string? CampaignChannel { get; set; } = "email";

        [Required(ErrorMessage = "Sender is required")]
        [StringLength(200)]
        public string? Sender { get; set; }

        // REMOVED [Required] - Validation handled in controller
        public string? Content { get; set; }

        [StringLength(500)]
        public string? EmailSubject { get; set; }

        [StringLength(200)]
        public string? CtaText { get; set; }

        public string? Attachments { get; set; }

        [Required(ErrorMessage = "Recipient Source is required")]
        [StringLength(20)]
        public string? RecipientSource { get; set; }

        public string? RecipientList { get; set; }
        public string? RecipientManual { get; set; }
        public string? RecipientExcelEmails { get; set; }

        [StringLength(20)]
        public string? Status { get; set; } = "Scheduled";

        public DateTime? SentAt { get; set; }
        public int? EmailMasterId { get; set; }
        public int? CompanyId { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        // Display Properties
        public int RecipientCount { get; set; }
        public int OpenCount { get; set; }
        public string StatusBadge => Status == "Sent" ? "success" : Status == "Scheduled" ? "warning" : "danger";
    }
}