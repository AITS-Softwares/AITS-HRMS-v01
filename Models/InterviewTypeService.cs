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
                conn.Open();
                EnsureErpNextIdColumn(conn);
                string query = "SELECT Id, Name, Description FROM InterviewTypes WHERE IsDeleted = 0";

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
                string query = "SELECT Id, ErpNextId, Name, Description FROM InterviewTypes WHERE Id = @Id AND IsDeleted = 0";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    EnsureErpNextIdColumn(conn);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new InterviewType
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                ErpNextId = reader["ErpNextId"]?.ToString(),
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
                    IF COL_LENGTH('dbo.InterviewTypes','ErpNextId') IS NULL ALTER TABLE dbo.InterviewTypes ADD ErpNextId NVARCHAR(255) NULL;
                    INSERT INTO InterviewTypes (ErpNextId, Name, Description)
                    OUTPUT INSERTED.Id
                    VALUES (@ErpNextId, @Name, @Description)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", t.ErpNextId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Name", t.Name ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Description", t.Description ?? (object)DBNull.Value);

                    conn.Open();
                    int newId = (int)cmd.ExecuteScalar();
                    return newId;
                }
            }
        }

        public void Update(InterviewType t)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            EnsureErpNextIdColumn(conn);
            using var cmd = new SqlCommand("UPDATE InterviewTypes SET ErpNextId=@ErpNextId, Description=@Description WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", t.Id);
            cmd.Parameters.AddWithValue("@ErpNextId", t.ErpNextId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Description", t.Description ?? (object)DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        private static void EnsureErpNextIdColumn(SqlConnection conn)
        {
            using var cmd = new SqlCommand("IF COL_LENGTH('dbo.InterviewTypes','ErpNextId') IS NULL ALTER TABLE dbo.InterviewTypes ADD ErpNextId NVARCHAR(255) NULL;", conn);
            cmd.ExecuteNonQuery();
        }
    }
}
