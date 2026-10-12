using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class EmployeeExitService
    {
        private readonly string _connectionString;

        public EmployeeExitService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<EmployeeExitRow> GetEmployeeExits(
            DateTime fromDate,
            DateTime toDate,
            string company = null,
            string department = null,
            string designation = null,
            string employee = null,
            string reportsTo = null,
            string interviewStatus = null,
            string finalDecision = null,
            bool? exitInterviewPending = null,
            bool? exitQuestionnairePending = null,
            bool? fnfPending = null)
        {
            var rows = new List<EmployeeExitRow>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT Employee, EmployeeName, Company, Department, Designation, ReportsTo,
                           RelievingDate, InterviewStatus, FinalDecision,
                           ExitInterviewPending, ExitQuestionnairePending, FnFPending
                    FROM EmployeeExits
                    WHERE IsDeleted = 0
                      AND RelievingDate BETWEEN @FromDate AND @ToDate";

                if (!string.IsNullOrEmpty(company)) query += " AND Company = @Company";
                if (!string.IsNullOrEmpty(department)) query += " AND Department = @Department";
                if (!string.IsNullOrEmpty(designation)) query += " AND Designation = @Designation";
                if (!string.IsNullOrEmpty(employee)) query += " AND Employee = @Employee";
                if (!string.IsNullOrEmpty(reportsTo)) query += " AND ReportsTo = @ReportsTo";
                if (!string.IsNullOrEmpty(interviewStatus)) query += " AND InterviewStatus = @InterviewStatus";
                if (!string.IsNullOrEmpty(finalDecision)) query += " AND FinalDecision = @FinalDecision";
                if (exitInterviewPending.HasValue) query += " AND ExitInterviewPending = @ExitInterviewPending";
                if (exitQuestionnairePending.HasValue) query += " AND ExitQuestionnairePending = @ExitQuestionnairePending";
                if (fnfPending.HasValue) query += " AND FnFPending = @FnFPending";

                query += " ORDER BY RelievingDate DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);

                    if (!string.IsNullOrEmpty(company)) cmd.Parameters.AddWithValue("@Company", company);
                    if (!string.IsNullOrEmpty(department)) cmd.Parameters.AddWithValue("@Department", department);
                    if (!string.IsNullOrEmpty(designation)) cmd.Parameters.AddWithValue("@Designation", designation);
                    if (!string.IsNullOrEmpty(employee)) cmd.Parameters.AddWithValue("@Employee", employee);
                    if (!string.IsNullOrEmpty(reportsTo)) cmd.Parameters.AddWithValue("@ReportsTo", reportsTo);
                    if (!string.IsNullOrEmpty(interviewStatus)) cmd.Parameters.AddWithValue("@InterviewStatus", interviewStatus);
                    if (!string.IsNullOrEmpty(finalDecision)) cmd.Parameters.AddWithValue("@FinalDecision", finalDecision);
                    if (exitInterviewPending.HasValue) cmd.Parameters.AddWithValue("@ExitInterviewPending", exitInterviewPending.Value);
                    if (exitQuestionnairePending.HasValue) cmd.Parameters.AddWithValue("@ExitQuestionnairePending", exitQuestionnairePending.Value);
                    if (fnfPending.HasValue) cmd.Parameters.AddWithValue("@FnFPending", fnfPending.Value);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            rows.Add(new EmployeeExitRow
                            {
                                Employee = reader["Employee"]?.ToString() ?? string.Empty,
                                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
                                Company = reader["Company"]?.ToString() ?? string.Empty,
                                Department = reader["Department"]?.ToString() ?? string.Empty,
                                Designation = reader["Designation"]?.ToString() ?? string.Empty,
                                ReportsTo = reader["ReportsTo"]?.ToString() ?? string.Empty,
                                RelievingDate = reader["RelievingDate"] as DateTime?,
                                InterviewStatus = reader["InterviewStatus"]?.ToString() ?? string.Empty,
                                FinalDecision = reader["FinalDecision"]?.ToString() ?? string.Empty,
                                ExitInterviewPending = reader["ExitInterviewPending"] as bool? ?? false,
                                ExitQuestionnairePending = reader["ExitQuestionnairePending"] as bool? ?? false,
                                FnFPending = reader["FnFPending"] as bool? ?? false
                            });
                        }
                    }
                }
            }

            return rows;
        }
    }
}
