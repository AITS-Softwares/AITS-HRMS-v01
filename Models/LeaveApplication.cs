using System;

namespace Erp_Aits.Models
{
    public class LeaveApplication
    {
        public int Id { get; set; }
        public string LeaveApplicationCode { get; set; }
        public string LeaveType { get; set; }
        public string Employee { get; set; }
        public string EmployeeName { get; set; }
        public string Company { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool HalfDay { get; set; }
        public string Reason { get; set; }
        public string LeaveApprover { get; set; }
        public DateTime? PostingDate { get; set; }
        public bool FollowViaEmail { get; set; }
        public string Status { get; set; }
        public string SalarySlip { get; set; }
        public string LetterHead { get; set; }
        public string Color { get; set; }
        public string ErpNextId { get; set; }
    }
}
