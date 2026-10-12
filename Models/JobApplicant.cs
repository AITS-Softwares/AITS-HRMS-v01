namespace Erp_Aits.Models
{
    public class JobApplicant
    {
        public int Id { get; set; }
        public string ApplicantName { get; set; }
        public string EmailAddress { get; set; }
        public string JobOpening { get; set; }
        public string Designation { get; set; }
        public string PhoneNumber { get; set; }
        public string Status { get; set; } = "Open";
        public string Country { get; set; } = "India";
        public string Source { get; set; }
        public int ApplicantRating { get; set; }
        public string CoverLetter { get; set; }
        public string ResumeAttachment { get; set; }
        public string ResumeAttachmentName { get; set; }
        public string ResumeLink { get; set; }
        public string Currency { get; set; } = "INR";
        public decimal? LowerRange { get; set; }
        public decimal? UpperRange { get; set; }
    }
}
