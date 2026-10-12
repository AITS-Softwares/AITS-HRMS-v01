using System;

namespace Erp_Aits.Models
{
    public class DailyWorkSummaryReplyRow
    {
        public int Id { get; set; }
        public string GroupName { get; set; }
        public string Employee { get; set; }
        public string EmployeeName { get; set; }
        public DateTime ReplyDate { get; set; }
        public string Summary { get; set; }
    }
}