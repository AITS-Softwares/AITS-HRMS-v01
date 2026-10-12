using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class ExpenseClaimTypeService
    {
        private readonly string _connectionString;

        public ExpenseClaimTypeService(string connectionString)
        {
            _connectionString = connectionString;
        }


        // ============================================================
        // GET ALL
        // ============================================================

        public List<ExpenseClaimType> GetAll(string idFilter)
        {
            var list = new List<ExpenseClaimType>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT *
                    FROM ExpenseClaimTypes
                    WHERE IsDeleted = 0";

                if (!string.IsNullOrWhiteSpace(idFilter))
                {
                    query += @"
                        AND (
                            Name LIKE @Filter
                            OR ErpNextId LIKE @Filter
                        )";
                }

                query += " ORDER BY Id DESC";


                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrWhiteSpace(idFilter))
                    {
                        cmd.Parameters.AddWithValue(
                            "@Filter",
                            "%" + idFilter + "%"
                        );
                    }

                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(
                                MapExpenseClaimType(reader)
                            );
                        }
                    }
                }
            }

            return list;
        }


        // ============================================================
        // GET BY LOCAL ID
        // ============================================================

        public ExpenseClaimType GetById(int id)
        {
            ExpenseClaimType model = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand(@"
                    SELECT *
                    FROM ExpenseClaimTypes
                    WHERE Id = @Id
                      AND IsDeleted = 0",
                    conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@Id",
                        id
                    );

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model =
                                MapExpenseClaimType(reader);
                        }
                    }
                }


                if (model != null)
                {
                    LoadAccounts(
                        conn,
                        model
                    );
                }
            }

            return model;
        }


        // ============================================================
        // GET BY ERP ID
        // ============================================================

        public ExpenseClaimType GetByErpNextId(
            string erpNextId)
        {
            if (string.IsNullOrWhiteSpace(erpNextId))
                return null;


            ExpenseClaimType model = null;


            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();


                using (var cmd = new SqlCommand(@"
                    SELECT TOP 1 *
                    FROM ExpenseClaimTypes
                    WHERE ErpNextId = @ErpNextId
                      AND IsDeleted = 0",
                    conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@ErpNextId",
                        erpNextId
                    );


                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model =
                                MapExpenseClaimType(reader);
                        }
                    }
                }


                if (model != null)
                {
                    LoadAccounts(
                        conn,
                        model
                    );
                }
            }


            return model;
        }


        // ============================================================
        // EXISTING ERP IDS
        // Keep this because your old controller may still use it.
        // ============================================================

        public HashSet<string> GetExistingErpNextIds()
        {
            var ids =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase
                );


            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT ErpNextId
                    FROM ExpenseClaimTypes
                    WHERE ErpNextId IS NOT NULL
                      AND ErpNextId != ''
                      AND IsDeleted = 0";


                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();


                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ids.Add(
                                reader["ErpNextId"]
                                    .ToString()
                            );
                        }
                    }
                }
            }


            return ids;
        }


        // ============================================================
        // GET UNSYNCED
        // Keep for your existing Push-to-ERP method.
        // ============================================================

        public List<ExpenseClaimType> GetUnsynced()
        {
            var list =
                new List<ExpenseClaimType>();


            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();


                using (var cmd = new SqlCommand(@"
                    SELECT Id
                    FROM ExpenseClaimTypes
                    WHERE IsDeleted = 0
                      AND (
                          ErpNextId IS NULL
                          OR ErpNextId = ''
                      )
                    ORDER BY Id DESC",
                    conn))
                {
                    var ids =
                        new List<int>();


                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ids.Add(
                                Convert.ToInt32(
                                    reader["Id"]
                                )
                            );
                        }
                    }


                    foreach (int id in ids)
                    {
                        var item =
                            GetById(id);

                        if (item != null)
                        {
                            list.Add(item);
                        }
                    }
                }
            }


            return list;
        }


        // ============================================================
        // INSERT OR UPDATE
        //
        // Key = ErpNextId.
        // Child accounts are replaced from ERP response.
        // ============================================================

        public int InsertOrUpdate(
            ExpenseClaimType model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        int existingId = 0;


                        // ============================================
                        // Find existing by ERPNext ID
                        // ============================================

                        if (!string.IsNullOrWhiteSpace(
                            model.ErpNextId))
                        {
                            using (var cmd =
                                new SqlCommand(@"
                                    SELECT TOP 1 Id
                                    FROM ExpenseClaimTypes
                                    WHERE ErpNextId = @ErpNextId",
                                    conn,
                                    tx))
                            {
                                cmd.Parameters.AddWithValue(
                                    "@ErpNextId",
                                    model.ErpNextId
                                );


                                object value =
                                    cmd.ExecuteScalar();


                                if (
                                    value != null &&
                                    value != DBNull.Value
                                )
                                {
                                    existingId =
                                        Convert.ToInt32(value);
                                }
                            }
                        }


                        // ============================================
                        // Fallback by Name
                        //
                        // ERP Expense Claim Type name is based on
                        // expense_type.
                        // ============================================

                        if (
                            existingId == 0 &&
                            !string.IsNullOrWhiteSpace(
                                model.Name
                            )
                        )
                        {
                            using (var cmd =
                                new SqlCommand(@"
                                    SELECT TOP 1 Id
                                    FROM ExpenseClaimTypes
                                    WHERE Name = @Name
                                      AND IsDeleted = 0",
                                    conn,
                                    tx))
                            {
                                cmd.Parameters.AddWithValue(
                                    "@Name",
                                    model.Name
                                );


                                object value =
                                    cmd.ExecuteScalar();


                                if (
                                    value != null &&
                                    value != DBNull.Value
                                )
                                {
                                    existingId =
                                        Convert.ToInt32(value);
                                }
                            }
                        }


                        int localId;


                        // ============================================
                        // UPDATE
                        // ============================================

                        if (existingId > 0)
                        {
                            using (var cmd =
                                new SqlCommand(@"
                                    UPDATE ExpenseClaimTypes
                                    SET
                                        ErpNextId =
                                            @ErpNextId,

                                        Name =
                                            @Name,

                                        DeferredExpenseAccount =
                                            @DeferredExpenseAccount,

                                        Description =
                                            @Description,

                                        IsDeleted = 0

                                    WHERE Id = @Id",
                                    conn,
                                    tx))
                            {
                                AddMasterParameters(
                                    cmd,
                                    model
                                );


                                cmd.Parameters.AddWithValue(
                                    "@Id",
                                    existingId
                                );


                                cmd.ExecuteNonQuery();
                            }


                            localId =
                                existingId;


                            // Replace child table
                            using (var deleteCmd =
                                new SqlCommand(@"
                                    DELETE FROM ExpenseClaimTypeAccounts
                                    WHERE ExpenseClaimTypeId = @Id",
                                    conn,
                                    tx))
                            {
                                deleteCmd.Parameters.AddWithValue(
                                    "@Id",
                                    localId
                                );

                                deleteCmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            // ========================================
                            // INSERT
                            // ========================================

                            using (var cmd =
                                new SqlCommand(@"
                                    INSERT INTO ExpenseClaimTypes
                                    (
                                        ErpNextId,
                                        Name,
                                        DeferredExpenseAccount,
                                        Description,
                                        IsDeleted
                                    )
                                    OUTPUT INSERTED.Id
                                    VALUES
                                    (
                                        @ErpNextId,
                                        @Name,
                                        @DeferredExpenseAccount,
                                        @Description,
                                        0
                                    )",
                                    conn,
                                    tx))
                            {
                                AddMasterParameters(
                                    cmd,
                                    model
                                );


                                localId =
                                    Convert.ToInt32(
                                        cmd.ExecuteScalar()
                                    );
                            }
                        }


                        // ============================================
                        // CHILD ACCOUNTS
                        // ============================================

                        if (model.Accounts != null)
                        {
                            foreach (
                                var account
                                in model.Accounts)
                            {
                                if (
                                    string.IsNullOrWhiteSpace(
                                        account.Company
                                    ) &&
                                    string.IsNullOrWhiteSpace(
                                        account.DefaultAccount
                                    )
                                )
                                {
                                    continue;
                                }


                                using (var cmd =
                                    new SqlCommand(@"
                                        INSERT INTO ExpenseClaimTypeAccounts
                                        (
                                            ExpenseClaimTypeId,
                                            Company,
                                            DefaultAccount
                                        )
                                        VALUES
                                        (
                                            @ExpenseClaimTypeId,
                                            @Company,
                                            @DefaultAccount
                                        )",
                                        conn,
                                        tx))
                                {
                                    cmd.Parameters.AddWithValue(
                                        "@ExpenseClaimTypeId",
                                        localId
                                    );


                                    cmd.Parameters.AddWithValue(
                                        "@Company",
                                        DbValue(
                                            account.Company
                                        )
                                    );


                                    cmd.Parameters.AddWithValue(
                                        "@DefaultAccount",
                                        DbValue(
                                            account.DefaultAccount
                                        )
                                    );


                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }


                        tx.Commit();

                        return localId;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }


        // ============================================================
        // OLD CONTROLLER COMPATIBILITY
        // ============================================================

        public int Insert(
            ExpenseClaimType model)
        {
            return InsertOrUpdate(model);
        }


        public void UpdateErpNextId(
            int id,
            string erpNextId)
        {
            using (var conn =
                new SqlConnection(_connectionString))
            {
                conn.Open();


                using (var cmd =
                    new SqlCommand(@"
                        UPDATE ExpenseClaimTypes
                        SET ErpNextId = @ErpNextId
                        WHERE Id = @Id",
                        conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@ErpNextId",
                        DbValue(erpNextId)
                    );


                    cmd.Parameters.AddWithValue(
                        "@Id",
                        id
                    );


                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void SoftDeleteById(int id)
        {
            using (var conn =
                new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd =
                    new SqlCommand(@"
                        UPDATE ExpenseClaimTypes
                        SET IsDeleted = 1
                        WHERE Id = @Id",
                        conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@Id",
                        id
                    );

                    cmd.ExecuteNonQuery();
                }
            }
        }


        // ============================================================
        // CHILD LOADER
        // ============================================================

        private void LoadAccounts(
            SqlConnection conn,
            ExpenseClaimType model)
        {
            model.Accounts =
                new List<ExpenseClaimTypeAccount>();


            using (var cmd =
                new SqlCommand(@"
                    SELECT *
                    FROM ExpenseClaimTypeAccounts
                    WHERE ExpenseClaimTypeId = @Id
                    ORDER BY Id",
                    conn))
            {
                cmd.Parameters.AddWithValue(
                    "@Id",
                    model.Id
                );


                using (var reader =
                    cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        model.Accounts.Add(
                            new ExpenseClaimTypeAccount
                            {
                                Id =
                                    Convert.ToInt32(
                                        reader["Id"]
                                    ),

                                ExpenseClaimTypeId =
                                    model.Id,

                                Company =
                                    reader["Company"] ==
                                    DBNull.Value
                                        ? ""
                                        : reader["Company"]
                                            .ToString(),

                                DefaultAccount =
                                    reader["DefaultAccount"] ==
                                    DBNull.Value
                                        ? ""
                                        : reader["DefaultAccount"]
                                            .ToString()
                            }
                        );
                    }
                }
            }
        }


        private static void AddMasterParameters(
            SqlCommand cmd,
            ExpenseClaimType model)
        {
            cmd.Parameters.AddWithValue(
                "@ErpNextId",
                DbValue(
                    model.ErpNextId
                )
            );


            cmd.Parameters.AddWithValue(
                "@Name",
                DbValue(
                    model.Name
                )
            );


            cmd.Parameters.AddWithValue(
                "@DeferredExpenseAccount",
                model.DeferredExpenseAccount
            );


            cmd.Parameters.AddWithValue(
                "@Description",
                DbValue(
                    model.Description
                )
            );
        }


        private ExpenseClaimType
            MapExpenseClaimType(
                SqlDataReader reader)
        {
            return new ExpenseClaimType
            {
                Id =
                    Convert.ToInt32(
                        reader["Id"]
                    ),

                ErpNextId =
                    reader["ErpNextId"] ==
                    DBNull.Value
                        ? ""
                        : reader["ErpNextId"]
                            .ToString(),

                Name =
                    reader["Name"] ==
                    DBNull.Value
                        ? ""
                        : reader["Name"]
                            .ToString(),

                DeferredExpenseAccount =
                    reader["DeferredExpenseAccount"] !=
                    DBNull.Value &&
                    Convert.ToBoolean(
                        reader[
                            "DeferredExpenseAccount"
                        ]
                    ),

                Description =
                    reader["Description"] ==
                    DBNull.Value
                        ? ""
                        : reader["Description"]
                            .ToString(),

                Accounts =
                    new List<ExpenseClaimTypeAccount>()
            };
        }


        private static object DbValue(
            string value)
        {
            return string.IsNullOrWhiteSpace(
                value
            )
                ? DBNull.Value
                : value;
        }
    }
}
