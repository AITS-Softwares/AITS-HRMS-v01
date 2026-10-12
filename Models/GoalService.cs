using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class GoalService
    {
        private readonly string _connectionString;

        public GoalService(string connectionString)
        {
            _connectionString = connectionString;
        }



        public List<Goal> GetUnsynced()
        {
            var list = new List<Goal>();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string query = @"
            SELECT *
            FROM Goals
            WHERE IsDeleted = 0
              AND (ErpNextId IS NULL OR ErpNextId = '')
            ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapRow(reader));
                    }
                }
            }

            return list;
        }


        public void UpdateErpNextId(
            int localId,
            string erpNextId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string query = @"
            UPDATE Goals
            SET ErpNextId = @ErpNextId
            WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@ErpNextId",
                        string.IsNullOrWhiteSpace(erpNextId)
                            ? (object)DBNull.Value
                            : erpNextId
                    );

                    cmd.Parameters.AddWithValue(
                        "@Id",
                        localId
                    );

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void MarkDeletedById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand(@"
                    UPDATE Goals
                    SET IsDeleted = 1
                    WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void MarkMissingErpRecordsDeleted(HashSet<string> erpIds)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                if (erpIds == null || erpIds.Count == 0)
                {
                    using (var cmd = new SqlCommand(@"
                        UPDATE Goals
                        SET IsDeleted = 1
                        WHERE IsDeleted = 0
                          AND NULLIF(ErpNextId, '') IS NOT NULL", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    return;
                }

                var names = new List<string>();
                int index = 0;

                foreach (string erpId in erpIds)
                {
                    names.Add("@ErpId" + index++);
                }

                string query = @"
                    UPDATE Goals
                    SET IsDeleted = 1
                    WHERE IsDeleted = 0
                      AND NULLIF(ErpNextId, '') IS NOT NULL
                      AND ErpNextId NOT IN (" + string.Join(",", names) + ")";

                using (var cmd = new SqlCommand(query, conn))
                {
                    index = 0;

                    foreach (string erpId in erpIds)
                    {
                        cmd.Parameters.AddWithValue("@ErpId" + index++, erpId);
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }






        // ============================================================
        // GET ALL
        // ============================================================

        public List<Goal> GetAll(
            string idFilter = null,
            string goalName = null,
            string status = null,
            string employee = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string appraisalCycle = null)
        {
            var list = new List<Goal>();

            using (var conn =
                new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT *
                    FROM Goals
                    WHERE IsDeleted = 0";


                if (!string.IsNullOrWhiteSpace(idFilter))
                    query += " AND CAST(Id AS VARCHAR) LIKE @IdFilter";

                if (!string.IsNullOrWhiteSpace(goalName))
                    query += " AND GoalName LIKE @GoalName";

                if (!string.IsNullOrWhiteSpace(status))
                    query += " AND Status = @Status";

                if (!string.IsNullOrWhiteSpace(employee))
                    query += " AND Employee LIKE @Employee";

                if (startDate.HasValue)
                    query += " AND StartDate >= @StartDate";

                if (endDate.HasValue)
                    query += " AND EndDate <= @EndDate";

                if (!string.IsNullOrWhiteSpace(appraisalCycle))
                    query += " AND AppraisalCycle LIKE @AppraisalCycle";


                query += " ORDER BY Id DESC";


                using (var cmd =
                    new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrWhiteSpace(idFilter))
                        cmd.Parameters.AddWithValue(
                            "@IdFilter",
                            "%" + idFilter + "%"
                        );

                    if (!string.IsNullOrWhiteSpace(goalName))
                        cmd.Parameters.AddWithValue(
                            "@GoalName",
                            "%" + goalName + "%"
                        );

                    if (!string.IsNullOrWhiteSpace(status))
                        cmd.Parameters.AddWithValue(
                            "@Status",
                            status
                        );

                    if (!string.IsNullOrWhiteSpace(employee))
                        cmd.Parameters.AddWithValue(
                            "@Employee",
                            "%" + employee + "%"
                        );

                    if (startDate.HasValue)
                        cmd.Parameters.AddWithValue(
                            "@StartDate",
                            startDate.Value
                        );

                    if (endDate.HasValue)
                        cmd.Parameters.AddWithValue(
                            "@EndDate",
                            endDate.Value
                        );

                    if (!string.IsNullOrWhiteSpace(appraisalCycle))
                        cmd.Parameters.AddWithValue(
                            "@AppraisalCycle",
                            "%" + appraisalCycle + "%"
                        );


                    conn.Open();

                    using (var reader =
                        cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(
                                MapRow(reader)
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

        public Goal GetById(int id)
        {
            using (var conn =
                new SqlConnection(_connectionString))
            {
                using (var cmd =
                    new SqlCommand(@"
                        SELECT *
                        FROM Goals
                        WHERE Id = @Id
                        AND IsDeleted = 0",
                        conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@Id",
                        id
                    );

                    conn.Open();

                    using (var reader =
                        cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapRow(reader);
                        }
                    }
                }
            }

            return null;
        }


        // ============================================================
        // GET BY ERP NEXT ID
        // ============================================================

        public Goal GetByErpNextId(
            string erpNextId)
        {
            if (string.IsNullOrWhiteSpace(
                erpNextId))
            {
                return null;
            }


            using (var conn =
                new SqlConnection(_connectionString))
            {
                using (var cmd =
                    new SqlCommand(@"
                        SELECT *
                        FROM Goals
                        WHERE ErpNextId = @ErpNextId
                        AND IsDeleted = 0",
                        conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@ErpNextId",
                        erpNextId
                    );

                    conn.Open();

                    using (var reader =
                        cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapRow(reader);
                        }
                    }
                }
            }


            return null;
        }


        // ============================================================
        // INSERT OR UPDATE
        // ============================================================

        public int InsertOrUpdate(
            Goal g)
        {
            using (var conn =
                new SqlConnection(_connectionString))
            {
                conn.Open();


                int existingId = 0;


                // Prefer ERPNext ID
                if (!string.IsNullOrWhiteSpace(
                    g.ErpNextId))
                {
                    using (var cmd =
                        new SqlCommand(@"
                            SELECT TOP 1 Id
                            FROM Goals
                            WHERE ErpNextId = @ErpNextId",
                            conn))
                    {
                        cmd.Parameters.AddWithValue(
                            "@ErpNextId",
                            g.ErpNextId
                        );


                        var value =
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


                // ====================================================
                // UPDATE
                // ====================================================

                if (existingId > 0)
                {
                    using (var cmd =
                        new SqlCommand(@"
                            UPDATE Goals
                            SET
                                GoalName = @GoalName,
                                Progress = @Progress,
                                Status = @Status,
                                IsGroup = @IsGroup,
                                ParentGoal = @ParentGoal,
                                Employee = @Employee,
                                EmployeeName = @EmployeeName,
                                Company = @Company,
                                StartDate = @StartDate,
                                EndDate = @EndDate,
                                AppraisalCycle = @AppraisalCycle,
                                Kra = @Kra,
                                Description = @Description,
                                IsDeleted = 0
                            WHERE Id = @Id",
                            conn))
                    {
                        AddParameters(
                            cmd,
                            g
                        );


                        cmd.Parameters.AddWithValue(
                            "@Id",
                            existingId
                        );


                        cmd.ExecuteNonQuery();
                    }


                    return existingId;
                }


                // ====================================================
                // INSERT
                // ====================================================

                using (var cmd =
                    new SqlCommand(@"
                        INSERT INTO Goals
                        (
                            ErpNextId,
                            GoalName,
                            Progress,
                            Status,
                            IsGroup,
                            ParentGoal,
                            Employee,
                            EmployeeName,
                            Company,
                            StartDate,
                            EndDate,
                            AppraisalCycle,
                            Kra,
                            Description,
                            IsDeleted
                        )
                        OUTPUT INSERTED.Id
                        VALUES
                        (
                            @ErpNextId,
                            @GoalName,
                            @Progress,
                            @Status,
                            @IsGroup,
                            @ParentGoal,
                            @Employee,
                            @EmployeeName,
                            @Company,
                            @StartDate,
                            @EndDate,
                            @AppraisalCycle,
                            @Kra,
                            @Description,
                            0
                        )",
                        conn))
                {
                    AddParameters(
                        cmd,
                        g
                    );


                    return Convert.ToInt32(
                        cmd.ExecuteScalar()
                    );
                }
            }
        }


        // Compatibility
        public int Insert(Goal g)
        {
            return InsertOrUpdate(g);
        }


        private static void AddParameters(
            SqlCommand cmd,
            Goal g)
        {
            cmd.Parameters.AddWithValue(
                "@ErpNextId",
                DbValue(g.ErpNextId)
            );

            cmd.Parameters.AddWithValue(
                "@GoalName",
                DbValue(g.GoalName)
            );

            cmd.Parameters.AddWithValue(
                "@Progress",
                g.Progress.HasValue
                    ? (object)g.Progress.Value
                    : DBNull.Value
            );

            cmd.Parameters.AddWithValue(
                "@Status",
                string.IsNullOrWhiteSpace(g.Status)
                    ? "Pending"
                    : g.Status
            );

            cmd.Parameters.AddWithValue(
                "@IsGroup",
                g.IsGroup
            );

            cmd.Parameters.AddWithValue(
                "@ParentGoal",
                DbValue(g.ParentGoal)
            );

            cmd.Parameters.AddWithValue(
                "@Employee",
                DbValue(g.Employee)
            );

            cmd.Parameters.AddWithValue(
                "@EmployeeName",
                DbValue(g.EmployeeName)
            );

            cmd.Parameters.AddWithValue(
                "@Company",
                DbValue(g.Company)
            );

            cmd.Parameters.AddWithValue(
                "@StartDate",
                g.StartDate.HasValue
                    ? (object)g.StartDate.Value
                    : DBNull.Value
            );

            cmd.Parameters.AddWithValue(
                "@EndDate",
                g.EndDate.HasValue
                    ? (object)g.EndDate.Value
                    : DBNull.Value
            );

            cmd.Parameters.AddWithValue(
                "@AppraisalCycle",
                DbValue(g.AppraisalCycle)
            );

            cmd.Parameters.AddWithValue(
                "@Kra",
                DbValue(g.Kra)
            );

            cmd.Parameters.AddWithValue(
                "@Description",
                DbValue(g.Description)
            );
        }


        private static object DbValue(
            string value)
        {
            return string.IsNullOrWhiteSpace(
                value)
                ? DBNull.Value
                : value;
        }


        private Goal MapRow(
            System.Data.Common.DbDataReader reader)
        {
            bool HasColumn(
                string columnName)
            {
                for (
                    int i = 0;
                    i < reader.FieldCount;
                    i++
                )
                {
                    if (
                        reader.GetName(i)
                            .Equals(
                                columnName,
                                StringComparison.OrdinalIgnoreCase
                            )
                    )
                    {
                        return true;
                    }
                }

                return false;
            }


            return new Goal
            {
                Id =
                    Convert.ToInt32(
                        reader["Id"]
                    ),

                ErpNextId =
                    HasColumn("ErpNextId") &&
                    reader["ErpNextId"] != DBNull.Value
                        ? reader["ErpNextId"].ToString()
                        : "",

                GoalName =
                    reader["GoalName"] == DBNull.Value
                        ? ""
                        : reader["GoalName"].ToString(),

                Progress =
                    reader["Progress"] == DBNull.Value
                        ? (decimal?)null
                        : Convert.ToDecimal(
                            reader["Progress"]
                        ),

                Status =
                    reader["Status"] == DBNull.Value
                        ? "Pending"
                        : reader["Status"].ToString(),

                IsGroup =
                    reader["IsGroup"] != DBNull.Value &&
                    Convert.ToBoolean(
                        reader["IsGroup"]
                    ),

                ParentGoal =
                    HasColumn("ParentGoal") &&
                    reader["ParentGoal"] != DBNull.Value
                        ? reader["ParentGoal"].ToString()
                        : "",

                Employee =
                    reader["Employee"] == DBNull.Value
                        ? ""
                        : reader["Employee"].ToString(),

                EmployeeName =
                    HasColumn("EmployeeName") &&
                    reader["EmployeeName"] != DBNull.Value
                        ? reader["EmployeeName"].ToString()
                        : "",

                Company =
                    reader["Company"] == DBNull.Value
                        ? ""
                        : reader["Company"].ToString(),

                StartDate =
                    reader["StartDate"] == DBNull.Value
                        ? (DateTime?)null
                        : Convert.ToDateTime(
                            reader["StartDate"]
                        ),

                EndDate =
                    reader["EndDate"] == DBNull.Value
                        ? (DateTime?)null
                        : Convert.ToDateTime(
                            reader["EndDate"]
                        ),

                AppraisalCycle =
                    reader["AppraisalCycle"] == DBNull.Value
                        ? ""
                        : reader["AppraisalCycle"].ToString(),

                Kra =
                    HasColumn("Kra") &&
                    reader["Kra"] != DBNull.Value
                        ? reader["Kra"].ToString()
                        : "",

                Description =
                    reader["Description"] == DBNull.Value
                        ? ""
                        : reader["Description"].ToString()
            };
        }
    }
}
