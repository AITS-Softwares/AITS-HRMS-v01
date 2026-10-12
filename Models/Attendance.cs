//using System;

//namespace Erp_Aits.Models
//{
//    public class Attendance
//    {
//        public int Id { get; set; }
//        public string AttendanceCode { get; set; }
//        public string Employee { get; set; }
//        public string EmployeeName { get; set; }
//        public string Company { get; set; }
//        public DateTime AttendanceDate { get; set; }
//        public string Status { get; set; }
//        public string Shift { get; set; }
//        public bool LateEntry { get; set; }
//        public bool EarlyExit { get; set; }
//    }
//}




using System;

namespace Erp_Aits.Models
{
    public class Attendance
    {
        public int Id { get; set; }
        public string ErpNextId { get; set; }
        public int DocStatus { get; set; } = 0; // 0 = Draft, 1 = Submitted, 2 = Cancelled
        public string AttendanceCode { get; set; }
        public string Employee { get; set; }
        public string EmployeeName { get; set; }

        public string LeaveType { get; set; }
        public string StatusForOtherHalf { get; set; }
        public string Company { get; set; }
        public DateTime AttendanceDate { get; set; }
        public string Status { get; set; }
        public string Shift { get; set; }
        public bool LateEntry { get; set; }
        public bool EarlyExit { get; set; }
    }
}