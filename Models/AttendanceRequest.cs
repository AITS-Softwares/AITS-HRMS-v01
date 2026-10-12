using System;

namespace Erp_Aits.Models
{
    public class AttendanceRequest
    {
        public int Id { get; set; }

        public string AttendanceRequestCode { get; set; }
        public string ErpNextId { get; set; }

        public int DocStatus { get; set; } = 0; // 0 Draft, 1 Submitted, 2 Cancelled

        public DateTime? HalfDayDate { get; set; }

        public string Employee { get; set; }
        public string EmployeeName { get; set; }
        public string Company { get; set; } = "AITS";

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public bool HalfDay { get; set; }
        public bool IncludeHolidays { get; set; }

        public string Shift { get; set; }

        public string Reason { get; set; } = "Work From Home";
        public string Explanation { get; set; }
    }
}