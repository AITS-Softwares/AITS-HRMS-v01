using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class MonthlyAttendanceSheetRow
    {
        public string Employee { get; set; }
        public string EmployeeName { get; set; }
        public string Shift { get; set; }
        public Dictionary<int, string> DayStatus { get; set; } = new Dictionary<int, string>();
    }

    public class DailySummary
    {
        public int Present { get; set; }
        public int Absent { get; set; }
        public int Leave { get; set; }
    }

    public class MonthlyAttendanceSheetViewModel
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int DaysInMonth { get; set; }
        public List<MonthlyAttendanceSheetRow> Rows { get; set; } = new List<MonthlyAttendanceSheetRow>();
        public Dictionary<int, DailySummary> DailySummaries { get; set; } = new Dictionary<int, DailySummary>();
    }
}
