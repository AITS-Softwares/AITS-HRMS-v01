//using System.Collections.Generic;

//namespace Erp_Aits.Models
//{
//    public class Appraisal
//    {
//        public int Id { get; set; }
//        public string Employee { get; set; }
//        public string EmployeeName { get; set; }
//        public string Company { get; set; }
//        public string AppraisalCycle { get; set; }
//        public string AppraisalTemplate { get; set; }
//        public bool RateGoalsManually { get; set; }
//        public string Feedback { get; set; }
//        public string Reflections { get; set; }
//        public List<AppraisalKra> Kras { get; set; } = new List<AppraisalKra>();
//        public List<AppraisalSelfRating> SelfRatings { get; set; } = new List<AppraisalSelfRating>();
//    }
//}







//using System.Collections.Generic;

//namespace Erp_Aits.Models
//{
//    public class Appraisal
//    {
//        public int Id { get; set; }
//        public string ErpNextId { get; set; }
//        public string Employee { get; set; }
//        public string EmployeeName { get; set; }
//        public string Company { get; set; }
//        public string AppraisalCycle { get; set; }
//        public string AppraisalTemplate { get; set; }
//        public bool RateGoalsManually { get; set; }
//        public decimal TotalScore { get; set; }
//        public decimal FinalScore { get; set; }
//        public decimal GoalScorePercentage { get; set; }
//        public decimal TotalSelfScore { get; set; }
//        public string Feedback { get; set; }
//        public string Reflections { get; set; }

//        public List<AppraisalKra> Kras { get; set; } = new List<AppraisalKra>();
//        public List<AppraisalSelfRating> SelfRatings { get; set; } = new List<AppraisalSelfRating>();
//    }
//}



using System;
using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class Appraisal
    {
        public int Id { get; set; }

        // ERPNext document identity
        public string ErpNextId { get; set; }

        public string Employee { get; set; }
        public string EmployeeName { get; set; }
        public string Company { get; set; }

        public string AppraisalCycle { get; set; }
        public string AppraisalTemplate { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public bool RateGoalsManually { get; set; }

        // ERPNext calculated fields
        public decimal TotalScore { get; set; }
        public decimal FinalScore { get; set; }
        public decimal GoalScorePercentage { get; set; }

        // Keep this property name because your DB currently uses TotalSelfScore.
        // ERPNext field = self_score
        public decimal TotalSelfScore { get; set; }

        public decimal AvgFeedbackScore { get; set; }

        public string Remarks { get; set; }
        public string Reflections { get; set; }

        // 0 = Draft
        // 1 = Submitted
        // 2 = Cancelled
        public int DocStatus { get; set; }

        public string AmendedFrom { get; set; }

        public DateTime? ErpModified { get; set; }

        // Existing field retained temporarily for compatibility.
        // ERPNext Appraisal itself does NOT use this as normal Appraisal feedback.
        public string Feedback { get; set; }

        public List<AppraisalKra> Kras { get; set; }
            = new List<AppraisalKra>();

        public List<AppraisalSelfRating> SelfRatings { get; set; }
            = new List<AppraisalSelfRating>();
    }
}