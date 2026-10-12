using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class EmployeeOnboardingTemplateService
    {
        private readonly string _connectionString;

        public EmployeeOnboardingTemplateService(
            string connectionString)
        {
            _connectionString =
                connectionString;
        }


        // ============================================================
        // GET ALL
        // ============================================================

        public List<EmployeeOnboardingTemplate> GetAll(
            string idFilter = null,
            string titleFilter = null)
        {
            var list =
                new List<EmployeeOnboardingTemplate>();


            using (var conn =
                new SqlConnection(
                    _connectionString))
            {
                string query = @"
                    SELECT *
                    FROM EmployeeOnboardingTemplates
                    WHERE IsDeleted = 0";


                if (!string.IsNullOrWhiteSpace(
                    idFilter))
                {
                    query +=
                        " AND CAST(Id AS VARCHAR) LIKE @IdFilter";
                }


                if (!string.IsNullOrWhiteSpace(
                    titleFilter))
                {
                    query +=
                        " AND Title LIKE @TitleFilter";
                }


                query +=
                    " ORDER BY Id DESC";


                using (var cmd =
                    new SqlCommand(
                        query,
                        conn))
                {
                    if (!string.IsNullOrWhiteSpace(
                        idFilter))
                    {
                        cmd.Parameters.AddWithValue(
                            "@IdFilter",
                            "%" + idFilter + "%"
                        );
                    }


                    if (!string.IsNullOrWhiteSpace(
                        titleFilter))
                    {
                        cmd.Parameters.AddWithValue(
                            "@TitleFilter",
                            "%" + titleFilter + "%"
                        );
                    }


                    conn.Open();


                    using (var reader =
                        cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(
                                MapTemplate(
                                    reader
                                )
                            );
                        }
                    }
                }
            }


            return list;
        }


        // ============================================================
        // GET BY ID
        // ============================================================

        public EmployeeOnboardingTemplate GetById(
            int id)
        {
            EmployeeOnboardingTemplate model =
                null;


            using (var conn =
                new SqlConnection(
                    _connectionString))
            {
                conn.Open();


                using (var cmd =
                    new SqlCommand(@"
                        SELECT *
                        FROM EmployeeOnboardingTemplates
                        WHERE Id = @Id
                        AND IsDeleted = 0",
                        conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@Id",
                        id
                    );


                    using (var reader =
                        cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model =
                                MapTemplate(
                                    reader
                                );
                        }
                    }
                }


                if (model == null)
                    return null;


                model.Activities =
                    LoadActivities(
                        conn,
                        id
                    );
            }


            return model;
        }


        // ============================================================
        // FIND BY ERP NEXT ID
        // ============================================================

        public EmployeeOnboardingTemplate GetByErpNextId(
            string erpNextId)
        {
            if (string.IsNullOrWhiteSpace(
                erpNextId))
            {
                return null;
            }


            int id = 0;


            using (var conn =
                new SqlConnection(
                    _connectionString))
            {
                conn.Open();


                using (var cmd =
                    new SqlCommand(@"
                        SELECT TOP 1 Id
                        FROM EmployeeOnboardingTemplates
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
                        id =
                            Convert.ToInt32(
                                result
                            );
                    }
                }
            }


            return id > 0
                ? GetById(id)
                : null;
        }


        // ============================================================
        // UPSERT
        // ERP -> HRMS
        // HRMS -> ERP AFTER ERP SUCCESS
        // ============================================================

        public int InsertOrUpdate(
            EmployeeOnboardingTemplate model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(
                    nameof(model)
                );
            }


            using (var conn =
                new SqlConnection(
                    _connectionString))
            {
                conn.Open();


                using (var transaction =
                    conn.BeginTransaction())
                {
                    try
                    {
                        int localId = 0;


                        // =================================================
                        // FIND BY ERP ID
                        // =================================================

                        if (!string.IsNullOrWhiteSpace(
                            model.ErpNextId))
                        {
                            using (var cmd =
                                new SqlCommand(@"
                                    SELECT TOP 1 Id
                                    FROM EmployeeOnboardingTemplates
                                    WHERE ErpNextId = @ErpNextId",
                                    conn,
                                    transaction))
                            {
                                cmd.Parameters.AddWithValue(
                                    "@ErpNextId",
                                    model.ErpNextId
                                );


                                var result =
                                    cmd.ExecuteScalar();


                                if (result != null &&
                                    result != DBNull.Value)
                                {
                                    localId =
                                        Convert.ToInt32(
                                            result
                                        );
                                }
                            }
                        }


                        // =================================================
                        // FALLBACK LOCAL ID
                        // =================================================

                        if (localId == 0 &&
                            model.Id > 0)
                        {
                            using (var cmd =
                                new SqlCommand(@"
                                    SELECT TOP 1 Id
                                    FROM EmployeeOnboardingTemplates
                                    WHERE Id = @Id",
                                    conn,
                                    transaction))
                            {
                                cmd.Parameters.AddWithValue(
                                    "@Id",
                                    model.Id
                                );


                                var result =
                                    cmd.ExecuteScalar();


                                if (result != null &&
                                    result != DBNull.Value)
                                {
                                    localId =
                                        Convert.ToInt32(
                                            result
                                        );
                                }
                            }
                        }


                        // =================================================
                        // UPDATE
                        // =================================================

                        if (localId > 0)
                        {
                            using (var cmd =
                                new SqlCommand(@"
                                    UPDATE EmployeeOnboardingTemplates
                                    SET
                                        ErpNextId = @ErpNextId,
                                        Title = @Title,
                                        Designation = @Designation,
                                        Company = @Company,
                                        EmployeeGrade = @EmployeeGrade,
                                        Department = @Department,
                                        IsDeleted = 0
                                    WHERE Id = @Id",
                                    conn,
                                    transaction))
                            {
                                AddTemplateParameters(
                                    cmd,
                                    model
                                );


                                cmd.Parameters.AddWithValue(
                                    "@Id",
                                    localId
                                );


                                cmd.ExecuteNonQuery();
                            }


                            using (var cmd =
                                new SqlCommand(@"
                                    DELETE FROM EmployeeOnboardingTemplateActivities
                                    WHERE TemplateId = @TemplateId",
                                    conn,
                                    transaction))
                            {
                                cmd.Parameters.AddWithValue(
                                    "@TemplateId",
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
                            using (var cmd =
                                new SqlCommand(@"
                                    INSERT INTO EmployeeOnboardingTemplates
                                    (
                                        ErpNextId,
                                        Title,
                                        Designation,
                                        Company,
                                        EmployeeGrade,
                                        Department,
                                        IsDeleted
                                    )
                                    OUTPUT INSERTED.Id
                                    VALUES
                                    (
                                        @ErpNextId,
                                        @Title,
                                        @Designation,
                                        @Company,
                                        @EmployeeGrade,
                                        @Department,
                                        0
                                    )",
                                    conn,
                                    transaction))
                            {
                                AddTemplateParameters(
                                    cmd,
                                    model
                                );


                                localId =
                                    Convert.ToInt32(
                                        cmd.ExecuteScalar()
                                    );
                            }
                        }


                        // =================================================
                        // ACTIVITIES
                        // =================================================

                        if (model.Activities != null)
                        {
                            foreach (
                                var activity
                                in model.Activities)
                            {
                                if (activity == null ||
                                    string.IsNullOrWhiteSpace(
                                        activity.ActivityName))
                                {
                                    continue;
                                }


                                using (var cmd =
                                    new SqlCommand(@"
                                        INSERT INTO EmployeeOnboardingTemplateActivities
                                        (
                                            TemplateId,
                                            ActivityName,
                                            UserName,
                                            BeginOnDays,
                                            DurationDays
                                        )
                                        VALUES
                                        (
                                            @TemplateId,
                                            @ActivityName,
                                            @UserName,
                                            @BeginOnDays,
                                            @DurationDays
                                        )",
                                        conn,
                                        transaction))
                                {
                                    cmd.Parameters.AddWithValue(
                                        "@TemplateId",
                                        localId
                                    );


                                    cmd.Parameters.AddWithValue(
                                        "@ActivityName",
                                        activity.ActivityName
                                    );


                                    cmd.Parameters.AddWithValue(
                                        "@UserName",
                                        DbValue(
                                            activity.UserName
                                        )
                                    );


                                    cmd.Parameters.AddWithValue(
                                        "@BeginOnDays",
                                        activity.BeginOnDays.HasValue
                                            ? (object)activity.BeginOnDays.Value
                                            : DBNull.Value
                                    );


                                    cmd.Parameters.AddWithValue(
                                        "@DurationDays",
                                        activity.DurationDays.HasValue
                                            ? (object)activity.DurationDays.Value
                                            : DBNull.Value
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


        public int Insert(
            EmployeeOnboardingTemplate model)
        {
            return InsertOrUpdate(
                model
            );
        }


        // ============================================================
        // GET EXISTING IDS
        // ============================================================

        public HashSet<string>
            GetExistingErpNextIds()
        {
            var ids =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase
                );


            using (var conn =
                new SqlConnection(
                    _connectionString))
            {
                conn.Open();


                using (var cmd =
                    new SqlCommand(@"
                        SELECT ErpNextId
                        FROM EmployeeOnboardingTemplates
                        WHERE ErpNextId IS NOT NULL
                        AND ErpNextId <> ''",
                        conn))
                using (var reader =
                    cmd.ExecuteReader())
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


            return ids;
        }


        // ============================================================
        // UNSYNCED
        // ============================================================

        public List<EmployeeOnboardingTemplate>
            GetUnsynced()
        {
            var list =
                new List<EmployeeOnboardingTemplate>();


            using (var conn =
                new SqlConnection(
                    _connectionString))
            {
                conn.Open();


                using (var cmd =
                    new SqlCommand(@"
                        SELECT *
                        FROM EmployeeOnboardingTemplates
                        WHERE
                        (
                            ErpNextId IS NULL
                            OR ErpNextId = ''
                        )
                        AND IsDeleted = 0",
                        conn))
                using (var reader =
                    cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(
                            MapTemplate(
                                reader
                            )
                        );
                    }
                }


                foreach (var item in list)
                {
                    item.Activities =
                        LoadActivities(
                            conn,
                            item.Id
                        );
                }
            }


            return list;
        }


        // ============================================================
        // UPDATE ERP ID
        // ============================================================

        public void UpdateErpNextId(
            int localId,
            string erpNextId)
        {
            using (var conn =
                new SqlConnection(
                    _connectionString))
            {
                conn.Open();


                using (var cmd =
                    new SqlCommand(@"
                        UPDATE EmployeeOnboardingTemplates
                        SET ErpNextId = @ErpNextId
                        WHERE Id = @Id",
                        conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@ErpNextId",
                        erpNextId
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
            UPDATE EmployeeOnboardingTemplates
            SET IsDeleted = 1
            WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public int MarkMissingErpNextTemplatesAsDeleted(HashSet<string> remoteIds)
        {
            int deletedCount = 0;

            remoteIds ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var template in GetAll())
            {
                if (!string.IsNullOrWhiteSpace(template.ErpNextId) &&
                    !remoteIds.Contains(template.ErpNextId))
                {
                    MarkDeletedById(template.Id);
                    deletedCount++;
                }
            }

            return deletedCount;
        }

        // ============================================================
        // LOAD ACTIVITIES
        // ============================================================

        private List<EmployeeOnboardingTemplateActivity>
            LoadActivities(
                SqlConnection conn,
                int templateId)
        {
            var list =
                new List<EmployeeOnboardingTemplateActivity>();


            using (var cmd =
                new SqlCommand(@"
                    SELECT *
                    FROM EmployeeOnboardingTemplateActivities
                    WHERE TemplateId = @TemplateId
                    ORDER BY Id",
                    conn))
            {
                cmd.Parameters.AddWithValue(
                    "@TemplateId",
                    templateId
                );


                using (var reader =
                    cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(
                            new EmployeeOnboardingTemplateActivity
                            {
                                Id =
                                    Convert.ToInt32(
                                        reader["Id"]
                                    ),

                                TemplateId =
                                    templateId,

                                ActivityName =
                                    reader["ActivityName"] == DBNull.Value
                                        ? ""
                                        : reader["ActivityName"].ToString(),

                                UserName =
                                    reader["UserName"] == DBNull.Value
                                        ? ""
                                        : reader["UserName"].ToString(),

                                BeginOnDays =
                                    reader["BeginOnDays"] == DBNull.Value
                                        ? (int?)null
                                        : Convert.ToInt32(
                                            reader["BeginOnDays"]
                                        ),

                                DurationDays =
                                    reader["DurationDays"] == DBNull.Value
                                        ? (int?)null
                                        : Convert.ToInt32(
                                            reader["DurationDays"]
                                        )
                            }
                        );
                    }
                }
            }


            return list;
        }


        // ============================================================
        // PARAMETER HELPER
        // ============================================================

        private static void AddTemplateParameters(
            SqlCommand cmd,
            EmployeeOnboardingTemplate model)
        {
            cmd.Parameters.AddWithValue(
                "@ErpNextId",
                DbValue(
                    model.ErpNextId
                )
            );


            cmd.Parameters.AddWithValue(
                "@Title",
                DbValue(
                    model.Title
                )
            );


            cmd.Parameters.AddWithValue(
                "@Designation",
                DbValue(
                    model.Designation
                )
            );


            cmd.Parameters.AddWithValue(
                "@Company",
                DbValue(
                    model.Company
                )
            );


            cmd.Parameters.AddWithValue(
                "@EmployeeGrade",
                DbValue(
                    model.EmployeeGrade
                )
            );


            cmd.Parameters.AddWithValue(
                "@Department",
                DbValue(
                    model.Department
                )
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


        // ============================================================
        // MAP
        // ============================================================

        private EmployeeOnboardingTemplate MapTemplate(
            SqlDataReader reader)
        {
            return new EmployeeOnboardingTemplate
            {
                Id =
                    Convert.ToInt32(
                        reader["Id"]
                    ),

                ErpNextId =
                    reader["ErpNextId"] == DBNull.Value
                        ? ""
                        : reader["ErpNextId"].ToString(),

                Title =
                    reader["Title"] == DBNull.Value
                        ? ""
                        : reader["Title"].ToString(),

                Designation =
                    reader["Designation"] == DBNull.Value
                        ? ""
                        : reader["Designation"].ToString(),

                Company =
                    reader["Company"] == DBNull.Value
                        ? ""
                        : reader["Company"].ToString(),

                EmployeeGrade =
                    reader["EmployeeGrade"] == DBNull.Value
                        ? ""
                        : reader["EmployeeGrade"].ToString(),

                Department =
                    reader["Department"] == DBNull.Value
                        ? ""
                        : reader["Department"].ToString(),

                Activities =
                    new List<EmployeeOnboardingTemplateActivity>()
            };
        }
    }
}