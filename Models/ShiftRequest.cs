using System;

namespace Erp_Aits.Models
{
    public class ShiftRequest
    {
        public int Id { get; set; }

        // ERPNext document name, e.g. HR-SHR-26-09-00001
        public string ShiftRequestCode { get; set; }

        public string ShiftType { get; set; }

        public string Company { get; set; }

        public string Employee { get; set; }

        public string EmployeeName { get; set; }

        public string Department { get; set; }

        public string Approver { get; set; }

        public string Status { get; set; } = "Draft";

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public int DocStatus { get; set; }

        public string AmendedFrom { get; set; }
    }
}