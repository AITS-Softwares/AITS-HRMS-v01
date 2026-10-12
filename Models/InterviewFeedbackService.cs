using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class InterviewFeedbackService
    {
        private readonly string _connectionString;

        public InterviewFeedbackService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<InterviewFeedbackListRow> GetAll(
            string idFilter, string interviewFilter, string interviewRound,
            string jobApplicant, string interviewer, string result)
        {
            var list = new List<InterviewFeedbackListRow>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT f.Id, f.InterviewId, i.InterviewRound, i.JobApplicant, f.Interviewer, f.Result
                    FROM InterviewFeedbacks f
                    INNER JOIN Interviews i ON i.Id = f.InterviewId
                    WHERE 1 = 1";

                if (!string.IsNullOrEmpty(idFilter))
                    query += " AND CAST(f.Id AS VARCHAR) LIKE @IdFilter";
                if (!string.IsNullOrEmpty(interviewFilter))
                    query += " AND CAST(f.InterviewId AS VARCHAR) LIKE @InterviewFilter";
                if (!string.IsNullOrEmpty(interviewRound))
                    query += " AND i.InterviewRound LIKE @InterviewRound";
                if (!string.IsNullOrEmpty(jobApplicant))
                    query += " AND i.JobApplicant LIKE @JobApplicant";
                if (!string.IsNullOrEmpty(interviewer))
                    query += " AND f.Interviewer LIKE @Interviewer";
                if (!string.IsNullOrEmpty(result))
                    query += " AND f.Result = @Result";

                query += " ORDER BY f.Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(idFilter)) cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");
                    if (!string.IsNullOrEmpty(interviewFilter)) cmd.Parameters.AddWithValue("@InterviewFilter", "%" + interviewFilter + "%");
                    if (!string.IsNullOrEmpty(interviewRound)) cmd.Parameters.AddWithValue("@InterviewRound", "%" + interviewRound + "%");
                    if (!string.IsNullOrEmpty(jobApplicant)) cmd.Parameters.AddWithValue("@JobApplicant", "%" + jobApplicant + "%");
                    if (!string.IsNullOrEmpty(interviewer)) cmd.Parameters.AddWithValue("@Interviewer", "%" + interviewer + "%");
                    if (!string.IsNullOrEmpty(result)) cmd.Parameters.AddWithValue("@Result", result);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new InterviewFeedbackListRow
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                InterviewId = Convert.ToInt32(reader["InterviewId"]),
                                InterviewRound = reader["InterviewRound"]?.ToString() ?? string.Empty,
                                JobApplicant = reader["JobApplicant"]?.ToString() ?? string.Empty,
                                Interviewer = reader["Interviewer"]?.ToString() ?? string.Empty,
                                Result = reader["Result"]?.ToString() ?? string.Empty
                            });
                        }
                    }
                }
            }

            return list;
        }

        public InterviewFeedbackModel GetById(int id)
        {
            InterviewFeedbackModel model = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string query = @"
                    SELECT f.Id, f.InterviewId, f.Interviewer, f.Result, f.FeedbackText, f.CreatedDate,
                           i.InterviewRound, i.JobApplicant
                    FROM InterviewFeedbacks f
                    INNER JOIN Interviews i ON i.Id = f.InterviewId
                    WHERE f.Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model = new InterviewFeedbackModel
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                InterviewId = Convert.ToInt32(reader["InterviewId"]),
                                Interviewer = reader["Interviewer"]?.ToString() ?? string.Empty,
                                Result = reader["Result"]?.ToString() ?? string.Empty,
                                FeedbackText = reader["FeedbackText"]?.ToString() ?? string.Empty,
                                CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                                InterviewRound = reader["InterviewRound"]?.ToString() ?? string.Empty,
                                JobApplicant = reader["JobApplicant"]?.ToString() ?? string.Empty
                            };
                        }
                    }
                }

                if (model == null) return null;

                string q2 = "SELECT Id, InterviewFeedbackId, Skill, Rating FROM InterviewFeedbackSkills WHERE InterviewFeedbackId = @Id";
                using (var cmd = new SqlCommand(q2, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.Skills.Add(new InterviewFeedbackSkill
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                InterviewFeedbackId = Convert.ToInt32(reader["InterviewFeedbackId"]),
                                Skill = reader["Skill"]?.ToString() ?? string.Empty,
                                Rating = Convert.ToInt32(reader["Rating"])
                            });
                        }
                    }
                }
            }

            return model;
        }

        public int Insert(InterviewFeedbackModel model, List<string> skillNames, List<int> skillRatings)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int newId;

                        string query = @"
                            INSERT INTO InterviewFeedbacks
                                (InterviewId, Interviewer, Result, FeedbackText, CreatedDate)
                            OUTPUT INSERTED.Id
                            VALUES
                                (@InterviewId, @Interviewer, @Result, @FeedbackText, GETDATE())";

                        using (var cmd = new SqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@InterviewId", model.InterviewId);
                            cmd.Parameters.AddWithValue("@Interviewer", model.Interviewer ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Result", model.Result ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@FeedbackText", (object)model.FeedbackText ?? DBNull.Value);

                            newId = (int)cmd.ExecuteScalar();
                        }

                        if (skillNames != null)
                        {
                            for (int i = 0; i < skillNames.Count; i++)
                            {
                                if (string.IsNullOrWhiteSpace(skillNames[i])) continue;

                                int rating = (skillRatings != null && i < skillRatings.Count) ? skillRatings[i] : 0;

                                string q = "INSERT INTO InterviewFeedbackSkills (InterviewFeedbackId, Skill, Rating) VALUES (@Fid, @Skill, @Rating)";
                                using (var cmd = new SqlCommand(q, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@Fid", newId);
                                    cmd.Parameters.AddWithValue("@Skill", skillNames[i]);
                                    cmd.Parameters.AddWithValue("@Rating", rating);
                                    cmd.ExecuteNonQuery();
                                }
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
    }
}