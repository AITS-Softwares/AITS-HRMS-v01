using System;

namespace Erp_Aits.Models
{
    public class LeavePolicyAssignment
    {
        public int Id { get; set; }

        // ERPNext document name
        public string ErpNextId { get; set; }

        public string Employee { get; set; }

        public string EmployeeName { get; set; }

        public string Company { get; set; }

        public string LeavePolicy { get; set; }

        public string AssignmentBasedOn { get; set; }

        // ERP field: leave_period
        public string LeavePeriod { get; set; }

        public DateTime EffectiveFrom { get; set; }

        public DateTime EffectiveTo { get; set; }

        // HRMS property name retained for compatibility.
        // ERP field = carry_forward
        public bool AddUnusedLeaves { get; set; }

        public bool LeavesAllocated { get; set; }

        // 0 Draft
        // 1 Submitted
        // 2 Cancelled
        public int DocStatus { get; set; }

        public string AmendedFrom { get; set; }
    }
}