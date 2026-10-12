using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class ShiftAttendanceService
    {
        private readonly string _connectionString;

        public ShiftAttendanceService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<ShiftAttendanceRow> GetShiftAttendance(
            DateTime fromDate, DateTime toDate, string employee, string shiftType,
            string department, string company, bool lateEntry, bool earlyExit,
            bool considerGracePeriod, bool includeWithoutCheckins)
        {
            var list = new List<ShiftAttendanceRow>();

            using (var conn = new SqlConnection(_connectionString))
            {
                //string query = @"
                //    SELECT a.Employee, a.EmployeeName, e.Department, a.Shift, a.Status, a.AttendanceDate, a.LateEntry, a.EarlyExit
                //    FROM Attendance a
                //    LEFT JOIN Employees e ON e.EmployeeCode = a.Employee
                //    WHERE a.AttendanceDate BETWEEN @FromDate AND @ToDate";
                string query = @"
                    SELECT a.Employee, a.EmployeeName, e.Department, a.Shift, a.Status, a.AttendanceDate, a.LateEntry, a.EarlyExit
                    FROM Attendances a
                    LEFT JOIN Employees e ON e.EmployeeCode = a.Employee
                    WHERE a.AttendanceDate BETWEEN @FromDate AND @ToDate";

                if (!string.IsNullOrEmpty(employee)) query += " AND a.Employee LIKE @Employee";
                if (!string.IsNullOrEmpty(shiftType)) query += " AND a.Shift LIKE @ShiftType";
                if (!string.IsNullOrEmpty(department)) query += " AND e.Department LIKE @Department";
                if (!string.IsNullOrEmpty(company)) query += " AND a.Company = @Company";
                if (lateEntry) query += " AND a.LateEntry = 1";
                if (earlyExit) query += " AND a.EarlyExit = 1";

                query += " ORDER BY a.AttendanceDate DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);
                    if (!string.IsNullOrEmpty(employee)) cmd.Parameters.AddWithValue("@Employee", "%" + employee + "%");
                    if (!string.IsNullOrEmpty(shiftType)) cmd.Parameters.AddWithValue("@ShiftType", "%" + shiftType + "%");
                    if (!string.IsNullOrEmpty(department)) cmd.Parameters.AddWithValue("@Department", "%" + department + "%");
                    if (!string.IsNullOrEmpty(company)) cmd.Parameters.AddWithValue("@Company", company);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new ShiftAttendanceRow
                            {
                                Employee = reader["Employee"]?.ToString() ?? string.Empty,
                                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
                                Department = reader["Department"]?.ToString() ?? string.Empty,
                                Shift = reader["Shift"]?.ToString() ?? string.Empty,
                                Status = reader["Status"]?.ToString() ?? string.Empty,
                                AttendanceDate = Convert.ToDateTime(reader["AttendanceDate"]),
                                LateEntry = reader["LateEntry"] != DBNull.Value && Convert.ToBoolean(reader["LateEntry"]),
                                EarlyExit = reader["EarlyExit"] != DBNull.Value && Convert.ToBoolean(reader["EarlyExit"])
                            });
                        }
                    }
                }
            }

            return list;
        }
    }
}