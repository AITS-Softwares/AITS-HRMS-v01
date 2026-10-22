//using System;
//using System.Collections.Generic;
//using Microsoft.Data.SqlClient;
//using Erp_Aits.Models;

//namespace Erp_Aits.Services
//{
//    public class EmployeePerformanceFeedbackService
//    {
//        private readonly string _connectionString;

//        public EmployeePerformanceFeedbackService(string connectionString)
//        {
//            _connectionString = connectionString;
//        }

//        public List<EmployeePerformanceFeedback> GetAll(string idFilter, string forEmployee, string employeeName, string reviewer, string appraisalCycle)
//        {
//            var list = new List<EmployeePerformanceFeedback>();

//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = "SELECT * FROM EmployeePerformanceFeedbacks WHERE IsDeleted = 0";

//                if (!string.IsNullOrEmpty(idFilter)) query += " AND CAST(Id AS VARCHAR) LIKE @IdFilter";
//                if (!string.IsNullOrEmpty(forEmployee)) query += " AND ForEmployee LIKE @ForEmployee";
//                if (!string.IsNullOrEmpty(employeeName)) query += " AND EmployeeName LIKE @EmployeeName";
//                if (!string.IsNullOrEmpty(reviewer)) query += " AND Reviewer LIKE @Reviewer";
//                if (!string.IsNullOrEmpty(appraisalCycle)) query += " AND AppraisalCycle LIKE @AppraisalCycle";

//                query += " ORDER BY Id DESC";

//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    if (!string.IsNullOrEmpty(idFilter)) cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");
//                    if (!string.IsNullOrEmpty(forEmployee)) cmd.Parameters.AddWithValue("@ForEmployee", "%" + forEmployee + "%");
//                    if (!string.IsNullOrEmpty(employeeName)) cmd.Parameters.AddWithValue("@EmployeeName", "%" + employeeName + "%");
//                    if (!string.IsNullOrEmpty(reviewer)) cmd.Parameters.AddWithValue("@Reviewer", "%" + reviewer + "%");
//                    if (!string.IsNullOrEmpty(appraisalCycle)) cmd.Parameters.AddWithValue("@AppraisalCycle", "%" + appraisalCycle + "%");

//                    conn.Open();
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            list.Add(MapRow(reader));
//                        }
//                    }
//                }
//            }

//            return list;
//        }

//        public EmployeePerformanceFeedback GetById(int id)
//        {
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = "SELECT * FROM EmployeePerformanceFeedbacks WHERE Id = @Id AND IsDeleted = 0";
//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    cmd.Parameters.AddWithValue("@Id", id);
//                    conn.Open();
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        if (reader.Read())
//                            return MapRow(reader);
//                    }
//                }
//            }
//            return null;
//        }

//        private EmployeePerformanceFeedback MapRow(System.Data.Common.DbDataReader reader)
//        {
//            return new EmployeePerformanceFeedback
//            {
//                Id = Convert.ToInt32(reader["Id"]),
//                ForEmployee = reader["ForEmployee"]?.ToString() ?? string.Empty,
//                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
//                Reviewer = reader["Reviewer"]?.ToString() ?? string.Empty,
//                Company = reader["Company"]?.ToString() ?? string.Empty,
//                AddedOn = Convert.ToDateTime(reader["AddedOn"]),
//                AppraisalCycle = reader["AppraisalCycle"]?.ToString() ?? string.Empty,
//                Feedback = reader["Feedback"]?.ToString() ?? string.Empty
//            };
//        }

//        public int Insert(EmployeePerformanceFeedback f)
//        {
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = @"
//                    INSERT INTO EmployeePerformanceFeedbacks
//                        (ForEmployee, EmployeeName, Reviewer, Company, AddedOn, AppraisalCycle, Feedback)
//                    OUTPUT INSERTED.Id
//                    VALUES
//                        (@ForEmployee, @EmployeeName, @Reviewer, @Company, @AddedOn, @AppraisalCycle, @Feedback)";

//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    cmd.Parameters.AddWithValue("@ForEmployee", f.ForEmployee ?? (object)DBNull.Value);
//                    cmd.Parameters.AddWithValue("@EmployeeName", f.EmployeeName ?? (object)DBNull.Value);
//                    cmd.Parameters.AddWithValue("@Reviewer", f.Reviewer ?? (object)DBNull.Value);
//                    cmd.Parameters.AddWithValue("@Company", f.Company ?? (object)DBNull.Value);
//                    cmd.Parameters.AddWithValue("@AddedOn", f.AddedOn);
//                    cmd.Parameters.AddWithValue("@AppraisalCycle", f.AppraisalCycle ?? (object)DBNull.Value);
//                    cmd.Parameters.AddWithValue("@Feedback", f.Feedback ?? (object)DBNull.Value);

//                    conn.Open();
//                    int newId = (int)cmd.ExecuteScalar();
//                    return newId;
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
    public class EmployeePerformanceFeedbackService
    {
        private readonly string _connectionString;

        public EmployeePerformanceFeedbackService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<EmployeePerformanceFeedback> GetAll(string idFilter = null, string forEmployee = null, string employeeName = null, string reviewer = null, string appraisal = null)
        {
            var list = new List<EmployeePerformanceFeedback>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM EmployeePerformanceFeedbacks WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(idFilter)) query += " AND CAST(Id AS VARCHAR) LIKE @IdFilter";
                if (!string.IsNullOrEmpty(forEmployee)) query += " AND ForEmployee LIKE @ForEmployee";
                if (!string.IsNullOrEmpty(employeeName)) query += " AND EmployeeName LIKE @EmployeeName";
                if (!string.IsNullOrEmpty(reviewer)) query += " AND Reviewer LIKE @Reviewer";
                if (!string.IsNullOrEmpty(appraisal)) query += " AND Appraisal LIKE @Appraisal";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(idFilter)) cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");
                    if (!string.IsNullOrEmpty(forEmployee)) cmd.Parameters.AddWithValue("@ForEmployee", "%" + forEmployee + "%");
                    if (!string.IsNullOrEmpty(employeeName)) cmd.Parameters.AddWithValue("@EmployeeName", "%" + employeeName + "%");
                    if (!string.IsNullOrEmpty(reviewer)) cmd.Parameters.AddWithValue("@Reviewer", "%" + reviewer + "%");
                    if (!string.IsNullOrEmpty(appraisal)) cmd.Parameters.AddWithValue("@Appraisal", "%" + appraisal + "%");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(MapRow(reader));
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
                string query = "SELECT ErpNextId FROM EmployeePerformanceFeedbacks WHERE ErpNextId IS NOT NULL AND ErpNextId != ''";
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

        public List<EmployeePerformanceFeedback> GetUnsynced()
        {
            var list = new List<EmployeePerformanceFeedback>();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM EmployeePerformanceFeedbacks WHERE (ErpNextId IS NULL OR ErpNextId = '') AND IsDeleted = 0";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapRow(reader));
                    }
                }

                foreach (var item in list)
                {
                    item.FeedbackRatings = GetRatingsByFeedbackId(item.Id, conn);
                }
            }

            return list;
        }

        public void UpdateErpNextId(int localId, string erpNextId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("UPDATE EmployeePerformanceFeedbacks SET ErpNextId = @ErpNextId WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", erpNextId);
                    cmd.Parameters.AddWithValue("@Id", localId);
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
                    UPDATE EmployeePerformanceFeedbacks
                    SET IsDeleted = 1
                    WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public EmployeePerformanceFeedback GetById(int id)
        {
            EmployeePerformanceFeedback model = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM EmployeePerformanceFeedbacks WHERE Id = @Id AND IsDeleted = 0";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model = MapRow(reader);
                        }
                    }
                }

                if (model != null)
                {
                    model.FeedbackRatings = GetRatingsByFeedbackId(model.Id, conn);
                }
            }

            return model;
        }

        public int Insert(EmployeePerformanceFeedback f)
        {
            int newId;

            EnsureReviewerNameColumn();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    INSERT INTO EmployeePerformanceFeedbacks
                        (ErpNextId, ForEmployee, EmployeeName, Reviewer, ReviewerName, Company, AddedOn, Appraisal, Feedback, TotalScore, IsDeleted)
                    OUTPUT INSERTED.Id
                    VALUES
                        (@ErpNextId, @ForEmployee, @EmployeeName, @Reviewer, @ReviewerName, @Company, @AddedOn, @Appraisal, @Feedback, @TotalScore, 0)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", (object)f.ErpNextId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ForEmployee", f.ForEmployee ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmployeeName", f.EmployeeName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Reviewer", f.Reviewer ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ReviewerName", f.ReviewerName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Company", f.Company ?? "AITS");
                    cmd.Parameters.AddWithValue("@AddedOn", f.AddedOn);
                    cmd.Parameters.AddWithValue("@Appraisal", f.Appraisal ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Feedback", f.Feedback ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@TotalScore", f.TotalScore);

                    conn.Open();
                    newId = (int)cmd.ExecuteScalar();
                }

                // Insert Child Table Entries (Feedback Ratings)
                if (f.FeedbackRatings != null)
                {
                    foreach (var r in f.FeedbackRatings)
                    {
                        if (string.IsNullOrWhiteSpace(r.Criteria)) continue;

                        using (var cmd = new SqlCommand(@"
                            INSERT INTO EmployeePerformanceFeedbackRatings (FeedbackId, Criteria, Weightage, Rating)
                            VALUES (@FeedbackId, @Criteria, @Weightage, @Rating)", conn))
                        {
                            cmd.Parameters.AddWithValue("@FeedbackId", newId);
                            cmd.Parameters.AddWithValue("@Criteria", r.Criteria ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Weightage", r.Weightage);
                            cmd.Parameters.AddWithValue("@Rating", r.Rating);

                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }

            return newId;
        }

        private List<EmployeePerformanceFeedbackRating> GetRatingsByFeedbackId(int feedbackId, SqlConnection conn)
        {
            var ratings = new List<EmployeePerformanceFeedbackRating>();

            using (var cmd = new SqlCommand("SELECT * FROM EmployeePerformanceFeedbackRatings WHERE FeedbackId = @FeedbackId", conn))
            {
                cmd.Parameters.AddWithValue("@FeedbackId", feedbackId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ratings.Add(new EmployeePerformanceFeedbackRating
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            FeedbackId = feedbackId,
                            Criteria = reader["Criteria"]?.ToString() ?? string.Empty,
                            Weightage = reader["Weightage"] != DBNull.Value ? Convert.ToDecimal(reader["Weightage"]) : 0,
                            Rating = reader["Rating"] != DBNull.Value ? Convert.ToDecimal(reader["Rating"]) : 0
                        });
                    }
                }
            }

            return ratings;
        }

        public int InsertOrUpdate(
    EmployeePerformanceFeedback model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(
                    nameof(model)
                );
            }

            EnsureReviewerNameColumn();

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


                        // =====================================================
                        // FIND EXISTING RECORD USING ERP DOCUMENT NAME
                        // =====================================================

                        if (!string.IsNullOrWhiteSpace(
                            model.ErpNextId))
                        {
                            using (var cmd =
                                new SqlCommand(@"
                            SELECT TOP 1 Id
                            FROM EmployeePerformanceFeedbacks
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


                        // =====================================================
                        // UPDATE EXISTING HEADER
                        // =====================================================

                        if (localId > 0)
                        {
                            using (var cmd =
                                new SqlCommand(@"
                            UPDATE EmployeePerformanceFeedbacks
                            SET
                                ForEmployee = @ForEmployee,
                                EmployeeName = @EmployeeName,
                                Reviewer = @Reviewer,
                                ReviewerName = @ReviewerName,
                                Company = @Company,
                                AddedOn = @AddedOn,
                                Appraisal = @Appraisal,
                                AppraisalCycle = @AppraisalCycle,
                                Feedback = @Feedback,
                                TotalScore = @TotalScore,
                                DocStatus = @DocStatus,
                                AmendedFrom = @AmendedFrom,
                                IsDeleted = 0
                            WHERE Id = @Id",
                                    conn,
                                    transaction))
                            {
                                AddFeedbackParameters(
                                    cmd,
                                    model
                                );


                                cmd.Parameters.AddWithValue(
                                    "@Id",
                                    localId
                                );


                                cmd.ExecuteNonQuery();
                            }


                            // Replace ERP child rows with current ERP rows.
                            using (var cmd =
                                new SqlCommand(@"
                            DELETE
                            FROM EmployeePerformanceFeedbackRatings
                            WHERE FeedbackId = @FeedbackId",
                                    conn,
                                    transaction))
                            {
                                cmd.Parameters.AddWithValue(
                                    "@FeedbackId",
                                    localId
                                );


                                cmd.ExecuteNonQuery();
                            }
                        }

                        // =====================================================
                        // INSERT NEW HEADER
                        // =====================================================

                        else
                        {
                            using (var cmd =
                                new SqlCommand(@"
                            INSERT INTO EmployeePerformanceFeedbacks
                            (
                                ErpNextId,
                                ForEmployee,
                                EmployeeName,
                                Reviewer,
                                ReviewerName,
                                Company,
                                AddedOn,
                                Appraisal,
                                AppraisalCycle,
                                Feedback,
                                TotalScore,
                                DocStatus,
                                AmendedFrom,
                                IsDeleted
                            )
                            OUTPUT INSERTED.Id
                            VALUES
                            (
                                @ErpNextId,
                                @ForEmployee,
                                @EmployeeName,
                                @Reviewer,
                                @ReviewerName,
                                @Company,
                                @AddedOn,
                                @Appraisal,
                                @AppraisalCycle,
                                @Feedback,
                                @TotalScore,
                                @DocStatus,
                                @AmendedFrom,
                                0
                            )",
                                    conn,
                                    transaction))
                            {
                                AddFeedbackParameters(
                                    cmd,
                                    model
                                );


                                localId =
                                    Convert.ToInt32(
                                        cmd.ExecuteScalar()
                                    );
                            }
                        }


                        // =====================================================
                        // CHILD TABLE
                        // =====================================================

                        if (model.FeedbackRatings != null)
                        {
                            foreach (
                                var rating
                                in model.FeedbackRatings)
                            {
                                if (
                                    rating == null ||
                                    string.IsNullOrWhiteSpace(
                                        rating.Criteria)
                                )
                                {
                                    continue;
                                }


                                using (var cmd =
                                    new SqlCommand(@"
                                INSERT INTO EmployeePerformanceFeedbackRatings
                                (
                                    FeedbackId,
                                    Criteria,
                                    Weightage,
                                    Rating
                                )
                                VALUES
                                (
                                    @FeedbackId,
                                    @Criteria,
                                    @Weightage,
                                    @Rating
                                )",
                                        conn,
                                        transaction))
                                {
                                    cmd.Parameters.AddWithValue(
                                        "@FeedbackId",
                                        localId
                                    );

                                    cmd.Parameters.AddWithValue(
                                        "@Criteria",
                                        rating.Criteria
                                    );

                                    cmd.Parameters.AddWithValue(
                                        "@Weightage",
                                        rating.Weightage
                                    );

                                    cmd.Parameters.AddWithValue(
                                        "@Rating",
                                        rating.Rating
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

        private static void AddFeedbackParameters(
    SqlCommand cmd,
    EmployeePerformanceFeedback model)
        {
            cmd.Parameters.AddWithValue(
                "@ErpNextId",
                DbValue(model.ErpNextId)
            );

            cmd.Parameters.AddWithValue(
                "@ForEmployee",
                DbValue(model.ForEmployee)
            );

            cmd.Parameters.AddWithValue(
                "@EmployeeName",
                DbValue(model.EmployeeName)
            );

            cmd.Parameters.AddWithValue(
                "@Reviewer",
                DbValue(model.Reviewer)
            );

            cmd.Parameters.AddWithValue(
                "@ReviewerName",
                DbValue(model.ReviewerName)
            );

            cmd.Parameters.AddWithValue(
                "@Company",
                DbValue(model.Company)
            );

            cmd.Parameters.AddWithValue(
                "@AddedOn",
                model.AddedOn
            );

            cmd.Parameters.AddWithValue(
                "@Appraisal",
                DbValue(model.Appraisal)
            );

            cmd.Parameters.AddWithValue(
                "@AppraisalCycle",
                DbValue(model.AppraisalCycle)
            );

            cmd.Parameters.AddWithValue(
                "@Feedback",
                DbValue(model.Feedback)
            );

            cmd.Parameters.AddWithValue(
                "@TotalScore",
                model.TotalScore
            );

            cmd.Parameters.AddWithValue(
                "@DocStatus",
                model.DocStatus
            );

            cmd.Parameters.AddWithValue(
                "@AmendedFrom",
                DbValue(model.AmendedFrom)
            );
        }

        private void EnsureReviewerNameColumn()
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(@"
                IF COL_LENGTH(N'dbo.EmployeePerformanceFeedbacks', N'ReviewerName') IS NULL
                BEGIN
                    ALTER TABLE dbo.EmployeePerformanceFeedbacks
                    ADD ReviewerName NVARCHAR(255) NULL;
                END", conn))
            {
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private static object DbValue(
            string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? DBNull.Value
                : value;
        }

        public EmployeePerformanceFeedback GetByErpNextId(string erpNextId)
        {
            if (string.IsNullOrWhiteSpace(erpNextId))
                return null;

            int localId = 0;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand(@"
            SELECT TOP 1 Id
            FROM EmployeePerformanceFeedbacks
            WHERE ErpNextId = @ErpNextId
              AND IsDeleted = 0",
                    conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@ErpNextId",
                        erpNextId
                    );

                    var result = cmd.ExecuteScalar();

                    if (result != null &&
                        result != DBNull.Value)
                    {
                        localId = Convert.ToInt32(result);
                    }
                }
            }

            return localId > 0
                ? GetById(localId)
                : null;
        }
        private EmployeePerformanceFeedback MapRow(System.Data.Common.DbDataReader reader)
        {
            bool HasColumn(System.Data.Common.DbDataReader r, string columnName)
            {
                for (int i = 0; i < r.FieldCount; i++)
                {
                    if (r.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                return false;
            }

            string GetAppraisalValue()
            {
                if (HasColumn(reader, "Appraisal") && reader["Appraisal"] != DBNull.Value)
                    return reader["Appraisal"].ToString();
                if (HasColumn(reader, "AppraisalCycle") && reader["AppraisalCycle"] != DBNull.Value)
                    return reader["AppraisalCycle"].ToString();
                return string.Empty;
            }

            return new EmployeePerformanceFeedback
            {
                Id = Convert.ToInt32(reader["Id"]),
                ErpNextId = (HasColumn(reader, "ErpNextId") && reader["ErpNextId"] != DBNull.Value) ? reader["ErpNextId"].ToString() : null,
                ForEmployee = reader["ForEmployee"]?.ToString() ?? string.Empty,
                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
                Reviewer = reader["Reviewer"]?.ToString() ?? string.Empty,
                ReviewerName = (HasColumn(reader, "ReviewerName") && reader["ReviewerName"] != DBNull.Value) ? reader["ReviewerName"].ToString() : string.Empty,
                Company = reader["Company"]?.ToString() ?? "AITS",
                AddedOn = Convert.ToDateTime(reader["AddedOn"]),
                AppraisalCycle = (HasColumn(reader, "AppraisalCycle") && reader["AppraisalCycle"] != DBNull.Value) ? reader["AppraisalCycle"].ToString() : string.Empty,
                Appraisal = GetAppraisalValue(),
                Feedback = reader["Feedback"]?.ToString() ?? string.Empty,
                TotalScore = (HasColumn(reader, "TotalScore") && reader["TotalScore"] != DBNull.Value) ? Convert.ToDecimal(reader["TotalScore"]) : 0,
                DocStatus = (HasColumn(reader, "DocStatus") && reader["DocStatus"] != DBNull.Value) ? Convert.ToInt32(reader["DocStatus"]) : 0,
                AmendedFrom = (HasColumn(reader, "AmendedFrom") && reader["AmendedFrom"] != DBNull.Value) ? reader["AmendedFrom"].ToString() : string.Empty
            };
        }
    }
}
