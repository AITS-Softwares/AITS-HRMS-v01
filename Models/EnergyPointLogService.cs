using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class EnergyPointLogService
    {
        private readonly string _connectionString;

        public EnergyPointLogService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<EnergyPointLog> GetAll(string id = null, string userName = null, string type = null, string rule = null, string referenceDoc = null)
        {
            var logs = new List<EnergyPointLog>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT Id, LogCode, UserName, Type, [Rule], ReferenceDoctype, ReferenceName, Points, CreatedOn
                                  FROM EnergyPointLogs WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(id)) query += " AND LogCode LIKE @Id";
                if (!string.IsNullOrEmpty(userName)) query += " AND UserName LIKE @UserName";
                if (!string.IsNullOrEmpty(type)) query += " AND Type = @Type";
                if (!string.IsNullOrEmpty(rule)) query += " AND [Rule] LIKE @Rule";
                if (!string.IsNullOrEmpty(referenceDoc)) query += " AND ReferenceDoctype LIKE @ReferenceDoc";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(id)) cmd.Parameters.AddWithValue("@Id", "%" + id + "%");
                    if (!string.IsNullOrEmpty(userName)) cmd.Parameters.AddWithValue("@UserName", "%" + userName + "%");
                    if (!string.IsNullOrEmpty(type)) cmd.Parameters.AddWithValue("@Type", type);
                    if (!string.IsNullOrEmpty(rule)) cmd.Parameters.AddWithValue("@Rule", "%" + rule + "%");
                    if (!string.IsNullOrEmpty(referenceDoc)) cmd.Parameters.AddWithValue("@ReferenceDoc", "%" + referenceDoc + "%");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            logs.Add(new EnergyPointLog
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                LogCode = reader["LogCode"]?.ToString() ?? string.Empty,
                                UserName = reader["UserName"]?.ToString() ?? string.Empty,
                                Type = reader["Type"]?.ToString() ?? string.Empty,
                                Rule = reader["Rule"]?.ToString() ?? string.Empty,
                                ReferenceDoctype = reader["ReferenceDoctype"]?.ToString() ?? string.Empty,
                                ReferenceName = reader["ReferenceName"]?.ToString() ?? string.Empty,
                                Points = reader["Points"] != DBNull.Value ? Convert.ToInt32(reader["Points"]) : 0,
                                CreatedOn = reader["CreatedOn"] as DateTime?
                            });
                        }
                    }
                }
            }

            return logs;
        }
    }
}
