using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class LeaveBlockListService
    {
        private readonly string _connectionString;

        public LeaveBlockListService(string connectionString)
        {
            _connectionString = connectionString;
            EnsureColumns();
        }

        public List<LeaveBlockList> GetAll(string id = null)
        {
            var list = new List<LeaveBlockList>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT Id, LeaveBlockListName, LeaveType, Company, AppliesToCompany, ErpNextId
                                  FROM LeaveBlockLists WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(id))
                    query += " AND LeaveBlockListName LIKE @Id";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(id))
                        cmd.Parameters.AddWithValue("@Id", "%" + id + "%");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new LeaveBlockList
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                LeaveBlockListName = reader["LeaveBlockListName"]?.ToString(),
                                LeaveType = reader["LeaveType"] == DBNull.Value ? null : reader["LeaveType"].ToString(),
                                Company = reader["Company"]?.ToString(),
                                AppliesToCompany = Convert.ToBoolean(reader["AppliesToCompany"]),
                                ErpNextId = reader["ErpNextId"] == DBNull.Value ? null : reader["ErpNextId"].ToString()
                            });
                        }
                    }
                }
            }
            return list;
        }

        public LeaveBlockList GetById(int id)
        {
            LeaveBlockList model = null;
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string query = @"SELECT Id, LeaveBlockListName, LeaveType, Company, AppliesToCompany, ErpNextId
                                  FROM LeaveBlockLists WHERE Id = @Id AND IsDeleted = 0";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model = new LeaveBlockList
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                LeaveBlockListName = reader["LeaveBlockListName"]?.ToString(),
                                LeaveType = reader["LeaveType"] == DBNull.Value ? null : reader["LeaveType"].ToString(),
                                Company = reader["Company"]?.ToString(),
                                AppliesToCompany = Convert.ToBoolean(reader["AppliesToCompany"]),
                                ErpNextId = reader["ErpNextId"] == DBNull.Value ? null : reader["ErpNextId"].ToString()
                            };
                        }
                    }
                }

                if (model == null) return null;

                string datesQuery = @"SELECT Id, LeaveBlockListId, BlockDate, Reason 
                                       FROM LeaveBlockListDates WHERE LeaveBlockListId = @Id ORDER BY RowNo";
                using (var cmd = new SqlCommand(datesQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.BlockDates.Add(new LeaveBlockListDate
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                LeaveBlockListId = Convert.ToInt32(reader["LeaveBlockListId"]),
                                BlockDate = Convert.ToDateTime(reader["BlockDate"]),
                                Reason = reader["Reason"]?.ToString()
                            });
                        }
                    }
                }

                string usersQuery = @"SELECT Id, LeaveBlockListId, AllowUser 
                                       FROM LeaveBlockListAllowedUsers WHERE LeaveBlockListId = @Id ORDER BY RowNo";
                using (var cmd = new SqlCommand(usersQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.AllowedUsers.Add(new LeaveBlockListAllowedUser
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                LeaveBlockListId = Convert.ToInt32(reader["LeaveBlockListId"]),
                                AllowUser = reader["AllowUser"]?.ToString()
                            });
                        }
                    }
                }
            }
            return model;
        }

        public int Insert(LeaveBlockList model)
        {
            return InsertOrUpdate(model);
        }

        public LeaveBlockList GetByErpNextId(string erpNextId)
        {
            if (string.IsNullOrWhiteSpace(erpNextId)) return null;

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT Id FROM LeaveBlockLists WHERE ErpNextId = @ErpNextId AND IsDeleted = 0";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", erpNextId);
                    conn.Open();
                    var value = cmd.ExecuteScalar();
                    return value == null || value == DBNull.Value ? null : GetById(Convert.ToInt32(value));
                }
            }
        }

        public bool ExistsByErpNextId(string erpNextId)
        {
            return GetByErpNextId(erpNextId) != null;
        }

        public int InsertOrUpdate(LeaveBlockList model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int existingId = 0;
                        if (!string.IsNullOrWhiteSpace(model.ErpNextId))
                        {
                            using (var cmd = new SqlCommand("SELECT TOP 1 Id FROM LeaveBlockLists WHERE ErpNextId = @ErpNextId", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@ErpNextId", model.ErpNextId);
                                var value = cmd.ExecuteScalar();
                                if (value != null && value != DBNull.Value) existingId = Convert.ToInt32(value);
                            }
                        }

                        if (existingId == 0 && !string.IsNullOrWhiteSpace(model.LeaveBlockListName))
                        {
                            using (var cmd = new SqlCommand("SELECT TOP 1 Id FROM LeaveBlockLists WHERE LeaveBlockListName = @LeaveBlockListName", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@LeaveBlockListName", model.LeaveBlockListName);
                                var value = cmd.ExecuteScalar();
                                if (value != null && value != DBNull.Value) existingId = Convert.ToInt32(value);
                            }
                        }

                        if (existingId > 0)
                        {
                            model.Id = existingId;
                            UpdateInsideTransaction(conn, transaction, model);
                            transaction.Commit();
                            return existingId;
                        }

                        int newId;
                        string query = @"INSERT INTO LeaveBlockLists (LeaveBlockListName, LeaveType, Company, AppliesToCompany, ErpNextId, IsDeleted)
                                          OUTPUT INSERTED.Id
                                          VALUES (@LeaveBlockListName, @LeaveType, @Company, @AppliesToCompany, @ErpNextId, 0)";

                        using (var cmd = new SqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@LeaveBlockListName", model.LeaveBlockListName ?? string.Empty);
                            cmd.Parameters.AddWithValue("@LeaveType", (object)model.LeaveType ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Company", model.Company ?? "AITS");
                            cmd.Parameters.AddWithValue("@AppliesToCompany", model.AppliesToCompany);
                            cmd.Parameters.AddWithValue("@ErpNextId", (object)model.ErpNextId ?? DBNull.Value);

                            newId = (int)cmd.ExecuteScalar();
                        }

                        int rowNo = 1;
                        foreach (var d in model.BlockDates)
                        {
                            string dq = @"INSERT INTO LeaveBlockListDates (LeaveBlockListId, BlockDate, Reason, RowNo)
                                          VALUES (@LeaveBlockListId, @BlockDate, @Reason, @RowNo)";
                            using (var cmd = new SqlCommand(dq, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@LeaveBlockListId", newId);
                                cmd.Parameters.AddWithValue("@BlockDate", d.BlockDate);
                                cmd.Parameters.AddWithValue("@Reason", d.Reason ?? string.Empty);
                                cmd.Parameters.AddWithValue("@RowNo", rowNo++);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        rowNo = 1;
                        foreach (var u in model.AllowedUsers)
                        {
                            string uq = @"INSERT INTO LeaveBlockListAllowedUsers (LeaveBlockListId, AllowUser, RowNo)
                                          VALUES (@LeaveBlockListId, @AllowUser, @RowNo)";
                            using (var cmd = new SqlCommand(uq, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@LeaveBlockListId", newId);
                                cmd.Parameters.AddWithValue("@AllowUser", u.AllowUser ?? string.Empty);
                                cmd.Parameters.AddWithValue("@RowNo", rowNo++);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return newId;
                    }
                    catch
                    {
                        try { transaction.Rollback(); } catch { }
                        throw;
                    }
                }
            }
        }

        public void Update(LeaveBlockList model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        UpdateInsideTransaction(conn, transaction, model);
                        transaction.Commit();
                    }
                    catch
                    {
                        try { transaction.Rollback(); } catch { }
                        throw;
                    }
                }
            }
        }

        public void MarkDeletedById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE LeaveBlockLists SET IsDeleted = 1 WHERE Id = @Id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void UpdateInsideTransaction(SqlConnection conn, SqlTransaction transaction, LeaveBlockList model)
        {
            using (var cmd = new SqlCommand(@"
                UPDATE LeaveBlockLists
                SET LeaveBlockListName = @LeaveBlockListName,
                    LeaveType = @LeaveType,
                    Company = @Company,
                    AppliesToCompany = @AppliesToCompany,
                    ErpNextId = @ErpNextId,
                    IsDeleted = 0
                WHERE Id = @Id", conn, transaction))
            {
                cmd.Parameters.AddWithValue("@LeaveBlockListName", model.LeaveBlockListName ?? string.Empty);
                cmd.Parameters.AddWithValue("@LeaveType", (object)model.LeaveType ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Company", model.Company ?? "AITS");
                cmd.Parameters.AddWithValue("@AppliesToCompany", model.AppliesToCompany);
                cmd.Parameters.AddWithValue("@ErpNextId", (object)model.ErpNextId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Id", model.Id);
                cmd.ExecuteNonQuery();
            }

            using (var cmd = new SqlCommand("DELETE FROM LeaveBlockListDates WHERE LeaveBlockListId = @Id", conn, transaction))
            {
                cmd.Parameters.AddWithValue("@Id", model.Id);
                cmd.ExecuteNonQuery();
            }

            using (var cmd = new SqlCommand("DELETE FROM LeaveBlockListAllowedUsers WHERE LeaveBlockListId = @Id", conn, transaction))
            {
                cmd.Parameters.AddWithValue("@Id", model.Id);
                cmd.ExecuteNonQuery();
            }

            int rowNo = 1;
            foreach (var d in model.BlockDates ?? new List<LeaveBlockListDate>())
            {
                if (d.BlockDate == default) continue;

                using (var cmd = new SqlCommand(@"INSERT INTO LeaveBlockListDates (LeaveBlockListId, BlockDate, Reason, RowNo)
                                                  VALUES (@LeaveBlockListId, @BlockDate, @Reason, @RowNo)", conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@LeaveBlockListId", model.Id);
                    cmd.Parameters.AddWithValue("@BlockDate", d.BlockDate);
                    cmd.Parameters.AddWithValue("@Reason", d.Reason ?? string.Empty);
                    cmd.Parameters.AddWithValue("@RowNo", rowNo++);
                    cmd.ExecuteNonQuery();
                }
            }

            rowNo = 1;
            foreach (var u in model.AllowedUsers ?? new List<LeaveBlockListAllowedUser>())
            {
                if (string.IsNullOrWhiteSpace(u.AllowUser)) continue;

                using (var cmd = new SqlCommand(@"INSERT INTO LeaveBlockListAllowedUsers (LeaveBlockListId, AllowUser, RowNo)
                                                  VALUES (@LeaveBlockListId, @AllowUser, @RowNo)", conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@LeaveBlockListId", model.Id);
                    cmd.Parameters.AddWithValue("@AllowUser", u.AllowUser);
                    cmd.Parameters.AddWithValue("@RowNo", rowNo++);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void EnsureColumns()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = @"
                    IF COL_LENGTH('LeaveBlockLists', 'ErpNextId') IS NULL
                    BEGIN
                        ALTER TABLE LeaveBlockLists ADD ErpNextId NVARCHAR(255) NULL
                    END";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}








