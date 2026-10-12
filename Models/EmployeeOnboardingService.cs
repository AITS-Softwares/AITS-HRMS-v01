//using System;
//using System.Collections.Generic;
//using Microsoft.Data.SqlClient;
//using Erp_Aits.Models;

//namespace Erp_Aits.Services
//{
//    public class EmployeeOnboardingService
//    {
//        private readonly string _connectionString;

//        public EmployeeOnboardingService(string connectionString)
//        {
//            _connectionString = connectionString;
//        }

//        public List<EmployeeOnboarding> GetAll(string id = null, string employeeName = null)
//        {
//            var list = new List<EmployeeOnboarding>();
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = @"SELECT Id, JobApplicant, JobOffer, EmployeeOnboardingTemplate, Company, BoardingStatus,
//                                         EmployeeName, DateOfJoining, Department, OnboardingBeginsOn, Designation, EmployeeGrade, HolidayList, NotifyUsersByEmail
//                                  FROM EmployeeOnboardings WHERE IsDeleted = 0";

//                if (!string.IsNullOrEmpty(id)) query += " AND CAST(Id AS VARCHAR) LIKE @Id";
//                if (!string.IsNullOrEmpty(employeeName)) query += " AND EmployeeName LIKE @EmployeeName";

//                query += " ORDER BY Id DESC";

//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    if (!string.IsNullOrEmpty(id)) cmd.Parameters.AddWithValue("@Id", "%" + id + "%");
//                    if (!string.IsNullOrEmpty(employeeName)) cmd.Parameters.AddWithValue("@EmployeeName", "%" + employeeName + "%");

//                    conn.Open();
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            list.Add(Map(reader));
//                        }
//                    }
//                }
//            }
//            return list;
//        }

//        public EmployeeOnboarding GetById(int id)
//        {
//            EmployeeOnboarding model = null;
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                conn.Open();

//                string query = @"SELECT Id, JobApplicant, JobOffer, EmployeeOnboardingTemplate, Company, BoardingStatus,
//                                         EmployeeName, DateOfJoining, Department, OnboardingBeginsOn, Designation, EmployeeGrade, HolidayList, NotifyUsersByEmail
//                                  FROM EmployeeOnboardings WHERE Id = @Id AND IsDeleted = 0";
//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    cmd.Parameters.AddWithValue("@Id", id);
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        if (reader.Read())
//                            model = Map(reader);
//                    }
//                }

//                if (model == null) return null;

//                string actQuery = @"SELECT Id, EmployeeOnboardingId, ActivityName, [User], BeginOnDays, DurationDays
//                                     FROM EmployeeOnboardingActivities WHERE EmployeeOnboardingId = @Id ORDER BY RowNo";
//                using (var cmd = new SqlCommand(actQuery, conn))
//                {
//                    cmd.Parameters.AddWithValue("@Id", id);
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            model.Activities.Add(new EmployeeOnboardingActivity
//                            {
//                                Id = Convert.ToInt32(reader["Id"]),
//                                EmployeeOnboardingId = Convert.ToInt32(reader["EmployeeOnboardingId"]),
//                                ActivityName = reader["ActivityName"]?.ToString(),
//                                User = reader["User"] == DBNull.Value ? null : reader["User"].ToString(),
//                                BeginOnDays = reader["BeginOnDays"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["BeginOnDays"]),
//                                DurationDays = reader["DurationDays"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["DurationDays"])
//                            });
//                        }
//                    }
//                }
//            }
//            return model;
//        }

//        public int Insert(EmployeeOnboarding model)
//        {
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                conn.Open();
//                using (var transaction = conn.BeginTransaction())
//                {
//                    try
//                    {
//                        int newId;
//                        string query = @"INSERT INTO EmployeeOnboardings
//                            (JobApplicant, JobOffer, EmployeeOnboardingTemplate, Company, BoardingStatus,
//                             EmployeeName, DateOfJoining, Department, OnboardingBeginsOn, Designation, EmployeeGrade, HolidayList, NotifyUsersByEmail)
//                            OUTPUT INSERTED.Id
//                            VALUES
//                            (@JobApplicant, @JobOffer, @EmployeeOnboardingTemplate, @Company, @BoardingStatus,
//                             @EmployeeName, @DateOfJoining, @Department, @OnboardingBeginsOn, @Designation, @EmployeeGrade, @HolidayList, @NotifyUsersByEmail)";

//                        using (var cmd = new SqlCommand(query, conn, transaction))
//                        {
//                            cmd.Parameters.AddWithValue("@JobApplicant", model.JobApplicant ?? string.Empty);
//                            cmd.Parameters.AddWithValue("@JobOffer", model.JobOffer ?? string.Empty);
//                            cmd.Parameters.AddWithValue("@EmployeeOnboardingTemplate", (object)model.EmployeeOnboardingTemplate ?? DBNull.Value);
//                            cmd.Parameters.AddWithValue("@Company", model.Company ?? "AITS");
//                            cmd.Parameters.AddWithValue("@BoardingStatus", model.BoardingStatus ?? "Pending");
//                            cmd.Parameters.AddWithValue("@EmployeeName", model.EmployeeName ?? string.Empty);
//                            cmd.Parameters.AddWithValue("@DateOfJoining", model.DateOfJoining);
//                            cmd.Parameters.AddWithValue("@Department", (object)model.Department ?? DBNull.Value);
//                            cmd.Parameters.AddWithValue("@OnboardingBeginsOn", model.OnboardingBeginsOn);
//                            cmd.Parameters.AddWithValue("@Designation", (object)model.Designation ?? DBNull.Value);
//                            cmd.Parameters.AddWithValue("@EmployeeGrade", (object)model.EmployeeGrade ?? DBNull.Value);
//                            cmd.Parameters.AddWithValue("@HolidayList", (object)model.HolidayList ?? DBNull.Value);
//                            cmd.Parameters.AddWithValue("@NotifyUsersByEmail", model.NotifyUsersByEmail);

//                            newId = (int)cmd.ExecuteScalar();
//                        }

//                        int rowNo = 1;
//                        foreach (var a in model.Activities)
//                        {
//                            string aq = @"INSERT INTO EmployeeOnboardingActivities (EmployeeOnboardingId, ActivityName, [User], BeginOnDays, DurationDays, RowNo)
//                                          VALUES (@EmployeeOnboardingId, @ActivityName, @User, @BeginOnDays, @DurationDays, @RowNo)";
//                            using (var cmd = new SqlCommand(aq, conn, transaction))
//                            {
//                                cmd.Parameters.AddWithValue("@EmployeeOnboardingId", newId);
//                                cmd.Parameters.AddWithValue("@ActivityName", a.ActivityName ?? string.Empty);
//                                cmd.Parameters.AddWithValue("@User", (object)a.User ?? DBNull.Value);
//                                cmd.Parameters.AddWithValue("@BeginOnDays", (object)a.BeginOnDays ?? DBNull.Value);
//                                cmd.Parameters.AddWithValue("@DurationDays", (object)a.DurationDays ?? DBNull.Value);
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

//        private EmployeeOnboarding Map(SqlDataReader reader)
//        {
//            return new EmployeeOnboarding
//            {
//                Id = Convert.ToInt32(reader["Id"]),
//                JobApplicant = reader["JobApplicant"]?.ToString(),
//                JobOffer = reader["JobOffer"]?.ToString(),
//                EmployeeOnboardingTemplate = reader["EmployeeOnboardingTemplate"] == DBNull.Value ? null : reader["EmployeeOnboardingTemplate"].ToString(),
//                Company = reader["Company"]?.ToString(),
//                BoardingStatus = reader["BoardingStatus"]?.ToString(),
//                EmployeeName = reader["EmployeeName"]?.ToString(),
//                DateOfJoining = Convert.ToDateTime(reader["DateOfJoining"]),
//                Department = reader["Department"] == DBNull.Value ? null : reader["Department"].ToString(),
//                OnboardingBeginsOn = Convert.ToDateTime(reader["OnboardingBeginsOn"]),
//                Designation = reader["Designation"] == DBNull.Value ? null : reader["Designation"].ToString(),
//                EmployeeGrade = reader["EmployeeGrade"] == DBNull.Value ? null : reader["EmployeeGrade"].ToString(),
//                HolidayList = reader["HolidayList"] == DBNull.Value ? null : reader["HolidayList"].ToString(),
//                NotifyUsersByEmail = Convert.ToBoolean(reader["NotifyUsersByEmail"])
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
    public class EmployeeOnboardingService
    {
        private readonly string _connectionString;

        public EmployeeOnboardingService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<EmployeeOnboarding> GetAll(string id = null, string employeeName = null)
        {
            var list = new List<EmployeeOnboarding>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT Id, ErpNextId, JobApplicant, JobOffer, EmployeeOnboardingTemplate, Company, BoardingStatus,
                                        EmployeeName, DateOfJoining, Department, OnboardingBeginsOn, Designation, EmployeeGrade, HolidayList, NotifyUsersByEmail
                                 FROM EmployeeOnboardings WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(id)) query += " AND CAST(Id AS VARCHAR) LIKE @Id";
                if (!string.IsNullOrEmpty(employeeName)) query += " AND EmployeeName LIKE @EmployeeName";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(id)) cmd.Parameters.AddWithValue("@Id", "%" + id + "%");
                    if (!string.IsNullOrEmpty(employeeName)) cmd.Parameters.AddWithValue("@EmployeeName", "%" + employeeName + "%");

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

        public HashSet<string> GetExistingErpNextIds()
        {
            var ids = new HashSet<string>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT ErpNextId FROM EmployeeOnboardings WHERE ErpNextId IS NOT NULL AND ErpNextId != ''";
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ids.Add(reader["ErpNextId"].ToString());
                        }
                    }
                }
            }
            return ids;
        }

        public EmployeeOnboarding GetById(int id)
        {
            EmployeeOnboarding model = null;
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string query = @"SELECT Id, ErpNextId, JobApplicant, JobOffer, EmployeeOnboardingTemplate, Company, BoardingStatus,
                                        EmployeeName, DateOfJoining, Department, OnboardingBeginsOn, Designation, EmployeeGrade, HolidayList, NotifyUsersByEmail
                                 FROM EmployeeOnboardings WHERE Id = @Id AND IsDeleted = 0";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            model = Map(reader);
                    }
                }

                if (model == null) return null;

                string actQuery = @"SELECT Id, EmployeeOnboardingId, ActivityName, [User], BeginOnDays, DurationDays
                                     FROM EmployeeOnboardingActivities WHERE EmployeeOnboardingId = @Id ORDER BY RowNo";
                using (var cmd = new SqlCommand(actQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.Activities.Add(new EmployeeOnboardingActivity
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                EmployeeOnboardingId = Convert.ToInt32(reader["EmployeeOnboardingId"]),
                                ActivityName = reader["ActivityName"]?.ToString(),
                                User = reader["User"] == DBNull.Value ? null : reader["User"].ToString(),
                                BeginOnDays = reader["BeginOnDays"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["BeginOnDays"]),
                                DurationDays = reader["DurationDays"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["DurationDays"])
                            });
                        }
                    }
                }
            }
            return model;
        }

        public int Insert(EmployeeOnboarding model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int newId;
                        string query = @"INSERT INTO EmployeeOnboardings
                            (ErpNextId, JobApplicant, JobOffer, EmployeeOnboardingTemplate, Company, BoardingStatus,
                             EmployeeName, DateOfJoining, Department, OnboardingBeginsOn, Designation, EmployeeGrade, HolidayList, NotifyUsersByEmail, IsDeleted)
                            OUTPUT INSERTED.Id
                            VALUES
                            (@ErpNextId, @JobApplicant, @JobOffer, @EmployeeOnboardingTemplate, @Company, @BoardingStatus,
                             @EmployeeName, @DateOfJoining, @Department, @OnboardingBeginsOn, @Designation, @EmployeeGrade, @HolidayList, @NotifyUsersByEmail, 0)";

                        using (var cmd = new SqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ErpNextId", (object)model.ErpNextId ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@JobApplicant", model.JobApplicant ?? string.Empty);
                            cmd.Parameters.AddWithValue("@JobOffer", model.JobOffer ?? string.Empty);
                            cmd.Parameters.AddWithValue("@EmployeeOnboardingTemplate", (object)model.EmployeeOnboardingTemplate ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Company", model.Company ?? "Vinod Cookware India Private Limited");
                            cmd.Parameters.AddWithValue("@BoardingStatus", model.BoardingStatus ?? "Pending");
                            cmd.Parameters.AddWithValue("@EmployeeName", model.EmployeeName ?? string.Empty);
                            cmd.Parameters.AddWithValue("@DateOfJoining", model.DateOfJoining);
                            cmd.Parameters.AddWithValue("@Department", (object)model.Department ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@OnboardingBeginsOn", model.OnboardingBeginsOn);
                            cmd.Parameters.AddWithValue("@Designation", (object)model.Designation ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@EmployeeGrade", (object)model.EmployeeGrade ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@HolidayList", (object)model.HolidayList ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@NotifyUsersByEmail", model.NotifyUsersByEmail);

                            newId = (int)cmd.ExecuteScalar();
                        }

                        int rowNo = 1;
                        foreach (var a in model.Activities)
                        {
                            string aq = @"INSERT INTO EmployeeOnboardingActivities (EmployeeOnboardingId, ActivityName, [User], BeginOnDays, DurationDays, RowNo)
                                          VALUES (@EmployeeOnboardingId, @ActivityName, @User, @BeginOnDays, @DurationDays, @RowNo)";
                            using (var cmd = new SqlCommand(aq, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@EmployeeOnboardingId", newId);
                                cmd.Parameters.AddWithValue("@ActivityName", a.ActivityName ?? string.Empty);
                                cmd.Parameters.AddWithValue("@User", (object)a.User ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@BeginOnDays", (object)a.BeginOnDays ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@DurationDays", (object)a.DurationDays ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@RowNo", rowNo++);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return newId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }



        public int InsertOrUpdate(EmployeeOnboarding model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int localId = 0;

                        // ====================================================
                        // FIND EXISTING BY ERP ID
                        // ====================================================

                        if (!string.IsNullOrWhiteSpace(model.ErpNextId))
                        {
                            using (var cmd = new SqlCommand(@"
                        SELECT TOP 1 Id
                        FROM EmployeeOnboardings
                        WHERE ErpNextId = @ErpNextId",
                                conn,
                                transaction))
                            {
                                cmd.Parameters.AddWithValue(
                                    "@ErpNextId",
                                    model.ErpNextId
                                );

                                var result = cmd.ExecuteScalar();

                                if (result != null &&
                                    result != DBNull.Value)
                                {
                                    localId =
                                        Convert.ToInt32(result);
                                }
                            }
                        }


                        // ====================================================
                        // UPDATE EXISTING
                        // ====================================================

                        if (localId > 0)
                        {
                            using (var cmd = new SqlCommand(@"
                        UPDATE EmployeeOnboardings
                        SET
                            JobApplicant = @JobApplicant,
                            JobOffer = @JobOffer,
                            EmployeeOnboardingTemplate = @EmployeeOnboardingTemplate,
                            Company = @Company,
                            BoardingStatus = @BoardingStatus,
                            Employee = @Employee,
                            EmployeeName = @EmployeeName,
                            DateOfJoining = @DateOfJoining,
                            Department = @Department,
                            OnboardingBeginsOn = @OnboardingBeginsOn,
                            Designation = @Designation,
                            EmployeeGrade = @EmployeeGrade,
                            HolidayList = @HolidayList,
                            Project = @Project,
                            NotifyUsersByEmail = @NotifyUsersByEmail,
                            DocStatus = @DocStatus,
                            AmendedFrom = @AmendedFrom,
                            IsDeleted = 0
                        WHERE Id = @Id",
                                conn,
                                transaction))
                            {
                                AddParameters(cmd, model);

                                cmd.Parameters.AddWithValue(
                                    "@Id",
                                    localId
                                );

                                cmd.ExecuteNonQuery();
                            }


                            using (var cmd = new SqlCommand(@"
                        DELETE FROM EmployeeOnboardingActivities
                        WHERE EmployeeOnboardingId = @Id",
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

                        // ====================================================
                        // INSERT NEW
                        // ====================================================

                        else
                        {
                            using (var cmd = new SqlCommand(@"
                        INSERT INTO EmployeeOnboardings
                        (
                            ErpNextId,
                            JobApplicant,
                            JobOffer,
                            EmployeeOnboardingTemplate,
                            Company,
                            BoardingStatus,
                            Employee,
                            EmployeeName,
                            DateOfJoining,
                            Department,
                            OnboardingBeginsOn,
                            Designation,
                            EmployeeGrade,
                            HolidayList,
                            Project,
                            NotifyUsersByEmail,
                            DocStatus,
                            AmendedFrom,
                            IsDeleted
                        )
                        OUTPUT INSERTED.Id
                        VALUES
                        (
                            @ErpNextId,
                            @JobApplicant,
                            @JobOffer,
                            @EmployeeOnboardingTemplate,
                            @Company,
                            @BoardingStatus,
                            @Employee,
                            @EmployeeName,
                            @DateOfJoining,
                            @Department,
                            @OnboardingBeginsOn,
                            @Designation,
                            @EmployeeGrade,
                            @HolidayList,
                            @Project,
                            @NotifyUsersByEmail,
                            @DocStatus,
                            @AmendedFrom,
                            0
                        )",
                                conn,
                                transaction))
                            {
                                AddParameters(cmd, model);

                                localId =
                                    Convert.ToInt32(
                                        cmd.ExecuteScalar()
                                    );
                            }
                        }


                        // ====================================================
                        // INSERT ACTIVITIES
                        // ====================================================

                        int rowNo = 1;

                        if (model.Activities != null)
                        {
                            foreach (var activity in model.Activities)
                            {
                                if (activity == null ||
                                    string.IsNullOrWhiteSpace(
                                        activity.ActivityName))
                                {
                                    continue;
                                }

                                using (var cmd = new SqlCommand(@"
                            INSERT INTO EmployeeOnboardingActivities
                            (
                                EmployeeOnboardingId,
                                ActivityName,
                                [User],
                                BeginOnDays,
                                DurationDays,
                                RowNo
                            )
                            VALUES
                            (
                                @EmployeeOnboardingId,
                                @ActivityName,
                                @User,
                                @BeginOnDays,
                                @DurationDays,
                                @RowNo
                            )",
                                    conn,
                                    transaction))
                                {
                                    cmd.Parameters.AddWithValue(
                                        "@EmployeeOnboardingId",
                                        localId
                                    );

                                    cmd.Parameters.AddWithValue(
                                        "@ActivityName",
                                        activity.ActivityName
                                    );

                                    cmd.Parameters.AddWithValue(
                                        "@User",
                                        (object)activity.User
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

                                    cmd.Parameters.AddWithValue(
                                        "@RowNo",
                                        rowNo++
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

        private static void AddParameters(
    SqlCommand cmd,
    EmployeeOnboarding model)
        {
            cmd.Parameters.AddWithValue(
                "@ErpNextId",
                (object)model.ErpNextId
                ?? DBNull.Value
            );

            cmd.Parameters.AddWithValue(
                "@JobApplicant",
                (object)model.JobApplicant
                ?? DBNull.Value
            );

            cmd.Parameters.AddWithValue(
                "@JobOffer",
                (object)model.JobOffer
                ?? DBNull.Value
            );

            cmd.Parameters.AddWithValue(
                "@EmployeeOnboardingTemplate",
                (object)model.EmployeeOnboardingTemplate
                ?? DBNull.Value
            );

            cmd.Parameters.AddWithValue(
                "@Company",
                (object)model.Company
                ?? DBNull.Value
            );

            cmd.Parameters.AddWithValue(
                "@BoardingStatus",
                (object)model.BoardingStatus
                ?? "Pending"
            );

            cmd.Parameters.AddWithValue(
                "@Employee",
                (object)model.Employee
                ?? DBNull.Value
            );

            cmd.Parameters.AddWithValue(
                "@EmployeeName",
                (object)model.EmployeeName
                ?? DBNull.Value
            );

            cmd.Parameters.AddWithValue(
                "@DateOfJoining",
                model.DateOfJoining
            );

            cmd.Parameters.AddWithValue(
                "@Department",
                (object)model.Department
                ?? DBNull.Value
            );

            cmd.Parameters.AddWithValue(
                "@OnboardingBeginsOn",
                model.OnboardingBeginsOn
            );

            cmd.Parameters.AddWithValue(
                "@Designation",
                (object)model.Designation
                ?? DBNull.Value
            );

            cmd.Parameters.AddWithValue(
                "@EmployeeGrade",
                (object)model.EmployeeGrade
                ?? DBNull.Value
            );

            cmd.Parameters.AddWithValue(
                "@HolidayList",
                (object)model.HolidayList
                ?? DBNull.Value
            );

            cmd.Parameters.AddWithValue(
                "@Project",
                (object)model.Project
                ?? DBNull.Value
            );

            cmd.Parameters.AddWithValue(
                "@NotifyUsersByEmail",
                model.NotifyUsersByEmail
            );

            cmd.Parameters.AddWithValue(
                "@DocStatus",
                model.DocStatus
            );

            cmd.Parameters.AddWithValue(
                "@AmendedFrom",
                (object)model.AmendedFrom
                ?? DBNull.Value
            );
        }


        public void MarkDeletedById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
            UPDATE EmployeeOnboardings
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


        private EmployeeOnboarding Map(SqlDataReader reader)
        {
            return new EmployeeOnboarding
            {
                Id = Convert.ToInt32(reader["Id"]),
                ErpNextId = reader["ErpNextId"] == DBNull.Value ? null : reader["ErpNextId"].ToString(),
                JobApplicant = reader["JobApplicant"]?.ToString(),
                JobOffer = reader["JobOffer"]?.ToString(),
                EmployeeOnboardingTemplate = reader["EmployeeOnboardingTemplate"] == DBNull.Value ? null : reader["EmployeeOnboardingTemplate"].ToString(),
                Company = reader["Company"]?.ToString(),
                BoardingStatus = reader["BoardingStatus"]?.ToString(),
                EmployeeName = reader["EmployeeName"]?.ToString(),
                DateOfJoining = Convert.ToDateTime(reader["DateOfJoining"]),
                Department = reader["Department"] == DBNull.Value ? null : reader["Department"].ToString(),
                OnboardingBeginsOn = Convert.ToDateTime(reader["OnboardingBeginsOn"]),
                Designation = reader["Designation"] == DBNull.Value ? null : reader["Designation"].ToString(),
                EmployeeGrade = reader["EmployeeGrade"] == DBNull.Value ? null : reader["EmployeeGrade"].ToString(),
                HolidayList = reader["HolidayList"] == DBNull.Value ? null : reader["HolidayList"].ToString(),
                NotifyUsersByEmail = Convert.ToBoolean(reader["NotifyUsersByEmail"])
            };
        }
    }
}