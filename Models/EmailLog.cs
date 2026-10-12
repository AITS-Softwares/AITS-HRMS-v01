using System;
using System.ComponentModel.DataAnnotations;

namespace Erp_Aits.Models
{
    public class EmailLog
    {
        public int Id { get; set; }
        public int? CompanyId { get; set; }
        public int? CampaignId { get; set; }
        public int? EmailMasterId { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "To")]
        public string To { get; set; }

        [StringLength(20)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "sending";

        [Display(Name = "Error")]
        public string Error { get; set; }

        [Display(Name = "Is Opened")]
        public bool IsOpened { get; set; }

        [Display(Name = "Open Count")]
        public int OpenCount { get; set; }

        [Display(Name = "First Opened At")]
        public DateTime? FirstOpenedAt { get; set; }

        [Display(Name = "Last Opened At")]
        public DateTime? LastOpenedAt { get; set; }

        [Display(Name = "Link Clicked")]
        public bool LinkClicked { get; set; }

        [Display(Name = "Click Count")]
        public int ClickCount { get; set; }

        [Display(Name = "Clicked At")]
        public DateTime? ClickedAt { get; set; }

        [Display(Name = "Last Click URL")]
        public string LastClickUrl { get; set; }

        [Display(Name = "Attachment Opened")]
        public bool AttachmentOpened { get; set; }

        [Display(Name = "IP Address")]
        public string Ip { get; set; }

        [Display(Name = "User Agent")]
        public string UserAgent { get; set; }

        [Display(Name = "City")]
        public string City { get; set; }

        [Display(Name = "Region")]
        public string Region { get; set; }

        [Display(Name = "Country")]
        public string Country { get; set; }

        [Display(Name = "Sent At")]
        public DateTime? SentAt { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}