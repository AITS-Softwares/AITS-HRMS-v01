using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class MonthlyAttendanceSheetService
    {
        private readonly string _connectionString;

        public MonthlyAttendanceSheetService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public MonthlyAttendanceSheetViewModel GetMonthlyAttendanceSheet(
            int month, int year, string employee, string company)
        {
            var vm = new MonthlyAttendanceSheetViewModel
            {
                Month = month,
                Year = year,
                DaysInMonth = DateTime.DaysInMonth(year, month)
            };

            for (int d = 1; d <= vm.DaysInMonth; d++)
            {
                vm.DailySummaries[d] = new DailySummary();
            }

            var rowsByEmployee = new Dictionary<string, MonthlyAttendanceSheetRow>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT Employee, EmployeeName, Shift, AttendanceDate, Status
                    FROM Attendances
                    WHERE IsDeleted = 0
                      AND MONTH(AttendanceDate) = @Month
                      AND YEAR(AttendanceDate) = @Year";

                if (!string.IsNullOrEmpty(employee)) query += " AND Employee = @Employee";
                if (!string.IsNullOrEmpty(company)) query += " AND Company = @Company";

                query += " ORDER BY Employee, AttendanceDate";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Month", month);
                    cmd.Parameters.AddWithValue("@Year", year);
                    if (!string.IsNullOrEmpty(employee)) cmd.Parameters.AddWithValue("@Employee", employee);
                    if (!string.IsNullOrEmpty(company)) cmd.Parameters.AddWithValue("@Company", company);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string emp = reader["Employee"].ToString();

                            if (!rowsByEmployee.ContainsKey(emp))
                            {
                                rowsByEmployee[emp] = new MonthlyAttendanceSheetRow
                                {
                                    Employee = emp,
                                    EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
                                    Shift = reader["Shift"]?.ToString() ?? string.Empty
                                };
                            }

                            var date = Convert.ToDateTime(reader["AttendanceDate"]);
                            var status = reader["Status"]?.ToString() ?? string.Empty;
                            rowsByEmployee[emp].DayStatus[date.Day] = status;

                            var summary = vm.DailySummaries[date.Day];
                            if (status == "P" || status == "WFH" || status == "HD/P")
                                summary.Present++;
                            else if (status == "A" || status == "HD/A")
                                summary.Absent++;
                            else if (status == "L")
                                summary.Leave++;
                        }
                    }
                }
            }

            vm.Rows = new List<MonthlyAttendanceSheetRow>(rowsByEmployee.Values);
            return vm;
        }
    }
}
