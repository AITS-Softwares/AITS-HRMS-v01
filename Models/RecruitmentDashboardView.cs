using System;
using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class RecruitmentDashboardViewModel
    {
        public int JobOpeningOpenCount { get; set; }
        public int JobApplicantOpenCount { get; set; }
        public int JobOfferAcceptedCount { get; set; }
        public List<InterviewThisWeekRow> InterviewsThisWeek { get; set; } = new List<InterviewThisWeekRow>();
    }

    public class InterviewThisWeekRow
    {
        public string CandidateName { get; set; }
        public string JobOpening { get; set; }
        public DateTime InterviewDate { get; set; }
        public string InterviewRound { get; set; }
        public string Status { get; set; }
    }
}
