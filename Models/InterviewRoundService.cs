using System;
using System.Collections.Generic;
using System.Linq;
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
                conn.Open();
                EnsureErpNextIdColumn(conn);
                string query = "SELECT Id, RoundName, ExpectedAverageRating FROM InterviewRounds WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(idFilter))
                    query += " AND CAST(Id AS VARCHAR) LIKE @IdFilter";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(idFilter))
                        cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new InterviewRound
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                RoundName = reader["RoundName"]?.ToString() ?? string.Empty,
                                Description = reader["Description"]?.ToString(),
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
                EnsureErpNextIdColumn(conn);

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
                                ErpNextId = reader["ErpNextId"]?.ToString(),
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
                EnsureErpNextIdColumn(conn);

                using (var cmd = new SqlCommand(@"
                    INSERT INTO InterviewRounds (ErpNextId, RoundName, Description, ExpectedAverageRating, InterviewType, Interviewers, Designation)
                    OUTPUT INSERTED.Id
                    VALUES (@ErpNextId, @RoundName, @Description, @ExpectedAverageRating, @InterviewType, @Interviewers, @Designation)", conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", r.ErpNextId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@RoundName", r.RoundName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Description", r.Description ?? (object)DBNull.Value);
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

        public void Update(InterviewRound r)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            EnsureErpNextIdColumn(conn);
            using (var cmd = new SqlCommand(@"UPDATE InterviewRounds SET ErpNextId=@ErpNextId, RoundName=@RoundName, Description=@Description, ExpectedAverageRating=@ExpectedAverageRating, InterviewType=@InterviewType, Interviewers=@Interviewers, Designation=@Designation WHERE Id=@Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", r.Id);
                cmd.Parameters.AddWithValue("@ErpNextId", r.ErpNextId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@RoundName", r.RoundName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Description", r.Description ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ExpectedAverageRating", r.ExpectedAverageRating);
                cmd.Parameters.AddWithValue("@InterviewType", r.InterviewType ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Interviewers", r.Interviewers ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Designation", r.Designation ?? (object)DBNull.Value);
                cmd.ExecuteNonQuery();
            }
            using (var delete = new SqlCommand("DELETE FROM InterviewRoundSkills WHERE InterviewRoundId=@Id", conn))
            {
                delete.Parameters.AddWithValue("@Id", r.Id);
                delete.ExecuteNonQuery();
            }
            foreach (var skill in r.Skills.Where(x => !string.IsNullOrWhiteSpace(x.Skill)))
            {
                using var insert = new SqlCommand("INSERT INTO InterviewRoundSkills (InterviewRoundId,Skill,Description) VALUES (@Id,@Skill,@Description)", conn);
                insert.Parameters.AddWithValue("@Id", r.Id);
                insert.Parameters.AddWithValue("@Skill", skill.Skill);
                insert.Parameters.AddWithValue("@Description", skill.Description ?? (object)DBNull.Value);
                insert.ExecuteNonQuery();
            }
        }

        private static void EnsureErpNextIdColumn(SqlConnection conn)
        {
            using var cmd = new SqlCommand("IF COL_LENGTH('dbo.InterviewRounds','ErpNextId') IS NULL ALTER TABLE dbo.InterviewRounds ADD ErpNextId NVARCHAR(255) NULL; IF COL_LENGTH('dbo.InterviewRounds','Description') IS NULL ALTER TABLE dbo.InterviewRounds ADD Description NVARCHAR(MAX) NULL;", conn);
            cmd.ExecuteNonQuery();
        }
    }
}
