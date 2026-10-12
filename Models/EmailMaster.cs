using System;
using System.ComponentModel.DataAnnotations;

namespace Erp_Aits.Models
{
    public class EmailMaster
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(200)]
        [Display(Name = "Purpose")]
        public string Purpose { get; set; }

        [StringLength(50)]
        [Display(Name = "Service")]
        public string Service { get; set; } = "gmail";

        [StringLength(200)]
        [EmailAddress]
        [Display(Name = "Recovery Email")]
        public string RecoveryEmail { get; set; }

        [StringLength(200)]
        [Display(Name = "Owner")]
        public string Owner { get; set; }

        [Display(Name = "Masked Password")]
        public string MaskedAppPassword { get; set; }

        [Required]
        [Display(Name = "Encrypted Password")]
        public string EncryptedAppPassword { get; set; }

        [StringLength(20)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Active";

        [Display(Name = "Notes")]
        public string Notes { get; set; }

        [Display(Name = "Company")]
        public int? CompanyId { get; set; }

        [Display(Name = "Last Updated By")]
        public string LastUpdatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}