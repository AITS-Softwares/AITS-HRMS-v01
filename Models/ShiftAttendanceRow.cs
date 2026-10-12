using System;

namespace Erp_Aits.Models
{
    public class ShiftAttendanceRow
    {
        public string Employee { get; set; }
        public string EmployeeName { get; set; }
        public string Department { get; set; }
        public string Shift { get; set; }
        public string Status { get; set; }
        public DateTime AttendanceDate { get; set; }
        public bool LateEntry { get; set; }
        public bool EarlyExit { get; set; }
    }
}
