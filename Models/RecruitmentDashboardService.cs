using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class RecruitmentDashboardService
    {
        private readonly string _connectionString;

        public RecruitmentDashboardService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public RecruitmentDashboardViewModel GetDashboard()
        {
            var vm = new RecruitmentDashboardViewModel();

            vm.JobOpeningOpenCount = SafeCount("SELECT COUNT(*) FROM JobOpenings WHERE Status = 'Open'");
            vm.JobApplicantOpenCount = SafeCount("SELECT COUNT(*) FROM JobApplicants WHERE Status = 'Open'");
            vm.JobOfferAcceptedCount = SafeCount("SELECT COUNT(*) FROM JobOffers WHERE Status = 'Accepted'");
            vm.InterviewsThisWeek = SafeInterviewsThisWeek();

            return vm;
        }

        private int SafeCount(string query)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch
            {
                // Table doesn't exist yet or query failed — default to 0 instead of crashing the page
                return 0;
            }
        }

        private List<InterviewThisWeekRow> SafeInterviewsThisWeek()
        {
            var rows = new List<InterviewThisWeekRow>();
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    string query = @"
                        SELECT CandidateName, JobOpening, InterviewDate, InterviewRound, Status
                        FROM Interviews
                        WHERE InterviewDate BETWEEN DATEADD(DAY, -(DATEPART(WEEKDAY, GETDATE())-1), CAST(GETDATE() AS DATE))
                                                 AND DATEADD(DAY, 7-(DATEPART(WEEKDAY, GETDATE())), CAST(GETDATE() AS DATE))
                        ORDER BY InterviewDate";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                rows.Add(new InterviewThisWeekRow
                                {
                                    CandidateName = reader["CandidateName"]?.ToString() ?? string.Empty,
                                    JobOpening = reader["JobOpening"]?.ToString() ?? string.Empty,
                                    InterviewDate = Convert.ToDateTime(reader["InterviewDate"]),
                                    InterviewRound = reader["InterviewRound"]?.ToString() ?? string.Empty,
                                    Status = reader["Status"]?.ToString() ?? string.Empty
                                });
                            }
                        }
                    }
                }
            }
            catch
            {
                // Table doesn't exist yet — return empty list, page will show "No Data..."
            }
            return rows;
        }
    }
}
