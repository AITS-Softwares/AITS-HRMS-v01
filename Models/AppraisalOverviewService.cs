using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class AppraisalOverviewService
    {
        private readonly string _connectionString;

        public AppraisalOverviewService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<AppraisalOverviewRow> GetAppraisalOverview(
            string company, string appraisalCycle, string employee, string department, string designation)
        {
            var rows = new List<AppraisalOverviewRow>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT Employee, EmployeeName, Department, Designation, AppraisalCycle, Status, FinalScore
                    FROM AppraisalOverview
                    WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(company)) query += " AND Company = @Company";
                if (!string.IsNullOrEmpty(appraisalCycle)) query += " AND AppraisalCycle = @AppraisalCycle";
                if (!string.IsNullOrEmpty(employee)) query += " AND Employee = @Employee";
                if (!string.IsNullOrEmpty(department)) query += " AND Department = @Department";
                if (!string.IsNullOrEmpty(designation)) query += " AND Designation = @Designation";

                query += " ORDER BY EmployeeName";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(company)) cmd.Parameters.AddWithValue("@Company", company);
                    if (!string.IsNullOrEmpty(appraisalCycle)) cmd.Parameters.AddWithValue("@AppraisalCycle", appraisalCycle);
                    if (!string.IsNullOrEmpty(employee)) cmd.Parameters.AddWithValue("@Employee", employee);
                    if (!string.IsNullOrEmpty(department)) cmd.Parameters.AddWithValue("@Department", department);
                    if (!string.IsNullOrEmpty(designation)) cmd.Parameters.AddWithValue("@Designation", designation);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            rows.Add(new AppraisalOverviewRow
                            {
                                Employee = reader["Employee"]?.ToString() ?? string.Empty,
                                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
                                Department = reader["Department"]?.ToString() ?? string.Empty,
                                Designation = reader["Designation"]?.ToString() ?? string.Empty,
                                AppraisalCycle = reader["AppraisalCycle"]?.ToString() ?? string.Empty,
                                Status = reader["Status"]?.ToString() ?? string.Empty,
                                FinalScore = reader["FinalScore"] != DBNull.Value ? Convert.ToDecimal(reader["FinalScore"]) : (decimal?)null
                            });
                        }
                    }
                }
            }

            return rows;
        }
    }
}
