using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class ATSJobOpeningService
    {
        private readonly string _connectionString;

        public ATSJobOpeningService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<ATSJobOpening> GetAll(string idFilter = null, string jobTitleFilter = null)
        {
            var list = new List<ATSJobOpening>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT Id, ATSJobOpeningCode, JobTitle, Department, Status
                                  FROM ATSJobOpenings WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(idFilter)) query += " AND ATSJobOpeningCode LIKE @IdFilter";
                if (!string.IsNullOrEmpty(jobTitleFilter)) query += " AND JobTitle LIKE @JobTitleFilter";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(idFilter)) cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");
                    if (!string.IsNullOrEmpty(jobTitleFilter)) cmd.Parameters.AddWithValue("@JobTitleFilter", "%" + jobTitleFilter + "%");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new ATSJobOpening
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                ATSJobOpeningCode = reader["ATSJobOpeningCode"]?.ToString() ?? string.Empty,
                                JobTitle = reader["JobTitle"]?.ToString() ?? string.Empty,
                                Department = reader["Department"]?.ToString() ?? string.Empty,
                                Status = reader["Status"]?.ToString() ?? string.Empty
                            });
                        }
                    }
                }
            }

            return list;
        }

        public ATSJobOpening GetById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM ATSJobOpenings WHERE Id = @Id AND IsDeleted = 0";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new ATSJobOpening
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                ATSJobOpeningCode = reader["ATSJobOpeningCode"]?.ToString() ?? string.Empty,
                                JobTitle = reader["JobTitle"]?.ToString() ?? string.Empty,
                                OpenPositions = reader["OpenPositions"] != DBNull.Value ? Convert.ToInt32(reader["OpenPositions"]) : 1,
                                Department = reader["Department"]?.ToString() ?? string.Empty,
                                ExperienceRequired = reader["ExperienceRequired"]?.ToString() ?? string.Empty,
                                Status = reader["Status"]?.ToString() ?? "Open",
                                RequiredSkills = reader["RequiredSkills"]?.ToString() ?? string.Empty,
                                JobDescription = reader["JobDescription"]?.ToString() ?? string.Empty,
                                HiringManager = reader["HiringManager"]?.ToString() ?? string.Empty
                            };
                        }
                    }
                }
            }

            return null;
        }

        public string GetNextCode()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT COUNT(*) FROM ATSJobOpenings";
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    int count = (int)cmd.ExecuteScalar() + 1;
                    return $"ATS-JOB-{count:D5}";
                }
            }
        }

        public int Insert(ATSJobOpening model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    INSERT INTO ATSJobOpenings
                        (ATSJobOpeningCode, JobTitle, OpenPositions, Department, ExperienceRequired, Status,
                         RequiredSkills, JobDescription, HiringManager)
                    OUTPUT INSERTED.Id
                    VALUES
                        (@ATSJobOpeningCode, @JobTitle, @OpenPositions, @Department, @ExperienceRequired, @Status,
                         @RequiredSkills, @JobDescription, @HiringManager)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ATSJobOpeningCode", model.ATSJobOpeningCode ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@JobTitle", model.JobTitle ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@OpenPositions", model.OpenPositions);
                    cmd.Parameters.AddWithValue("@Department", model.Department ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ExperienceRequired", model.ExperienceRequired ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", model.Status ?? "Open");
                    cmd.Parameters.AddWithValue("@RequiredSkills", model.RequiredSkills ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@JobDescription", model.JobDescription ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@HiringManager", model.HiringManager ?? (object)DBNull.Value);

                    conn.Open();
                    return (int)cmd.ExecuteScalar();
                }
            }
        }
    }
}