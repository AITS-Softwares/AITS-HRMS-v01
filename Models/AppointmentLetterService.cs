using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class AppointmentLetterService
    {
        private readonly string _connectionString;

        public AppointmentLetterService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<AppointmentLetter> GetAll(string idFilter = null, string applicantFilter = null)
        {
            var letters = new List<AppointmentLetter>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT Id, AppointmentLetterCode, JobApplicant
                                  FROM AppointmentLetters WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(idFilter))
                    query += " AND AppointmentLetterCode LIKE @IdFilter";
                if (!string.IsNullOrEmpty(applicantFilter))
                    query += " AND JobApplicant LIKE @ApplicantFilter";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(idFilter))
                        cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");
                    if (!string.IsNullOrEmpty(applicantFilter))
                        cmd.Parameters.AddWithValue("@ApplicantFilter", "%" + applicantFilter + "%");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            letters.Add(new AppointmentLetter
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                AppointmentLetterCode = reader["AppointmentLetterCode"]?.ToString() ?? string.Empty,
                                JobApplicant = reader["JobApplicant"]?.ToString() ?? string.Empty
                            });
                        }
                    }
                }
            }

            return letters;
        }

        public AppointmentLetter GetById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM AppointmentLetters WHERE Id = @Id AND IsDeleted = 0";

                AppointmentLetter letter = null;

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            letter = new AppointmentLetter
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                AppointmentLetterCode = reader["AppointmentLetterCode"]?.ToString() ?? string.Empty,
                                JobApplicant = reader["JobApplicant"]?.ToString() ?? string.Empty,
                                Company = reader["Company"]?.ToString() ?? string.Empty,
                                AppointmentDate = reader["AppointmentDate"] as DateTime?,
                                AppointmentLetterTemplate = reader["AppointmentLetterTemplate"]?.ToString() ?? string.Empty,
                                Introduction = reader["Introduction"]?.ToString() ?? string.Empty,
                                ClosingNotes = reader["ClosingNotes"]?.ToString() ?? string.Empty
                            };
                        }
                    }

                    if (letter != null)
                    {
                        letter.Terms = GetTerms(conn, letter.Id);
                    }
                }

                return letter;
            }
        }

        private List<AppointmentLetterTerm> GetTerms(SqlConnection conn, int appointmentLetterId)
        {
            var list = new List<AppointmentLetterTerm>();
            string query = "SELECT * FROM AppointmentLetterTerms WHERE AppointmentLetterId = @Id";

            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", appointmentLetterId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new AppointmentLetterTerm
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            AppointmentLetterId = appointmentLetterId,
                            Title = reader["Title"]?.ToString() ?? string.Empty,
                            Description = reader["Description"]?.ToString() ?? string.Empty
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
                string query = "SELECT COUNT(*) FROM AppointmentLetters";
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    int count = (int)cmd.ExecuteScalar() + 1;
                    return $"HR-APL-{DateTime.Now.Year}-{count:D5}";
                }
            }
        }

        public int Insert(AppointmentLetter letter)
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
                            INSERT INTO AppointmentLetters
                                (AppointmentLetterCode, JobApplicant, Company, AppointmentDate, AppointmentLetterTemplate,
                                 Introduction, ClosingNotes)
                            OUTPUT INSERTED.Id
                            VALUES
                                (@AppointmentLetterCode, @JobApplicant, @Company, @AppointmentDate, @AppointmentLetterTemplate,
                                 @Introduction, @ClosingNotes)";

                        using (var cmd = new SqlCommand(headerQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@AppointmentLetterCode", letter.AppointmentLetterCode ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@JobApplicant", letter.JobApplicant ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Company", letter.Company ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@AppointmentDate", (object)letter.AppointmentDate ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@AppointmentLetterTemplate", letter.AppointmentLetterTemplate ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Introduction", letter.Introduction ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@ClosingNotes", letter.ClosingNotes ?? (object)DBNull.Value);

                            newId = (int)cmd.ExecuteScalar();
                        }

                        if (letter.Terms != null)
                        {
                            foreach (var t in letter.Terms)
                            {
                                if (string.IsNullOrEmpty(t.Title)) continue;

                                string q = @"INSERT INTO AppointmentLetterTerms
                                             (AppointmentLetterId, Title, Description)
                                             VALUES (@AppointmentLetterId, @Title, @Description)";
                                using (var cmd = new SqlCommand(q, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@AppointmentLetterId", newId);
                                    cmd.Parameters.AddWithValue("@Title", t.Title ?? (object)DBNull.Value);
                                    cmd.Parameters.AddWithValue("@Description", t.Description ?? (object)DBNull.Value);
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