using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class InterviewTypeService
    {
        private readonly string _connectionString;

        public InterviewTypeService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<InterviewType> GetAll(string idFilter)
        {
            var list = new List<InterviewType>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT Id, Name, Description FROM InterviewTypes WHERE IsDeleted = 0";

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
                            list.Add(new InterviewType
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Name = reader["Name"]?.ToString() ?? string.Empty,
                                Description = reader["Description"]?.ToString() ?? string.Empty
                            });
                        }
                    }
                }
            }

            return list;
        }

        public InterviewType GetById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT Id, Name, Description FROM InterviewTypes WHERE Id = @Id AND IsDeleted = 0";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new InterviewType
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Name = reader["Name"]?.ToString() ?? string.Empty,
                                Description = reader["Description"]?.ToString() ?? string.Empty
                            };
                        }
                    }
                }
            }
            return null;
        }

        public int Insert(InterviewType t)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    INSERT INTO InterviewTypes (Name, Description)
                    OUTPUT INSERTED.Id
                    VALUES (@Name, @Description)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", t.Name ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Description", t.Description ?? (object)DBNull.Value);

                    conn.Open();
                    int newId = (int)cmd.ExecuteScalar();
                    return newId;
                }
            }
        }
    }
}
