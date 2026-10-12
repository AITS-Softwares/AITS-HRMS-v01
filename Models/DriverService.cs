using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class DriverService
    {
        private readonly string _connectionString;

        public DriverService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Driver> GetAll(string idFilter, string fullNameFilter)
        {
            var list = new List<Driver>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Drivers WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(idFilter)) query += " AND DriverCode LIKE @IdFilter";
                if (!string.IsNullOrEmpty(fullNameFilter)) query += " AND FullName LIKE @FullNameFilter";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(idFilter)) cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");
                    if (!string.IsNullOrEmpty(fullNameFilter)) cmd.Parameters.AddWithValue("@FullNameFilter", "%" + fullNameFilter + "%");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(MapDriver(reader));
                        }
                    }
                }
            }

            return list;
        }

        public Driver GetById(int id)
        {
            Driver model = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand("SELECT * FROM Drivers WHERE Id = @Id AND IsDeleted = 0", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model = MapDriver(reader);
                        }
                    }
                }

                if (model != null)
                {
                    using (var cmd = new SqlCommand("SELECT * FROM DriverLicenseCategories WHERE DriverId = @Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                model.LicenseCategories.Add(new DriverLicenseCategory
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    DriverId = id,
                                    Category = reader["Category"]?.ToString() ?? string.Empty,
                                    Description = reader["Description"]?.ToString() ?? string.Empty,
                                    IssuingDate = reader["IssuingDate"] as DateTime?,
                                    ExpiryDate = reader["ExpiryDate"] as DateTime?
                                });
                            }
                        }
                    }
                }
            }

            return model;
        }

        public string GetNextCode()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Drivers", conn))
                {
                    conn.Open();
                    int count = (int)cmd.ExecuteScalar() + 1;
                    return $"DRV-{count:D5}";
                }
            }
        }

        public int Insert(Driver d)
        {
            int newId;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand(@"
                    INSERT INTO Drivers
                        (DriverCode, Series, FullName, Employee, CellNumber, Address, Transporter, LicenseNumber, IssuingDate, ExpiryDate, Status)
                    OUTPUT INSERTED.Id
                    VALUES
                        (@DriverCode, @Series, @FullName, @Employee, @CellNumber, @Address, @Transporter, @LicenseNumber, @IssuingDate, @ExpiryDate, @Status)", conn))
                {
                    cmd.Parameters.AddWithValue("@DriverCode", (object)d.DriverCode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Series", d.Series ?? "HR-DRI-.YYYY.-");
                    cmd.Parameters.AddWithValue("@FullName", d.FullName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Employee", (object)d.Employee ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CellNumber", (object)d.CellNumber ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address", (object)d.Address ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Transporter", (object)d.Transporter ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@LicenseNumber", (object)d.LicenseNumber ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IssuingDate", (object)d.IssuingDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ExpiryDate", (object)d.ExpiryDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", d.Status ?? "Active");

                    newId = (int)cmd.ExecuteScalar();
                }

                foreach (var cat in d.LicenseCategories)
                {
                    if (string.IsNullOrWhiteSpace(cat.Category)) continue;

                    using (var cmd = new SqlCommand(@"
                        INSERT INTO DriverLicenseCategories (DriverId, Category, Description, IssuingDate, ExpiryDate)
                        VALUES (@DriverId, @Category, @Description, @IssuingDate, @ExpiryDate)", conn))
                    {
                        cmd.Parameters.AddWithValue("@DriverId", newId);
                        cmd.Parameters.AddWithValue("@Category", cat.Category ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Description", (object)cat.Description ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IssuingDate", (object)cat.IssuingDate ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ExpiryDate", (object)cat.ExpiryDate ?? DBNull.Value);

                        cmd.ExecuteNonQuery();
                    }
                }
            }

            return newId;
        }

        private Driver MapDriver(SqlDataReader reader)
        {
            return new Driver
            {
                Id = Convert.ToInt32(reader["Id"]),
                DriverCode = reader["DriverCode"]?.ToString() ?? string.Empty,
                Series = reader["Series"]?.ToString() ?? "HR-DRI-.YYYY.-",
                FullName = reader["FullName"]?.ToString() ?? string.Empty,
                Employee = reader["Employee"]?.ToString() ?? string.Empty,
                CellNumber = reader["CellNumber"]?.ToString() ?? string.Empty,
                Address = reader["Address"]?.ToString() ?? string.Empty,
                Transporter = reader["Transporter"]?.ToString() ?? string.Empty,
                LicenseNumber = reader["LicenseNumber"]?.ToString() ?? string.Empty,
                IssuingDate = reader["IssuingDate"] as DateTime?,
                ExpiryDate = reader["ExpiryDate"] as DateTime?,
                Status = reader["Status"]?.ToString() ?? "Active"
            };
        }
    }
}