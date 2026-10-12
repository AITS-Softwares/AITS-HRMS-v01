using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class EmployeeAttendanceToolService
    {
        private readonly string _connectionString;

        public EmployeeAttendanceToolService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<AttendanceMarkRow> GetUnmarkedEmployees(
            DateTime date, string company, string employmentType,
            string designation, string department, string employeeGrade)
        {
            var list = new List<AttendanceMarkRow>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT EmployeeCode, FullName
                    FROM Employees
                    WHERE Status = 'Active'
                      AND EmployeeCode NOT IN (
                          SELECT Employee FROM Attendances
                          WHERE AttendanceDate = @Date AND IsDeleted = 0
                      )";

                if (!string.IsNullOrEmpty(company)) query += " AND Company = @Company";
                if (!string.IsNullOrEmpty(employmentType)) query += " AND EmploymentType = @EmploymentType";
                if (!string.IsNullOrEmpty(designation)) query += " AND Designation = @Designation";
                if (!string.IsNullOrEmpty(department)) query += " AND Department = @Department";
                if (!string.IsNullOrEmpty(employeeGrade)) query += " AND Grade = @Grade";

                query += " ORDER BY EmployeeCode";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Date", date.Date);
                    if (!string.IsNullOrEmpty(company)) cmd.Parameters.AddWithValue("@Company", company);
                    if (!string.IsNullOrEmpty(employmentType)) cmd.Parameters.AddWithValue("@EmploymentType", employmentType);
                    if (!string.IsNullOrEmpty(designation)) cmd.Parameters.AddWithValue("@Designation", designation);
                    if (!string.IsNullOrEmpty(department)) cmd.Parameters.AddWithValue("@Department", department);
                    if (!string.IsNullOrEmpty(employeeGrade)) cmd.Parameters.AddWithValue("@Grade", employeeGrade);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new AttendanceMarkRow
                            {
                                EmployeeCode = reader["EmployeeCode"]?.ToString() ?? string.Empty,
                                FullName = reader["FullName"]?.ToString() ?? string.Empty
                            });
                        }
                    }
                }
            }

            return list;
        }

        public List<MarkedAttendanceRow> GetMarkedAttendance(
            DateTime date, string company, string employmentType,
            string designation, string department, string employeeGrade)
        {
            var list = new List<MarkedAttendanceRow>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT a.Employee, a.EmployeeName, a.Status, a.Shift
                    FROM Attendances a
                    INNER JOIN Employees e ON e.EmployeeCode = a.Employee
                    WHERE a.AttendanceDate = @Date AND a.IsDeleted = 0";

                if (!string.IsNullOrEmpty(company)) query += " AND a.Company = @Company";
                if (!string.IsNullOrEmpty(employmentType)) query += " AND e.EmploymentType = @EmploymentType";
                if (!string.IsNullOrEmpty(designation)) query += " AND e.Designation = @Designation";
                if (!string.IsNullOrEmpty(department)) query += " AND e.Department = @Department";
                if (!string.IsNullOrEmpty(employeeGrade)) query += " AND e.Grade = @Grade";

                query += " ORDER BY a.Employee";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Date", date.Date);
                    if (!string.IsNullOrEmpty(company)) cmd.Parameters.AddWithValue("@Company", company);
                    if (!string.IsNullOrEmpty(employmentType)) cmd.Parameters.AddWithValue("@EmploymentType", employmentType);
                    if (!string.IsNullOrEmpty(designation)) cmd.Parameters.AddWithValue("@Designation", designation);
                    if (!string.IsNullOrEmpty(department)) cmd.Parameters.AddWithValue("@Department", department);
                    if (!string.IsNullOrEmpty(employeeGrade)) cmd.Parameters.AddWithValue("@Grade", employeeGrade);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new MarkedAttendanceRow
                            {
                                Employee = reader["Employee"]?.ToString() ?? string.Empty,
                                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
                                Status = reader["Status"]?.ToString() ?? string.Empty,
                                Shift = reader["Shift"]?.ToString() ?? string.Empty
                            });
                        }
                    }
                }
            }

            return list;
        }
    }
}