using System;

namespace Erp_Aits.Models
{
    public class LeaveAllocation
    {
        public int Id { get; set; }
        public string Series { get; set; } = "HR-LAL-.YYYY.-";
        public string Employee { get; set; }
        public string EmployeeName { get; set; }
        public string Company { get; set; } = "AITS";
        public string LeaveType { get; set; }
        public string LeavePeriod { get; set; }
        public string LeavePolicy { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal? NewLeavesAllocated { get; set; }
        public bool AddUnusedLeaves { get; set; }
        public string Description { get; set; }
        public bool Expired { get; set; }
        public string ErpNextId { get; set; }
        public int DocStatus { get; set; }
        public string Status { get; set; } = "Draft";
    }
}
