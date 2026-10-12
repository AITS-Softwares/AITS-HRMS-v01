using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class EmployeeGroupService
    {
        private readonly string _connectionString;

        public EmployeeGroupService(string connectionString)
        {
            _connectionString = connectionString;
        }


        // ============================================================
        // GET ALL
        // ============================================================

        public List<EmployeeGroup> GetAll()
        {
            var list = new List<EmployeeGroup>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT
                        Id,
                        ErpNextId,
                        GroupCode,
                        GroupName
                    FROM EmployeeGroups
                    WHERE IsDeleted = 0
                    ORDER BY GroupName";

                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new EmployeeGroup
                            {
                                Id = Convert.ToInt32(reader["Id"]),

                                ErpNextId =
                                    reader["ErpNextId"] == DBNull.Value
                                        ? ""
                                        : reader["ErpNextId"].ToString(),

                                GroupCode =
                                    reader["GroupCode"] == DBNull.Value
                                        ? ""
                                        : reader["GroupCode"].ToString(),

                                GroupName =
                                    reader["GroupName"] == DBNull.Value
                                        ? ""
                                        : reader["GroupName"].ToString()
                            });
                        }
                    }
                }
            }

            return list;
        }


        public List<EmployeeGroup> GetAllGroups()
        {
            return GetAll();
        }


        // ============================================================
        // GET BY LOCAL ID
        // ============================================================

        public EmployeeGroup GetById(int id)
        {
            EmployeeGroup group = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand(@"
                    SELECT
                        Id,
                        ErpNextId,
                        GroupCode,
                        GroupName
                    FROM EmployeeGroups
                    WHERE Id = @Id
                      AND IsDeleted = 0",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            group = new EmployeeGroup
                            {
                                Id =
                                    Convert.ToInt32(
                                        reader["Id"]
                                    ),

                                ErpNextId =
                                    reader["ErpNextId"] == DBNull.Value
                                        ? ""
                                        : reader["ErpNextId"].ToString(),

                                GroupCode =
                                    reader["GroupCode"] == DBNull.Value
                                        ? ""
                                        : reader["GroupCode"].ToString(),

                                GroupName =
                                    reader["GroupName"] == DBNull.Value
                                        ? ""
                                        : reader["GroupName"].ToString(),

                                Members =
                                    new List<EmployeeGroupMember>()
                            };
                        }
                    }
                }


                if (group != null)
                {
                    using (var cmd = new SqlCommand(@"
                        SELECT
                            Id,
                            EmployeeGroupId,
                            Employee,
                            EmployeeName,
                            ERPNextUserId
                        FROM EmployeeGroupMembers
                        WHERE EmployeeGroupId = @GroupId
                        ORDER BY Id",
                        conn))
                    {
                        cmd.Parameters.AddWithValue(
                            "@GroupId",
                            id
                        );

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                group.Members.Add(
                                    new EmployeeGroupMember
                                    {
                                        Id =
                                            Convert.ToInt32(
                                                reader["Id"]
                                            ),

                                        EmployeeGroupId =
                                            Convert.ToInt32(
                                                reader["EmployeeGroupId"]
                                            ),

                                        Employee =
                                            reader["Employee"] == DBNull.Value
                                                ? ""
                                                : reader["Employee"].ToString(),

                                        EmployeeName =
                                            reader["EmployeeName"] == DBNull.Value
                                                ? ""
                                                : reader["EmployeeName"].ToString(),

                                        ERPNextUserId =
                                            reader["ERPNextUserId"] == DBNull.Value
                                                ? ""
                                                : reader["ERPNextUserId"].ToString()
                                    }
                                );
                            }
                        }
                    }
                }
            }

            return group;
        }


        // ============================================================
        // GET BY ERP NEXT ID
        // ============================================================

        public EmployeeGroup GetByErpNextId(string erpNextId)
        {
            if (string.IsNullOrWhiteSpace(erpNextId))
                return null;

            int localId = 0;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand(@"
                    SELECT TOP 1 Id
                    FROM EmployeeGroups
                    WHERE ErpNextId = @ErpNextId
                      AND IsDeleted = 0",
                    conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@ErpNextId",
                        erpNextId
                    );

                    var result =
                        cmd.ExecuteScalar();

                    if (result != null &&
                        result != DBNull.Value)
                    {
                        localId =
                            Convert.ToInt32(result);
                    }
                }
            }

            if (localId <= 0)
                return null;

            return GetById(localId);
        }


        // ============================================================
        // INSERT OR UPDATE
        // MAIN BI-DIRECTIONAL UPSERT
        // ============================================================

        public int InsertOrUpdate(EmployeeGroup group)
        {
            if (group == null)
                throw new ArgumentNullException(
                    nameof(group)
                );

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var transaction =
                    conn.BeginTransaction())
                {
                    try
                    {
                        int localId = 0;


                        // =================================================
                        // FIND BY ERP ID FIRST
                        // =================================================

                        if (!string.IsNullOrWhiteSpace(
                            group.ErpNextId))
                        {
                            using (var cmd = new SqlCommand(@"
                                SELECT TOP 1 Id
                                FROM EmployeeGroups
                                WHERE ErpNextId = @ErpNextId",
                                conn,
                                transaction))
                            {
                                cmd.Parameters.AddWithValue(
                                    "@ErpNextId",
                                    group.ErpNextId
                                );

                                var result =
                                    cmd.ExecuteScalar();

                                if (result != null &&
                                    result != DBNull.Value)
                                {
                                    localId =
                                        Convert.ToInt32(result);
                                }
                            }
                        }


                        // =================================================
                        // FALLBACK LOCAL ID
                        // =================================================

                        if (localId == 0 &&
                            group.Id > 0)
                        {
                            using (var cmd = new SqlCommand(@"
                                SELECT TOP 1 Id
                                FROM EmployeeGroups
                                WHERE Id = @Id",
                                conn,
                                transaction))
                            {
                                cmd.Parameters.AddWithValue(
                                    "@Id",
                                    group.Id
                                );

                                var result =
                                    cmd.ExecuteScalar();

                                if (result != null &&
                                    result != DBNull.Value)
                                {
                                    localId =
                                        Convert.ToInt32(result);
                                }
                            }
                        }


                        // =================================================
                        // UPDATE
                        // =================================================

                        if (localId > 0)
                        {
                            using (var cmd = new SqlCommand(@"
                                UPDATE EmployeeGroups
                                SET
                                    ErpNextId = @ErpNextId,
                                    GroupCode = @GroupCode,
                                    GroupName = @GroupName,
                                    IsDeleted = 0
                                WHERE Id = @Id",
                                conn,
                                transaction))
                            {
                                cmd.Parameters.AddWithValue(
                                    "@ErpNextId",
                                    DbValue(group.ErpNextId)
                                );

                                cmd.Parameters.AddWithValue(
                                    "@GroupCode",
                                    DbValue(group.GroupCode)
                                );

                                cmd.Parameters.AddWithValue(
                                    "@GroupName",
                                    DbValue(group.GroupName)
                                );

                                cmd.Parameters.AddWithValue(
                                    "@Id",
                                    localId
                                );

                                cmd.ExecuteNonQuery();
                            }


                            // ERP child table is authoritative
                            using (var cmd = new SqlCommand(@"
                                DELETE FROM EmployeeGroupMembers
                                WHERE EmployeeGroupId = @GroupId",
                                conn,
                                transaction))
                            {
                                cmd.Parameters.AddWithValue(
                                    "@GroupId",
                                    localId
                                );

                                cmd.ExecuteNonQuery();
                            }
                        }

                        // =================================================
                        // INSERT
                        // =================================================

                        else
                        {
                            using (var cmd = new SqlCommand(@"
                                INSERT INTO EmployeeGroups
                                (
                                    ErpNextId,
                                    GroupCode,
                                    GroupName,
                                    IsDeleted
                                )
                                OUTPUT INSERTED.Id
                                VALUES
                                (
                                    @ErpNextId,
                                    @GroupCode,
                                    @GroupName,
                                    0
                                )",
                                conn,
                                transaction))
                            {
                                cmd.Parameters.AddWithValue(
                                    "@ErpNextId",
                                    DbValue(group.ErpNextId)
                                );

                                cmd.Parameters.AddWithValue(
                                    "@GroupCode",
                                    DbValue(group.GroupCode)
                                );

                                cmd.Parameters.AddWithValue(
                                    "@GroupName",
                                    DbValue(group.GroupName)
                                );

                                localId =
                                    Convert.ToInt32(
                                        cmd.ExecuteScalar()
                                    );
                            }
                        }


                        // =================================================
                        // INSERT MEMBERS
                        // =================================================

                        if (group.Members != null)
                        {
                            foreach (var member in group.Members)
                            {
                                if (member == null ||
                                    string.IsNullOrWhiteSpace(
                                        member.Employee))
                                {
                                    continue;
                                }

                                using (var cmd = new SqlCommand(@"
                                    INSERT INTO EmployeeGroupMembers
                                    (
                                        EmployeeGroupId,
                                        Employee,
                                        EmployeeName,
                                        ERPNextUserId
                                    )
                                    VALUES
                                    (
                                        @EmployeeGroupId,
                                        @Employee,
                                        @EmployeeName,
                                        @ERPNextUserId
                                    )",
                                    conn,
                                    transaction))
                                {
                                    cmd.Parameters.AddWithValue(
                                        "@EmployeeGroupId",
                                        localId
                                    );

                                    cmd.Parameters.AddWithValue(
                                        "@Employee",
                                        DbValue(member.Employee)
                                    );

                                    cmd.Parameters.AddWithValue(
                                        "@EmployeeName",
                                        DbValue(member.EmployeeName)
                                    );

                                    cmd.Parameters.AddWithValue(
                                        "@ERPNextUserId",
                                        DbValue(member.ERPNextUserId)
                                    );

                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }


                        transaction.Commit();

                        return localId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }


        // ============================================================
        // EXISTING COMPATIBILITY METHODS
        // ============================================================

        public int Insert(EmployeeGroup group)
        {
            return InsertOrUpdate(group);
        }


        public int InsertGroup(EmployeeGroup group)
        {
            return InsertOrUpdate(group);
        }


        public void Update(EmployeeGroup group)
        {
            InsertOrUpdate(group);
        }


        public void SaveMembers(
            int groupId,
            List<EmployeeGroupMember> members)
        {
            var group =
                GetById(groupId);

            if (group == null)
            {
                throw new Exception(
                    "Employee Group not found."
                );
            }

            group.Members =
                members
                ?? new List<EmployeeGroupMember>();

            InsertOrUpdate(group);
        }


        // ============================================================
        // SOFT DELETE FROM ERP SYNC
        // ============================================================

        public void MarkDeletedByErpNextId(
            string erpNextId)
        {
            if (string.IsNullOrWhiteSpace(
                erpNextId))
            {
                return;
            }

            using (var conn =
                new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand(@"
                    UPDATE EmployeeGroups
                    SET IsDeleted = 1
                    WHERE ErpNextId = @ErpNextId",
                    conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@ErpNextId",
                        erpNextId
                    );

                    cmd.ExecuteNonQuery();
                }
            }
        }


        public void MarkDeletedByLocalId(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid Employee Group ID.");

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new SqlCommand(@"
                    UPDATE EmployeeGroups
                    SET IsDeleted = 1
                    WHERE Id = @Id
                      AND IsDeleted = 0",
                            conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@Id", id);

                            int affected = cmd.ExecuteNonQuery();

                            if (affected == 0)
                                throw new Exception(
                                    "Employee Group was not found in HRMS.");
                        }

                        tx.Commit();
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
        // DB VALUE HELPER
        // ============================================================

        private static object DbValue(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? DBNull.Value
                : value;
        }
    }
}