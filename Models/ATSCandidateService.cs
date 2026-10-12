using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class ATSCandidateService
    {
        private readonly string _connectionString;

        public ATSCandidateService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<ATSCandidate> GetAll(string idFilter = null, string nameFilter = null)
        {
            var list = new List<ATSCandidate>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT Id, ATSCandidateCode, CandidateName, CandidateStatus, Email, ATSScore
                                  FROM ATSCandidates WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(idFilter)) query += " AND ATSCandidateCode LIKE @IdFilter";
                if (!string.IsNullOrEmpty(nameFilter)) query += " AND CandidateName LIKE @NameFilter";

                query += " ORDER BY ATSScore DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(idFilter)) cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");
                    if (!string.IsNullOrEmpty(nameFilter)) cmd.Parameters.AddWithValue("@NameFilter", "%" + nameFilter + "%");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new ATSCandidate
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                ATSCandidateCode = reader["ATSCandidateCode"]?.ToString() ?? string.Empty,
                                CandidateName = reader["CandidateName"]?.ToString() ?? string.Empty,
                                CandidateStatus = reader["CandidateStatus"]?.ToString() ?? string.Empty,
                                Email = reader["Email"]?.ToString() ?? string.Empty,
                                ATSScore = reader["ATSScore"] != DBNull.Value ? Convert.ToInt32(reader["ATSScore"]) : 0
                            });
                        }
                    }
                }
            }

            return list;
        }

        public ATSCandidate GetById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM ATSCandidates WHERE Id = @Id AND IsDeleted = 0";

                ATSCandidate candidate = null;

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            candidate = new ATSCandidate
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                ATSCandidateCode = reader["ATSCandidateCode"]?.ToString() ?? string.Empty,
                                CandidateName = reader["CandidateName"]?.ToString() ?? string.Empty,
                                AppliedJob = reader["AppliedJob"]?.ToString() ?? string.Empty,
                                Email = reader["Email"]?.ToString() ?? string.Empty,
                                CandidateStatus = reader["CandidateStatus"]?.ToString() ?? "Applied",
                                Phone = reader["Phone"]?.ToString() ?? string.Empty,
                                Source = reader["Source"]?.ToString() ?? string.Empty,
                                ResumeAttachment = reader["ResumeAttachment"]?.ToString() ?? string.Empty,
                                ResumeParseStatus = reader["ResumeParseStatus"]?.ToString() ?? "Not Parsed",
                                Skills = reader["Skills"]?.ToString() ?? string.Empty,
                                Experience = reader["Experience"]?.ToString() ?? string.Empty,
                                CurrentCompany = reader["CurrentCompany"]?.ToString() ?? string.Empty,
                                ExpectedSalary = reader["ExpectedSalary"] != DBNull.Value ? Convert.ToDecimal(reader["ExpectedSalary"]) : (decimal?)null,
                                NoticePeriod = reader["NoticePeriod"]?.ToString() ?? string.Empty,
                                RecruiterNotes = reader["RecruiterNotes"]?.ToString() ?? string.Empty,
                                InterviewRating = reader["InterviewRating"] != DBNull.Value ? Convert.ToInt32(reader["InterviewRating"]) : (int?)null,
                                ATSScore = reader["ATSScore"] != DBNull.Value ? Convert.ToInt32(reader["ATSScore"]) : 0
                            };
                        }
                    }

                    if (candidate != null)
                    {
                        candidate.SkillMatrix = GetSkillMatrix(conn, candidate.Id);
                    }
                }

                return candidate;
            }
        }

        private List<ATSCandidateSkill> GetSkillMatrix(SqlConnection conn, int candidateId)
        {
            var list = new List<ATSCandidateSkill>();
            string query = "SELECT * FROM ATSCandidateSkillMatrix WHERE ATSCandidateId = @Id";

            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", candidateId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new ATSCandidateSkill
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            ATSCandidateId = candidateId,
                            Skill = reader["Skill"]?.ToString() ?? string.Empty,
                            Experience = reader["Experience"]?.ToString() ?? string.Empty,
                            Rating = reader["Rating"] != DBNull.Value ? Convert.ToInt32(reader["Rating"]) : (int?)null
                        });
                    }
                }
            }

            return list;
        }

        public string GetNextCode()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT COUNT(*) FROM ATSCandidates";
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    int count = (int)cmd.ExecuteScalar() + 1;
                    return $"ATS-CAND-{count:D5}";
                }
            }
        }

        public int Insert(ATSCandidate model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int newId;

                        string headerQuery = @"
                            INSERT INTO ATSCandidates
                                (ATSCandidateCode, CandidateName, AppliedJob, Email, CandidateStatus, Phone, Source,
                                 ResumeAttachment, ResumeParseStatus, Skills, Experience, CurrentCompany,
                                 ExpectedSalary, NoticePeriod, RecruiterNotes, InterviewRating, ATSScore)
                            OUTPUT INSERTED.Id
                            VALUES
                                (@ATSCandidateCode, @CandidateName, @AppliedJob, @Email, @CandidateStatus, @Phone, @Source,
                                 @ResumeAttachment, @ResumeParseStatus, @Skills, @Experience, @CurrentCompany,
                                 @ExpectedSalary, @NoticePeriod, @RecruiterNotes, @InterviewRating, @ATSScore)";

                        using (var cmd = new SqlCommand(headerQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ATSCandidateCode", model.ATSCandidateCode ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@CandidateName", model.CandidateName ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@AppliedJob", model.AppliedJob ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Email", model.Email ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@CandidateStatus", model.CandidateStatus ?? "Applied");
                            cmd.Parameters.AddWithValue("@Phone", model.Phone ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Source", model.Source ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@ResumeAttachment", model.ResumeAttachment ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@ResumeParseStatus", model.ResumeParseStatus ?? "Not Parsed");
                            cmd.Parameters.AddWithValue("@Skills", model.Skills ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Experience", model.Experience ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@CurrentCompany", model.CurrentCompany ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@ExpectedSalary", (object)model.ExpectedSalary ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@NoticePeriod", model.NoticePeriod ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@RecruiterNotes", model.RecruiterNotes ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@InterviewRating", (object)model.InterviewRating ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ATSScore", model.ATSScore);

                            newId = (int)cmd.ExecuteScalar();
                        }

                        if (model.SkillMatrix != null)
                        {
                            foreach (var s in model.SkillMatrix)
                            {
                                if (string.IsNullOrEmpty(s.Skill)) continue;

                                string q = @"INSERT INTO ATSCandidateSkillMatrix (ATSCandidateId, Skill, Experience, Rating)
                                             VALUES (@ATSCandidateId, @Skill, @Experience, @Rating)";
                                using (var cmd = new SqlCommand(q, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@ATSCandidateId", newId);
                                    cmd.Parameters.AddWithValue("@Skill", s.Skill ?? (object)DBNull.Value);
                                    cmd.Parameters.AddWithValue("@Experience", s.Experience ?? (object)DBNull.Value);
                                    cmd.Parameters.AddWithValue("@Rating", (object)s.Rating ?? DBNull.Value);
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
