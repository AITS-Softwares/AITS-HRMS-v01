using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class AdditionalSalaryService
    {
        private readonly string _connectionString;

        public AdditionalSalaryService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<AdditionalSalary> GetAll(string idFilter = null, string employeeFilter = null, string employeeNameFilter = null, string componentFilter = null)
        {
            var list = new List<AdditionalSalary>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT Id, AdditionalSalaryCode, Employee, EmployeeName, SalaryComponent
                                  FROM AdditionalSalaries WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(idFilter)) query += " AND AdditionalSalaryCode LIKE @IdFilter";
                if (!string.IsNullOrEmpty(employeeFilter)) query += " AND Employee LIKE @EmployeeFilter";
                if (!string.IsNullOrEmpty(employeeNameFilter)) query += " AND EmployeeName LIKE @EmployeeNameFilter";
                if (!string.IsNullOrEmpty(componentFilter)) query += " AND SalaryComponent LIKE @ComponentFilter";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(idFilter)) cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");
                    if (!string.IsNullOrEmpty(employeeFilter)) cmd.Parameters.AddWithValue("@EmployeeFilter", "%" + employeeFilter + "%");
                    if (!string.IsNullOrEmpty(employeeNameFilter)) cmd.Parameters.AddWithValue("@EmployeeNameFilter", "%" + employeeNameFilter + "%");
                    if (!string.IsNullOrEmpty(componentFilter)) cmd.Parameters.AddWithValue("@ComponentFilter", "%" + componentFilter + "%");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new AdditionalSalary
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                AdditionalSalaryCode = reader["AdditionalSalaryCode"]?.ToString() ?? string.Empty,
                                Employee = reader["Employee"]?.ToString() ?? string.Empty,
                                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
                                SalaryComponent = reader["SalaryComponent"]?.ToString() ?? string.Empty
                            });
                        }
                    }
                }
            }

            return list;
        }

        public AdditionalSalary GetById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM AdditionalSalaries WHERE Id = @Id AND IsDeleted = 0";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new AdditionalSalary
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                AdditionalSalaryCode = reader["AdditionalSalaryCode"]?.ToString() ?? string.Empty,
                                Company = reader["Company"]?.ToString() ?? string.Empty,
                                Employee = reader["Employee"]?.ToString() ?? string.Empty,
                                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
                                IsRecurring = Convert.ToBoolean(reader["IsRecurring"]),
                                PayrollDate = reader["PayrollDate"] as DateTime?,
                                SalaryComponent = reader["SalaryComponent"]?.ToString() ?? string.Empty,
                                Amount = reader["Amount"] != DBNull.Value ? Convert.ToDecimal(reader["Amount"]) : 0,
                                DeductFullTaxOnSelectedPayrollDate = Convert.ToBoolean(reader["DeductFullTaxOnSelectedPayrollDate"]),
                                OverwriteSalaryStructureAmount = Convert.ToBoolean(reader["OverwriteSalaryStructureAmount"])
                            };
                        }
                    }
                }
            }

            return null;
        }

        public string GetNextCode()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT COUNT(*) FROM AdditionalSalaries";
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    int count = (int)cmd.ExecuteScalar() + 1;
                    return $"HR-ADS-{DateTime.Now:yy}-{DateTime.Now:MM}-{count:D5}";
                }
            }
        }

        public int Insert(AdditionalSalary model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    INSERT INTO AdditionalSalaries
                        (AdditionalSalaryCode, Company, Employee, EmployeeName, IsRecurring, PayrollDate,
                         SalaryComponent, Amount, DeductFullTaxOnSelectedPayrollDate, OverwriteSalaryStructureAmount)
                    OUTPUT INSERTED.Id
                    VALUES
                        (@AdditionalSalaryCode, @Company, @Employee, @EmployeeName, @IsRecurring, @PayrollDate,
                         @SalaryComponent, @Amount, @DeductFullTaxOnSelectedPayrollDate, @OverwriteSalaryStructureAmount)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@AdditionalSalaryCode", model.AdditionalSalaryCode ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Company", model.Company ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Employee", model.Employee ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmployeeName", model.EmployeeName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsRecurring", model.IsRecurring);
                    cmd.Parameters.AddWithValue("@PayrollDate", (object)model.PayrollDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SalaryComponent", model.SalaryComponent ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Amount", model.Amount);
                    cmd.Parameters.AddWithValue("@DeductFullTaxOnSelectedPayrollDate", model.DeductFullTaxOnSelectedPayrollDate);
                    cmd.Parameters.AddWithValue("@OverwriteSalaryStructureAmount", model.OverwriteSalaryStructureAmount);

                    conn.Open();
                    return (int)cmd.ExecuteScalar();
                }
            }
        }
    }
}
