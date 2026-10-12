using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class ATSCandidate
    {
        public int Id { get; set; }
        public string ATSCandidateCode { get; set; }

        // Candidate Profile
        public string CandidateName { get; set; }
        public string AppliedJob { get; set; }
        public string Email { get; set; }
        public string CandidateStatus { get; set; } = "Applied";
        public string Phone { get; set; }
        public string Source { get; set; }
        public string ResumeAttachment { get; set; }

        // Resume Parser
        public string ResumeParseStatus { get; set; } = "Not Parsed";

        // ATS Scoring
        public string Skills { get; set; }
        public List<ATSCandidateSkill> SkillMatrix { get; set; } = new List<ATSCandidateSkill>();

        // Experience and Offer Fit
        public string Experience { get; set; }
        public string CurrentCompany { get; set; }
        public decimal? ExpectedSalary { get; set; }
        public string NoticePeriod { get; set; }

        // Recruiter Review
        public string RecruiterNotes { get; set; }
        public int? InterviewRating { get; set; }

        public int ATSScore { get; set; }
    }

    public class ATSCandidateSkill
    {
        public int Id { get; set; }
        public int ATSCandidateId { get; set; }
        public string Skill { get; set; }
        public string Experience { get; set; }
        public int? Rating { get; set; }
    }
}
