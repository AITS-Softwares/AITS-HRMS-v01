using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;
namespace Erp_Aits.Services
{
    public class HolidayAttendanceService
    {
        private readonly string _connectionString;
        public HolidayAttendanceService(string connectionString)
        {
            _connectionString = connectionString;
        }
        public List<EmployeeWorkingOnHolidayRow> GetEmployeesWorkingOnHoliday(
            DateTime fromDate,
            DateTime toDate,
            string holidayListName = null,
            string department = null,
            string company = null)
        {
            var rows = new List<EmployeeWorkingOnHolidayRow>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT
                        a.Employee,
                        a.EmployeeName,
                        e.Department,
                        a.Company,
                        h.HolidayDate,
                        h.Description AS HolidayDescription,
                        a.Status AS AttendanceStatus
                    FROM Attendances a
                    INNER JOIN Holidays h ON h.HolidayDate = a.AttendanceDate
                    INNER JOIN HolidayLists hl ON hl.Id = h.HolidayListId
                    LEFT JOIN Employees e ON e.EmployeeCode = a.Employee
                    WHERE a.Status = 'Present'
                      AND h.HolidayDate BETWEEN @FromDate AND @ToDate
                      AND hl.IsDeleted = 0";
                if (!string.IsNullOrEmpty(holidayListName))
                    query += " AND hl.HolidayListName = @HolidayListName";
                if (!string.IsNullOrEmpty(department))
                    query += " AND e.Department = @Department";
                if (!string.IsNullOrEmpty(company))
                    query += " AND a.Company = @Company";
                query += " ORDER BY h.HolidayDate DESC, a.EmployeeName";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);
                    if (!string.IsNullOrEmpty(holidayListName))
                        cmd.Parameters.AddWithValue("@HolidayListName", holidayListName);
                    if (!string.IsNullOrEmpty(department))
                        cmd.Parameters.AddWithValue("@Department", department);
                    if (!string.IsNullOrEmpty(company))
                        cmd.Parameters.AddWithValue("@Company", company);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            rows.Add(new EmployeeWorkingOnHolidayRow
                            {
                                Employee = reader["Employee"]?.ToString() ?? string.Empty,
                                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
                                Department = reader["Department"]?.ToString() ?? string.Empty,
                                Company = reader["Company"]?.ToString() ?? string.Empty,
                                HolidayDate = Convert.ToDateTime(reader["HolidayDate"]),
                                HolidayDescription = reader["HolidayDescription"]?.ToString() ?? string.Empty,
                                AttendanceStatus = reader["AttendanceStatus"]?.ToString() ?? string.Empty
                            });
                        }
                    }
                }
            }
            return rows;
        }
    }
}