using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class TrainingResultService
    {
        private readonly string _connectionString;

        public TrainingResultService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<TrainingResultListRow> GetAll(string idFilter)
        {
            var list = new List<TrainingResultListRow>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT Id, TrainingEvent FROM TrainingResults WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(idFilter))
                    query += " AND (CAST(Id AS VARCHAR) LIKE @IdFilter OR TrainingEvent LIKE @IdFilter)";

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
                            list.Add(new TrainingResultListRow
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                TrainingEvent = reader["TrainingEvent"]?.ToString() ?? string.Empty
                            });
                        }
                    }
                }
            }

            return list;
        }

        public TrainingResult GetById(int id)
        {
            TrainingResult model = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string query = "SELECT * FROM TrainingResults WHERE Id = @Id AND IsDeleted = 0";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model = new TrainingResult
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                TrainingEvent = reader["TrainingEvent"]?.ToString() ?? string.Empty
                            };
                        }
                    }
                }

                if (model == null) return null;

                string q2 = "SELECT Id, TrainingResultId, Employee, Hours, Grade, Comments FROM TrainingResultEmployees WHERE TrainingResultId = @Id";
                using (var cmd = new SqlCommand(q2, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.Employees.Add(new TrainingResultEmployee
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                TrainingResultId = Convert.ToInt32(reader["TrainingResultId"]),
                                Employee = reader["Employee"]?.ToString() ?? string.Empty,
                                Hours = reader["Hours"] as decimal?,
                                Grade = reader["Grade"]?.ToString() ?? string.Empty,
                                Comments = reader["Comments"]?.ToString() ?? string.Empty
                            });
                        }
                    }
                }
            }

            return model;
        }

        public int Insert(TrainingResult model,
            List<string> employees, List<decimal?> hours, List<string> grades, List<string> comments)
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
                            INSERT INTO TrainingResults (TrainingEvent, IsDeleted, CreatedOn)
                            OUTPUT INSERTED.Id
                            VALUES (@TrainingEvent, 0, GETDATE())";

                        using (var cmd = new SqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@TrainingEvent", model.TrainingEvent ?? (object)DBNull.Value);
                            newId = (int)cmd.ExecuteScalar();
                        }

                        if (employees != null)
                        {
                            for (int i = 0; i < employees.Count; i++)
                            {
                                if (string.IsNullOrWhiteSpace(employees[i])) continue;

                                string q = @"INSERT INTO TrainingResultEmployees
                                             (TrainingResultId, Employee, Hours, Grade, Comments)
                                             VALUES (@Tid, @Employee, @Hours, @Grade, @Comments)";

                                using (var cmd = new SqlCommand(q, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@Tid", newId);
                                    cmd.Parameters.AddWithValue("@Employee", employees[i]);
                                    cmd.Parameters.AddWithValue("@Hours", (hours != null && i < hours.Count && hours[i].HasValue) ? (object)hours[i] : DBNull.Value);
                                    cmd.Parameters.AddWithValue("@Grade", (grades != null && i < grades.Count) ? (object)grades[i] : DBNull.Value);
                                    cmd.Parameters.AddWithValue("@Comments", (comments != null && i < comments.Count) ? (object)comments[i] : DBNull.Value);
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
