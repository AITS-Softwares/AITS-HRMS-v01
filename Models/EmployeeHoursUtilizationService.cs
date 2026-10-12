using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class EmployeeHoursUtilizationService
    {
        private readonly string _connectionString;

        public EmployeeHoursUtilizationService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<EmployeeHoursUtilizationRow> GetUtilization(
            DateTime fromDate, DateTime toDate, string employee, string department, string project, string company)
        {
            var list = new List<EmployeeHoursUtilizationRow>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT t.Employee, e.FullName AS EmployeeName, e.Department, d.Project, SUM(d.Hrs) AS TotalHours
                    FROM TimeSheetDetails d
                    INNER JOIN TimeSheets t ON t.Id = d.TimeSheetId
                    LEFT JOIN Employees e ON e.EmployeeCode = t.Employee
                    WHERE ISNULL(d.FromTime, t.CreatedOn) BETWEEN @FromDate AND @ToDate
                      AND t.IsDeleted = 0";

                if (!string.IsNullOrEmpty(company)) query += " AND t.Company = @Company";
                if (!string.IsNullOrEmpty(employee)) query += " AND t.Employee LIKE @Employee";
                if (!string.IsNullOrEmpty(department)) query += " AND e.Department LIKE @Department";
                if (!string.IsNullOrEmpty(project)) query += " AND d.Project LIKE @Project";

                query += " GROUP BY t.Employee, e.FullName, e.Department, d.Project ORDER BY t.Employee";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);
                    if (!string.IsNullOrEmpty(company)) cmd.Parameters.AddWithValue("@Company", company);
                    if (!string.IsNullOrEmpty(employee)) cmd.Parameters.AddWithValue("@Employee", "%" + employee + "%");
                    if (!string.IsNullOrEmpty(department)) cmd.Parameters.AddWithValue("@Department", "%" + department + "%");
                    if (!string.IsNullOrEmpty(project)) cmd.Parameters.AddWithValue("@Project", "%" + project + "%");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new EmployeeHoursUtilizationRow
                            {
                                Employee = reader["Employee"]?.ToString() ?? string.Empty,
                                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
                                Department = reader["Department"]?.ToString() ?? string.Empty,
                                Project = reader["Project"]?.ToString() ?? string.Empty,
                                TotalHours = reader["TotalHours"] != DBNull.Value ? Convert.ToDecimal(reader["TotalHours"]) : 0
                            });
                        }
                    }
                }
            }

            return list;
        }
    }
}
