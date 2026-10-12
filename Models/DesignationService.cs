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
    }
}