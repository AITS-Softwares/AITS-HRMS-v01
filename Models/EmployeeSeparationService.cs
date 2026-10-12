//using System;
//using System.Collections.Generic;
//using Microsoft.Data.SqlClient;
//using Erp_Aits.Models;

//namespace Erp_Aits.Services
//{
//    public class EmployeeSeparationService
//    {
//        private readonly string _connectionString;

//        public EmployeeSeparationService(string connectionString)
//        {
//            _connectionString = connectionString;
//        }

//        public List<EmployeeSeparation> GetAll(string idFilter, string employeeName)
//        {
//            var list = new List<EmployeeSeparation>();

//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = "SELECT * FROM EmployeeSeparations WHERE IsDeleted = 0";

//                if (!string.IsNullOrEmpty(idFilter)) query += " AND CAST(Id AS VARCHAR) LIKE @IdFilter";
//                if (!string.IsNullOrEmpty(employeeName)) query += " AND EmployeeName LIKE @EmployeeName";

//                query += " ORDER BY Id DESC";

//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    if (!string.IsNullOrEmpty(idFilter)) cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");
//                    if (!string.IsNullOrEmpty(employeeName)) cmd.Parameters.AddWithValue("@EmployeeName", "%" + employeeName + "%");

//                    conn.Open();
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            list.Add(MapSeparation(reader));
//                        }
//                    }
//                }
//            }

//            return list;
//        }

//        public EmployeeSeparation GetById(int id)
//        {
//            EmployeeSeparation model = null;

//            using (var conn = new SqlConnection(_connectionString))
//            {
//                conn.Open();

//                using (var cmd = new SqlCommand("SELECT * FROM EmployeeSeparations WHERE Id = @Id AND IsDeleted = 0", conn))
//                {
//                    cmd.Parameters.AddWithValue("@Id", id);
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        if (reader.Read())
//                        {
//                            model = MapSeparation(reader);
//                        }
//                    }
//                }

//                if (model != null)
//                {
//                    using (var cmd = new SqlCommand("SELECT * FROM EmployeeSeparationActivities WHERE EmployeeSeparationId = @Id", conn))
//                    {
//                        cmd.Parameters.AddWithValue("@Id", id);
//                        using (var reader = cmd.ExecuteReader())
//                        {
//                            while (reader.Read())
//                            {
//                                model.Activities.Add(new EmployeeSeparationActivity
//                                {
//                                    Id = Convert.ToInt32(reader["Id"]),
//                                    EmployeeSeparationId = id,
//                                    ActivityName = reader["ActivityName"]?.ToString() ?? string.Empty,
//                                    UserName = reader["UserName"]?.ToString() ?? string.Empty,
//                                    BeginOnDays = reader["BeginOnDays"] != DBNull.Value ? Convert.ToInt32(reader["BeginOnDays"]) : (int?)null,
//                                    DurationDays = reader["DurationDays"] != DBNull.Value ? Convert.ToInt32(reader["DurationDays"]) : (int?)null
//                                });
//                            }
//                        }
//                    }
//                }
//            }

//            return model;
//        }

//        public int Insert(EmployeeSeparation s)
//        {
//            int newId;

//            using (var conn = new SqlConnection(_connectionString))
//            {
//                conn.Open();

//                using (var cmd = new SqlCommand(@"
//                    INSERT INTO EmployeeSeparations
//                        (Employee, EmployeeName, Company, Status, SeparationBeginsOn, EmployeeSeparationTemplate, NotifyUsersByEmail, ExitInterviewSummary)
//                    OUTPUT INSERTED.Id
//                    VALUES
//                        (@Employee, @EmployeeName, @Company, @Status, @SeparationBeginsOn, @EmployeeSeparationTemplate, @NotifyUsersByEmail, @ExitInterviewSummary)", conn))
//                {
//                    cmd.Parameters.AddWithValue("@Employee", s.Employee ?? (object)DBNull.Value);
//                    cmd.Parameters.AddWithValue("@EmployeeName", (object)s.EmployeeName ?? DBNull.Value);
//                    cmd.Parameters.AddWithValue("@Company", s.Company ?? "AITS");
//                    cmd.Parameters.AddWithValue("@Status", s.Status ?? "Pending");
//                    cmd.Parameters.AddWithValue("@SeparationBeginsOn", s.SeparationBeginsOn);
//                    cmd.Parameters.AddWithValue("@EmployeeSeparationTemplate", (object)s.EmployeeSeparationTemplate ?? DBNull.Value);
//                    cmd.Parameters.AddWithValue("@NotifyUsersByEmail", s.NotifyUsersByEmail);
//                    cmd.Parameters.AddWithValue("@ExitInterviewSummary", (object)s.ExitInterviewSummary ?? DBNull.Value);

//                    newId = (int)cmd.ExecuteScalar();
//                }

//                foreach (var a in s.Activities)
//                {
//                    if (string.IsNullOrWhiteSpace(a.ActivityName)) continue;

//                    using (var cmd = new SqlCommand(@"
//                        INSERT INTO EmployeeSeparationActivities (EmployeeSeparationId, ActivityName, UserName, BeginOnDays, DurationDays)
//                        VALUES (@EmployeeSeparationId, @ActivityName, @UserName, @BeginOnDays, @DurationDays)", conn))
//                    {
//                        cmd.Parameters.AddWithValue("@EmployeeSeparationId", newId);
//                        cmd.Parameters.AddWithValue("@ActivityName", a.ActivityName ?? (object)DBNull.Value);
//                        cmd.Parameters.AddWithValue("@UserName", (object)a.UserName ?? DBNull.Value);
//                        cmd.Parameters.AddWithValue("@BeginOnDays", (object)a.BeginOnDays ?? DBNull.Value);
//                        cmd.Parameters.AddWithValue("@DurationDays", (object)a.DurationDays ?? DBNull.Value);

//                        cmd.ExecuteNonQuery();
//                    }
//                }
//            }

//            return newId;
//        }

//        private EmployeeSeparation MapSeparation(SqlDataReader reader)
//        {
//            return new EmployeeSeparation
//            {
//                Id = Convert.ToInt32(reader["Id"]),
//                Employee = reader["Employee"]?.ToString() ?? string.Empty,
//                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
//                Company = reader["Company"]?.ToString() ?? "AITS",
//                Status = reader["Status"]?.ToString() ?? "Pending",
//                SeparationBeginsOn = Convert.ToDateTime(reader["SeparationBeginsOn"]),
//                EmployeeSeparationTemplate = reader["EmployeeSeparationTemplate"]?.ToString() ?? string.Empty,
//                NotifyUsersByEmail = reader["NotifyUsersByEmail"] != DBNull.Value && Convert.ToBoolean(reader["NotifyUsersByEmail"]),
//                ExitInterviewSummary = reader["ExitInterviewSummary"]?.ToString() ?? string.Empty
//            };
//        }
//    }
//}




using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class EmployeeSeparationService
    {
        private readonly string _connectionString;

        public EmployeeSeparationService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<EmployeeSeparation> GetAll(string idFilter, string employeeName)
        {
            var list = new List<EmployeeSeparation>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM EmployeeSeparations WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(idFilter)) query += " AND (CAST(Id AS VARCHAR) LIKE @IdFilter OR EmployeeSeparationCode LIKE @IdFilter)";
                if (!string.IsNullOrEmpty(employeeName)) query += " AND EmployeeName LIKE @EmployeeName";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(idFilter)) cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");
                    if (!string.IsNullOrEmpty(employeeName)) cmd.Parameters.AddWithValue("@EmployeeName", "%" + employeeName + "%");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(MapSeparation(reader));
                        }
                    }
                }
            }

            return list;
        }

        public EmployeeSeparation GetById(int id)
        {
            EmployeeSeparation model = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand("SELECT * FROM EmployeeSeparations WHERE Id = @Id AND IsDeleted = 0", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model = MapSeparation(reader);
                        }
                    }
                }

                if (model != null)
                {
                    using (var cmd = new SqlCommand("SELECT * FROM EmployeeSeparationActivities WHERE EmployeeSeparationId = @Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                model.Activities.Add(new EmployeeSeparationActivity
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    EmployeeSeparationId = id,
                                    ActivityName = reader["ActivityName"]?.ToString() ?? string.Empty,
                                    UserName = reader["UserName"]?.ToString() ?? string.Empty,
                                    BeginOnDays = reader["BeginOnDays"] != DBNull.Value ? Convert.ToInt32(reader["BeginOnDays"]) : (int?)null,
                                    DurationDays = reader["DurationDays"] != DBNull.Value ? Convert.ToInt32(reader["DurationDays"]) : (int?)null
                                });
                            }
                        }
                    }
                }
            }

            return model;
        }

        public HashSet<string> GetExistingCodes()
        {
            var codes = new HashSet<string>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT EmployeeSeparationCode FROM EmployeeSeparations WHERE IsDeleted = 0 AND EmployeeSeparationCode IS NOT NULL";
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string code = reader["EmployeeSeparationCode"]?.ToString();
                            if (!string.IsNullOrEmpty(code)) codes.Add(code);
                        }
                    }
                }
            }
            return codes;
        }

        public int Insert(EmployeeSeparation s)
        {
            int newId;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand(@"
                    INSERT INTO EmployeeSeparations
                        (EmployeeSeparationCode, Employee, EmployeeName, Company, Status, SeparationBeginsOn, EmployeeSeparationTemplate, NotifyUsersByEmail, ExitInterviewSummary)
                    OUTPUT INSERTED.Id
                    VALUES
                        (@EmployeeSeparationCode, @Employee, @EmployeeName, @Company, @Status, @SeparationBeginsOn, @EmployeeSeparationTemplate, @NotifyUsersByEmail, @ExitInterviewSummary)", conn))
                {
                    cmd.Parameters.AddWithValue("@EmployeeSeparationCode", (object)s.EmployeeSeparationCode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Employee", s.Employee ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmployeeName", (object)s.EmployeeName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Company", s.Company ?? "Vinod Cookware India Private Limited");
                    cmd.Parameters.AddWithValue("@Status", s.Status ?? "Pending");
                    cmd.Parameters.AddWithValue("@SeparationBeginsOn", s.SeparationBeginsOn);
                    cmd.Parameters.AddWithValue("@EmployeeSeparationTemplate", (object)s.EmployeeSeparationTemplate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NotifyUsersByEmail", s.NotifyUsersByEmail);
                    cmd.Parameters.AddWithValue("@ExitInterviewSummary", (object)s.ExitInterviewSummary ?? DBNull.Value);

                    newId = (int)cmd.ExecuteScalar();
                }

                foreach (var a in s.Activities)
                {
                    if (string.IsNullOrWhiteSpace(a.ActivityName)) continue;

                    using (var cmd = new SqlCommand(@"
                        INSERT INTO EmployeeSeparationActivities (EmployeeSeparationId, ActivityName, UserName, BeginOnDays, DurationDays)
                        VALUES (@EmployeeSeparationId, @ActivityName, @UserName, @BeginOnDays, @DurationDays)", conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeeSeparationId", newId);
                        cmd.Parameters.AddWithValue("@ActivityName", a.ActivityName ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@UserName", (object)a.UserName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@BeginOnDays", (object)a.BeginOnDays ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@DurationDays", (object)a.DurationDays ?? DBNull.Value);

                        cmd.ExecuteNonQuery();
                    }
                }
            }

            return newId;
        }

        public void UpdateByCode(EmployeeSeparation s)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                int separationId = 0;
                using (var cmd = new SqlCommand("SELECT Id FROM EmployeeSeparations WHERE EmployeeSeparationCode = @Code", conn))
                {
                    cmd.Parameters.AddWithValue("@Code", s.EmployeeSeparationCode);
                    var res = cmd.ExecuteScalar();
                    if (res != null) separationId = Convert.ToInt32(res);
                }

                if (separationId == 0) return;

                using (var cmd = new SqlCommand(@"
                    UPDATE EmployeeSeparations
                    SET Employee = @Employee,
                        EmployeeName = @EmployeeName,
                        Company = @Company,
                        Status = @Status,
                        SeparationBeginsOn = @SeparationBeginsOn,
                        EmployeeSeparationTemplate = @EmployeeSeparationTemplate,
                        NotifyUsersByEmail = @NotifyUsersByEmail,
                        ExitInterviewSummary = @ExitInterviewSummary,
                        IsDeleted = 0
                    WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", separationId);
                    cmd.Parameters.AddWithValue("@Employee", s.Employee ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmployeeName", (object)s.EmployeeName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Company", s.Company ?? "Vinod Cookware India Private Limited");
                    cmd.Parameters.AddWithValue("@Status", s.Status ?? "Pending");
                    cmd.Parameters.AddWithValue("@SeparationBeginsOn", s.SeparationBeginsOn);
                    cmd.Parameters.AddWithValue("@EmployeeSeparationTemplate", (object)s.EmployeeSeparationTemplate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NotifyUsersByEmail", s.NotifyUsersByEmail);
                    cmd.Parameters.AddWithValue("@ExitInterviewSummary", (object)s.ExitInterviewSummary ?? DBNull.Value);

                    cmd.ExecuteNonQuery();
                }

                // Refresh activities
                using (var delCmd = new SqlCommand("DELETE FROM EmployeeSeparationActivities WHERE EmployeeSeparationId = @Id", conn))
                {
                    delCmd.Parameters.AddWithValue("@Id", separationId);
                    delCmd.ExecuteNonQuery();
                }

                foreach (var a in s.Activities)
                {
                    if (string.IsNullOrWhiteSpace(a.ActivityName)) continue;

                    using (var cmd = new SqlCommand(@"
                        INSERT INTO EmployeeSeparationActivities (EmployeeSeparationId, ActivityName, UserName, BeginOnDays, DurationDays)
                        VALUES (@EmployeeSeparationId, @ActivityName, @UserName, @BeginOnDays, @DurationDays)", conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeeSeparationId", separationId);
                        cmd.Parameters.AddWithValue("@ActivityName", a.ActivityName ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@UserName", (object)a.UserName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@BeginOnDays", (object)a.BeginOnDays ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@DurationDays", (object)a.DurationDays ?? DBNull.Value);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }



        public void MarkDeletedById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                const string query = @"
            UPDATE EmployeeSeparations
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

        public int InsertOrUpdateByCode(EmployeeSeparation s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));

            if (string.IsNullOrWhiteSpace(s.EmployeeSeparationCode))
                throw new Exception(
                    "Employee Separation ERPNext code is required."
                );

            string code =
                s.EmployeeSeparationCode.Trim();

            using (var conn =
                new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var transaction =
                    conn.BeginTransaction(
                        System.Data.IsolationLevel.Serializable))
                {
                    try
                    {
                        int existingId = 0;

                        // ============================================
                        // FIND EXISTING RECORD BY ERP DOCUMENT NAME
                        // ============================================

                        using (var cmd = new SqlCommand(@"
                    SELECT TOP 1 Id
                    FROM EmployeeSeparations
                    WITH (UPDLOCK, HOLDLOCK)
                    WHERE LTRIM(RTRIM(EmployeeSeparationCode))
                          = @Code
                    ORDER BY Id DESC",
                            conn,
                            transaction))
                        {
                            cmd.Parameters.AddWithValue(
                                "@Code",
                                code
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

                        // ============================================
                        // UPDATE EXISTING
                        // ============================================

                        if (existingId > 0)
                        {
                            using (var cmd = new SqlCommand(@"
                        UPDATE EmployeeSeparations
                        SET Employee = @Employee,
                            EmployeeName = @EmployeeName,
                            Company = @Company,
                            Status = @Status,
                            SeparationBeginsOn =
                                @SeparationBeginsOn,
                            EmployeeSeparationTemplate =
                                @Template,
                            NotifyUsersByEmail =
                                @Notify,
                            ExitInterviewSummary =
                                @ExitSummary,
                            IsDeleted = 0
                        WHERE Id = @Id",
                                conn,
                                transaction))
                            {
                                cmd.Parameters.AddWithValue(
                                    "@Id",
                                    existingId
                                );

                                cmd.Parameters.AddWithValue(
                                    "@Employee",
                                    (object)s.Employee
                                    ?? DBNull.Value
                                );

                                cmd.Parameters.AddWithValue(
                                    "@EmployeeName",
                                    (object)s.EmployeeName
                                    ?? DBNull.Value
                                );

                                cmd.Parameters.AddWithValue(
                                    "@Company",
                                    (object)s.Company
                                    ?? DBNull.Value
                                );

                                cmd.Parameters.AddWithValue(
                                    "@Status",
                                    (object)s.Status
                                    ?? DBNull.Value
                                );

                                cmd.Parameters.AddWithValue(
                                    "@SeparationBeginsOn",
                                    s.SeparationBeginsOn
                                );

                                cmd.Parameters.AddWithValue(
                                    "@Template",
                                    (object)s.EmployeeSeparationTemplate
                                    ?? DBNull.Value
                                );

                                cmd.Parameters.AddWithValue(
                                    "@Notify",
                                    s.NotifyUsersByEmail
                                );

                                cmd.Parameters.AddWithValue(
                                    "@ExitSummary",
                                    (object)s.ExitInterviewSummary
                                    ?? DBNull.Value
                                );

                                cmd.ExecuteNonQuery();
                            }

                            // Replace activities
                            using (var cmd = new SqlCommand(@"
                        DELETE
                        FROM EmployeeSeparationActivities
                        WHERE EmployeeSeparationId = @Id",
                                conn,
                                transaction))
                            {
                                cmd.Parameters.AddWithValue(
                                    "@Id",
                                    existingId
                                );

                                cmd.ExecuteNonQuery();
                            }
                        }

                        // ============================================
                        // INSERT ONLY WHEN ERP CODE DOES NOT EXIST
                        // ============================================

                        else
                        {
                            using (var cmd = new SqlCommand(@"
                        INSERT INTO EmployeeSeparations
                        (
                            EmployeeSeparationCode,
                            Employee,
                            EmployeeName,
                            Company,
                            Status,
                            SeparationBeginsOn,
                            EmployeeSeparationTemplate,
                            NotifyUsersByEmail,
                            ExitInterviewSummary,
                            IsDeleted
                        )
                        OUTPUT INSERTED.Id
                        VALUES
                        (
                            @Code,
                            @Employee,
                            @EmployeeName,
                            @Company,
                            @Status,
                            @SeparationBeginsOn,
                            @Template,
                            @Notify,
                            @ExitSummary,
                            0
                        )",
                                conn,
                                transaction))
                            {
                                cmd.Parameters.AddWithValue(
                                    "@Code",
                                    code
                                );

                                cmd.Parameters.AddWithValue(
                                    "@Employee",
                                    (object)s.Employee
                                    ?? DBNull.Value
                                );

                                cmd.Parameters.AddWithValue(
                                    "@EmployeeName",
                                    (object)s.EmployeeName
                                    ?? DBNull.Value
                                );

                                cmd.Parameters.AddWithValue(
                                    "@Company",
                                    (object)s.Company
                                    ?? DBNull.Value
                                );

                                cmd.Parameters.AddWithValue(
                                    "@Status",
                                    (object)s.Status
                                    ?? DBNull.Value
                                );

                                cmd.Parameters.AddWithValue(
                                    "@SeparationBeginsOn",
                                    s.SeparationBeginsOn
                                );

                                cmd.Parameters.AddWithValue(
                                    "@Template",
                                    (object)s.EmployeeSeparationTemplate
                                    ?? DBNull.Value
                                );

                                cmd.Parameters.AddWithValue(
                                    "@Notify",
                                    s.NotifyUsersByEmail
                                );

                                cmd.Parameters.AddWithValue(
                                    "@ExitSummary",
                                    (object)s.ExitInterviewSummary
                                    ?? DBNull.Value
                                );

                                existingId =
                                    Convert.ToInt32(
                                        cmd.ExecuteScalar()
                                    );
                            }
                        }

                        // ============================================
                        // INSERT CURRENT ACTIVITIES
                        // ============================================

                        if (s.Activities != null)
                        {
                            foreach (var activity in s.Activities)
                            {
                                if (activity == null ||
                                    string.IsNullOrWhiteSpace(
                                        activity.ActivityName))
                                {
                                    continue;
                                }

                                using (var cmd =
                                    new SqlCommand(@"
                                INSERT INTO
                                EmployeeSeparationActivities
                                (
                                    EmployeeSeparationId,
                                    ActivityName,
                                    UserName,
                                    BeginOnDays,
                                    DurationDays
                                )
                                VALUES
                                (
                                    @Id,
                                    @ActivityName,
                                    @UserName,
                                    @BeginOnDays,
                                    @DurationDays
                                )",
                                        conn,
                                        transaction))
                                {
                                    cmd.Parameters.AddWithValue(
                                        "@Id",
                                        existingId
                                    );

                                    cmd.Parameters.AddWithValue(
                                        "@ActivityName",
                                        activity.ActivityName
                                    );

                                    cmd.Parameters.AddWithValue(
                                        "@UserName",
                                        (object)activity.UserName
                                        ?? DBNull.Value
                                    );

                                    cmd.Parameters.AddWithValue(
                                        "@BeginOnDays",
                                        (object)activity.BeginOnDays
                                        ?? DBNull.Value
                                    );

                                    cmd.Parameters.AddWithValue(
                                        "@DurationDays",
                                        (object)activity.DurationDays
                                        ?? DBNull.Value
                                    );

                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();

                        return existingId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
















        public void MarkDeletedByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return;

            using (var conn = new SqlConnection(_connectionString))
            {
                const string query = @"
            UPDATE EmployeeSeparations
            SET IsDeleted = 1
            WHERE EmployeeSeparationCode = @Code";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Code", code);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
















        //private EmployeeSeparation MapSeparation(SqlDataReader reader)
        //{
        //    return new EmployeeSeparation
        //    {
        //        Id = Convert.ToInt32(reader["Id"]),
        //        EmployeeSeparationCode = reader["EmployeeSeparationCode"]?.ToString() ?? string.Empty,
        //        Employee = reader["Employee"]?.ToString() ?? string.Empty,
        //        EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
        //        Company = reader["Company"]?.ToString() ?? "Vinod Cookware India Private Limited",
        //        Status = reader["Status"]?.ToString() ?? "Pending",
        //        SeparationBeginsOn = Convert.ToDateTime(reader["SeparationBeginsOn"]),
        //        EmployeeSeparationTemplate = reader["EmployeeSeparationTemplate"]?.ToString() ?? string.Empty,
        //        NotifyUsersByEmail = reader["NotifyUsersByEmail"] != DBNull.Value && Convert.ToBoolean(reader["NotifyUsersByEmail"]),
        //        ExitInterviewSummary = reader["ExitInterviewSummary"]?.ToString() ?? string.Empty
        //    };
        //}

        private EmployeeSeparation MapSeparation(SqlDataReader reader)
        {
            string separationCode = string.Empty;

            // Safely check if the column exists in the reader
            try
            {
                separationCode = reader["EmployeeSeparationCode"]?.ToString() ?? string.Empty;
            }
            catch { }

            return new EmployeeSeparation
            {
                Id = Convert.ToInt32(reader["Id"]),
                EmployeeSeparationCode = separationCode,
                Employee = reader["Employee"]?.ToString() ?? string.Empty,
                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
                Company = reader["Company"]?.ToString() ?? "Vinod Cookware India Private Limited",
                Status = reader["Status"]?.ToString() ?? "Pending",
                SeparationBeginsOn = Convert.ToDateTime(reader["SeparationBeginsOn"]),
                EmployeeSeparationTemplate = reader["EmployeeSeparationTemplate"]?.ToString() ?? string.Empty,
                NotifyUsersByEmail = reader["NotifyUsersByEmail"] != DBNull.Value && Convert.ToBoolean(reader["NotifyUsersByEmail"]),
                ExitInterviewSummary = reader["ExitInterviewSummary"]?.ToString() ?? string.Empty
            };
        }
    }
}