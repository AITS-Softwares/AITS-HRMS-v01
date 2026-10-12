using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class BranchService
    {
        private readonly string _connectionString;

        public BranchService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Branch> GetAllBranches()
        {
            var branches = new List<Branch>();

            using (var conn = new SqlConnection(_connectionString))
            {
                const string query = @"
                    SELECT Id, BranchName, ErpName
                    FROM Branches
                    WHERE IsDeleted = 0
                    ORDER BY BranchName";

                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            branches.Add(new Branch
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                BranchName = reader["BranchName"]?.ToString() ?? "",
                                ErpName = reader["ErpName"]?.ToString() ?? ""
                            });
                        }
                    }
                }
            }

            return branches;
        }

        public Branch GetById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                const string query = @"
                    SELECT Id, BranchName, ErpName
                    FROM Branches
                    WHERE Id = @Id
                      AND IsDeleted = 0";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            return null;

                        return new Branch
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            BranchName = reader["BranchName"]?.ToString() ?? "",
                            ErpName = reader["ErpName"]?.ToString() ?? ""
                        };
                    }
                }
            }
        }

        public int GetLocalIdByErpName(string erpName)
        {
            if (string.IsNullOrWhiteSpace(erpName))
                return 0;

            using (var conn = new SqlConnection(_connectionString))
            {
                const string query = @"
                    SELECT TOP 1 Id
                    FROM Branches
                    WHERE ErpName = @ErpName
                    ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ErpName", erpName);

                    conn.Open();

                    object result = cmd.ExecuteScalar();

                    return result == null || result == DBNull.Value
                        ? 0
                        : Convert.ToInt32(result);
                }
            }
        }

        public int InsertBranch(string branchName, string erpName = null)
        {
            if (string.IsNullOrWhiteSpace(branchName))
                throw new Exception("Branch Name is required.");

            using (var conn = new SqlConnection(_connectionString))
            {
                const string query = @"
                    INSERT INTO Branches
                    (
                        BranchName,
                        ErpName,
                        IsDeleted
                    )
                    OUTPUT INSERTED.Id
                    VALUES
                    (
                        @BranchName,
                        @ErpName,
                        0
                    )";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@BranchName",
                        branchName.Trim()
                    );

                    cmd.Parameters.AddWithValue(
                        "@ErpName",
                        (object)erpName ?? DBNull.Value
                    );

                    conn.Open();

                    return Convert.ToInt32(
                        cmd.ExecuteScalar()
                    );
                }
            }
        }

        public int InsertOrUpdateFromErpNext(
            string erpName,
            string branchName)
        {
            if (string.IsNullOrWhiteSpace(erpName))
            {
                throw new Exception(
                    "ERPNext Branch document name is required."
                );
            }

            erpName = erpName.Trim();

            if (string.IsNullOrWhiteSpace(branchName))
            {
                branchName = erpName;
            }

            branchName = branchName.Trim();


            using (var conn =
                new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var tx =
                    conn.BeginTransaction(
                        System.Data.IsolationLevel.Serializable))
                {
                    try
                    {
                        int existingId = 0;


                        // ====================================================
                        // STEP 1
                        // First find Branch already linked with this ERP ID.
                        // ====================================================

                        using (var cmd = new SqlCommand(@"
                    SELECT TOP 1 Id
                    FROM Branches
                    WITH (UPDLOCK, HOLDLOCK)
                    WHERE ErpName = @ErpName
                    ORDER BY Id DESC",
                            conn,
                            tx))
                        {
                            cmd.Parameters.AddWithValue(
                                "@ErpName",
                                erpName
                            );

                            object result =
                                cmd.ExecuteScalar();

                            if (result != null &&
                                result != DBNull.Value)
                            {
                                existingId =
                                    Convert.ToInt32(result);
                            }
                        }


                        // ====================================================
                        // STEP 2
                        // Existing old HRMS Branch may have ErpName = NULL.
                        //
                        // Match it using BranchName and LINK it to ERPNext
                        // instead of inserting duplicate BranchName.
                        // ====================================================

                        if (existingId == 0)
                        {
                            using (var cmd = new SqlCommand(@"
                        SELECT TOP 1 Id
                        FROM Branches
                        WITH (UPDLOCK, HOLDLOCK)
                        WHERE BranchName = @BranchName
                        ORDER BY
                            CASE
                                WHEN IsDeleted = 0 THEN 0
                                ELSE 1
                            END,
                            Id DESC",
                                conn,
                                tx))
                            {
                                cmd.Parameters.AddWithValue(
                                    "@BranchName",
                                    branchName
                                );

                                object result =
                                    cmd.ExecuteScalar();

                                if (result != null &&
                                    result != DBNull.Value)
                                {
                                    existingId =
                                        Convert.ToInt32(result);
                                }
                            }
                        }


                        // ====================================================
                        // STEP 3
                        // UPDATE / LINK EXISTING RECORD
                        // ====================================================

                        if (existingId > 0)
                        {
                            using (var cmd = new SqlCommand(@"
                        UPDATE Branches
                        SET BranchName = @BranchName,
                            ErpName = @ErpName,
                            IsDeleted = 0
                        WHERE Id = @Id",
                                conn,
                                tx))
                            {
                                cmd.Parameters.AddWithValue(
                                    "@Id",
                                    existingId
                                );

                                cmd.Parameters.AddWithValue(
                                    "@BranchName",
                                    branchName
                                );

                                cmd.Parameters.AddWithValue(
                                    "@ErpName",
                                    erpName
                                );

                                cmd.ExecuteNonQuery();
                            }


                            tx.Commit();

                            return existingId;
                        }


                        // ====================================================
                        // STEP 4
                        // INSERT ONLY WHEN BRANCH DOES NOT EXIST AT ALL
                        // ====================================================

                        int newId;

                        using (var cmd = new SqlCommand(@"
                    INSERT INTO Branches
                    (
                        BranchName,
                        ErpName,
                        IsDeleted
                    )
                    OUTPUT INSERTED.Id
                    VALUES
                    (
                        @BranchName,
                        @ErpName,
                        0
                    )",
                            conn,
                            tx))
                        {
                            cmd.Parameters.AddWithValue(
                                "@BranchName",
                                branchName
                            );

                            cmd.Parameters.AddWithValue(
                                "@ErpName",
                                erpName
                            );

                            newId =
                                Convert.ToInt32(
                                    cmd.ExecuteScalar()
                                );
                        }


                        tx.Commit();

                        return newId;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public void UpdateLocal(
            int id,
            string branchName,
            string erpName)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                const string query = @"
                    UPDATE Branches
                    SET BranchName = @BranchName,
                        ErpName = @ErpName,
                        IsDeleted = 0
                    WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    cmd.Parameters.AddWithValue(
                        "@BranchName",
                        branchName ?? ""
                    );

                    cmd.Parameters.AddWithValue(
                        "@ErpName",
                        (object)erpName ?? DBNull.Value
                    );

                    conn.Open();

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void MarkDeletedById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                const string query = @"
                    UPDATE Branches
                    SET IsDeleted = 1
                    WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    conn.Open();

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void MarkDeletedByErpName(string erpName)
        {
            if (string.IsNullOrWhiteSpace(erpName))
                return;

            using (var conn = new SqlConnection(_connectionString))
            {
                const string query = @"
                    UPDATE Branches
                    SET IsDeleted = 1
                    WHERE ErpName = @ErpName";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@ErpName",
                        erpName
                    );

                    conn.Open();

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}