using System;

namespace Erp_Aits.Models
{
    public class EmployeeCheckin
    {
        public int Id { get; set; }
        public string EmployeeCheckinCode { get; set; }

        public string Employee { get; set; }
        public string EmployeeName { get; set; }

        public DateTime? Time { get; set; }
        public string LogType { get; set; }   // "IN" or "OUT"

        public string LocationDeviceId { get; set; }
        public bool SkipAutoAttendance { get; set; }

        public string Shift { get; set; }
    }
}
