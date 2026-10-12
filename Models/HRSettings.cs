namespace Erp_Aits.Models
{
    public class HRSettings
    {
        public int Id { get; set; }
        public string EmployeeNamingBy { get; set; }
        public int? RetirementAge { get; set; }
        public decimal? StandardWorkingHours { get; set; }

        public bool SendLeaveNotification { get; set; }
        public bool ExpenseApproverMandatoryInExpenseClaim { get; set; }
        public string LeaveApprovalNotificationTemplate { get; set; }
        public bool ShowLeavesOfAllDepartmentMembersInCalendar { get; set; }
        public string LeaveStatusNotificationTemplate { get; set; }
        public bool AutoLeaveEncashment { get; set; }
        public bool LeaveApproverMandatoryInLeaveApplication { get; set; }
        public bool RestrictBackdatedLeaveApplication { get; set; }
        public bool PreventSelfApprovalForLeaves { get; set; }
        public bool PreventSelfApprovalForExpenseClaims { get; set; }

        public bool AllowMultipleShiftAssignmentsForSameDate { get; set; }

        public bool CheckVacanciesOnJobOfferCreation { get; set; }
        public bool SendInterviewReminder { get; set; }
        public bool SendInterviewFeedbackReminder { get; set; }
        public string Sender { get; set; }

        public string ExitQuestionnaireWebForm { get; set; }
        public string ExitQuestionnaireNotificationTemplate { get; set; }

        public bool AllowEmployeeCheckinFromMobileApp { get; set; }
        public bool AllowGeolocationTracking { get; set; }

        public bool UnlinkPaymentOnCancellationOfEmployeeAdvance { get; set; }
        public bool RemindWorkAnniversaries { get; set; }
        public bool RemindBirthdays { get; set; }
        public bool RemindHolidays { get; set; }
        public string HolidayReminderFrequency { get; set; }
    }
}
