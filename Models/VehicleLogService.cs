using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class VehicleLogService
    {
        private readonly string _connectionString;

        public VehicleLogService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<VehicleLog> GetAll(string idFilter, string employeeFilter)
        {
            var list = new List<VehicleLog>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM VehicleLogs WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(idFilter)) query += " AND CAST(Id AS VARCHAR) LIKE @IdFilter";
                if (!string.IsNullOrEmpty(employeeFilter)) query += " AND Employee LIKE @EmployeeFilter";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(idFilter)) cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");
                    if (!string.IsNullOrEmpty(employeeFilter)) cmd.Parameters.AddWithValue("@EmployeeFilter", "%" + employeeFilter + "%");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(MapVehicleLog(reader));
                        }
                    }
                }
            }

            return list;
        }

        public VehicleLog GetById(int id)
        {
            VehicleLog model = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand("SELECT * FROM VehicleLogs WHERE Id = @Id AND IsDeleted = 0", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model = MapVehicleLog(reader);
                        }
                    }
                }

                if (model != null)
                {
                    using (var cmd = new SqlCommand("SELECT * FROM VehicleLogServiceDetails WHERE VehicleLogId = @Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                model.ServiceDetails.Add(new VehicleLogServiceDetail
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    VehicleLogId = id,
                                    ServiceItem = reader["ServiceItem"]?.ToString() ?? string.Empty,
                                    Type = reader["Type"]?.ToString() ?? string.Empty,
                                    Frequency = reader["Frequency"]?.ToString() ?? string.Empty,
                                    Expense = reader["Expense"] != DBNull.Value ? Convert.ToDecimal(reader["Expense"]) : (decimal?)null
                                });
                            }
                        }
                    }
                }
            }

            return model;
        }

        public int Insert(VehicleLog v)
        {
            int newId;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand(@"
                    INSERT INTO VehicleLogs
                        (Series, LicensePlate, Employee, OdometerDate, CurrentOdometerValue, FuelQty, Supplier, FuelPrice, InvoiceRef)
                    OUTPUT INSERTED.Id
                    VALUES
                        (@Series, @LicensePlate, @Employee, @OdometerDate, @CurrentOdometerValue, @FuelQty, @Supplier, @FuelPrice, @InvoiceRef)", conn))
                {
                    cmd.Parameters.AddWithValue("@Series", v.Series ?? "HR-VLOG-.YYYY.-");
                    cmd.Parameters.AddWithValue("@LicensePlate", v.LicensePlate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Employee", v.Employee ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@OdometerDate", (object)v.OdometerDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CurrentOdometerValue", (object)v.CurrentOdometerValue ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FuelQty", (object)v.FuelQty ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Supplier", (object)v.Supplier ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FuelPrice", (object)v.FuelPrice ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@InvoiceRef", (object)v.InvoiceRef ?? DBNull.Value);

                    newId = (int)cmd.ExecuteScalar();
                }

                foreach (var s in v.ServiceDetails)
                {
                    if (string.IsNullOrWhiteSpace(s.ServiceItem)) continue;

                    using (var cmd = new SqlCommand(@"
                        INSERT INTO VehicleLogServiceDetails (VehicleLogId, ServiceItem, Type, Frequency, Expense)
                        VALUES (@VehicleLogId, @ServiceItem, @Type, @Frequency, @Expense)", conn))
                    {
                        cmd.Parameters.AddWithValue("@VehicleLogId", newId);
                        cmd.Parameters.AddWithValue("@ServiceItem", s.ServiceItem ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Type", (object)s.Type ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Frequency", (object)s.Frequency ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Expense", (object)s.Expense ?? DBNull.Value);

                        cmd.ExecuteNonQuery();
                    }
                }
            }

            return newId;
        }

        private VehicleLog MapVehicleLog(SqlDataReader reader)
        {
            return new VehicleLog
            {
                Id = Convert.ToInt32(reader["Id"]),
                Series = reader["Series"]?.ToString() ?? "HR-VLOG-.YYYY.-",
                LicensePlate = reader["LicensePlate"]?.ToString() ?? string.Empty,
                Employee = reader["Employee"]?.ToString() ?? string.Empty,
                OdometerDate = reader["OdometerDate"] as DateTime?,
                CurrentOdometerValue = reader["CurrentOdometerValue"] != DBNull.Value ? Convert.ToDecimal(reader["CurrentOdometerValue"]) : (decimal?)null,
                FuelQty = reader["FuelQty"] != DBNull.Value ? Convert.ToDecimal(reader["FuelQty"]) : (decimal?)null,
                Supplier = reader["Supplier"]?.ToString() ?? string.Empty,
                FuelPrice = reader["FuelPrice"] != DBNull.Value ? Convert.ToDecimal(reader["FuelPrice"]) : (decimal?)null,
                InvoiceRef = reader["InvoiceRef"]?.ToString() ?? string.Empty
            };
        }
    }
}
