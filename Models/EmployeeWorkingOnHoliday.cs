using System;

namespace Erp_Aits.Models
{
    public class EmployeeWorkingOnHolidayRow
    {
        public string Employee { get; set; }
        public string EmployeeName { get; set; }
        public string Department { get; set; }
        public string Company { get; set; }
        public DateTime HolidayDate { get; set; }
        public string HolidayDescription { get; set; }
        public string AttendanceStatus { get; set; }
    }
}
