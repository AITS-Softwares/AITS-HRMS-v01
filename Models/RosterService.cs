using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class RosterService
    {
        private readonly string _connectionString;

        public RosterService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public RosterViewModel GetRoster(int month, int year, string employeeFilter = null)
        {
            var vm = new RosterViewModel
            {
                Month = month,
                Year = year,
                DaysInMonth = DateTime.DaysInMonth(year, month)
            };

            var rowsByEmployee = new Dictionary<string, RosterEmployeeRow>();

            using (var conn = new SqlConnection(_connectionString))
            {
                // First, get all distinct employees (from Employees table so rows show even with no shifts yet)
                string empQuery = "SELECT EmployeeCode, FullName FROM Employees WHERE IsDeleted = 0";
                if (!string.IsNullOrEmpty(employeeFilter))
                    empQuery += " AND FullName LIKE @EmployeeFilter";
                empQuery += " ORDER BY FullName";

                using (var cmd = new SqlCommand(empQuery, conn))
                {
                    if (!string.IsNullOrEmpty(employeeFilter))
                        cmd.Parameters.AddWithValue("@EmployeeFilter", "%" + employeeFilter + "%");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string code = reader["EmployeeCode"].ToString();
                            rowsByEmployee[code] = new RosterEmployeeRow
                            {
                                Employee = code,
                                EmployeeName = reader["FullName"]?.ToString() ?? string.Empty
                            };
                        }
                    }
                }

                // Then, load shift assignments for the month
                string shiftQuery = @"SELECT Employee, ShiftType, ShiftDate, FromTime, ToTime
                                       FROM ShiftAssignments
                                       WHERE IsDeleted = 0
                                         AND MONTH(ShiftDate) = @Month
                                         AND YEAR(ShiftDate) = @Year";

                using (var cmd = new SqlCommand(shiftQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Month", month);
                    cmd.Parameters.AddWithValue("@Year", year);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string emp = reader["Employee"].ToString();
                            if (!rowsByEmployee.ContainsKey(emp)) continue;

                            var date = Convert.ToDateTime(reader["ShiftDate"]);
                            rowsByEmployee[emp].ShiftsByDay[date.Day] = new RosterShift
                            {
                                ShiftType = reader["ShiftType"]?.ToString() ?? string.Empty,
                                FromTime = reader["FromTime"] as TimeSpan?,
                                ToTime = reader["ToTime"] as TimeSpan?
                            };
                        }
                    }
                }
            }

            vm.Employees = new List<RosterEmployeeRow>(rowsByEmployee.Values);
            return vm;
        }
    }
}
