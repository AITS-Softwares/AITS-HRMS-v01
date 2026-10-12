using System;

namespace Erp_Aits.Models
{
    public class Interview
    {
        public int Id { get; set; }
        public string InterviewRound { get; set; }
        public string JobApplicant { get; set; }
        public string Designation { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime ScheduledOn { get; set; } = DateTime.Now;
        public string ResumeLink { get; set; }
        public TimeSpan FromTime { get; set; }
        public TimeSpan ToTime { get; set; }
        public string InterviewSummary { get; set; }
        public string Feedback { get; set; }
    }
}
