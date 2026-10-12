using System;
using System.ComponentModel.DataAnnotations;

namespace Erp_Aits.Models
{
    public class WhatsAppSettings
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Phone Number ID")]
        public string PhoneNumberId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Business Account ID")]
        public string BusinessAccountId { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "Access Token")]
        public string AccessToken { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Test Number")]
        public string TestNumber { get; set; }

        [StringLength(50)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [StringLength(50)]
        [Display(Name = "WhatsApp Business Account Name")]
        public string BusinessName { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public int? CompanyId { get; set; }

        // Display Properties
        public string DisplayName => !string.IsNullOrEmpty(PhoneNumber) ? $"{PhoneNumber} ({BusinessName})" : TestNumber;
    }
}