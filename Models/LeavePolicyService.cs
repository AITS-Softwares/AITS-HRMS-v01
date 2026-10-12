//using System;
//using System.Collections.Generic;
//using Microsoft.Data.SqlClient;
//using Erp_Aits.Models;

//namespace Erp_Aits.Services
//{
//    public class LeavePolicyService
//    {
//        private readonly string _connectionString;

//        public LeavePolicyService(string connectionString)
//        {
//            _connectionString = connectionString;
//        }

//        public List<LeavePolicy> GetAll(string id = null)
//        {
//            var list = new List<LeavePolicy>();
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = @"SELECT Id, Title FROM LeavePolicies WHERE IsDeleted = 0";

//                if (!string.IsNullOrEmpty(id))
//                    query += " AND Title LIKE @Id";

//                query += " ORDER BY Id DESC";

//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    if (!string.IsNullOrEmpty(id))
//                        cmd.Parameters.AddWithValue("@Id", "%" + id + "%");

//                    conn.Open();
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            list.Add(new LeavePolicy
//                            {
//                                Id = Convert.ToInt32(reader["Id"]),
//                                Title = reader["Title"]?.ToString()
//                            });
//                        }
//                    }
//                }
//            }
//            return list;
//        }

//        public LeavePolicy GetById(int id)
//        {
//            LeavePolicy model = null;
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                conn.Open();

//                string query = "SELECT Id, Title FROM LeavePolicies WHERE Id = @Id AND IsDeleted = 0";
//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    cmd.Parameters.AddWithValue("@Id", id);
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        if (reader.Read())
//                        {
//                            model = new LeavePolicy
//                            {
//                                Id = Convert.ToInt32(reader["Id"]),
//                                Title = reader["Title"]?.ToString()
//                            };
//                        }
//                    }
//                }

//                if (model == null) return null;

//                string detailQuery = @"SELECT Id, LeavePolicyId, LeaveType, AnnualAllocation 
//                                        FROM LeavePolicyDetails WHERE LeavePolicyId = @Id ORDER BY RowNo";
//                using (var cmd = new SqlCommand(detailQuery, conn))
//                {
//                    cmd.Parameters.AddWithValue("@Id", id);
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            model.Details.Add(new LeavePolicyDetail
//                            {
//                                Id = Convert.ToInt32(reader["Id"]),
//                                LeavePolicyId = Convert.ToInt32(reader["LeavePolicyId"]),
//                                LeaveType = reader["LeaveType"]?.ToString(),
//                                AnnualAllocation = Convert.ToDecimal(reader["AnnualAllocation"])
//                            });
//                        }
//                    }
//                }
//            }
//            return model;
//        }

//        public int Insert(LeavePolicy model)
//        {
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                conn.Open();
//                using (var transaction = conn.BeginTransaction())
//                {
//                    try
//                    {
//                        int newId;
//                        string query = @"INSERT INTO LeavePolicies (Title)
//                                          OUTPUT INSERTED.Id
//                                          VALUES (@Title)";

//                        using (var cmd = new SqlCommand(query, conn, transaction))
//                        {
//                            cmd.Parameters.AddWithValue("@Title", model.Title ?? string.Empty);
//                            newId = (int)cmd.ExecuteScalar();
//                        }

//                        int rowNo = 1;
//                        foreach (var d in model.Details)
//                        {
//                            string detailQuery = @"INSERT INTO LeavePolicyDetails (LeavePolicyId, LeaveType, AnnualAllocation, RowNo)
//                                                    VALUES (@LeavePolicyId, @LeaveType, @AnnualAllocation, @RowNo)";
//                            using (var cmd = new SqlCommand(detailQuery, conn, transaction))
//                            {
//                                cmd.Parameters.AddWithValue("@LeavePolicyId", newId);
//                                cmd.Parameters.AddWithValue("@LeaveType", d.LeaveType ?? string.Empty);
//                                cmd.Parameters.AddWithValue("@AnnualAllocation", d.AnnualAllocation);
//                                cmd.Parameters.AddWithValue("@RowNo", rowNo++);
//                                cmd.ExecuteNonQuery();
//                            }
//                        }

//                        transaction.Commit();
//                        return newId;
//                    }
//                    catch
//                    {
//                        transaction.Rollback();
//                        throw;
//                    }
//                }
//            }
//        }
//    }
//}

using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class LeavePolicyService
    {
        private readonly string _connectionString;

        public LeavePolicyService(string connectionString)
        {
            _connectionString = connectionString;
            EnsureColumns();
        }

        public List<LeavePolicy> GetAll(string id = null)
        {
            var list = new List<LeavePolicy>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT Id, Title, ErpNextId, DocStatus, Status FROM LeavePolicies WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(id))
                    query += " AND Title LIKE @Id";

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
                            list.Add(MapPolicy(reader));
                        }
                    }
                }
            }
            return list;
        }

        public LeavePolicy GetById(int id)
        {
            LeavePolicy model = null;
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string query = "SELECT Id, Title, ErpNextId, DocStatus, Status FROM LeavePolicies WHERE Id = @Id AND IsDeleted = 0";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model = MapPolicy(reader);
                        }
                    }
                }

                if (model == null) return null;

                string detailQuery = @"SELECT Id, LeavePolicyId, LeaveType, AnnualAllocation 
                                        FROM LeavePolicyDetails WHERE LeavePolicyId = @Id ORDER BY RowNo";
                using (var cmd = new SqlCommand(detailQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.Details.Add(new LeavePolicyDetail
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                LeavePolicyId = Convert.ToInt32(reader["LeavePolicyId"]),
                                LeaveType = reader["LeaveType"]?.ToString(),
                                AnnualAllocation = Convert.ToDecimal(reader["AnnualAllocation"])
                            });
                        }
                    }
                }
            }
            return model;
        }

        public LeavePolicy GetByErpNextId(string erpNextId)
        {
            if (string.IsNullOrWhiteSpace(erpNextId)) return null;

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT Id FROM LeavePolicies WHERE ErpNextId = @ErpNextId AND IsDeleted = 0";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", erpNextId);
                    conn.Open();
                    var value = cmd.ExecuteScalar();
                    if (value == null || value == DBNull.Value) return null;
                    return GetById(Convert.ToInt32(value));
                }
            }
        }

        public int Insert(LeavePolicy model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int newId;
                        string query = @"INSERT INTO LeavePolicies (Title, ErpNextId, DocStatus, Status)
                                          OUTPUT INSERTED.Id
                                          VALUES (@Title, @ErpNextId, @DocStatus, @Status)";

                        using (var cmd = new SqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Title", model.Title ?? string.Empty);
                            cmd.Parameters.AddWithValue("@ErpNextId", (object)model.ErpNextId ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@DocStatus", model.DocStatus);
                            cmd.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(model.Status) ? StatusFromDocStatus(model.DocStatus) : model.Status);
                            newId = (int)cmd.ExecuteScalar();
                        }

                        int rowNo = 1;
                        foreach (var d in model.Details)
                        {
                            string detailQuery = @"INSERT INTO LeavePolicyDetails (LeavePolicyId, LeaveType, AnnualAllocation, RowNo)
                                                    VALUES (@LeavePolicyId, @LeaveType, @AnnualAllocation, @RowNo)";
                            using (var cmd = new SqlCommand(detailQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@LeavePolicyId", newId);
                                cmd.Parameters.AddWithValue("@LeaveType", d.LeaveType ?? string.Empty);
                                cmd.Parameters.AddWithValue("@AnnualAllocation", d.AnnualAllocation);
                                cmd.Parameters.AddWithValue("@RowNo", rowNo++);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return newId;
                    }
                    catch
                    {
                        try
                        {
                            transaction.Rollback();
                        }
                        catch
                        {
                        }
                        throw;
                    }
                }
            }
        }

        public void UpdateErpNextId(int id, string erpNextId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE LeavePolicies SET ErpNextId = @ErpNextId WHERE Id = @Id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", (object)erpNextId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Update(LeavePolicy model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new SqlCommand(
                            @"UPDATE LeavePolicies
                              SET Title = @Title,
                                  ErpNextId = @ErpNextId,
                                  DocStatus = @DocStatus,
                                  Status = @Status,
                                  IsDeleted = 0
                              WHERE Id = @Id",
                            conn,
                            transaction))
                        {
                            cmd.Parameters.AddWithValue("@Title", model.Title ?? string.Empty);
                            cmd.Parameters.AddWithValue("@ErpNextId", (object)model.ErpNextId ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@DocStatus", model.DocStatus);
                            cmd.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(model.Status) ? StatusFromDocStatus(model.DocStatus) : model.Status);
                            cmd.Parameters.AddWithValue("@Id", model.Id);
                            cmd.ExecuteNonQuery();
                        }

                        using (var cmd = new SqlCommand("DELETE FROM LeavePolicyDetails WHERE LeavePolicyId = @Id", conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Id", model.Id);
                            cmd.ExecuteNonQuery();
                        }

                        int rowNo = 1;
                        foreach (var d in model.Details ?? new List<LeavePolicyDetail>())
                        {
                            if (string.IsNullOrWhiteSpace(d.LeaveType)) continue;

                            using (var cmd = new SqlCommand(
                                @"INSERT INTO LeavePolicyDetails (LeavePolicyId, LeaveType, AnnualAllocation, RowNo)
                                  VALUES (@LeavePolicyId, @LeaveType, @AnnualAllocation, @RowNo)",
                                conn,
                                transaction))
                            {
                                cmd.Parameters.AddWithValue("@LeavePolicyId", model.Id);
                                cmd.Parameters.AddWithValue("@LeaveType", d.LeaveType ?? string.Empty);
                                cmd.Parameters.AddWithValue("@AnnualAllocation", d.AnnualAllocation);
                                cmd.Parameters.AddWithValue("@RowNo", rowNo++);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        try
                        {
                            transaction.Rollback();
                        }
                        catch
                        {
                        }
                        throw;
                    }
                }
            }
        }

        public void MarkDeletedById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE LeavePolicies SET IsDeleted = 1 WHERE Id = @Id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public bool ExistsByErpNextId(string erpNextId)
        {
            if (string.IsNullOrWhiteSpace(erpNextId)) return false;

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT COUNT(*) FROM LeavePolicies WHERE ErpNextId = @ErpNextId AND IsDeleted = 0";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", erpNextId);
                    conn.Open();
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }

        public void UpdateFromErpNext(LeavePolicy model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Get existing ID from ErpNextId
                        int policyId = 0;
                        using (var cmd = new SqlCommand("SELECT Id FROM LeavePolicies WHERE ErpNextId = @ErpNextId AND IsDeleted = 0", conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ErpNextId", model.ErpNextId);
                            var obj = cmd.ExecuteScalar();
                            if (obj != null) policyId = Convert.ToInt32(obj);
                        }

                        if (policyId > 0)
                        {
                            // 2. Update parent Title
                            using (var cmd = new SqlCommand("UPDATE LeavePolicies SET Title = @Title, DocStatus = @DocStatus, Status = @Status, IsDeleted = 0 WHERE Id = @Id", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Title", model.Title ?? string.Empty);
                                cmd.Parameters.AddWithValue("@DocStatus", model.DocStatus);
                                cmd.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(model.Status) ? StatusFromDocStatus(model.DocStatus) : model.Status);
                                cmd.Parameters.AddWithValue("@Id", policyId);
                                cmd.ExecuteNonQuery();
                            }

                            // 3. Refresh details
                            using (var cmd = new SqlCommand("DELETE FROM LeavePolicyDetails WHERE LeavePolicyId = @Id", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Id", policyId);
                                cmd.ExecuteNonQuery();
                            }

                            int rowNo = 1;
                            foreach (var d in model.Details)
                            {
                                string detailQuery = @"INSERT INTO LeavePolicyDetails (LeavePolicyId, LeaveType, AnnualAllocation, RowNo)
                                                        VALUES (@LeavePolicyId, @LeaveType, @AnnualAllocation, @RowNo)";
                                using (var cmd = new SqlCommand(detailQuery, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@LeavePolicyId", policyId);
                                    cmd.Parameters.AddWithValue("@LeaveType", d.LeaveType ?? string.Empty);
                                    cmd.Parameters.AddWithValue("@AnnualAllocation", d.AnnualAllocation);
                                    cmd.Parameters.AddWithValue("@RowNo", rowNo++);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        try
                        {
                            transaction.Rollback();
                        }
                        catch
                        {
                        }
                        throw;
                    }
                }
            }
        }

        private LeavePolicy MapPolicy(SqlDataReader reader)
        {
            bool hasErpNextId = false;
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals("ErpNextId", StringComparison.OrdinalIgnoreCase))
                {
                    hasErpNextId = true;
                    break;
                }
            }

            return new LeavePolicy
            {
                Id = Convert.ToInt32(reader["Id"]),
                Title = reader["Title"]?.ToString(),
                ErpNextId = hasErpNextId && reader["ErpNextId"] != DBNull.Value ? reader["ErpNextId"].ToString() : null,
                DocStatus = HasColumn(reader, "DocStatus") && reader["DocStatus"] != DBNull.Value ? Convert.ToInt32(reader["DocStatus"]) : 0,
                Status = HasColumn(reader, "Status") && reader["Status"] != DBNull.Value ? reader["Status"].ToString() : "Draft"
            };
        }

        private static bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string StatusFromDocStatus(int docStatus)
        {
            return docStatus == 1
                ? "Submitted"
                : docStatus == 2
                    ? "Cancelled"
                    : "Draft";
        }

        private void EnsureColumns()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string query = @"
                    IF COL_LENGTH('LeavePolicies', 'DocStatus') IS NULL
                    BEGIN
                        ALTER TABLE LeavePolicies ADD DocStatus INT NOT NULL CONSTRAINT DF_LeavePolicies_DocStatus DEFAULT 0
                    END

                    IF COL_LENGTH('LeavePolicies', 'Status') IS NULL
                    BEGIN
                        ALTER TABLE LeavePolicies ADD Status NVARCHAR(50) NULL
                    END";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
