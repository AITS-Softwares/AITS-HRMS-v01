namespace Erp_Aits.Models
{
    public class LeaveType
    {
        public int Id { get; set; }
        public string LeaveTypeName { get; set; }
        public decimal? MaxLeavesAllowed { get; set; }
        public int? ApplicableAfterWorkingDays { get; set; }
        public int? MaxContinuousDaysAllowed { get; set; }

        public bool IsCarryForward { get; set; }
        public bool IsLWP { get; set; }
        public bool IsPartiallyPaidLeave { get; set; }
        public bool IsOptionalLeave { get; set; }
        public bool AllowNegativeBalance { get; set; }
        public bool AllowOverAllocation { get; set; }
        public bool IncludeHolidaysWithinLeaves { get; set; }
        public bool IsCompensatory { get; set; }

        public bool AllowEncashment { get; set; }
        public decimal? MaxEncashableLeaves { get; set; }
        public decimal? NonEncashableLeaves { get; set; }
        public string EarningComponent { get; set; }
        public bool IsEarnedLeave { get; set; }
        public string EarnedLeaveFrequency { get; set; }
        public string AllocateOnDay { get; set; }
        public decimal? Rounding { get; set; }
        public decimal? MaxCarryForwardedLeaves { get; set; }
        public int? ExpireCarryForwardedLeaves { get; set; }
        public decimal? FractionOfDailySalaryPerLeave { get; set; }
        public string ErpNextId { get; set; }
    }
}
