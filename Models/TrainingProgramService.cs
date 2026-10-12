using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class TrainingProgramService
    {
        private readonly string _connectionString;

        public TrainingProgramService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<TrainingProgram> GetAll(string idFilter = null, string nameFilter = null)
        {
            var list = new List<TrainingProgram>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT Id, TrainingProgramCode, TrainingProgramName, Status
                                  FROM TrainingPrograms WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(idFilter)) query += " AND TrainingProgramCode LIKE @IdFilter";
                if (!string.IsNullOrEmpty(nameFilter)) query += " AND TrainingProgramName LIKE @NameFilter";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(idFilter)) cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");
                    if (!string.IsNullOrEmpty(nameFilter)) cmd.Parameters.AddWithValue("@NameFilter", "%" + nameFilter + "%");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new TrainingProgram
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                TrainingProgramCode = reader["TrainingProgramCode"]?.ToString() ?? string.Empty,
                                TrainingProgramName = reader["TrainingProgramName"]?.ToString() ?? string.Empty,
                                Status = reader["Status"]?.ToString() ?? string.Empty
                            });
                        }
                    }
                }
            }

            return list;
        }

        public TrainingProgram GetById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM TrainingPrograms WHERE Id = @Id AND IsDeleted = 0";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new TrainingProgram
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                TrainingProgramCode = reader["TrainingProgramCode"]?.ToString() ?? string.Empty,
                                TrainingProgramName = reader["TrainingProgramName"]?.ToString() ?? string.Empty,
                                Company = reader["Company"]?.ToString() ?? string.Empty,
                                Status = reader["Status"]?.ToString() ?? "Scheduled",
                                TrainerName = reader["TrainerName"]?.ToString() ?? string.Empty,
                                Supplier = reader["Supplier"]?.ToString() ?? string.Empty,
                                TrainerEmail = reader["TrainerEmail"]?.ToString() ?? string.Empty,
                                ContactNumber = reader["ContactNumber"]?.ToString() ?? string.Empty,
                                Description = reader["Description"]?.ToString() ?? string.Empty
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
                string query = "SELECT COUNT(*) FROM TrainingPrograms";
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    int count = (int)cmd.ExecuteScalar() + 1;
                    return $"TRN-PRG-{count:D5}";
                }
            }
        }

        public int Insert(TrainingProgram model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    INSERT INTO TrainingPrograms
                        (TrainingProgramCode, TrainingProgramName, Company, Status, TrainerName, Supplier,
                         TrainerEmail, ContactNumber, Description)
                    OUTPUT INSERTED.Id
                    VALUES
                        (@TrainingProgramCode, @TrainingProgramName, @Company, @Status, @TrainerName, @Supplier,
                         @TrainerEmail, @ContactNumber, @Description)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TrainingProgramCode", model.TrainingProgramCode ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@TrainingProgramName", model.TrainingProgramName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Company", model.Company ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", model.Status ?? "Scheduled");
                    cmd.Parameters.AddWithValue("@TrainerName", model.TrainerName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Supplier", model.Supplier ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@TrainerEmail", model.TrainerEmail ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ContactNumber", model.ContactNumber ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Description", model.Description ?? (object)DBNull.Value);

                    conn.Open();
                    return (int)cmd.ExecuteScalar();
                }
            }
        }
    }
}
