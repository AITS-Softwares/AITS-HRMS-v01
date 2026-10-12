using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class EmployeeSkillMapService
    {
        private readonly string _connectionString;


        public EmployeeSkillMapService(
            string connectionString)
        {
            _connectionString =
                connectionString;
        }


        // ============================================================
        // GET ALL
        // ============================================================

        public List<EmployeeSkillMapListRow> GetAll(
            string idFilter,
            string employeeName)
        {
            var list =
                new List<EmployeeSkillMapListRow>();


            using (var conn =
                new SqlConnection(
                    _connectionString))
            {
                string query = @"
                    SELECT
                        Id,
                        ErpNextId,
                        Employee,
                        EmployeeName,
                        Designation
                    FROM EmployeeSkillMaps
                    WHERE IsDeleted = 0";


                if (!string.IsNullOrWhiteSpace(
                    idFilter))
                {
                    query +=
                        " AND CAST(Id AS VARCHAR) LIKE @IdFilter";
                }


                if (!string.IsNullOrWhiteSpace(
                    employeeName))
                {
                    query +=
                        " AND EmployeeName LIKE @EmployeeName";
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
                        employeeName))
                    {
                        cmd.Parameters.AddWithValue(
                            "@EmployeeName",
                            "%" + employeeName + "%"
                        );
                    }


                    conn.Open();


                    using (var reader =
                        cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(
                                new EmployeeSkillMapListRow
                                {
                                    Id =
                                        Convert.ToInt32(
                                            reader["Id"]
                                        ),

                                    ErpNextId =
                                        reader["ErpNextId"] == DBNull.Value
                                            ? ""
                                            : reader["ErpNextId"].ToString(),

                                    Employee =
                                        reader["Employee"] == DBNull.Value
                                            ? ""
                                            : reader["Employee"].ToString(),

                                    EmployeeName =
                                        reader["EmployeeName"] == DBNull.Value
                                            ? ""
                                            : reader["EmployeeName"].ToString(),

                                    Designation =
                                        reader["Designation"] == DBNull.Value
                                            ? ""
                                            : reader["Designation"].ToString()
                                }
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

        public EmployeeSkillMap GetById(
            int id)
        {
            EmployeeSkillMap model =
                null;


            using (var conn =
                new SqlConnection(
                    _connectionString))
            {
                conn.Open();


                using (var cmd =
                    new SqlCommand(@"
                        SELECT
                            Id,
                            ErpNextId,
                            Employee,
                            EmployeeName,
                            Designation
                        FROM EmployeeSkillMaps
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
                                new EmployeeSkillMap
                                {
                                    Id =
                                        Convert.ToInt32(
                                            reader["Id"]
                                        ),

                                    ErpNextId =
                                        reader["ErpNextId"] == DBNull.Value
                                            ? ""
                                            : reader["ErpNextId"].ToString(),

                                    Employee =
                                        reader["Employee"] == DBNull.Value
                                            ? ""
                                            : reader["Employee"].ToString(),

                                    EmployeeName =
                                        reader["EmployeeName"] == DBNull.Value
                                            ? ""
                                            : reader["EmployeeName"].ToString(),

                                    Designation =
                                        reader["Designation"] == DBNull.Value
                                            ? ""
                                            : reader["Designation"].ToString(),

                                    Skills =
                                        new List<EmployeeSkillMapSkill>(),

                                    Trainings =
                                        new List<EmployeeSkillMapTraining>()
                                };
                        }
                    }
                }


                if (model == null)
                    return null;


                // ====================================================
                // SKILLS
                // ====================================================

                using (var cmd =
                    new SqlCommand(@"
                        SELECT
                            Id,
                            EmployeeSkillMapId,
                            Skill,
                            Proficiency,
                            EvaluationDate
                        FROM EmployeeSkillMapSkills
                        WHERE EmployeeSkillMapId = @Id
                        ORDER BY Id",
                        conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@Id",
                        id
                    );


                    using (var reader =
                        cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            decimal proficiency = 0;


                            if (
                                reader["Proficiency"] != DBNull.Value
                            )
                            {
                                decimal.TryParse(
                                    reader["Proficiency"].ToString(),
                                    out proficiency
                                );
                            }


                            model.Skills.Add(
                                new EmployeeSkillMapSkill
                                {
                                    Id =
                                        Convert.ToInt32(
                                            reader["Id"]
                                        ),

                                    EmployeeSkillMapId =
                                        Convert.ToInt32(
                                            reader["EmployeeSkillMapId"]
                                        ),

                                    Skill =
                                        reader["Skill"] == DBNull.Value
                                            ? ""
                                            : reader["Skill"].ToString(),

                                    Proficiency =
                                        proficiency,

                                    EvaluationDate =
                                        reader["EvaluationDate"] == DBNull.Value
                                            ? (DateTime?)null
                                            : Convert.ToDateTime(
                                                reader["EvaluationDate"]
                                            )
                                }
                            );
                        }
                    }
                }


                // ====================================================
                // TRAININGS
                // ====================================================

                using (var cmd =
                    new SqlCommand(@"
                        SELECT
                            Id,
                            EmployeeSkillMapId,
                            Training,
                            TrainingDate
                        FROM EmployeeSkillMapTrainings
                        WHERE EmployeeSkillMapId = @Id
                        ORDER BY Id",
                        conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@Id",
                        id
                    );


                    using (var reader =
                        cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.Trainings.Add(
                                new EmployeeSkillMapTraining
                                {
                                    Id =
                                        Convert.ToInt32(
                                            reader["Id"]
                                        ),

                                    EmployeeSkillMapId =
                                        Convert.ToInt32(
                                            reader["EmployeeSkillMapId"]
                                        ),

                                    Training =
                                        reader["Training"] == DBNull.Value
                                            ? ""
                                            : reader["Training"].ToString(),

                                    TrainingDate =
                                        reader["TrainingDate"] == DBNull.Value
                                            ? (DateTime?)null
                                            : Convert.ToDateTime(
                                                reader["TrainingDate"]
                                            )
                                }
                            );
                        }
                    }
                }
            }


            return model;
        }


        // ============================================================
        // GET BY ERP ID
        // ============================================================

        public EmployeeSkillMap GetByErpNextId(
            string erpNextId)
        {
            if (string.IsNullOrWhiteSpace(
                erpNextId))
            {
                return null;
            }


            int localId = 0;


            using (var conn =
                new SqlConnection(
                    _connectionString))
            {
                conn.Open();


                using (var cmd =
                    new SqlCommand(@"
                        SELECT TOP 1 Id
                        FROM EmployeeSkillMaps
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
                            Convert.ToInt32(
                                result
                            );
                    }
                }
            }


            return localId > 0
                ? GetById(localId)
                : null;
        }


        public void MarkDeletedIfMissingFromErpNext(List<string> erpIds)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                if (erpIds == null || !erpIds.Any())
                    return;

                var parameterNames = new List<string>();

                using (var cmd = conn.CreateCommand())
                {
                    for (int i = 0; i < erpIds.Count; i++)
                    {
                        string paramName = "@ErpId" + i;
                        parameterNames.Add(paramName);
                        cmd.Parameters.AddWithValue(paramName, erpIds[i]);
                    }

                    cmd.CommandText = $@"
                UPDATE EmployeeSkillMaps
                SET IsDeleted = 1
                WHERE IsDeleted = 0
                AND ISNULL(ErpNextId, '') <> ''
                AND ErpNextId NOT IN ({string.Join(",", parameterNames)})";

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
            UPDATE EmployeeSkillMaps
            SET IsDeleted = 1
            WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }













        public void DeleteByEmployee(string employee)
        {
            if (string.IsNullOrWhiteSpace(employee))
            {
                return;
            }

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand(@"
            UPDATE EmployeeSkillMaps
            SET IsDeleted = 1
            WHERE Employee = @Employee
            OR ErpNextId = @Employee",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@Employee", employee);
                    cmd.ExecuteNonQuery();
                }
            }
        }



















        // ============================================================
        // UPSERT
        // ============================================================

        public int InsertOrUpdate(
            EmployeeSkillMap model)
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
                        // FIND BY ERP ID FIRST
                        // =================================================

                        if (!string.IsNullOrWhiteSpace(
                            model.ErpNextId))
                        {
                            using (var cmd =
                                new SqlCommand(@"
                                    SELECT TOP 1 Id
                                    FROM EmployeeSkillMaps
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
                        // FALLBACK BY EMPLOYEE
                        //
                        // ERP Employee Skill Map employee is unique.
                        // =================================================

                        if (
                            localId == 0 &&
                            !string.IsNullOrWhiteSpace(
                                model.Employee)
                        )
                        {
                            using (var cmd =
                                new SqlCommand(@"
                                    SELECT TOP 1 Id
                                    FROM EmployeeSkillMaps
                                    WHERE Employee = @Employee
                                    AND IsDeleted = 0",
                                    conn,
                                    transaction))
                            {
                                cmd.Parameters.AddWithValue(
                                    "@Employee",
                                    model.Employee
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
                        // UPDATE HEADER
                        // =================================================

                        if (localId > 0)
                        {
                            using (var cmd =
                                new SqlCommand(@"
                                    UPDATE EmployeeSkillMaps
                                    SET
                                        ErpNextId = @ErpNextId,
                                        Employee = @Employee,
                                        EmployeeName = @EmployeeName,
                                        Designation = @Designation,
                                        IsDeleted = 0
                                    WHERE Id = @Id",
                                    conn,
                                    transaction))
                            {
                                AddHeaderParameters(
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
                                    DELETE FROM EmployeeSkillMapSkills
                                    WHERE EmployeeSkillMapId = @Id",
                                    conn,
                                    transaction))
                            {
                                cmd.Parameters.AddWithValue(
                                    "@Id",
                                    localId
                                );

                                cmd.ExecuteNonQuery();
                            }


                            using (var cmd =
                                new SqlCommand(@"
                                    DELETE FROM EmployeeSkillMapTrainings
                                    WHERE EmployeeSkillMapId = @Id",
                                    conn,
                                    transaction))
                            {
                                cmd.Parameters.AddWithValue(
                                    "@Id",
                                    localId
                                );

                                cmd.ExecuteNonQuery();
                            }
                        }

                        // =================================================
                        // INSERT HEADER
                        // =================================================

                        else
                        {
                            using (var cmd =
                                new SqlCommand(@"
                                    INSERT INTO EmployeeSkillMaps
                                    (
                                        ErpNextId,
                                        Employee,
                                        EmployeeName,
                                        Designation,
                                        IsDeleted,
                                        CreatedOn
                                    )
                                    OUTPUT INSERTED.Id
                                    VALUES
                                    (
                                        @ErpNextId,
                                        @Employee,
                                        @EmployeeName,
                                        @Designation,
                                        0,
                                        GETDATE()
                                    )",
                                    conn,
                                    transaction))
                            {
                                AddHeaderParameters(
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
                        // SKILLS
                        // =================================================

                        if (model.Skills != null)
                        {
                            foreach (
                                var skill
                                in model.Skills)
                            {
                                if (
                                    skill == null ||
                                    string.IsNullOrWhiteSpace(
                                        skill.Skill)
                                )
                                {
                                    continue;
                                }


                                using (var cmd =
                                    new SqlCommand(@"
                                        INSERT INTO EmployeeSkillMapSkills
                                        (
                                            EmployeeSkillMapId,
                                            Skill,
                                            Proficiency,
                                            EvaluationDate
                                        )
                                        VALUES
                                        (
                                            @EmployeeSkillMapId,
                                            @Skill,
                                            @Proficiency,
                                            @EvaluationDate
                                        )",
                                        conn,
                                        transaction))
                                {
                                    cmd.Parameters.AddWithValue(
                                        "@EmployeeSkillMapId",
                                        localId
                                    );


                                    cmd.Parameters.AddWithValue(
                                        "@Skill",
                                        skill.Skill
                                    );


                                    cmd.Parameters.AddWithValue(
                                        "@Proficiency",
                                        skill.Proficiency
                                    );


                                    cmd.Parameters.AddWithValue(
                                        "@EvaluationDate",
                                        skill.EvaluationDate.HasValue
                                            ? (object)skill.EvaluationDate.Value
                                            : DBNull.Value
                                    );


                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }


                        // =================================================
                        // TRAININGS
                        // =================================================

                        if (model.Trainings != null)
                        {
                            foreach (
                                var training
                                in model.Trainings)
                            {
                                if (
                                    training == null ||
                                    string.IsNullOrWhiteSpace(
                                        training.Training)
                                )
                                {
                                    continue;
                                }


                                using (var cmd =
                                    new SqlCommand(@"
                                        INSERT INTO EmployeeSkillMapTrainings
                                        (
                                            EmployeeSkillMapId,
                                            Training,
                                            TrainingDate
                                        )
                                        VALUES
                                        (
                                            @EmployeeSkillMapId,
                                            @Training,
                                            @TrainingDate
                                        )",
                                        conn,
                                        transaction))
                                {
                                    cmd.Parameters.AddWithValue(
                                        "@EmployeeSkillMapId",
                                        localId
                                    );


                                    cmd.Parameters.AddWithValue(
                                        "@Training",
                                        training.Training
                                    );


                                    cmd.Parameters.AddWithValue(
                                        "@TrainingDate",
                                        training.TrainingDate.HasValue
                                            ? (object)training.TrainingDate.Value
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


        // ============================================================
        // OLD COMPATIBILITY INSERT
        // ============================================================

        public int Insert(
            EmployeeSkillMap model,
            List<string> skillNames,
            List<string> proficiencies,
            List<DateTime?> evalDates,
            List<string> trainingNames,
            List<DateTime?> trainingDates)
        {
            model.Skills =
                new List<EmployeeSkillMapSkill>();


            model.Trainings =
                new List<EmployeeSkillMapTraining>();


            if (skillNames != null)
            {
                for (
                    int i = 0;
                    i < skillNames.Count;
                    i++)
                {
                    if (string.IsNullOrWhiteSpace(
                        skillNames[i]))
                    {
                        continue;
                    }


                    decimal proficiency = 0;


                    if (
                        proficiencies != null &&
                        i < proficiencies.Count
                    )
                    {
                        decimal.TryParse(
                            proficiencies[i],
                            out proficiency
                        );
                    }


                    model.Skills.Add(
                        new EmployeeSkillMapSkill
                        {
                            Skill =
                                skillNames[i],

                            Proficiency =
                                proficiency,

                            EvaluationDate =
                                evalDates != null &&
                                i < evalDates.Count
                                    ? evalDates[i]
                                    : null
                        }
                    );
                }
            }


            if (trainingNames != null)
            {
                for (
                    int i = 0;
                    i < trainingNames.Count;
                    i++)
                {
                    if (string.IsNullOrWhiteSpace(
                        trainingNames[i]))
                    {
                        continue;
                    }


                    model.Trainings.Add(
                        new EmployeeSkillMapTraining
                        {
                            Training =
                                trainingNames[i],

                            TrainingDate =
                                trainingDates != null &&
                                i < trainingDates.Count
                                    ? trainingDates[i]
                                    : null
                        }
                    );
                }
            }


            return InsertOrUpdate(
                model
            );
        }


        private static void AddHeaderParameters(
            SqlCommand cmd,
            EmployeeSkillMap model)
        {
            cmd.Parameters.AddWithValue(
                "@ErpNextId",
                DbValue(
                    model.ErpNextId
                )
            );


            cmd.Parameters.AddWithValue(
                "@Employee",
                DbValue(
                    model.Employee
                )
            );


            cmd.Parameters.AddWithValue(
                "@EmployeeName",
                DbValue(
                    model.EmployeeName
                )
            );


            cmd.Parameters.AddWithValue(
                "@Designation",
                DbValue(
                    model.Designation
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
    }
}