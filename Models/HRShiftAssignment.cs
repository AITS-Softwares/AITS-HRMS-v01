using System;

namespace Erp_Aits.Models
{
    public class HRShiftAssignment
    {
        public int Id { get; set; }
        public string ErpNextId { get; set; }
        public string Employee { get; set; }
        public string EmployeeName { get; set; }
        public string Company { get; set; }
        public string ShiftType { get; set; }
        public string ShiftLocation { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = "Active";
    }
}
