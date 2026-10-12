using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class InterviewRound
    {
        public int Id { get; set; }
        public string RoundName { get; set; }
        public int ExpectedAverageRating { get; set; }
        public string InterviewType { get; set; }
        public string Interviewers { get; set; }
        public string Designation { get; set; }
        public List<InterviewRoundSkill> Skills { get; set; } = new List<InterviewRoundSkill>();
    }
}
