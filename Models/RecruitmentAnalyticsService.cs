
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class RecruitmentAnalyticsService
    {
        private readonly string _connectionString;

        public RecruitmentAnalyticsService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<RecruitmentAnalyticsRow> GetRecruitmentAnalytics(string company, DateTime onDate)
        {
            var rows = new List<RecruitmentAnalyticsRow>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT Department, Designation,
                           SUM(PlannedVacancies) AS PlannedVacancies,
                           SUM(ActualVacancies) AS ActualVacancies,
                           SUM(PlannedVacancies - ActualVacancies) AS OpenPositions
                    FROM RecruitmentAnalytics
                    WHERE IsDeleted = 0
                      AND Company = @Company
                      AND OpeningDate <= @OnDate
                      AND (ClosedDate IS NULL OR ClosedDate >= @OnDate)
                    GROUP BY Department, Designation
                    ORDER BY Department, Designation";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Company", company);
                    cmd.Parameters.AddWithValue("@OnDate", onDate);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            rows.Add(new RecruitmentAnalyticsRow
                            {
                                Department = reader["Department"]?.ToString() ?? string.Empty,
                                Designation = reader["Designation"]?.ToString() ?? string.Empty,
                                PlannedVacancies = reader["PlannedVacancies"] != DBNull.Value ? Convert.ToInt32(reader["PlannedVacancies"]) : 0,
                                ActualVacancies = reader["ActualVacancies"] != DBNull.Value ? Convert.ToInt32(reader["ActualVacancies"]) : 0,
                                OpenPositions = reader["OpenPositions"] != DBNull.Value ? Convert.ToInt32(reader["OpenPositions"]) : 0
                            });
                        }
                    }
                }
            }

            return rows;
        }
    }
}