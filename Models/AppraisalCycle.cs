using System;
using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class AppraisalCycle
    {
        public int Id { get; set; }
        public string ErpNextId { get; set; }
        public string CycleName { get; set; }
        public string Company { get; set; } = "AITS";
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now.AddMonths(1);
        public string Status { get; set; } = "Not Started";
        public string Description { get; set; }
        public string KRAEvaluationMethod { get; set; } = "Automated Based on Goal Progress";
        public bool CalculateFinalScoreFormula { get; set; }
        public string FinalScoreFormula { get; set; }

        public string Branch { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }

        public List<AppraisalCycleEmployee> Employees { get; set; } = new List<AppraisalCycleEmployee>();
    }

    public class AppraisalCycleEmployee
    {
        public int Id { get; set; }
        public int AppraisalCycleId { get; set; }
        public string Employee { get; set; }
        public string EmployeeName { get; set; }
        public string AppraisalTemplate { get; set; }
        public string Department { get; set; }
        public string Designation { get; set; }
        public string Branch { get; set; }
    }

    public class AppraisalCycleListRow
    {
        public int Id { get; set; }
        public string ErpNextId { get; set; }
        public string CycleName { get; set; }
        public string Company { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}