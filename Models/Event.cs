using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Erp_Aits.Models
{
    public class Event
    {
        public int Id { get; set; }

        [Display(Name = "Lead")]
        public int? LeadId { get; set; }

        [Display(Name = "Opportunity")]
        public int? OpportunityId { get; set; }

        [Required(ErrorMessage = "Assigned To is required")]
        [Display(Name = "Assigned To")]
        public int? AssignedTo { get; set; }

        [Required(ErrorMessage = "Event Type is required")]
        [Display(Name = "Event Type")]
        public string EventType { get; set; } = "Call";

        [Required(ErrorMessage = "Event Date is required")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Event Date")]
        public DateTime EventDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Summary is required")]
        [StringLength(500, ErrorMessage = "Summary cannot exceed 500 characters")]
        [Display(Name = "Summary")]
        public string Summary { get; set; } = string.Empty;

        [Display(Name = "Attachment")]
        public string? AttachmentPath { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = "Scheduled";

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public string? AssignedToName { get; set; }
        public string? LeadName { get; set; }
        public string? OpportunityName { get; set; }
    }

    public class EventViewModel
    {
        public Event Event { get; set; } = new Event();
        public List<User> UserList { get; set; } = new List<User>();
        public List<string> EventTypeList { get; set; } = new List<string>();
        public List<string> StatusList { get; set; } = new List<string>();
        public IFormFile? AttachmentFile { get; set; }  // Add this for file upload
    }
}