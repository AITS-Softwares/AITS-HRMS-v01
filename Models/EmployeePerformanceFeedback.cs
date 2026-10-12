using System;
using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class EmployeePerformanceFeedback
    {
        public int Id { get; set; }

        public string ErpNextId { get; set; }

        public string ForEmployee { get; set; }

        public string EmployeeName { get; set; }

        public string Reviewer { get; set; }

        public string ReviewerName { get; set; }

        public string Company { get; set; }

        public DateTime AddedOn { get; set; } = DateTime.Now;

        public string AppraisalCycle { get; set; }

        public string Appraisal { get; set; }

        public string Feedback { get; set; }

        public decimal TotalScore { get; set; }

        public int DocStatus { get; set; }

        public string AmendedFrom { get; set; }

        public List<EmployeePerformanceFeedbackRating> FeedbackRatings { get; set; }
            = new List<EmployeePerformanceFeedbackRating>();
    }


    public class EmployeePerformanceFeedbackRating
    {
        public int Id { get; set; }

        public int FeedbackId { get; set; }

        public string Criteria { get; set; }

        public decimal Weightage { get; set; }

        // ERP Rating field is effectively 0-1 internally
        public decimal Rating { get; set; }
    }
}