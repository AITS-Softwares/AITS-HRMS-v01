using System;
using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class InterviewFeedbackModel
    {
        public int Id { get; set; }
        public int InterviewId { get; set; }
        public string Interviewer { get; set; }
        public string Result { get; set; } = "Cleared";
        public string FeedbackText { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public List<InterviewFeedbackSkill> Skills { get; set; } = new List<InterviewFeedbackSkill>();

        public string InterviewRound { get; set; }
        public string JobApplicant { get; set; }
    }

    public class InterviewFeedbackSkill
    {
        public int Id { get; set; }
        public int InterviewFeedbackId { get; set; }
        public string Skill { get; set; }
        public int Rating { get; set; }
    }

    public class InterviewFeedbackListRow
    {
        public int Id { get; set; }
        public int InterviewId { get; set; }
        public string InterviewRound { get; set; }
        public string JobApplicant { get; set; }
        public string Interviewer { get; set; }
        public string Result { get; set; }
    }
}