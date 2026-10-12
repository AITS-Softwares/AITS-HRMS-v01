using System;

namespace Erp_Aits.Models
{
    public class CompensatoryLeaveRequest
    {
        public int Id { get; set; }
        public string Employee { get; set; }
        public string EmployeeName { get; set; }
        public string LeaveType { get; set; }
        public DateTime? WorkFromDate { get; set; }
        public DateTime? WorkEndDate { get; set; }
        public bool HalfDay { get; set; }
        public string Reason { get; set; }
        public string ErpNextId { get; set; }
        public string Status { get; set; } = "Draft";
    }
}
