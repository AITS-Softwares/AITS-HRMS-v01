namespace Erp_Aits.Models
{
    public class PayrollSettings
    {
        public string CalculatePayrollWorkingDaysBasedOn { get; set; } = "Attendance";
        public string ConsiderUnmarkedAttendanceAs { get; set; } = "Absent";

        public bool IncludeHolidaysInTotalWorkingDays { get; set; }

        public bool ConsiderMarkedAttendanceOnHolidays { get; set; }

        public decimal MaxWorkingHoursAgainstTimesheet { get; set; }
        public decimal FractionOfDailySalaryForHalfDay { get; set; } = 0.50m;

        public bool DisableRoundedTotal { get; set; }
        public bool ShowLeaveBalancesInSalarySlip { get; set; }

        public bool EmailSalarySlipToEmployee { get; set; }
        public string SalarySlipEmailTemplate { get; set; }
        public string EmailAccount { get; set; }

        public bool ProcessPayrollAccountingEntryBasedOnEmployee { get; set; }
    }
}