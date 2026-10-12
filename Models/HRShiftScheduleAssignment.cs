using System;

namespace Erp_Aits.Models
{
    public class HRShiftScheduleAssignment
    {
        public int Id { get; set; }
        public string ErpNextId { get; set; }
        public string Employee { get; set; }
        public string EmployeeName { get; set; }
        public string Company { get; set; }
        public string ShiftSchedule { get; set; }
        public string ShiftLocation { get; set; }
        public bool Enabled { get; set; } = true;
        public DateTime CreateShiftsAfter { get; set; } = DateTime.Now;
        public string ShiftStatus { get; set; } = "Active";
    }
}
