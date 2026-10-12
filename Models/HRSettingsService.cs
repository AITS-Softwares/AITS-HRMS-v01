using System;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class HRSettingsService
    {
        private readonly string _connectionString;

        public HRSettingsService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public HRSettings GetSettings()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT TOP 1 * FROM HRSettings ORDER BY Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new HRSettings
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                EmployeeNamingBy = reader["EmployeeNamingBy"]?.ToString() ?? "Naming Series",
                                RetirementAge = reader["RetirementAge"] != DBNull.Value ? Convert.ToInt32(reader["RetirementAge"]) : (int?)null,
                                StandardWorkingHours = reader["StandardWorkingHours"] != DBNull.Value ? Convert.ToDecimal(reader["StandardWorkingHours"]) : (decimal?)null,
                                RemindWorkAnniversaries = Convert.ToBoolean(reader["RemindWorkAnniversaries"]),
                                RemindBirthdays = Convert.ToBoolean(reader["RemindBirthdays"]),
                                RemindHolidays = Convert.ToBoolean(reader["RemindHolidays"]),
                                HolidayReminderFrequency = reader["HolidayReminderFrequency"]?.ToString() ?? "Weekly",

                                SendLeaveNotification = Convert.ToBoolean(reader["SendLeaveNotification"]),
                                ExpenseApproverMandatoryInExpenseClaim = Convert.ToBoolean(reader["ExpenseApproverMandatoryInExpenseClaim"]),
                                LeaveApprovalNotificationTemplate = reader["LeaveApprovalNotificationTemplate"]?.ToString() ?? string.Empty,
                                ShowLeavesOfAllDepartmentMembersInCalendar = Convert.ToBoolean(reader["ShowLeavesOfAllDepartmentMembersInCalendar"]),
                                LeaveStatusNotificationTemplate = reader["LeaveStatusNotificationTemplate"]?.ToString() ?? string.Empty,
                                AutoLeaveEncashment = Convert.ToBoolean(reader["AutoLeaveEncashment"]),
                                LeaveApproverMandatoryInLeaveApplication = Convert.ToBoolean(reader["LeaveApproverMandatoryInLeaveApplication"]),
                                RestrictBackdatedLeaveApplication = Convert.ToBoolean(reader["RestrictBackdatedLeaveApplication"]),
                                PreventSelfApprovalForLeaves = Convert.ToBoolean(reader["PreventSelfApprovalForLeaves"]),
                                PreventSelfApprovalForExpenseClaims = Convert.ToBoolean(reader["PreventSelfApprovalForExpenseClaims"]),

                                AllowMultipleShiftAssignmentsForSameDate = Convert.ToBoolean(reader["AllowMultipleShiftAssignmentsForSameDate"]),

                                CheckVacanciesOnJobOfferCreation = Convert.ToBoolean(reader["CheckVacanciesOnJobOfferCreation"]),
                                SendInterviewReminder = Convert.ToBoolean(reader["SendInterviewReminder"]),
                                SendInterviewFeedbackReminder = Convert.ToBoolean(reader["SendInterviewFeedbackReminder"]),
                                Sender = reader["Sender"]?.ToString() ?? string.Empty,

                                ExitQuestionnaireWebForm = reader["ExitQuestionnaireWebForm"]?.ToString() ?? string.Empty,
                                ExitQuestionnaireNotificationTemplate = reader["ExitQuestionnaireNotificationTemplate"]?.ToString() ?? string.Empty,

                                AllowEmployeeCheckinFromMobileApp = Convert.ToBoolean(reader["AllowEmployeeCheckinFromMobileApp"]),
                                AllowGeolocationTracking = Convert.ToBoolean(reader["AllowGeolocationTracking"]),

                                UnlinkPaymentOnCancellationOfEmployeeAdvance = Convert.ToBoolean(reader["UnlinkPaymentOnCancellationOfEmployeeAdvance"])

                            };
                        }
                    }
                }
            }

            return new HRSettings { EmployeeNamingBy = "Naming Series" };
        }

        public void SaveSettings(HRSettings s)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    UPDATE HRSettings SET
                        EmployeeNamingBy = @EmployeeNamingBy,
                        RetirementAge = @RetirementAge,
                        StandardWorkingHours = @StandardWorkingHours,
                        SendLeaveNotification = @SendLeaveNotification,
                        ExpenseApproverMandatoryInExpenseClaim = @ExpenseApproverMandatoryInExpenseClaim,
                        LeaveApprovalNotificationTemplate = @LeaveApprovalNotificationTemplate,
                        ShowLeavesOfAllDepartmentMembersInCalendar = @ShowLeavesOfAllDepartmentMembersInCalendar,
                        LeaveStatusNotificationTemplate = @LeaveStatusNotificationTemplate,
                        AutoLeaveEncashment = @AutoLeaveEncashment,
                        LeaveApproverMandatoryInLeaveApplication = @LeaveApproverMandatoryInLeaveApplication,
                        RestrictBackdatedLeaveApplication = @RestrictBackdatedLeaveApplication,
                        PreventSelfApprovalForLeaves = @PreventSelfApprovalForLeaves,
                        PreventSelfApprovalForExpenseClaims = @PreventSelfApprovalForExpenseClaims,
                        AllowMultipleShiftAssignmentsForSameDate = @AllowMultipleShiftAssignmentsForSameDate,
                        CheckVacanciesOnJobOfferCreation = @CheckVacanciesOnJobOfferCreation,
                        SendInterviewReminder = @SendInterviewReminder,
                        SendInterviewFeedbackReminder = @SendInterviewFeedbackReminder,
                        Sender = @Sender,
                        ExitQuestionnaireWebForm = @ExitQuestionnaireWebForm,
                        ExitQuestionnaireNotificationTemplate = @ExitQuestionnaireNotificationTemplate,
                        AllowEmployeeCheckinFromMobileApp = @AllowEmployeeCheckinFromMobileApp,
                        AllowGeolocationTracking = @AllowGeolocationTracking,
                        UnlinkPaymentOnCancellationOfEmployeeAdvance = @UnlinkPaymentOnCancellationOfEmployeeAdvance
                        RemindWorkAnniversaries = @RemindWorkAnniversaries,
                        RemindBirthdays = @RemindBirthdays,
                        RemindHolidays = @RemindHolidays,
                        HolidayReminderFrequency = @HolidayReminderFrequency,
                        WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", s.Id);
                    cmd.Parameters.AddWithValue("@EmployeeNamingBy", s.EmployeeNamingBy ?? "Naming Series");
                    cmd.Parameters.AddWithValue("@RetirementAge", (object)s.RetirementAge ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@StandardWorkingHours", (object)s.StandardWorkingHours ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SendLeaveNotification", s.SendLeaveNotification);
                    cmd.Parameters.AddWithValue("@ExpenseApproverMandatoryInExpenseClaim", s.ExpenseApproverMandatoryInExpenseClaim);
                    cmd.Parameters.AddWithValue("@LeaveApprovalNotificationTemplate", s.LeaveApprovalNotificationTemplate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ShowLeavesOfAllDepartmentMembersInCalendar", s.ShowLeavesOfAllDepartmentMembersInCalendar);
                    cmd.Parameters.AddWithValue("@LeaveStatusNotificationTemplate", s.LeaveStatusNotificationTemplate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AutoLeaveEncashment", s.AutoLeaveEncashment);
                    cmd.Parameters.AddWithValue("@LeaveApproverMandatoryInLeaveApplication", s.LeaveApproverMandatoryInLeaveApplication);
                    cmd.Parameters.AddWithValue("@RestrictBackdatedLeaveApplication", s.RestrictBackdatedLeaveApplication);
                    cmd.Parameters.AddWithValue("@PreventSelfApprovalForLeaves", s.PreventSelfApprovalForLeaves);
                    cmd.Parameters.AddWithValue("@PreventSelfApprovalForExpenseClaims", s.PreventSelfApprovalForExpenseClaims);
                    cmd.Parameters.AddWithValue("@AllowMultipleShiftAssignmentsForSameDate", s.AllowMultipleShiftAssignmentsForSameDate);
                    cmd.Parameters.AddWithValue("@CheckVacanciesOnJobOfferCreation", s.CheckVacanciesOnJobOfferCreation);
                    cmd.Parameters.AddWithValue("@SendInterviewReminder", s.SendInterviewReminder);
                    cmd.Parameters.AddWithValue("@SendInterviewFeedbackReminder", s.SendInterviewFeedbackReminder);
                    cmd.Parameters.AddWithValue("@Sender", s.Sender ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ExitQuestionnaireWebForm", s.ExitQuestionnaireWebForm ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ExitQuestionnaireNotificationTemplate", s.ExitQuestionnaireNotificationTemplate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AllowEmployeeCheckinFromMobileApp", s.AllowEmployeeCheckinFromMobileApp);
                    cmd.Parameters.AddWithValue("@AllowGeolocationTracking", s.AllowGeolocationTracking);
                    cmd.Parameters.AddWithValue("@UnlinkPaymentOnCancellationOfEmployeeAdvance", s.UnlinkPaymentOnCancellationOfEmployeeAdvance);
                    cmd.Parameters.AddWithValue("@RemindWorkAnniversaries", s.RemindWorkAnniversaries);
                    cmd.Parameters.AddWithValue("@RemindBirthdays", s.RemindBirthdays);
                    cmd.Parameters.AddWithValue("@RemindHolidays", s.RemindHolidays);
                    cmd.Parameters.AddWithValue("@HolidayReminderFrequency", s.HolidayReminderFrequency ?? "Weekly");

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
