namespace Erp_Aits.Models
{
    public class AttendanceMarkRow
    {
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
    }

    public class MarkedAttendanceRow
    {
        public string Employee { get; set; }
        public string EmployeeName { get; set; }
        public string Status { get; set; }
        public string Shift { get; set; }
    }
}