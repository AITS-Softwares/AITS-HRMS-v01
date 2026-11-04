namespace Erp_Aits.Models
{
    public class ShiftType
    {
        public int Id { get; set; }
        public string ShiftTypeCode { get; set; }

        public string Name { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }

        public string HolidayList { get; set; }
        public string RosterColor { get; set; } = "Blue";
        public bool EnableAutoAttendance { get; set; }
        public string DetermineCheckInAndCheckOut { get; set; }
        public string WorkingHoursCalculationBasedOn { get; set; }
        public int BeginCheckInBeforeShiftStartTime { get; set; } = 60;
        public int AllowCheckOutAfterShiftEndTime { get; set; } = 60;
        public bool MarkAutoAttendanceOnHolidays { get; set; }
        public decimal? WorkingHoursThresholdForHalfDay { get; set; }
        public decimal? WorkingHoursThresholdForAbsent { get; set; }
        public DateTime? ProcessAttendanceAfter { get; set; }
        public DateTime? LastSyncOfCheckin { get; set; }
        public bool AutoUpdateLastSync { get; set; }
        public bool EnableLateEntryMarking { get; set; }
        public int? LateEntryGracePeriod { get; set; }
        public bool EnableEarlyExitMarking { get; set; }
        public int? EarlyExitGracePeriod { get; set; }
    }
}
