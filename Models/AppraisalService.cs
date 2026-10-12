//using System;
//using System.Collections.Generic;
//using Microsoft.Data.SqlClient;
//using Erp_Aits.Models;

//namespace Erp_Aits.Services
//{
//    public class AppraisalService
//    {
//        private readonly string _connectionString;

//        public AppraisalService(string connectionString)
//        {
//            _connectionString = connectionString;
//        }

//        public List<Appraisal> GetAll(string idFilter, string employee, string employeeName, string appraisalCycle, string appraisalTemplate)
//        {
//            var list = new List<Appraisal>();

//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = "SELECT Id, Employee, EmployeeName, AppraisalCycle, AppraisalTemplate FROM Appraisals WHERE IsDeleted = 0";

//                if (!string.IsNullOrEmpty(idFilter)) query += " AND CAST(Id AS VARCHAR) LIKE @IdFilter";
//                if (!string.IsNullOrEmpty(employee)) query += " AND Employee LIKE @Employee";
//                if (!string.IsNullOrEmpty(employeeName)) query += " AND EmployeeName LIKE @EmployeeName";
//                if (!string.IsNullOrEmpty(appraisalCycle)) query += " AND AppraisalCycle LIKE @AppraisalCycle";
//                if (!string.IsNullOrEmpty(appraisalTemplate)) query += " AND AppraisalTemplate LIKE @AppraisalTemplate";

//                query += " ORDER BY Id DESC";

//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    if (!string.IsNullOrEmpty(idFilter)) cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");
//                    if (!string.IsNullOrEmpty(employee)) cmd.Parameters.AddWithValue("@Employee", "%" + employee + "%");
//                    if (!string.IsNullOrEmpty(employeeName)) cmd.Parameters.AddWithValue("@EmployeeName", "%" + employeeName + "%");
//                    if (!string.IsNullOrEmpty(appraisalCycle)) cmd.Parameters.AddWithValue("@AppraisalCycle", "%" + appraisalCycle + "%");
//                    if (!string.IsNullOrEmpty(appraisalTemplate)) cmd.Parameters.AddWithValue("@AppraisalTemplate", "%" + appraisalTemplate + "%");

//                    conn.Open();
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            list.Add(new Appraisal
//                            {
//                                Id = Convert.ToInt32(reader["Id"]),
//                                Employee = reader["Employee"]?.ToString() ?? string.Empty,
//                                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
//                                AppraisalCycle = reader["AppraisalCycle"]?.ToString() ?? string.Empty,
//                                AppraisalTemplate = reader["AppraisalTemplate"]?.ToString() ?? string.Empty
//                            });
//                        }
//                    }
//                }
//            }

//            return list;
//        }

//        public Appraisal GetById(int id)
//        {
//            Appraisal appraisal = null;

//            using (var conn = new SqlConnection(_connectionString))
//            {
//                conn.Open();

//                using (var cmd = new SqlCommand("SELECT * FROM Appraisals WHERE Id = @Id AND IsDeleted = 0", conn))
//                {
//                    cmd.Parameters.AddWithValue("@Id", id);
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        if (reader.Read())
//                        {
//                            appraisal = new Appraisal
//                            {
//                                Id = Convert.ToInt32(reader["Id"]),
//                                Employee = reader["Employee"]?.ToString() ?? string.Empty,
//                                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
//                                Company = reader["Company"]?.ToString() ?? string.Empty,
//                                AppraisalCycle = reader["AppraisalCycle"]?.ToString() ?? string.Empty,
//                                AppraisalTemplate = reader["AppraisalTemplate"]?.ToString() ?? string.Empty,
//                                RateGoalsManually = Convert.ToBoolean(reader["RateGoalsManually"]),
//                                Feedback = reader["Feedback"]?.ToString() ?? string.Empty,
//                                Reflections = reader["Reflections"]?.ToString() ?? string.Empty
//                            };
//                        }
//                    }
//                }

//                if (appraisal != null)
//                {
//                    using (var cmd = new SqlCommand("SELECT * FROM AppraisalKras WHERE AppraisalId = @Id", conn))
//                    {
//                        cmd.Parameters.AddWithValue("@Id", id);
//                        using (var reader = cmd.ExecuteReader())
//                        {
//                            while (reader.Read())
//                            {
//                                appraisal.Kras.Add(new AppraisalKra
//                                {
//                                    Id = Convert.ToInt32(reader["Id"]),
//                                    AppraisalId = id,
//                                    Kra = reader["Kra"]?.ToString() ?? string.Empty,
//                                    Weightage = Convert.ToDecimal(reader["Weightage"]),
//                                    GoalCompletion = reader["GoalCompletion"] != DBNull.Value ? Convert.ToDecimal(reader["GoalCompletion"]) : (decimal?)null,
//                                    GoalScore = reader["GoalScore"] != DBNull.Value ? Convert.ToDecimal(reader["GoalScore"]) : (decimal?)null
//                                });
//                            }
//                        }
//                    }

//                    using (var cmd = new SqlCommand("SELECT * FROM AppraisalSelfRatings WHERE AppraisalId = @Id", conn))
//                    {
//                        cmd.Parameters.AddWithValue("@Id", id);
//                        using (var reader = cmd.ExecuteReader())
//                        {
//                            while (reader.Read())
//                            {
//                                appraisal.SelfRatings.Add(new AppraisalSelfRating
//                                {
//                                    Id = Convert.ToInt32(reader["Id"]),
//                                    AppraisalId = id,
//                                    Criteria = reader["Criteria"]?.ToString() ?? string.Empty,
//                                    Weightage = Convert.ToDecimal(reader["Weightage"]),
//                                    Rating = Convert.ToInt32(reader["Rating"])
//                                });
//                            }
//                        }
//                    }
//                }
//            }

//            return appraisal;
//        }


//        public void InsertOrUpdate(Appraisal a)
//        {
//            // If you already have an Insert method, you can call it directly or check existence by ErpNextId
//            Insert(a);
//        }


//        public int Insert(Appraisal a)
//        {
//            int newId;

//            using (var conn = new SqlConnection(_connectionString))
//            {
//                conn.Open();

//                using (var cmd = new SqlCommand(@"
//                    INSERT INTO Appraisals
//                        (Employee, EmployeeName, Company, AppraisalCycle, AppraisalTemplate, RateGoalsManually, Feedback, Reflections)
//                    OUTPUT INSERTED.Id
//                    VALUES
//                        (@Employee, @EmployeeName, @Company, @AppraisalCycle, @AppraisalTemplate, @RateGoalsManually, @Feedback, @Reflections)", conn))
//                {
//                    cmd.Parameters.AddWithValue("@Employee", a.Employee ?? (object)DBNull.Value);
//                    cmd.Parameters.AddWithValue("@EmployeeName", a.EmployeeName ?? (object)DBNull.Value);
//                    cmd.Parameters.AddWithValue("@Company", a.Company ?? (object)DBNull.Value);
//                    cmd.Parameters.AddWithValue("@AppraisalCycle", a.AppraisalCycle ?? (object)DBNull.Value);
//                    cmd.Parameters.AddWithValue("@AppraisalTemplate", a.AppraisalTemplate ?? (object)DBNull.Value);
//                    cmd.Parameters.AddWithValue("@RateGoalsManually", a.RateGoalsManually);
//                    cmd.Parameters.AddWithValue("@Feedback", a.Feedback ?? (object)DBNull.Value);
//                    cmd.Parameters.AddWithValue("@Reflections", a.Reflections ?? (object)DBNull.Value);

//                    newId = (int)cmd.ExecuteScalar();
//                }

//                foreach (var kra in a.Kras)
//                {
//                    if (string.IsNullOrWhiteSpace(kra.Kra)) continue;

//                    using (var cmd = new SqlCommand(@"
//                        INSERT INTO AppraisalKras (AppraisalId, Kra, Weightage, GoalCompletion, GoalScore)
//                        VALUES (@AppraisalId, @Kra, @Weightage, @GoalCompletion, @GoalScore)", conn))
//                    {
//                        cmd.Parameters.AddWithValue("@AppraisalId", newId);
//                        cmd.Parameters.AddWithValue("@Kra", kra.Kra ?? (object)DBNull.Value);
//                        cmd.Parameters.AddWithValue("@Weightage", kra.Weightage);
//                        cmd.Parameters.AddWithValue("@GoalCompletion", (object)kra.GoalCompletion ?? DBNull.Value);
//                        cmd.Parameters.AddWithValue("@GoalScore", (object)kra.GoalScore ?? DBNull.Value);

//                        cmd.ExecuteNonQuery();
//                    }
//                }

//                foreach (var rating in a.SelfRatings)
//                {
//                    if (string.IsNullOrWhiteSpace(rating.Criteria)) continue;

//                    using (var cmd = new SqlCommand(@"
//                        INSERT INTO AppraisalSelfRatings (AppraisalId, Criteria, Weightage, Rating)
//                        VALUES (@AppraisalId, @Criteria, @Weightage, @Rating)", conn))
//                    {
//                        cmd.Parameters.AddWithValue("@AppraisalId", newId);
//                        cmd.Parameters.AddWithValue("@Criteria", rating.Criteria ?? (object)DBNull.Value);
//                        cmd.Parameters.AddWithValue("@Weightage", rating.Weightage);
//                        cmd.Parameters.AddWithValue("@Rating", rating.Rating);

//                        cmd.ExecuteNonQuery();
//                    }
//                }
//            }

//            return newId;
//        }
//    }
//}





//using System;
//using System.Collections.Generic;
//using Microsoft.Data.SqlClient;
//using Erp_Aits.Models;

//namespace Erp_Aits.Services
//{
//    public class AppraisalService
//    {
//        private readonly string _connectionString;

//        public AppraisalService(string connectionString)
//        {
//            _connectionString = connectionString;
//        }

//        public List<Appraisal> GetAll(string idFilter, string employee, string employeeName, string appraisalCycle, string appraisalTemplate)
//        {
//            var list = new List<Appraisal>();

//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = "SELECT Id, Employee, EmployeeName, AppraisalCycle, AppraisalTemplate FROM Appraisals WHERE IsDeleted = 0";

//                if (!string.IsNullOrEmpty(idFilter)) query += " AND CAST(Id AS VARCHAR) LIKE @IdFilter";
//                if (!string.IsNullOrEmpty(employee)) query += " AND Employee LIKE @Employee";
//                if (!string.IsNullOrEmpty(employeeName)) query += " AND EmployeeName LIKE @EmployeeName";
//                if (!string.IsNullOrEmpty(appraisalCycle)) query += " AND AppraisalCycle LIKE @AppraisalCycle";
//                if (!string.IsNullOrEmpty(appraisalTemplate)) query += " AND AppraisalTemplate LIKE @AppraisalTemplate";

//                query += " ORDER BY Id DESC";

//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    if (!string.IsNullOrEmpty(idFilter)) cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");
//                    if (!string.IsNullOrEmpty(employee)) cmd.Parameters.AddWithValue("@Employee", "%" + employee + "%");
//                    if (!string.IsNullOrEmpty(employeeName)) cmd.Parameters.AddWithValue("@EmployeeName", "%" + employeeName + "%");
//                    if (!string.IsNullOrEmpty(appraisalCycle)) cmd.Parameters.AddWithValue("@AppraisalCycle", "%" + appraisalCycle + "%");
//                    if (!string.IsNullOrEmpty(appraisalTemplate)) cmd.Parameters.AddWithValue("@AppraisalTemplate", "%" + appraisalTemplate + "%");

//                    conn.Open();
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            list.Add(new Appraisal
//                            {
//                                Id = Convert.ToInt32(reader["Id"]),
//                                Employee = reader["Employee"]?.ToString() ?? string.Empty,
//                                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
//                                AppraisalCycle = reader["AppraisalCycle"]?.ToString() ?? string.Empty,
//                                AppraisalTemplate = reader["AppraisalTemplate"]?.ToString() ?? string.Empty
//                            });
//                        }
//                    }
//                }
//            }

//            return list;
//        }

//        public Appraisal GetById(int id)
//        {
//            Appraisal appraisal = null;

//            using (var conn = new SqlConnection(_connectionString))
//            {
//                conn.Open();

//                using (var cmd = new SqlCommand("SELECT * FROM Appraisals WHERE Id = @Id AND IsDeleted = 0", conn))
//                {
//                    cmd.Parameters.AddWithValue("@Id", id);
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        if (reader.Read())
//                        {
//                            appraisal = new Appraisal
//                            {
//                                Id = Convert.ToInt32(reader["Id"]),
//                                Employee = reader["Employee"]?.ToString() ?? string.Empty,
//                                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
//                                Company = reader["Company"]?.ToString() ?? string.Empty,
//                                AppraisalCycle = reader["AppraisalCycle"]?.ToString() ?? string.Empty,
//                                AppraisalTemplate = reader["AppraisalTemplate"]?.ToString() ?? string.Empty,
//                                RateGoalsManually = Convert.ToBoolean(reader["RateGoalsManually"]),
//                                Feedback = reader["Feedback"]?.ToString() ?? string.Empty,
//                                Reflections = reader["Reflections"]?.ToString() ?? string.Empty,
//                                TotalScore = reader["TotalScore"] != DBNull.Value ? Convert.ToDecimal(reader["TotalScore"]) : 0m,
//                                FinalScore = reader["FinalScore"] != DBNull.Value ? Convert.ToDecimal(reader["FinalScore"]) : 0m,
//                                GoalScorePercentage = reader["GoalScorePercentage"] != DBNull.Value ? Convert.ToDecimal(reader["GoalScorePercentage"]) : 0m,
//                                TotalSelfScore = reader["TotalSelfScore"] != DBNull.Value ? Convert.ToDecimal(reader["TotalSelfScore"]) : 0m
//                            };
//                        }
//                    }
//                }

//                if (appraisal != null)
//                {
//                    using (var cmd = new SqlCommand("SELECT * FROM AppraisalKras WHERE AppraisalId = @Id", conn))
//                    {
//                        cmd.Parameters.AddWithValue("@Id", id);
//                        using (var reader = cmd.ExecuteReader())
//                        {
//                            while (reader.Read())
//                            {
//                                appraisal.Kras.Add(new AppraisalKra
//                                {
//                                    Id = Convert.ToInt32(reader["Id"]),
//                                    AppraisalId = id,
//                                    Kra = reader["Kra"]?.ToString() ?? string.Empty,
//                                    Weightage = reader["Weightage"] != DBNull.Value ? Convert.ToDecimal(reader["Weightage"]) : 0m,
//                                    GoalCompletion = reader["GoalCompletion"] != DBNull.Value ? Convert.ToDecimal(reader["GoalCompletion"]) : 0m,
//                                    GoalScore = reader["GoalScore"] != DBNull.Value ? Convert.ToDecimal(reader["GoalScore"]) : 0m
//                                });
//                            }
//                        }
//                    }

//                    using (var cmd = new SqlCommand("SELECT * FROM AppraisalSelfRatings WHERE AppraisalId = @Id", conn))
//                    {
//                        cmd.Parameters.AddWithValue("@Id", id);
//                        using (var reader = cmd.ExecuteReader())
//                        {
//                            while (reader.Read())
//                            {
//                                appraisal.SelfRatings.Add(new AppraisalSelfRating
//                                {
//                                    Id = Convert.ToInt32(reader["Id"]),
//                                    AppraisalId = id,
//                                    Criteria = reader["Criteria"]?.ToString() ?? string.Empty,
//                                    Weightage = reader["Weightage"] != DBNull.Value ? Convert.ToDecimal(reader["Weightage"]) : 0m,
//                                    Rating = reader["Rating"] != DBNull.Value ? Convert.ToDecimal(reader["Rating"]) : 0m,
//                                    Reflections = reader["Reflections"]?.ToString() ?? string.Empty
//                                });
//                            }
//                        }
//                    }
//                }
//            }

//            return appraisal;
//        }



//        public void SyncRecordFromErpNext(string erpId, string employee, string employeeName, string appraisalCycle, string appraisalTemplate)
//        {
//            using (var connection = new SqlConnection(_connectionString))
//            {
//                connection.Open();

//                // 1. Check if record already exists by ErpNextId using standard SqlCommand
//                string checkQuery = "SELECT COUNT(1) FROM Appraisals WHERE ErpNextId = @ErpNextId";
//                int count = 0;

//                using (var checkCmd = new SqlCommand(checkQuery, connection))
//                {
//                    checkCmd.Parameters.AddWithValue("@ErpNextId", (object)erpId ?? DBNull.Value);
//                    count = Convert.ToInt32(checkCmd.ExecuteScalar());
//                }

//                // 2. Insert only if it does not exist
//                if (count == 0)
//                {
//                    string insertQuery = @"INSERT INTO Appraisals (ErpNextId, Employee, EmployeeName, AppraisalCycle, AppraisalTemplate) 
//                                   VALUES (@ErpNextId, @Employee, @EmployeeName, @AppraisalCycle, @AppraisalTemplate)";

//                    using (var insertCmd = new SqlCommand(insertQuery, connection))
//                    {
//                        insertCmd.Parameters.AddWithValue("@ErpNextId", (object)erpId ?? DBNull.Value);
//                        insertCmd.Parameters.AddWithValue("@Employee", (object)employee ?? DBNull.Value);
//                        insertCmd.Parameters.AddWithValue("@EmployeeName", (object)employeeName ?? DBNull.Value);
//                        insertCmd.Parameters.AddWithValue("@AppraisalCycle", (object)appraisalCycle ?? DBNull.Value);
//                        insertCmd.Parameters.AddWithValue("@AppraisalTemplate", (object)appraisalTemplate ?? DBNull.Value);

//                        insertCmd.ExecuteNonQuery();
//                    }
//                }
//            }
//        }











//        public void InsertOrUpdate(Appraisal a)
//        {
//            Insert(a);
//        }

//        public int Insert(Appraisal a)
//        {
//            int newId;

//            using (var conn = new SqlConnection(_connectionString))
//            {
//                conn.Open();

//                using (var cmd = new SqlCommand(@"
//                    INSERT INTO Appraisals
//                        (Employee, EmployeeName, Company, AppraisalCycle, AppraisalTemplate, RateGoalsManually, Feedback, Reflections, TotalScore, FinalScore, GoalScorePercentage, TotalSelfScore)
//                    OUTPUT INSERTED.Id
//                    VALUES
//                        (@Employee, @EmployeeName, @Company, @AppraisalCycle, @AppraisalTemplate, @RateGoalsManually, @Feedback, @Reflections, @TotalScore, @FinalScore, @GoalScorePercentage, @TotalSelfScore)", conn))
//                {
//                    cmd.Parameters.AddWithValue("@Employee", a.Employee ?? (object)DBNull.Value);
//                    cmd.Parameters.AddWithValue("@EmployeeName", a.EmployeeName ?? (object)DBNull.Value);
//                    cmd.Parameters.AddWithValue("@Company", a.Company ?? (object)DBNull.Value);
//                    cmd.Parameters.AddWithValue("@AppraisalCycle", a.AppraisalCycle ?? (object)DBNull.Value);
//                    cmd.Parameters.AddWithValue("@AppraisalTemplate", a.AppraisalTemplate ?? (object)DBNull.Value);
//                    cmd.Parameters.AddWithValue("@RateGoalsManually", a.RateGoalsManually);
//                    cmd.Parameters.AddWithValue("@Feedback", a.Feedback ?? (object)DBNull.Value);
//                    cmd.Parameters.AddWithValue("@Reflections", a.Reflections ?? (object)DBNull.Value);
//                    cmd.Parameters.AddWithValue("@TotalScore", a.TotalScore);
//                    cmd.Parameters.AddWithValue("@FinalScore", a.FinalScore);
//                    cmd.Parameters.AddWithValue("@GoalScorePercentage", a.GoalScorePercentage);
//                    cmd.Parameters.AddWithValue("@TotalSelfScore", a.TotalSelfScore);


//                    newId = (int)cmd.ExecuteScalar();
//                }

//                foreach (var kra in a.Kras)
//                {
//                    if (string.IsNullOrWhiteSpace(kra.Kra)) continue;

//                    using (var cmd = new SqlCommand(@"
//                        INSERT INTO AppraisalKras (AppraisalId, Kra, Weightage, GoalCompletion, GoalScore)
//                        VALUES (@AppraisalId, @Kra, @Weightage, @GoalCompletion, @GoalScore)", conn))
//                    {
//                        cmd.Parameters.AddWithValue("@AppraisalId", newId);
//                        cmd.Parameters.AddWithValue("@Kra", kra.Kra ?? (object)DBNull.Value);
//                        cmd.Parameters.AddWithValue("@Weightage", kra.Weightage);
//                        cmd.Parameters.AddWithValue("@GoalCompletion", kra.GoalCompletion);
//                        cmd.Parameters.AddWithValue("@GoalScore", kra.GoalScore);
//                        cmd.Parameters.AddWithValue("@GoalCompletion", kra.GoalCompletion); // Ensure it's non-nullable decimal

//                        cmd.ExecuteNonQuery();
//                    }
//                }

//                foreach (var rating in a.SelfRatings)
//                {
//                    if (string.IsNullOrWhiteSpace(rating.Criteria)) continue;

//                    using (var cmd = new SqlCommand(@"
//                        INSERT INTO AppraisalSelfRatings (AppraisalId, Criteria, Weightage, Rating, Reflections)
//                        VALUES (@AppraisalId, @Criteria, @Weightage, @Rating, @Reflections)", conn))
//                    {
//                        cmd.Parameters.AddWithValue("@AppraisalId", newId);
//                        cmd.Parameters.AddWithValue("@Criteria", rating.Criteria ?? (object)DBNull.Value);
//                        cmd.Parameters.AddWithValue("@Weightage", rating.Weightage);
//                        cmd.Parameters.AddWithValue("@Rating", rating.Rating);
//                        cmd.Parameters.AddWithValue("@Reflections", rating.Reflections ?? (object)DBNull.Value);



//                        cmd.ExecuteNonQuery();
//                    }
//                }
//            }

//            return newId;
//        }
//    }
//}




using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class AppraisalService
    {
        private readonly string _connectionString;

        public AppraisalService(string connectionString)
        {
            _connectionString = connectionString;
        }


        // ============================================================
        // GET ALL APPRAISALS
        // ============================================================

        public List<Appraisal> GetAll(
            string idFilter,
            string employee,
            string employeeName,
            string appraisalCycle,
            string appraisalTemplate)
        {
            var list = new List<Appraisal>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT
                        Id,
                        ErpNextId,
                        Employee,
                        EmployeeName,
                        Company,
                        AppraisalCycle,
                        AppraisalTemplate,
                        RateGoalsManually,
                        TotalScore,
                        FinalScore,
                        GoalScorePercentage,
                        TotalSelfScore,
                        AvgFeedbackScore,
                        DocStatus,
                        StartDate,
                        EndDate,
                        AmendedFrom,
                        ErpModified
                    FROM Appraisals
                    WHERE IsDeleted = 0
                ";

                if (!string.IsNullOrWhiteSpace(idFilter))
                    query += " AND CAST(Id AS VARCHAR) LIKE @IdFilter";

                if (!string.IsNullOrWhiteSpace(employee))
                    query += " AND Employee LIKE @Employee";

                if (!string.IsNullOrWhiteSpace(employeeName))
                    query += " AND EmployeeName LIKE @EmployeeName";

                if (!string.IsNullOrWhiteSpace(appraisalCycle))
                    query += " AND AppraisalCycle LIKE @AppraisalCycle";

                if (!string.IsNullOrWhiteSpace(appraisalTemplate))
                    query += " AND AppraisalTemplate LIKE @AppraisalTemplate";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrWhiteSpace(idFilter))
                    {
                        cmd.Parameters.AddWithValue(
                            "@IdFilter",
                            "%" + idFilter + "%"
                        );
                    }

                    if (!string.IsNullOrWhiteSpace(employee))
                    {
                        cmd.Parameters.AddWithValue(
                            "@Employee",
                            "%" + employee + "%"
                        );
                    }

                    if (!string.IsNullOrWhiteSpace(employeeName))
                    {
                        cmd.Parameters.AddWithValue(
                            "@EmployeeName",
                            "%" + employeeName + "%"
                        );
                    }

                    if (!string.IsNullOrWhiteSpace(appraisalCycle))
                    {
                        cmd.Parameters.AddWithValue(
                            "@AppraisalCycle",
                            "%" + appraisalCycle + "%"
                        );
                    }

                    if (!string.IsNullOrWhiteSpace(appraisalTemplate))
                    {
                        cmd.Parameters.AddWithValue(
                            "@AppraisalTemplate",
                            "%" + appraisalTemplate + "%"
                        );
                    }

                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var appraisal = new Appraisal
                            {
                                Id = GetInt(reader, "Id"),

                                ErpNextId =
                                    GetString(reader, "ErpNextId"),

                                Employee =
                                    GetString(reader, "Employee"),

                                EmployeeName =
                                    GetString(reader, "EmployeeName"),

                                Company =
                                    GetString(reader, "Company"),

                                AppraisalCycle =
                                    GetString(reader, "AppraisalCycle"),

                                AppraisalTemplate =
                                    GetString(reader, "AppraisalTemplate"),

                                RateGoalsManually =
                                    GetBool(
                                        reader,
                                        "RateGoalsManually"
                                    ),

                                TotalScore =
                                    GetDecimal(
                                        reader,
                                        "TotalScore"
                                    ),

                                FinalScore =
                                    GetDecimal(
                                        reader,
                                        "FinalScore"
                                    ),

                                GoalScorePercentage =
                                    GetDecimal(
                                        reader,
                                        "GoalScorePercentage"
                                    ),

                                TotalSelfScore =
                                    GetDecimal(
                                        reader,
                                        "TotalSelfScore"
                                    ),

                                AvgFeedbackScore =
                                    GetDecimal(
                                        reader,
                                        "AvgFeedbackScore"
                                    ),

                                DocStatus =
                                    GetInt(
                                        reader,
                                        "DocStatus"
                                    ),

                                StartDate =
                                    GetNullableDate(
                                        reader,
                                        "StartDate"
                                    ),

                                EndDate =
                                    GetNullableDate(
                                        reader,
                                        "EndDate"
                                    ),

                                AmendedFrom =
                                    GetString(
                                        reader,
                                        "AmendedFrom"
                                    ),

                                ErpModified =
                                    GetNullableDateTime(
                                        reader,
                                        "ErpModified"
                                    )
                            };

                            list.Add(appraisal);
                        }
                    }
                }
            }

            return list;
        }


        // ============================================================
        // GET APPRAISAL BY LOCAL ID
        // ============================================================

        public Appraisal GetById(int id)
        {
            Appraisal appraisal = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // ====================================================
                // HEADER
                // ====================================================

                using (var cmd = new SqlCommand(@"
                    SELECT *
                    FROM Appraisals
                    WHERE Id = @Id
                      AND IsDeleted = 0",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            appraisal = new Appraisal
                            {
                                Id =
                                    GetInt(
                                        reader,
                                        "Id"
                                    ),

                                ErpNextId =
                                    GetString(
                                        reader,
                                        "ErpNextId"
                                    ),

                                Employee =
                                    GetString(
                                        reader,
                                        "Employee"
                                    ),

                                EmployeeName =
                                    GetString(
                                        reader,
                                        "EmployeeName"
                                    ),

                                Company =
                                    GetString(
                                        reader,
                                        "Company"
                                    ),

                                AppraisalCycle =
                                    GetString(
                                        reader,
                                        "AppraisalCycle"
                                    ),

                                AppraisalTemplate =
                                    GetString(
                                        reader,
                                        "AppraisalTemplate"
                                    ),

                                RateGoalsManually =
                                    GetBool(
                                        reader,
                                        "RateGoalsManually"
                                    ),

                                TotalScore =
                                    GetDecimal(
                                        reader,
                                        "TotalScore"
                                    ),

                                FinalScore =
                                    GetDecimal(
                                        reader,
                                        "FinalScore"
                                    ),

                                GoalScorePercentage =
                                    GetDecimal(
                                        reader,
                                        "GoalScorePercentage"
                                    ),

                                TotalSelfScore =
                                    GetDecimal(
                                        reader,
                                        "TotalSelfScore"
                                    ),

                                AvgFeedbackScore =
                                    GetDecimal(
                                        reader,
                                        "AvgFeedbackScore"
                                    ),

                                Feedback =
                                    GetString(
                                        reader,
                                        "Feedback"
                                    ),

                                Remarks =
                                    GetString(
                                        reader,
                                        "Remarks"
                                    ),

                                Reflections =
                                    GetString(
                                        reader,
                                        "Reflections"
                                    ),

                                DocStatus =
                                    GetInt(
                                        reader,
                                        "DocStatus"
                                    ),

                                StartDate =
                                    GetNullableDate(
                                        reader,
                                        "StartDate"
                                    ),

                                EndDate =
                                    GetNullableDate(
                                        reader,
                                        "EndDate"
                                    ),

                                AmendedFrom =
                                    GetString(
                                        reader,
                                        "AmendedFrom"
                                    ),

                                ErpModified =
                                    GetNullableDateTime(
                                        reader,
                                        "ErpModified"
                                    )
                            };
                        }
                    }
                }


                // ====================================================
                // CHILD TABLES
                // ====================================================

                if (appraisal != null)
                {
                    // ================================================
                    // KRA TABLE
                    // ================================================

                    using (var cmd = new SqlCommand(@"
                        SELECT *
                        FROM AppraisalKras
                        WHERE AppraisalId = @Id
                        ORDER BY Id",
                        conn))
                    {
                        cmd.Parameters.AddWithValue(
                            "@Id",
                            id
                        );

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                appraisal.Kras.Add(
                                    new AppraisalKra
                                    {
                                        Id =
                                            GetInt(
                                                reader,
                                                "Id"
                                            ),

                                        AppraisalId =
                                            id,

                                        Kra =
                                            GetString(
                                                reader,
                                                "Kra"
                                            ),

                                        Weightage =
                                            GetDecimal(
                                                reader,
                                                "Weightage"
                                            ),

                                        GoalCompletion =
                                            GetDecimal(
                                                reader,
                                                "GoalCompletion"
                                            ),

                                        GoalScore =
                                            GetDecimal(
                                                reader,
                                                "GoalScore"
                                            )
                                    }
                                );
                            }
                        }
                    }


                    // ================================================
                    // SELF RATING TABLE
                    // ================================================

                    using (var cmd = new SqlCommand(@"
                        SELECT *
                        FROM AppraisalSelfRatings
                        WHERE AppraisalId = @Id
                        ORDER BY Id",
                        conn))
                    {
                        cmd.Parameters.AddWithValue(
                            "@Id",
                            id
                        );

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                appraisal.SelfRatings.Add(
                                    new AppraisalSelfRating
                                    {
                                        Id =
                                            GetInt(
                                                reader,
                                                "Id"
                                            ),

                                        AppraisalId =
                                            id,

                                        Criteria =
                                            GetString(
                                                reader,
                                                "Criteria"
                                            ),

                                        Weightage =
                                            GetDecimal(
                                                reader,
                                                "Weightage"
                                            ),

                                        Rating =
                                            GetDecimal(
                                                reader,
                                                "Rating"
                                            ),

                                        Reflections =
                                            GetString(
                                                reader,
                                                "Reflections"
                                            )
                                    }
                                );
                            }
                        }
                    }
                }
            }

            return appraisal;
        }


        // ============================================================
        // GET APPRAISAL BY ERPNext DOCUMENT NAME
        // ============================================================

        public Appraisal GetByErpNextId(string erpNextId)
        {
            if (string.IsNullOrWhiteSpace(erpNextId))
                return null;

            int localId = 0;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand(@"
                    SELECT TOP 1 Id
                    FROM Appraisals
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
        // CHECK WHETHER ERP DOCUMENT EXISTS LOCALLY
        // ============================================================

        public bool ExistsByErpNextId(string erpNextId)
        {
            if (string.IsNullOrWhiteSpace(erpNextId))
                return false;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand(@"
                    SELECT COUNT(1)
                    FROM Appraisals
                    WHERE ErpNextId = @ErpNextId
                      AND IsDeleted = 0",
                    conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@ErpNextId",
                        erpNextId
                    );

                    return Convert.ToInt32(
                        cmd.ExecuteScalar()
                    ) > 0;
                }
            }
        }


        // ============================================================
        // INSERT OR UPDATE
        //
        // MAIN METHOD FOR ERP <-> HRMS SYNC
        // ============================================================

        public int InsertOrUpdate(Appraisal appraisal)
        {
            if (appraisal == null)
                throw new ArgumentNullException(
                    nameof(appraisal)
                );

            using (var conn =
                new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var transaction =
                    conn.BeginTransaction())
                {
                    try
                    {
                        int appraisalId =
                            FindExistingLocalId(
                                conn,
                                transaction,
                                appraisal
                            );

                        // ============================================
                        // EXISTING RECORD
                        // ============================================

                        if (appraisalId > 0)
                        {
                            UpdateHeader(
                                conn,
                                transaction,
                                appraisalId,
                                appraisal
                            );

                            DeleteChildren(
                                conn,
                                transaction,
                                appraisalId
                            );
                        }

                        // ============================================
                        // NEW RECORD
                        // ============================================

                        else
                        {
                            appraisalId =
                                InsertHeader(
                                    conn,
                                    transaction,
                                    appraisal
                                );
                        }

                        // ============================================
                        // REINSERT CHILD ROWS
                        // ============================================

                        InsertKras(
                            conn,
                            transaction,
                            appraisalId,
                            appraisal.Kras
                        );

                        InsertSelfRatings(
                            conn,
                            transaction,
                            appraisalId,
                            appraisal.SelfRatings
                        );

                        transaction.Commit();

                        return appraisalId;
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
        // NORMAL INSERT
        //
        // Retained because other existing controller code may call
        // service.Insert(model).
        //
        // If ErpNextId already exists, this safely updates instead.
        // ============================================================

        public int Insert(Appraisal appraisal)
        {
            return InsertOrUpdate(appraisal);
        }


        // ============================================================
        // MARK LOCAL RECORD AS DELETED
        //
        // This does NOT delete ERPNext.
        // It is for ERP -> HRMS delete synchronization.
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
                    UPDATE Appraisals
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


        // ============================================================
        // FIND EXISTING LOCAL APPRAISAL
        // ============================================================

        private int FindExistingLocalId(
            SqlConnection conn,
            SqlTransaction transaction,
            Appraisal appraisal)
        {
            // First preference:
            // ERPNext document name

            if (!string.IsNullOrWhiteSpace(
                appraisal.ErpNextId))
            {
                using (var cmd = new SqlCommand(@"
                    SELECT TOP 1 Id
                    FROM Appraisals
                    WHERE ErpNextId = @ErpNextId
                      AND IsDeleted = 0",
                    conn,
                    transaction))
                {
                    cmd.Parameters.AddWithValue(
                        "@ErpNextId",
                        appraisal.ErpNextId
                    );

                    var result =
                        cmd.ExecuteScalar();

                    if (result != null &&
                        result != DBNull.Value)
                    {
                        return Convert.ToInt32(
                            result
                        );
                    }
                }
            }

            // Second preference:
            // Local record ID
            //
            // Useful when updating a local record.

            if (appraisal.Id > 0)
            {
                using (var cmd = new SqlCommand(@"
                    SELECT TOP 1 Id
                    FROM Appraisals
                    WHERE Id = @Id
                      AND IsDeleted = 0",
                    conn,
                    transaction))
                {
                    cmd.Parameters.AddWithValue(
                        "@Id",
                        appraisal.Id
                    );

                    var result =
                        cmd.ExecuteScalar();

                    if (result != null &&
                        result != DBNull.Value)
                    {
                        return Convert.ToInt32(
                            result
                        );
                    }
                }
            }

            return 0;
        }


        // ============================================================
        // INSERT HEADER
        // ============================================================

        private int InsertHeader(
            SqlConnection conn,
            SqlTransaction transaction,
            Appraisal appraisal)
        {
            using (var cmd = new SqlCommand(@"
                INSERT INTO Appraisals
                (
                    ErpNextId,
                    Employee,
                    EmployeeName,
                    Company,
                    AppraisalCycle,
                    AppraisalTemplate,
                    RateGoalsManually,

                    TotalScore,
                    FinalScore,
                    GoalScorePercentage,
                    TotalSelfScore,
                    AvgFeedbackScore,

                    Feedback,
                    Remarks,
                    Reflections,

                    DocStatus,
                    StartDate,
                    EndDate,
                    AmendedFrom,
                    ErpModified,

                    IsDeleted
                )
                OUTPUT INSERTED.Id
                VALUES
                (
                    @ErpNextId,
                    @Employee,
                    @EmployeeName,
                    @Company,
                    @AppraisalCycle,
                    @AppraisalTemplate,
                    @RateGoalsManually,

                    @TotalScore,
                    @FinalScore,
                    @GoalScorePercentage,
                    @TotalSelfScore,
                    @AvgFeedbackScore,

                    @Feedback,
                    @Remarks,
                    @Reflections,

                    @DocStatus,
                    @StartDate,
                    @EndDate,
                    @AmendedFrom,
                    @ErpModified,

                    0
                )",
                conn,
                transaction))
            {
                AddHeaderParameters(
                    cmd,
                    appraisal
                );

                return Convert.ToInt32(
                    cmd.ExecuteScalar()
                );
            }
        }


        // ============================================================
        // UPDATE HEADER
        // ============================================================

        private void UpdateHeader(
            SqlConnection conn,
            SqlTransaction transaction,
            int appraisalId,
            Appraisal appraisal)
        {
            using (var cmd = new SqlCommand(@"
                UPDATE Appraisals
                SET
                    ErpNextId = @ErpNextId,
                    Employee = @Employee,
                    EmployeeName = @EmployeeName,
                    Company = @Company,
                    AppraisalCycle = @AppraisalCycle,
                    AppraisalTemplate = @AppraisalTemplate,

                    RateGoalsManually =
                        @RateGoalsManually,

                    TotalScore =
                        @TotalScore,

                    FinalScore =
                        @FinalScore,

                    GoalScorePercentage =
                        @GoalScorePercentage,

                    TotalSelfScore =
                        @TotalSelfScore,

                    AvgFeedbackScore =
                        @AvgFeedbackScore,

                    Feedback =
                        @Feedback,

                    Remarks =
                        @Remarks,

                    Reflections =
                        @Reflections,

                    DocStatus =
                        @DocStatus,

                    StartDate =
                        @StartDate,

                    EndDate =
                        @EndDate,

                    AmendedFrom =
                        @AmendedFrom,

                    ErpModified =
                        @ErpModified,

                    IsDeleted = 0

                WHERE Id = @Id",
                conn,
                transaction))
            {
                AddHeaderParameters(
                    cmd,
                    appraisal
                );

                cmd.Parameters.AddWithValue(
                    "@Id",
                    appraisalId
                );

                cmd.ExecuteNonQuery();
            }
        }


        // ============================================================
        // HEADER PARAMETERS
        // ============================================================

        private void AddHeaderParameters(
            SqlCommand cmd,
            Appraisal appraisal)
        {
            cmd.Parameters.AddWithValue(
                "@ErpNextId",
                DbValue(appraisal.ErpNextId)
            );

            cmd.Parameters.AddWithValue(
                "@Employee",
                DbValue(appraisal.Employee)
            );

            cmd.Parameters.AddWithValue(
                "@EmployeeName",
                DbValue(appraisal.EmployeeName)
            );

            cmd.Parameters.AddWithValue(
                "@Company",
                DbValue(appraisal.Company)
            );

            cmd.Parameters.AddWithValue(
                "@AppraisalCycle",
                DbValue(appraisal.AppraisalCycle)
            );

            cmd.Parameters.AddWithValue(
                "@AppraisalTemplate",
                DbValue(appraisal.AppraisalTemplate)
            );

            cmd.Parameters.AddWithValue(
                "@RateGoalsManually",
                appraisal.RateGoalsManually
            );

            cmd.Parameters.AddWithValue(
                "@TotalScore",
                appraisal.TotalScore
            );

            cmd.Parameters.AddWithValue(
                "@FinalScore",
                appraisal.FinalScore
            );

            cmd.Parameters.AddWithValue(
                "@GoalScorePercentage",
                appraisal.GoalScorePercentage
            );

            cmd.Parameters.AddWithValue(
                "@TotalSelfScore",
                appraisal.TotalSelfScore
            );

            cmd.Parameters.AddWithValue(
                "@AvgFeedbackScore",
                appraisal.AvgFeedbackScore
            );

            cmd.Parameters.AddWithValue(
                "@Feedback",
                DbValue(appraisal.Feedback)
            );

            cmd.Parameters.AddWithValue(
                "@Remarks",
                DbValue(appraisal.Remarks)
            );

            cmd.Parameters.AddWithValue(
                "@Reflections",
                DbValue(appraisal.Reflections)
            );

            cmd.Parameters.AddWithValue(
                "@DocStatus",
                appraisal.DocStatus
            );

            cmd.Parameters.AddWithValue(
                "@StartDate",
                appraisal.StartDate.HasValue
                    ? appraisal.StartDate.Value
                    : (object)DBNull.Value
            );

            cmd.Parameters.AddWithValue(
                "@EndDate",
                appraisal.EndDate.HasValue
                    ? appraisal.EndDate.Value
                    : (object)DBNull.Value
            );

            cmd.Parameters.AddWithValue(
                "@AmendedFrom",
                DbValue(appraisal.AmendedFrom)
            );

            cmd.Parameters.AddWithValue(
                "@ErpModified",
                appraisal.ErpModified.HasValue
                    ? appraisal.ErpModified.Value
                    : (object)DBNull.Value
            );
        }


        // ============================================================
        // DELETE CHILD ROWS BEFORE ERP RE-SYNC
        // ============================================================

        private void DeleteChildren(
            SqlConnection conn,
            SqlTransaction transaction,
            int appraisalId)
        {
            using (var cmd = new SqlCommand(@"
                DELETE FROM AppraisalKras
                WHERE AppraisalId = @AppraisalId",
                conn,
                transaction))
            {
                cmd.Parameters.AddWithValue(
                    "@AppraisalId",
                    appraisalId
                );

                cmd.ExecuteNonQuery();
            }

            using (var cmd = new SqlCommand(@"
                DELETE FROM AppraisalSelfRatings
                WHERE AppraisalId = @AppraisalId",
                conn,
                transaction))
            {
                cmd.Parameters.AddWithValue(
                    "@AppraisalId",
                    appraisalId
                );

                cmd.ExecuteNonQuery();
            }
        }


        // ============================================================
        // INSERT KRA CHILD ROWS
        // ============================================================

        private void InsertKras(
            SqlConnection conn,
            SqlTransaction transaction,
            int appraisalId,
            List<AppraisalKra> kras)
        {
            if (kras == null)
                return;

            foreach (var kra in kras)
            {
                if (kra == null ||
                    string.IsNullOrWhiteSpace(
                        kra.Kra))
                {
                    continue;
                }

                using (var cmd = new SqlCommand(@"
                    INSERT INTO AppraisalKras
                    (
                        AppraisalId,
                        Kra,
                        Weightage,
                        GoalCompletion,
                        GoalScore
                    )
                    VALUES
                    (
                        @AppraisalId,
                        @Kra,
                        @Weightage,
                        @GoalCompletion,
                        @GoalScore
                    )",
                    conn,
                    transaction))
                {
                    cmd.Parameters.AddWithValue(
                        "@AppraisalId",
                        appraisalId
                    );

                    cmd.Parameters.AddWithValue(
                        "@Kra",
                        kra.Kra
                    );

                    cmd.Parameters.AddWithValue(
                        "@Weightage",
                        kra.Weightage
                    );

                    cmd.Parameters.AddWithValue(
                        "@GoalCompletion",
                        kra.GoalCompletion
                    );

                    cmd.Parameters.AddWithValue(
                        "@GoalScore",
                        kra.GoalScore
                    );

                    cmd.ExecuteNonQuery();
                }
            }
        }


        // ============================================================
        // INSERT SELF RATING CHILD ROWS
        // ============================================================

        private void InsertSelfRatings(
            SqlConnection conn,
            SqlTransaction transaction,
            int appraisalId,
            List<AppraisalSelfRating> ratings)
        {
            if (ratings == null)
                return;

            foreach (var rating in ratings)
            {
                if (rating == null ||
                    string.IsNullOrWhiteSpace(
                        rating.Criteria))
                {
                    continue;
                }

                using (var cmd = new SqlCommand(@"
                    INSERT INTO AppraisalSelfRatings
                    (
                        AppraisalId,
                        Criteria,
                        Weightage,
                        Rating,
                        Reflections
                    )
                    VALUES
                    (
                        @AppraisalId,
                        @Criteria,
                        @Weightage,
                        @Rating,
                        @Reflections
                    )",
                    conn,
                    transaction))
                {
                    cmd.Parameters.AddWithValue(
                        "@AppraisalId",
                        appraisalId
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

                    cmd.Parameters.AddWithValue(
                        "@Reflections",
                        DbValue(
                            rating.Reflections
                        )
                    );

                    cmd.ExecuteNonQuery();
                }
            }
        }


        // ============================================================
        // OLD METHOD COMPATIBILITY
        //
        // You had this method already.
        // Keep it so old controller code does not break.
        // ============================================================

        public void SyncRecordFromErpNext(
            string erpId,
            string employee,
            string employeeName,
            string appraisalCycle,
            string appraisalTemplate)
        {
            var appraisal =
                GetByErpNextId(erpId);

            if (appraisal == null)
            {
                appraisal =
                    new Appraisal
                    {
                        ErpNextId =
                            erpId,

                        Employee =
                            employee,

                        EmployeeName =
                            employeeName,

                        AppraisalCycle =
                            appraisalCycle,

                        AppraisalTemplate =
                            appraisalTemplate
                    };
            }
            else
            {
                appraisal.Employee =
                    employee;

                appraisal.EmployeeName =
                    employeeName;

                appraisal.AppraisalCycle =
                    appraisalCycle;

                appraisal.AppraisalTemplate =
                    appraisalTemplate;
            }

            InsertOrUpdate(appraisal);
        }


        // ============================================================
        // DATABASE HELPERS
        // ============================================================

        private static object DbValue(
            string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return DBNull.Value;

            return value;
        }


        private static string GetString(
            SqlDataReader reader,
            string column)
        {
            if (!ColumnExists(reader, column))
                return string.Empty;

            if (reader[column] == DBNull.Value)
                return string.Empty;

            return reader[column]
                ?.ToString()
                ?? string.Empty;
        }


        private static int GetInt(
            SqlDataReader reader,
            string column)
        {
            if (!ColumnExists(reader, column))
                return 0;

            if (reader[column] == DBNull.Value)
                return 0;

            return Convert.ToInt32(
                reader[column]
            );
        }


        private static decimal GetDecimal(
            SqlDataReader reader,
            string column)
        {
            if (!ColumnExists(reader, column))
                return 0m;

            if (reader[column] == DBNull.Value)
                return 0m;

            return Convert.ToDecimal(
                reader[column]
            );
        }


        private static bool GetBool(
            SqlDataReader reader,
            string column)
        {
            if (!ColumnExists(reader, column))
                return false;

            if (reader[column] == DBNull.Value)
                return false;

            return Convert.ToBoolean(
                reader[column]
            );
        }


        private static DateTime? GetNullableDate(
            SqlDataReader reader,
            string column)
        {
            if (!ColumnExists(reader, column))
                return null;

            if (reader[column] == DBNull.Value)
                return null;

            return Convert.ToDateTime(
                reader[column]
            ).Date;
        }


        private static DateTime? GetNullableDateTime(
            SqlDataReader reader,
            string column)
        {
            if (!ColumnExists(reader, column))
                return null;

            if (reader[column] == DBNull.Value)
                return null;

            return Convert.ToDateTime(
                reader[column]
            );
        }


        private static bool ColumnExists(
            SqlDataReader reader,
            string columnName)
        {
            for (
                int i = 0;
                i < reader.FieldCount;
                i++
            )
            {
                if (string.Equals(
                    reader.GetName(i),
                    columnName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

