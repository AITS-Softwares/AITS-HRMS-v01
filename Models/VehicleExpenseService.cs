using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class VehicleExpenseService
    {
        private readonly string _connectionString;

        public VehicleExpenseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<VehicleExpenseRow> GetVehicleExpenses(
            string fiscalYearDropdown, DateTime? fromDate, DateTime? toDate,
            string fiscalYearText, string vehicle, string employee)
        {
            var rows = new List<VehicleExpenseRow>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT Vehicle, Employee, EmployeeName, LogDate, FiscalYear,
                           FuelExpense, ServiceExpense, OtherExpense, TotalExpense
                    FROM VehicleExpenseLogs
                    WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(fiscalYearDropdown)) query += " AND FiscalYear = @FiscalYearDropdown";
                if (fromDate.HasValue) query += " AND LogDate >= @FromDate";
                if (toDate.HasValue) query += " AND LogDate <= @ToDate";
                if (!string.IsNullOrEmpty(fiscalYearText)) query += " AND FiscalYear LIKE @FiscalYearText";
                if (!string.IsNullOrEmpty(vehicle)) query += " AND Vehicle LIKE @Vehicle";
                if (!string.IsNullOrEmpty(employee)) query += " AND (Employee LIKE @Employee OR EmployeeName LIKE @Employee)";

                query += " ORDER BY LogDate DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(fiscalYearDropdown)) cmd.Parameters.AddWithValue("@FiscalYearDropdown", fiscalYearDropdown);
                    if (fromDate.HasValue) cmd.Parameters.AddWithValue("@FromDate", fromDate.Value);
                    if (toDate.HasValue) cmd.Parameters.AddWithValue("@ToDate", toDate.Value);
                    if (!string.IsNullOrEmpty(fiscalYearText)) cmd.Parameters.AddWithValue("@FiscalYearText", "%" + fiscalYearText + "%");
                    if (!string.IsNullOrEmpty(vehicle)) cmd.Parameters.AddWithValue("@Vehicle", "%" + vehicle + "%");
                    if (!string.IsNullOrEmpty(employee)) cmd.Parameters.AddWithValue("@Employee", "%" + employee + "%");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            rows.Add(new VehicleExpenseRow
                            {
                                Vehicle = reader["Vehicle"]?.ToString() ?? string.Empty,
                                Employee = reader["Employee"]?.ToString() ?? string.Empty,
                                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
                                LogDate = reader["LogDate"] as DateTime?,
                                FiscalYear = reader["FiscalYear"]?.ToString() ?? string.Empty,
                                FuelExpense = reader["FuelExpense"] != DBNull.Value ? Convert.ToDecimal(reader["FuelExpense"]) : 0,
                                ServiceExpense = reader["ServiceExpense"] != DBNull.Value ? Convert.ToDecimal(reader["ServiceExpense"]) : 0,
                                OtherExpense = reader["OtherExpense"] != DBNull.Value ? Convert.ToDecimal(reader["OtherExpense"]) : 0,
                                TotalExpense = reader["TotalExpense"] != DBNull.Value ? Convert.ToDecimal(reader["TotalExpense"]) : 0
                            });
                        }
                    }
                }
            }

            return rows;
        }
    }
}
