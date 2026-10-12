using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class GrievanceTypeService
    {
        private readonly string _connectionString;

        public GrievanceTypeService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<GrievanceType> GetAll(string idFilter = null)
        {
            var list = new List<GrievanceType>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM GrievanceTypes WHERE IsDeleted = 0";

                if (!string.IsNullOrWhiteSpace(idFilter))
                    query += " AND Name LIKE @IdFilter";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrWhiteSpace(idFilter))
                        cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");

                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            list.Add(Map(reader));
                    }
                }
            }

            return list;
        }

        public GrievanceType GetById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand("SELECT * FROM GrievanceTypes WHERE Id = @Id AND IsDeleted = 0", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            return Map(reader);
                    }
                }
            }

            return null;
        }

        public GrievanceType GetByErpNextId(string erpNextId)
        {
            if (string.IsNullOrWhiteSpace(erpNextId))
                return null;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand("SELECT TOP 1 * FROM GrievanceTypes WHERE ErpNextId = @ErpNextId AND IsDeleted = 0", conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", erpNextId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            return Map(reader);
                    }
                }
            }

            return null;
        }

        public GrievanceType GetByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand("SELECT TOP 1 * FROM GrievanceTypes WHERE Name = @Name AND IsDeleted = 0", conn))
                {
                    cmd.Parameters.AddWithValue("@Name", name);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            return Map(reader);
                    }
                }
            }

            return null;
        }

        public int Insert(GrievanceType model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand(@"
                    INSERT INTO GrievanceTypes
                        (ErpNextId, Name, Description, IsDeleted)
                    OUTPUT INSERTED.Id
                    VALUES
                        (@ErpNextId, @Name, @Description, 0)", conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", (object)model.ErpNextId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Name", (object)model.Name ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Description", (object)model.Description ?? DBNull.Value);

                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        public void Update(GrievanceType model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand(@"
                    UPDATE GrievanceTypes SET
                        ErpNextId = @ErpNextId,
                        Name = @Name,
                        Description = @Description
                    WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@ErpNextId", (object)model.ErpNextId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Name", (object)model.Name ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Description", (object)model.Description ?? DBNull.Value);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateErpNextId(int id, string erpNextId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand("UPDATE GrievanceTypes SET ErpNextId = @ErpNextId WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@ErpNextId", (object)erpNextId ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public int UpsertFromErpNext(GrievanceType model)
        {
            var existing = GetByErpNextId(model.ErpNextId) ?? GetByName(model.Name);

            if (existing == null)
                return Insert(model);

            model.Id = existing.Id;
            Update(model);
            return existing.Id;
        }

        private GrievanceType Map(SqlDataReader reader)
        {
            return new GrievanceType
            {
                Id = Convert.ToInt32(reader["Id"]),
                ErpNextId = reader["ErpNextId"]?.ToString() ?? string.Empty,
                Name = reader["Name"]?.ToString() ?? string.Empty,
                Description = reader["Description"]?.ToString() ?? string.Empty
            };
        }
    }
}