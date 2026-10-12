
using System;

namespace Erp_Aits.Models
{
    public class EmployeeExitRow
    {
        public string Employee { get; set; }
        public string EmployeeName { get; set; }
        public string Company { get; set; }
        public string Department { get; set; }
        public string Designation { get; set; }
        public string ReportsTo { get; set; }
        public DateTime? RelievingDate { get; set; }
        public string InterviewStatus { get; set; }
        public string FinalDecision { get; set; }
        public bool ExitInterviewPending { get; set; }
        public bool ExitQuestionnairePending { get; set; }
        public bool FnFPending { get; set; }
    }
}