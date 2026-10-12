using System;
using System.ComponentModel.DataAnnotations;

namespace Erp_Aits.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Customer Code")]
        public string CustomerCode { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; }

        [StringLength(50)]
        [Display(Name = "Customer Group")]
        public string CustomerGroup { get; set; }

        [StringLength(50)]
        [Display(Name = "Customer Type")]
        public string CustomerType { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Mobile Number")]
        public string MobileNumber { get; set; }

        [StringLength(100)]
        [EmailAddress]
        [Display(Name = "Primary Email")]
        public string PrimaryEmail { get; set; }

        [StringLength(100)]
        [EmailAddress]
        [Display(Name = "Additional Email")]
        public string AdditionalEmail { get; set; }

        [StringLength(100)]
        [Display(Name = "Domain / Website")]
        public string Domain { get; set; }

        [StringLength(20)]
        [Display(Name = "PAN Number")]
        public string PanNumber { get; set; }

        [StringLength(30)]
        [Display(Name = "GST Number")]
        public string GstNumber { get; set; }

        [StringLength(50)]
        [Display(Name = "GST Category")]
        public string GstCategory { get; set; }

        [Display(Name = "Commission Rate (%)")]
        public decimal? CommissionRate { get; set; }

        [Display(Name = "Agent")]
        public int? AgentId { get; set; }

        // Navigation property (for display only) - REMOVE Required attribute
        [Display(Name = "Agent Name")]
        public string? AgentName { get; set; }

        [StringLength(200)]
        [Display(Name = "Organization Name")]
        public string? OrganizationName { get; set; }

        [StringLength(200)]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        [StringLength(100)]
        [Display(Name = "City")]
        public string? City { get; set; }

        [StringLength(100)]
        [Display(Name = "State")]
        public string? State { get; set; }

        [StringLength(100)]
        [Display(Name = "Country")]
        public string? Country { get; set; }

        [StringLength(20)]
        [Display(Name = "Pin Code")]
        public string? PinCode { get; set; }

        [Display(Name = "Lead")]
        public int? LeadId { get; set; }

        // Audit fields - REMOVE Required attribute
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public bool IsActive { get; set; } = true;

        // Display Properties
        public string Status => IsActive ? "Active" : "Inactive";
        public string FullAddress => $"{Address}, {City}, {State}, {Country} - {PinCode}";
    }
}