using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class LeaveEncashmentService
    {
        private readonly string _connectionString;

        public LeaveEncashmentService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<LeaveEncashment> GetAll(string id = null, string employeeName = null)
        {
            var list = new List<LeaveEncashment>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT Id, Employee, EmployeeName, LeavePeriod, EncashmentDays, Company, LeaveType, PayViaPaymentEntry, EncashmentDate, Status
                                  FROM LeaveEncashments WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(id)) query += " AND CAST(Id AS VARCHAR) LIKE @Id";
                if (!string.IsNullOrEmpty(employeeName)) query += " AND EmployeeName LIKE @EmployeeName";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(id)) cmd.Parameters.AddWithValue("@Id", "%" + id + "%");
                    if (!string.IsNullOrEmpty(employeeName)) cmd.Parameters.AddWithValue("@EmployeeName", "%" + employeeName + "%");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(Map(reader));
                        }
                    }
                }
            }
            return list;
        }

        public LeaveEncashment GetById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT Id, Employee, EmployeeName, LeavePeriod, EncashmentDays, Company, LeaveType, PayViaPaymentEntry, EncashmentDate, Status
                                  FROM LeaveEncashments WHERE Id = @Id AND IsDeleted = 0";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            return Map(reader);
                    }
                }
            }
            return null;
        }

        public int Insert(LeaveEncashment model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"INSERT INTO LeaveEncashments
                    (Employee, EmployeeName, LeavePeriod, EncashmentDays, Company, LeaveType, PayViaPaymentEntry, EncashmentDate, Status)
                    OUTPUT INSERTED.Id
                    VALUES
                    (@Employee, @EmployeeName, @LeavePeriod, @EncashmentDays, @Company, @LeaveType, @PayViaPaymentEntry, @EncashmentDate, @Status)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Employee", model.Employee ?? string.Empty);
                    cmd.Parameters.AddWithValue("@EmployeeName", (object)model.EmployeeName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@LeavePeriod", (object)model.LeavePeriod ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EncashmentDays", (object)model.EncashmentDays ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Company", model.Company ?? "AITS");
                    cmd.Parameters.AddWithValue("@LeaveType", model.LeaveType ?? string.Empty);
                    cmd.Parameters.AddWithValue("@PayViaPaymentEntry", model.PayViaPaymentEntry);
                    cmd.Parameters.AddWithValue("@EncashmentDate", model.EncashmentDate);
                    cmd.Parameters.AddWithValue("@Status", model.Status ?? "Draft");

                    conn.Open();
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        private LeaveEncashment Map(SqlDataReader reader)
        {
            return new LeaveEncashment
            {
                Id = Convert.ToInt32(reader["Id"]),
                Employee = reader["Employee"]?.ToString(),
                EmployeeName = reader["EmployeeName"] == DBNull.Value ? null : reader["EmployeeName"].ToString(),
                LeavePeriod = reader["LeavePeriod"] == DBNull.Value ? null : reader["LeavePeriod"].ToString(),
                EncashmentDays = reader["EncashmentDays"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["EncashmentDays"]),
                Company = reader["Company"]?.ToString(),
                LeaveType = reader["LeaveType"]?.ToString(),
                PayViaPaymentEntry = Convert.ToBoolean(reader["PayViaPaymentEntry"]),
                EncashmentDate = Convert.ToDateTime(reader["EncashmentDate"]),
                Status = reader["Status"]?.ToString()
            };
        }
    }
}