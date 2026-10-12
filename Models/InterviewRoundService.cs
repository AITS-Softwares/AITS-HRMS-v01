using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class InterviewRoundService
    {
        private readonly string _connectionString;

        public InterviewRoundService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<InterviewRound> GetAll(string idFilter)
        {
            var list = new List<InterviewRound>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT Id, RoundName, ExpectedAverageRating FROM InterviewRounds WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(idFilter))
                    query += " AND CAST(Id AS VARCHAR) LIKE @IdFilter";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(idFilter))
                        cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new InterviewRound
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                RoundName = reader["RoundName"]?.ToString() ?? string.Empty,
                                ExpectedAverageRating = Convert.ToInt32(reader["ExpectedAverageRating"])
                            });
                        }
                    }
                }
            }

            return list;
        }

        public InterviewRound GetById(int id)
        {
            InterviewRound round = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand("SELECT * FROM InterviewRounds WHERE Id = @Id AND IsDeleted = 0", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            round = new InterviewRound
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                RoundName = reader["RoundName"]?.ToString() ?? string.Empty,
                                ExpectedAverageRating = Convert.ToInt32(reader["ExpectedAverageRating"]),
                                InterviewType = reader["InterviewType"]?.ToString() ?? string.Empty,
                                Interviewers = reader["Interviewers"]?.ToString() ?? string.Empty,
                                Designation = reader["Designation"]?.ToString() ?? string.Empty
                            };
                        }
                    }
                }

                if (round != null)
                {
                    using (var cmd = new SqlCommand("SELECT * FROM InterviewRoundSkills WHERE InterviewRoundId = @Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                round.Skills.Add(new InterviewRoundSkill
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    InterviewRoundId = id,
                                    Skill = reader["Skill"]?.ToString() ?? string.Empty,
                                    Description = reader["Description"]?.ToString() ?? string.Empty
                                });
                            }
                        }
                    }
                }
            }

            return round;
        }

        public int Insert(InterviewRound r)
        {
            int newId;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand(@"
                    INSERT INTO InterviewRounds (RoundName, ExpectedAverageRating, InterviewType, Interviewers, Designation)
                    OUTPUT INSERTED.Id
                    VALUES (@RoundName, @ExpectedAverageRating, @InterviewType, @Interviewers, @Designation)", conn))
                {
                    cmd.Parameters.AddWithValue("@RoundName", r.RoundName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ExpectedAverageRating", r.ExpectedAverageRating);
                    cmd.Parameters.AddWithValue("@InterviewType", r.InterviewType ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Interviewers", r.Interviewers ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Designation", r.Designation ?? (object)DBNull.Value);

                    newId = (int)cmd.ExecuteScalar();
                }

                foreach (var skill in r.Skills)
                {
                    if (string.IsNullOrWhiteSpace(skill.Skill)) continue;

                    using (var cmd = new SqlCommand(@"
                        INSERT INTO InterviewRoundSkills (InterviewRoundId, Skill, Description)
                        VALUES (@InterviewRoundId, @Skill, @Description)", conn))
                    {
                        cmd.Parameters.AddWithValue("@InterviewRoundId", newId);
                        cmd.Parameters.AddWithValue("@Skill", skill.Skill ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Description", skill.Description ?? (object)DBNull.Value);

                        cmd.ExecuteNonQuery();
                    }
                }
            }

            return newId;
        }
    }
}
