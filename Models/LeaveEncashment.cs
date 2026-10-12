using System;

namespace Erp_Aits.Models
{
    public class LeaveEncashment
    {
        public int Id { get; set; }
        public string Employee { get; set; }
        public string EmployeeName { get; set; }
        public string LeavePeriod { get; set; }
        public decimal? EncashmentDays { get; set; }
        public string Company { get; set; }
        public string LeaveType { get; set; }
        public bool PayViaPaymentEntry { get; set; }
        public DateTime EncashmentDate { get; set; }
        public string Status { get; set; }
    }
}
