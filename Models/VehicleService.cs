using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class VehicleService
    {
        private readonly string _connectionString;

        public VehicleService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Vehicle> GetAll(string idFilter = null, string modelFilter = null, string employeeFilter = null, string fuelTypeFilter = null)
        {
            var list = new List<Vehicle>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT Id, VehicleCode, LicensePlate, Model, Employee, FuelType
                                  FROM Vehicles WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(idFilter)) query += " AND VehicleCode LIKE @IdFilter";
                if (!string.IsNullOrEmpty(modelFilter)) query += " AND Model LIKE @ModelFilter";
                if (!string.IsNullOrEmpty(employeeFilter)) query += " AND Employee LIKE @EmployeeFilter";
                if (!string.IsNullOrEmpty(fuelTypeFilter)) query += " AND FuelType = @FuelTypeFilter";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(idFilter)) cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");
                    if (!string.IsNullOrEmpty(modelFilter)) cmd.Parameters.AddWithValue("@ModelFilter", "%" + modelFilter + "%");
                    if (!string.IsNullOrEmpty(employeeFilter)) cmd.Parameters.AddWithValue("@EmployeeFilter", "%" + employeeFilter + "%");
                    if (!string.IsNullOrEmpty(fuelTypeFilter)) cmd.Parameters.AddWithValue("@FuelTypeFilter", fuelTypeFilter);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new Vehicle
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                VehicleCode = reader["VehicleCode"]?.ToString() ?? string.Empty,
                                LicensePlate = reader["LicensePlate"]?.ToString() ?? string.Empty,
                                Model = reader["Model"]?.ToString() ?? string.Empty,
                                Employee = reader["Employee"]?.ToString() ?? string.Empty,
                                FuelType = reader["FuelType"]?.ToString() ?? string.Empty
                            });
                        }
                    }
                }
            }

            return list;
        }

        public Vehicle GetById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Vehicles WHERE Id = @Id AND IsDeleted = 0";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Vehicle
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                VehicleCode = reader["VehicleCode"]?.ToString() ?? string.Empty,
                                LicensePlate = reader["LicensePlate"]?.ToString() ?? string.Empty,
                                Model = reader["Model"]?.ToString() ?? string.Empty,
                                Make = reader["Make"]?.ToString() ?? string.Empty,
                                OdometerValueLast = reader["OdometerValueLast"] != DBNull.Value ? Convert.ToDecimal(reader["OdometerValueLast"]) : 0,
                                ChassisNo = reader["ChassisNo"]?.ToString() ?? string.Empty,
                                AcquisitionDate = reader["AcquisitionDate"] as DateTime?,
                                VehicleValue = reader["VehicleValue"] != DBNull.Value ? Convert.ToDecimal(reader["VehicleValue"]) : (decimal?)null,
                                Location = reader["Location"]?.ToString() ?? string.Empty,
                                Employee = reader["Employee"]?.ToString() ?? string.Empty,
                                InsuranceCompany = reader["InsuranceCompany"]?.ToString() ?? string.Empty,
                                InsuranceStartDate = reader["InsuranceStartDate"] as DateTime?,
                                PolicyNo = reader["PolicyNo"]?.ToString() ?? string.Empty,
                                InsuranceEndDate = reader["InsuranceEndDate"] as DateTime?,
                                FuelType = reader["FuelType"]?.ToString() ?? "Petrol",
                                Color = reader["Color"]?.ToString() ?? string.Empty,
                                FuelUOM = reader["FuelUOM"]?.ToString() ?? string.Empty,
                                Wheels = reader["Wheels"] != DBNull.Value ? Convert.ToInt32(reader["Wheels"]) : (int?)null,
                                LastCarbonCheck = reader["LastCarbonCheck"] as DateTime?,
                                Doors = reader["Doors"] != DBNull.Value ? Convert.ToInt32(reader["Doors"]) : (int?)null
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
                string query = "SELECT COUNT(*) FROM Vehicles";
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    int count = (int)cmd.ExecuteScalar() + 1;
                    return $"VEH-{count:D5}";
                }
            }
        }

        public bool ExistsByErpNextId(string erpNextId)
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("SELECT COUNT(1) FROM Vehicles WHERE ErpNextId = @ErpNextId AND IsDeleted = 0", conn))
            {
                cmd.Parameters.AddWithValue("@ErpNextId", erpNextId);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public void UpdateErpNextId(int id, string erpNextId)
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("UPDATE Vehicles SET ErpNextId = @ErpNextId WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@ErpNextId", erpNextId ?? (object)DBNull.Value);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void UpsertFromErpNext(Vehicle model)
        {
            if (ExistsByErpNextId(model.ErpNextId))
            {
                UpdateFromErpNext(model);
                return;
            }

            Insert(model);
        }

        public void UpdateFromErpNext(Vehicle model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
            UPDATE Vehicles SET
                VehicleCode = @VehicleCode,
                LicensePlate = @LicensePlate,
                Model = @Model,
                Make = @Make,
                OdometerValueLast = @OdometerValueLast,
                ChassisNo = @ChassisNo,
                AcquisitionDate = @AcquisitionDate,
                VehicleValue = @VehicleValue,
                Location = @Location,
                Employee = @Employee,
                InsuranceCompany = @InsuranceCompany,
                InsuranceStartDate = @InsuranceStartDate,
                PolicyNo = @PolicyNo,
                InsuranceEndDate = @InsuranceEndDate,
                FuelType = @FuelType,
                Color = @Color,
                FuelUOM = @FuelUOM,
                Wheels = @Wheels,
                LastCarbonCheck = @LastCarbonCheck,
                Doors = @Doors
            WHERE ErpNextId = @ErpNextId";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", model.ErpNextId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@VehicleCode", model.VehicleCode ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@LicensePlate", model.LicensePlate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Model", model.Model ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Make", model.Make ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@OdometerValueLast", model.OdometerValueLast);
                    cmd.Parameters.AddWithValue("@ChassisNo", model.ChassisNo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AcquisitionDate", (object)model.AcquisitionDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@VehicleValue", (object)model.VehicleValue ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Location", model.Location ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Employee", model.Employee ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@InsuranceCompany", model.InsuranceCompany ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@InsuranceStartDate", (object)model.InsuranceStartDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PolicyNo", model.PolicyNo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@InsuranceEndDate", (object)model.InsuranceEndDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FuelType", model.FuelType ?? "Petrol");
                    cmd.Parameters.AddWithValue("@Color", model.Color ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FuelUOM", model.FuelUOM ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Wheels", (object)model.Wheels ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@LastCarbonCheck", (object)model.LastCarbonCheck ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Doors", (object)model.Doors ?? DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }





        public int Insert(Vehicle model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
            INSERT INTO Vehicles
                (ErpNextId, VehicleCode, LicensePlate, Model, Make, OdometerValueLast, ChassisNo, AcquisitionDate,
                 VehicleValue, Location, Employee, InsuranceCompany, InsuranceStartDate, PolicyNo,
                 InsuranceEndDate, FuelType, Color, FuelUOM, Wheels, LastCarbonCheck, Doors)
            OUTPUT INSERTED.Id
            VALUES
                (@ErpNextId, @VehicleCode, @LicensePlate, @Model, @Make, @OdometerValueLast, @ChassisNo, @AcquisitionDate,
                 @VehicleValue, @Location, @Employee, @InsuranceCompany, @InsuranceStartDate, @PolicyNo,
                 @InsuranceEndDate, @FuelType, @Color, @FuelUOM, @Wheels, @LastCarbonCheck, @Doors)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", model.ErpNextId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@VehicleCode", model.VehicleCode ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@LicensePlate", model.LicensePlate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Model", model.Model ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Make", model.Make ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@OdometerValueLast", model.OdometerValueLast);
                    cmd.Parameters.AddWithValue("@ChassisNo", model.ChassisNo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AcquisitionDate", (object)model.AcquisitionDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@VehicleValue", (object)model.VehicleValue ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Location", model.Location ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Employee", model.Employee ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@InsuranceCompany", model.InsuranceCompany ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@InsuranceStartDate", (object)model.InsuranceStartDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PolicyNo", model.PolicyNo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@InsuranceEndDate", (object)model.InsuranceEndDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FuelType", model.FuelType ?? "Petrol");
                    cmd.Parameters.AddWithValue("@Color", model.Color ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FuelUOM", model.FuelUOM ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Wheels", (object)model.Wheels ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@LastCarbonCheck", (object)model.LastCarbonCheck ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Doors", (object)model.Doors ?? DBNull.Value);

                    conn.Open();
                    return (int)cmd.ExecuteScalar();
                }
            }
        }
    }
}