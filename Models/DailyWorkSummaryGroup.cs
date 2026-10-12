using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class DailyWorkSummaryGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; } = true;

        public List<DailyWorkSummaryGroupUser> Users { get; set; } = new List<DailyWorkSummaryGroupUser>();

        public string SendEmailsAt { get; set; } = "00:00";
        public string HolidayList { get; set; }

        public string Subject { get; set; } = "What did you work on today?";
        public string Message { get; set; } = "Please share what did you do today. If you reply by midnight, your response will be recorded!";
    }

    public class DailyWorkSummaryGroupUser
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string UserName { get; set; }
    }
}
