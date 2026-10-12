using System;
using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class EmployeeSeparation
    {
        public int Id { get; set; }

        // ERPNext document name
        public string EmployeeSeparationCode { get; set; }

        public string Employee { get; set; }
        public string EmployeeName { get; set; }

        public string Company { get; set; }

        // ERP field = boarding_status
        public string Status { get; set; } = "Pending";

        public DateTime? ResignationLetterDate { get; set; }

        // ERP field = boarding_begins_on
        public DateTime SeparationBeginsOn { get; set; } = DateTime.Today;

        // ERP field = employee_separation_template
        public string EmployeeSeparationTemplate { get; set; }

        public string Department { get; set; }
        public string Designation { get; set; }
        public string EmployeeGrade { get; set; }

        public string Project { get; set; }

        public bool NotifyUsersByEmail { get; set; }

        // ERP field = exit_interview
        public string ExitInterviewSummary { get; set; }

        // ERP lifecycle
        public int DocStatus { get; set; }

        public string AmendedFrom { get; set; }

        public List<EmployeeSeparationActivity> Activities { get; set; }
            = new List<EmployeeSeparationActivity>();
    }


    public class EmployeeSeparationActivity
    {
        public int Id { get; set; }

        public int EmployeeSeparationId { get; set; }

        public string ActivityName { get; set; }

        public string UserName { get; set; }

        public int? BeginOnDays { get; set; }

        public int? DurationDays { get; set; }
    }
}