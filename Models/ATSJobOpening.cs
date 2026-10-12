namespace Erp_Aits.Models
{
    public class ATSJobOpening
    {
        public int Id { get; set; }
        public string ATSJobOpeningCode { get; set; }

        // Job Details
        public string JobTitle { get; set; }
        public int OpenPositions { get; set; } = 1;
        public string Department { get; set; }
        public string ExperienceRequired { get; set; }
        public string Status { get; set; } = "Open";

        // Role Description
        public string RequiredSkills { get; set; }
        public string JobDescription { get; set; }

        // Hiring Ownership
        public string HiringManager { get; set; }
    }
}
