using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class LeavePolicyAssignmentService
    {
        private readonly string _connectionString;

        public LeavePolicyAssignmentService(string connectionString)
        {
            _connectionString = connectionString;
        }


        // ============================================================
        // GET ALL
        // ============================================================

        public List<LeavePolicyAssignment> GetAll(
            string id = null,
            string employee = null,
            string employeeName = null,
            string company = null,
            string leavePolicy = null)
        {
            var list = new List<LeavePolicyAssignment>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT *
                    FROM LeavePolicyAssignments
                    WHERE IsDeleted = 0";


                if (!string.IsNullOrWhiteSpace(id))
                {
                    query += @"
                        AND (
                            CAST(Id AS VARCHAR) LIKE @Id
                            OR ErpNextId LIKE @Id
                        )";
                }

                if (!string.IsNullOrWhiteSpace(employee))
                    query += " AND Employee LIKE @Employee";

                if (!string.IsNullOrWhiteSpace(employeeName))
                    query += " AND EmployeeName LIKE @EmployeeName";

                if (!string.IsNullOrWhiteSpace(company))
                    query += " AND Company LIKE @Company";

                if (!string.IsNullOrWhiteSpace(leavePolicy))
                    query += " AND LeavePolicy LIKE @LeavePolicy";


                query += " ORDER BY Id DESC";


                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrWhiteSpace(id))
                        cmd.Parameters.AddWithValue("@Id", "%" + id + "%");

                    if (!string.IsNullOrWhiteSpace(employee))
                        cmd.Parameters.AddWithValue("@Employee", "%" + employee + "%");

                    if (!string.IsNullOrWhiteSpace(employeeName))
                        cmd.Parameters.AddWithValue("@EmployeeName", "%" + employeeName + "%");

                    if (!string.IsNullOrWhiteSpace(company))
                        cmd.Parameters.AddWithValue("@Company", "%" + company + "%");

                    if (!string.IsNullOrWhiteSpace(leavePolicy))
                        cmd.Parameters.AddWithValue("@LeavePolicy", "%" + leavePolicy + "%");


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


        // ============================================================
        // GET BY LOCAL ID
        // ============================================================

        public LeavePolicyAssignment GetById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand(@"
                    SELECT *
                    FROM LeavePolicyAssignments
                    WHERE Id = @Id
                      AND IsDeleted = 0",
                    conn))
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


        // ============================================================
        // GET BY ERP ID
        // ============================================================

        public LeavePolicyAssignment GetByErpNextId(
            string erpNextId)
        {
            if (string.IsNullOrWhiteSpace(erpNextId))
                return null;


            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand(@"
                    SELECT TOP 1 *
                    FROM LeavePolicyAssignments
                    WHERE ErpNextId = @ErpNextId
                      AND IsDeleted = 0",
                    conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@ErpNextId",
                        erpNextId
                    );

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


        // ============================================================
        // INSERT / UPDATE FROM ERP
        // ============================================================

        public int InsertOrUpdate(
            LeavePolicyAssignment model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                int existingId = 0;


                if (!string.IsNullOrWhiteSpace(model.ErpNextId))
                {
                    using (var find = new SqlCommand(@"
                        SELECT TOP 1 Id
                        FROM LeavePolicyAssignments
                        WHERE ErpNextId = @ErpNextId",
                        conn))
                    {
                        find.Parameters.AddWithValue(
                            "@ErpNextId",
                            model.ErpNextId
                        );

                        object value = find.ExecuteScalar();

                        if (value != null && value != DBNull.Value)
                            existingId = Convert.ToInt32(value);
                    }
                }


                if (existingId > 0)
                {
                    using (var cmd = new SqlCommand(@"
                        UPDATE LeavePolicyAssignments
                        SET
                            Employee = @Employee,
                            EmployeeName = @EmployeeName,
                            Company = @Company,
                            LeavePolicy = @LeavePolicy,
                            AssignmentBasedOn = @AssignmentBasedOn,
                            LeavePeriod = @LeavePeriod,
                            EffectiveFrom = @EffectiveFrom,
                            EffectiveTo = @EffectiveTo,
                            AddUnusedLeaves = @AddUnusedLeaves,
                            LeavesAllocated = @LeavesAllocated,
                            DocStatus = @DocStatus,
                            AmendedFrom = @AmendedFrom,
                            IsDeleted = 0
                        WHERE Id = @Id",
                        conn))
                    {
                        AddParameters(cmd, model);

                        cmd.Parameters.AddWithValue(
                            "@Id",
                            existingId
                        );

                        cmd.ExecuteNonQuery();
                    }

                    return existingId;
                }


                using (var cmd = new SqlCommand(@"
                    INSERT INTO LeavePolicyAssignments
                    (
                        ErpNextId,
                        Employee,
                        EmployeeName,
                        Company,
                        LeavePolicy,
                        AssignmentBasedOn,
                        LeavePeriod,
                        EffectiveFrom,
                        EffectiveTo,
                        AddUnusedLeaves,
                        LeavesAllocated,
                        DocStatus,
                        AmendedFrom,
                        IsDeleted
                    )
                    OUTPUT INSERTED.Id
                    VALUES
                    (
                        @ErpNextId,
                        @Employee,
                        @EmployeeName,
                        @Company,
                        @LeavePolicy,
                        @AssignmentBasedOn,
                        @LeavePeriod,
                        @EffectiveFrom,
                        @EffectiveTo,
                        @AddUnusedLeaves,
                        @LeavesAllocated,
                        @DocStatus,
                        @AmendedFrom,
                        0
                    )",
                    conn))
                {
                    AddParameters(cmd, model);

                    return Convert.ToInt32(
                        cmd.ExecuteScalar()
                    );
                }
            }
        }


        // Compatibility with old controller calls
        public int Insert(
            LeavePolicyAssignment model)
        {
            return InsertOrUpdate(model);
        }


        private static void AddParameters(
            SqlCommand cmd,
            LeavePolicyAssignment model)
        {
            cmd.Parameters.AddWithValue(
                "@ErpNextId",
                Db(model.ErpNextId)
            );

            cmd.Parameters.AddWithValue(
                "@Employee",
                Db(model.Employee)
            );

            cmd.Parameters.AddWithValue(
                "@EmployeeName",
                Db(model.EmployeeName)
            );

            cmd.Parameters.AddWithValue(
                "@Company",
                Db(model.Company)
            );

            cmd.Parameters.AddWithValue(
                "@LeavePolicy",
                Db(model.LeavePolicy)
            );

            cmd.Parameters.AddWithValue(
                "@AssignmentBasedOn",
                Db(model.AssignmentBasedOn)
            );

            cmd.Parameters.AddWithValue(
                "@LeavePeriod",
                Db(model.LeavePeriod)
            );

            cmd.Parameters.AddWithValue(
                "@EffectiveFrom",
                model.EffectiveFrom
            );

            cmd.Parameters.AddWithValue(
                "@EffectiveTo",
                model.EffectiveTo
            );

            cmd.Parameters.AddWithValue(
                "@AddUnusedLeaves",
                model.AddUnusedLeaves
            );

            cmd.Parameters.AddWithValue(
                "@LeavesAllocated",
                model.LeavesAllocated
            );

            cmd.Parameters.AddWithValue(
                "@DocStatus",
                model.DocStatus
            );

            cmd.Parameters.AddWithValue(
                "@AmendedFrom",
                Db(model.AmendedFrom)
            );
        }


        private LeavePolicyAssignment Map(
            SqlDataReader reader)
        {
            bool Has(string name)
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (reader.GetName(i)
                        .Equals(name, StringComparison.OrdinalIgnoreCase))
                        return true;
                }

                return false;
            }


            return new LeavePolicyAssignment
            {
                Id =
                    Convert.ToInt32(reader["Id"]),

                ErpNextId =
                    Has("ErpNextId") &&
                    reader["ErpNextId"] != DBNull.Value
                        ? reader["ErpNextId"].ToString()
                        : "",

                Employee =
                    reader["Employee"] == DBNull.Value
                        ? ""
                        : reader["Employee"].ToString(),

                EmployeeName =
                    reader["EmployeeName"] == DBNull.Value
                        ? ""
                        : reader["EmployeeName"].ToString(),

                Company =
                    reader["Company"] == DBNull.Value
                        ? ""
                        : reader["Company"].ToString(),

                LeavePolicy =
                    reader["LeavePolicy"] == DBNull.Value
                        ? ""
                        : reader["LeavePolicy"].ToString(),

                AssignmentBasedOn =
                    reader["AssignmentBasedOn"] == DBNull.Value
                        ? ""
                        : reader["AssignmentBasedOn"].ToString(),

                LeavePeriod =
                    Has("LeavePeriod") &&
                    reader["LeavePeriod"] != DBNull.Value
                        ? reader["LeavePeriod"].ToString()
                        : "",

                EffectiveFrom =
                    Convert.ToDateTime(
                        reader["EffectiveFrom"]
                    ),

                EffectiveTo =
                    Convert.ToDateTime(
                        reader["EffectiveTo"]
                    ),

                AddUnusedLeaves =
                    reader["AddUnusedLeaves"] != DBNull.Value &&
                    Convert.ToBoolean(
                        reader["AddUnusedLeaves"]
                    ),

                LeavesAllocated =
                    Has("LeavesAllocated") &&
                    reader["LeavesAllocated"] != DBNull.Value &&
                    Convert.ToBoolean(
                        reader["LeavesAllocated"]
                    ),

                DocStatus =
                    Has("DocStatus") &&
                    reader["DocStatus"] != DBNull.Value
                        ? Convert.ToInt32(
                            reader["DocStatus"]
                        )
                        : 0,

                AmendedFrom =
                    Has("AmendedFrom") &&
                    reader["AmendedFrom"] != DBNull.Value
                        ? reader["AmendedFrom"].ToString()
                        : ""
            };
        }


        private static object Db(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? DBNull.Value
                : value;
        }
    }
}