using System;
using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class RosterShift
    {
        public string ShiftType { get; set; }
        public TimeSpan? FromTime { get; set; }
        public TimeSpan? ToTime { get; set; }
    }

    public class RosterEmployeeRow
    {
        public string Employee { get; set; }
        public string EmployeeName { get; set; }
        // key = day of month (1-31)
        public Dictionary<int, RosterShift> ShiftsByDay { get; set; } = new Dictionary<int, RosterShift>();
    }

    public class RosterViewModel
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int DaysInMonth { get; set; }
        public List<RosterEmployeeRow> Employees { get; set; } = new List<RosterEmployeeRow>();
    }
}
