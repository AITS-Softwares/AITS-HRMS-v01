using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class EmployeeAdvanceService
    {
        private readonly string _connectionString;

        public EmployeeAdvanceService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<EmployeeAdvance> GetAllEmployeeAdvances()
        {
            var advances = new List<EmployeeAdvance>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT Id, EmployeeAdvanceCode, Employee, EmployeeName
                                  FROM EmployeeAdvances WHERE IsDeleted = 0 ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            advances.Add(new EmployeeAdvance
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                EmployeeAdvanceCode = reader["EmployeeAdvanceCode"]?.ToString() ?? string.Empty,
                                Employee = reader["Employee"]?.ToString() ?? string.Empty,
                                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty
                            });
                        }
                    }
                }
            }

            return advances;
        }

        public List<EmployeeAdvanceSummaryRow> GetEmployeeAdvanceSummary(
    DateTime fromDate,
    DateTime toDate,
    string employee = null,
    string company = null,
    string status = null)
        {
            var rows = new List<EmployeeAdvanceSummaryRow>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
            SELECT
                Employee,
                MAX(EmployeeName) AS EmployeeName,
                MAX(Company) AS Company,
                Status,
                COUNT(*) AS NoOfAdvances,
                SUM(AdvanceAmount) AS TotalAdvanceAmount
            FROM EmployeeAdvances
            WHERE IsDeleted = 0
              AND PostingDate BETWEEN @FromDate AND @ToDate";

                if (!string.IsNullOrEmpty(employee))
                    query += " AND Employee = @Employee";

                if (!string.IsNullOrEmpty(company))
                    query += " AND Company = @Company";

                if (!string.IsNullOrEmpty(status))
                    query += " AND Status = @Status";

                query += " GROUP BY Employee, Status ORDER BY MAX(EmployeeName)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);

                    if (!string.IsNullOrEmpty(employee))
                        cmd.Parameters.AddWithValue("@Employee", employee);

                    if (!string.IsNullOrEmpty(company))
                        cmd.Parameters.AddWithValue("@Company", company);

                    if (!string.IsNullOrEmpty(status))
                        cmd.Parameters.AddWithValue("@Status", status);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            rows.Add(new EmployeeAdvanceSummaryRow
                            {
                                Employee = reader["Employee"]?.ToString() ?? string.Empty,
                                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
                                Company = reader["Company"]?.ToString() ?? string.Empty,
                                Status = reader["Status"]?.ToString() ?? string.Empty,
                                NoOfAdvances = Convert.ToInt32(reader["NoOfAdvances"]),
                                TotalAdvanceAmount = reader["TotalAdvanceAmount"] as decimal? ?? 0
                            });
                        }
                    }
                }
            }

            return rows;
        }

        public EmployeeAdvance GetById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT * FROM EmployeeAdvances WHERE Id = @Id AND IsDeleted = 0";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new EmployeeAdvance
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                EmployeeAdvanceCode = reader["EmployeeAdvanceCode"]?.ToString() ?? string.Empty,
                                PostingDate = reader["PostingDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["PostingDate"]),
                                Employee = reader["Employee"]?.ToString() ?? string.Empty,
                                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
                                Company = reader["Company"]?.ToString() ?? string.Empty,
                                Purpose = reader["Purpose"]?.ToString() ?? string.Empty,
                                AdvanceAmount = reader["AdvanceAmount"] != DBNull.Value ? Convert.ToDecimal(reader["AdvanceAmount"]) : 0,
                                AdvanceAccount = reader["AdvanceAccount"]?.ToString() ?? string.Empty,
                                RepayUnclaimedFromSalary = Convert.ToBoolean(reader["RepayUnclaimedFromSalary"]),
                                ModeOfPayment = reader["ModeOfPayment"]?.ToString() ?? string.Empty,
                                Status = reader["Status"]?.ToString() ?? "Draft"
                            };
                        }
                    }
                }
            }

            return null;
        }

        public string GetNextEmployeeAdvanceCode()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT COUNT(*) FROM EmployeeAdvances";
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    int count = (int)cmd.ExecuteScalar() + 1;
                    return $"HR-EAD-{DateTime.Now.Year}-{count:D5}";
                }
            }
        }

        public HashSet<string> GetExistingErpNextCodes()
        {
            var existingCodes = new HashSet<string>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT EmployeeAdvanceCode FROM EmployeeAdvances WHERE IsDeleted = 0";
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string code = reader["EmployeeAdvanceCode"]?.ToString();
                            if (!string.IsNullOrEmpty(code))
                            {
                                existingCodes.Add(code);
                            }
                        }
                    }
                }
            }
            return existingCodes;
        }

        public void InsertEmployeeAdvanceWithCode(EmployeeAdvance advance)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
            INSERT INTO EmployeeAdvances
                (EmployeeAdvanceCode, PostingDate, Employee, EmployeeName, Company,
                 Purpose, AdvanceAmount, AdvanceAccount, RepayUnclaimedFromSalary,
                 ModeOfPayment, Status)
            VALUES
                (@EmployeeAdvanceCode, @PostingDate, @Employee, @EmployeeName, @Company,
                 @Purpose, @AdvanceAmount, @AdvanceAccount, @RepayUnclaimedFromSalary,
                 @ModeOfPayment, @Status)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@EmployeeAdvanceCode", advance.EmployeeAdvanceCode ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PostingDate", (object)advance.PostingDate ?? DateTime.Now);
                    cmd.Parameters.AddWithValue("@Employee", advance.Employee ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmployeeName", advance.EmployeeName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Company", advance.Company ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Purpose", advance.Purpose ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AdvanceAmount", advance.AdvanceAmount);
                    cmd.Parameters.AddWithValue("@AdvanceAccount", advance.AdvanceAccount ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@RepayUnclaimedFromSalary", advance.RepayUnclaimedFromSalary);
                    cmd.Parameters.AddWithValue("@ModeOfPayment", advance.ModeOfPayment ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", advance.Status ?? "Draft");

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateEmployeeAdvanceByCode(EmployeeAdvance advance)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
            UPDATE EmployeeAdvances
            SET PostingDate = @PostingDate,
                Employee = @Employee,
                EmployeeName = @EmployeeName,
                Company = @Company,
                Purpose = @Purpose,
                AdvanceAmount = @AdvanceAmount,
                AdvanceAccount = @AdvanceAccount,
                RepayUnclaimedFromSalary = @RepayUnclaimedFromSalary,
                ModeOfPayment = @ModeOfPayment,
                Status = @Status,
                IsDeleted = 0
            WHERE EmployeeAdvanceCode = @EmployeeAdvanceCode";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@EmployeeAdvanceCode", advance.EmployeeAdvanceCode);
                    cmd.Parameters.AddWithValue("@PostingDate", (object)advance.PostingDate ?? DateTime.Now);
                    cmd.Parameters.AddWithValue("@Employee", advance.Employee ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmployeeName", advance.EmployeeName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Company", advance.Company ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Purpose", advance.Purpose ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AdvanceAmount", advance.AdvanceAmount);
                    cmd.Parameters.AddWithValue("@AdvanceAccount", advance.AdvanceAccount ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@RepayUnclaimedFromSalary", advance.RepayUnclaimedFromSalary);
                    cmd.Parameters.AddWithValue("@ModeOfPayment", advance.ModeOfPayment ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", advance.Status ?? "Draft");

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public EmployeeAdvance GetByCode(string employeeAdvanceCode)
        {
            if (string.IsNullOrWhiteSpace(employeeAdvanceCode))
                return null;

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT TOP 1 Id FROM EmployeeAdvances WHERE EmployeeAdvanceCode = @EmployeeAdvanceCode AND IsDeleted = 0";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@EmployeeAdvanceCode", employeeAdvanceCode);
                    conn.Open();
                    object value = cmd.ExecuteScalar();

                    if (value == null || value == DBNull.Value)
                        return null;

                    return GetById(Convert.ToInt32(value));
                }
            }
        }

        public int InsertOrUpdate(EmployeeAdvance advance)
        {
            EmployeeAdvance existing = GetByCode(advance.EmployeeAdvanceCode);

            if (existing != null)
            {
                UpdateEmployeeAdvanceByCode(advance);
                return existing.Id;
            }

            InsertEmployeeAdvanceWithCode(advance);

            EmployeeAdvance inserted = GetByCode(advance.EmployeeAdvanceCode);
            return inserted?.Id ?? 0;
        }

        public void InsertEmployeeAdvance(EmployeeAdvance advance)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    INSERT INTO EmployeeAdvances
                        (EmployeeAdvanceCode, PostingDate, Employee, EmployeeName, Company,
                         Purpose, AdvanceAmount, AdvanceAccount, RepayUnclaimedFromSalary,
                         ModeOfPayment, Status)
                    VALUES
                        (@EmployeeAdvanceCode, @PostingDate, @Employee, @EmployeeName, @Company,
                         @Purpose, @AdvanceAmount, @AdvanceAccount, @RepayUnclaimedFromSalary,
                         @ModeOfPayment, @Status)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@EmployeeAdvanceCode", advance.EmployeeAdvanceCode ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PostingDate", (object)advance.PostingDate ?? DateTime.Now);
                    cmd.Parameters.AddWithValue("@Employee", advance.Employee ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmployeeName", advance.EmployeeName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Company", advance.Company ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Purpose", advance.Purpose ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AdvanceAmount", advance.AdvanceAmount);
                    cmd.Parameters.AddWithValue("@AdvanceAccount", advance.AdvanceAccount ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@RepayUnclaimedFromSalary", advance.RepayUnclaimedFromSalary);
                    cmd.Parameters.AddWithValue("@ModeOfPayment", advance.ModeOfPayment ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", advance.Status ?? "Draft");

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
