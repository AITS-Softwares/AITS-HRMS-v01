using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class DesignationService
    {
        private readonly string _connectionString;

        public DesignationService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Designation> GetAllDesignations()
        {
            var designations = new List<Designation>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT Id, DesignationCode, DesignationName, Description, AppraisalTemplate FROM Designations WHERE IsDeleted = 0 ORDER BY DesignationName";

                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            designations.Add(new Designation
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                DesignationCode = reader["DesignationCode"]?.ToString() ?? string.Empty,
                                DesignationName = reader["DesignationName"]?.ToString() ?? string.Empty,
                                Description = reader["Description"]?.ToString() ?? string.Empty,
                                AppraisalTemplate = reader["AppraisalTemplate"]?.ToString() ?? string.Empty
                            });
                        }
                    }
                }
            }

            return designations;
        }

        public HashSet<string> GetExistingNames()
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT DesignationName FROM Designations WHERE IsDeleted = 0 AND DesignationName IS NOT NULL";
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string name = reader["DesignationName"]?.ToString();
                            if (!string.IsNullOrEmpty(name)) names.Add(name);
                        }
                    }
                }
            }
            return names;
        }

        public int InsertDesignation(Designation d)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    INSERT INTO Designations (DesignationCode, DesignationName, Description, AppraisalTemplate) 
                    OUTPUT INSERTED.Id
                    VALUES (@DesignationCode, @DesignationName, @Description, @AppraisalTemplate)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@DesignationCode", (object)d.DesignationCode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DesignationName", d.DesignationName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Description", (object)d.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AppraisalTemplate", (object)d.AppraisalTemplate ?? DBNull.Value);
                    conn.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public Designation GetByName(string designationName)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT Id, DesignationCode, DesignationName, Description, AppraisalTemplate FROM Designations WHERE DesignationName = @Name AND IsDeleted = 0";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", designationName ?? (object)DBNull.Value);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Designation
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                DesignationCode = reader["DesignationCode"]?.ToString() ?? string.Empty,
                                DesignationName = reader["DesignationName"]?.ToString() ?? string.Empty,
                                Description = reader["Description"]?.ToString() ?? string.Empty,
                                AppraisalTemplate = reader["AppraisalTemplate"]?.ToString() ?? string.Empty
                            };
                        }
                    }
                }
            }
            return null;
        }

        public Designation GetById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(@"
                SELECT Id, DesignationCode, DesignationName, Description, AppraisalTemplate
                FROM Designations
                WHERE Id = @Id AND IsDeleted = 0", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    return new Designation
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        DesignationCode = reader["DesignationCode"]?.ToString() ?? string.Empty,
                        DesignationName = reader["DesignationName"]?.ToString() ?? string.Empty,
                        Description = reader["Description"]?.ToString() ?? string.Empty,
                        AppraisalTemplate = reader["AppraisalTemplate"]?.ToString() ?? string.Empty
                    };
                }
            }
        }

        public int UpsertFromErpNext(Designation designation)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                int id = 0;

                using (var find = new SqlCommand(@"
                    SELECT TOP 1 Id
                    FROM Designations
                    WHERE DesignationCode = @DesignationCode
                       OR (NULLIF(DesignationCode, '') IS NULL AND DesignationName = @DesignationName)
                    ORDER BY CASE WHEN DesignationCode = @DesignationCode THEN 0 ELSE 1 END", conn))
                {
                    find.Parameters.AddWithValue("@DesignationCode", designation.DesignationCode ?? string.Empty);
                    find.Parameters.AddWithValue("@DesignationName", designation.DesignationName ?? string.Empty);
                    object value = find.ExecuteScalar();
                    if (value != null && value != DBNull.Value)
                        id = Convert.ToInt32(value);
                }

                if (id > 0)
                {
                    using (var update = new SqlCommand(@"
                        UPDATE Designations
                        SET DesignationCode = @DesignationCode,
                            DesignationName = @DesignationName,
                            Description = @Description,
                            AppraisalTemplate = @AppraisalTemplate,
                            IsDeleted = 0
                        WHERE Id = @Id", conn))
                    {
                        AddParameters(update, designation);
                        update.Parameters.AddWithValue("@Id", id);
                        update.ExecuteNonQuery();
                    }

                    return id;
                }

                using (var insert = new SqlCommand(@"
                    INSERT INTO Designations (DesignationCode, DesignationName, Description, AppraisalTemplate, IsDeleted)
                    OUTPUT INSERTED.Id
                    VALUES (@DesignationCode, @DesignationName, @Description, @AppraisalTemplate, 0)", conn))
                {
                    AddParameters(insert, designation);
                    return Convert.ToInt32(insert.ExecuteScalar());
                }
            }
        }

        public void UpdateDesignationByName(Designation d)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    UPDATE Designations 
                    SET DesignationCode = @DesignationCode,
                        Description = @Description,
                        AppraisalTemplate = @AppraisalTemplate,
                        IsDeleted = 0
                    WHERE DesignationName = @DesignationName";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@DesignationCode", (object)d.DesignationCode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DesignationName", d.DesignationName);
                    cmd.Parameters.AddWithValue("@Description", (object)d.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AppraisalTemplate", (object)d.AppraisalTemplate ?? DBNull.Value);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void AddParameters(SqlCommand cmd, Designation designation)
        {
            cmd.Parameters.AddWithValue("@DesignationCode", (object)designation.DesignationCode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DesignationName", (object)designation.DesignationName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Description", (object)designation.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AppraisalTemplate", (object)designation.AppraisalTemplate ?? DBNull.Value);
        }
    }
}
