using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class HRShiftScheduleAssignmentService
    {
        private readonly string _connectionString;

        public HRShiftScheduleAssignmentService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<HRShiftScheduleAssignment> GetAll(string idFilter, string employee, string employeeName)
        {
            var list = new List<HRShiftScheduleAssignment>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM HRShiftScheduleAssignments WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(idFilter)) query += " AND CAST(Id AS VARCHAR) LIKE @IdFilter";
                if (!string.IsNullOrEmpty(employee)) query += " AND Employee LIKE @Employee";
                if (!string.IsNullOrEmpty(employeeName)) query += " AND EmployeeName LIKE @EmployeeName";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(idFilter)) cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");
                    if (!string.IsNullOrEmpty(employee)) cmd.Parameters.AddWithValue("@Employee", "%" + employee + "%");
                    if (!string.IsNullOrEmpty(employeeName)) cmd.Parameters.AddWithValue("@EmployeeName", "%" + employeeName + "%");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(MapRow(reader));
                        }
                    }
                }
            }

            return list;
        }

        public HRShiftScheduleAssignment GetById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM HRShiftScheduleAssignments WHERE Id = @Id AND IsDeleted = 0";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            return MapRow(reader);
                    }
                }
            }
            return null;
        }

        private HRShiftScheduleAssignment MapRow(System.Data.Common.DbDataReader reader)
        {
            return new HRShiftScheduleAssignment
            {
                Id = Convert.ToInt32(reader["Id"]),
                Employee = reader["Employee"]?.ToString() ?? string.Empty,
                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
                Company = reader["Company"]?.ToString() ?? string.Empty,
                ShiftSchedule = reader["ShiftSchedule"]?.ToString() ?? string.Empty,
                ShiftLocation = reader["ShiftLocation"]?.ToString() ?? string.Empty,
                Enabled = Convert.ToBoolean(reader["Enabled"]),
                CreateShiftsAfter = Convert.ToDateTime(reader["CreateShiftsAfter"]),
                ShiftStatus = reader["ShiftStatus"]?.ToString() ?? "Active",
                ErpNextId = HasColumn(reader, "ErpNextId") && reader["ErpNextId"] != DBNull.Value ? reader["ErpNextId"].ToString() : string.Empty
            };
        }

        public int Insert(HRShiftScheduleAssignment s)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    INSERT INTO HRShiftScheduleAssignments
                        (ErpNextId, Employee, EmployeeName, Company, ShiftSchedule, ShiftLocation, Enabled, CreateShiftsAfter, ShiftStatus)
                    OUTPUT INSERTED.Id
                    VALUES
                        (@ErpNextId, @Employee, @EmployeeName, @Company, @ShiftSchedule, @ShiftLocation, @Enabled, @CreateShiftsAfter, @ShiftStatus)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", s.ErpNextId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Employee", s.Employee ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmployeeName", s.EmployeeName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Company", s.Company ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ShiftSchedule", s.ShiftSchedule ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ShiftLocation", s.ShiftLocation ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Enabled", s.Enabled);
                    cmd.Parameters.AddWithValue("@CreateShiftsAfter", s.CreateShiftsAfter);
                    cmd.Parameters.AddWithValue("@ShiftStatus", s.ShiftStatus ?? "Active");

                    conn.Open();
                    int newId = (int)cmd.ExecuteScalar();
                    return newId;
                }
            }
        }

        public int InsertOrUpdate(HRShiftScheduleAssignment s)
        {
            var existing = GetByErpNextId(s.ErpNextId);
            if (existing == null)
            {
                return Insert(s);
            }

            s.Id = existing.Id;
            Update(s);
            return existing.Id;
        }

        public HRShiftScheduleAssignment GetByErpNextId(string erpNextId)
        {
            if (string.IsNullOrWhiteSpace(erpNextId)) return null;

            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("SELECT TOP 1 * FROM HRShiftScheduleAssignments WHERE ErpNextId = @ErpNextId", conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", erpNextId);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) return MapRow(reader);
                    }
                }
            }

            return null;
        }

        public void Update(HRShiftScheduleAssignment s)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    UPDATE HRShiftScheduleAssignments
                    SET ErpNextId = @ErpNextId,
                        Employee = @Employee,
                        EmployeeName = @EmployeeName,
                        Company = @Company,
                        ShiftSchedule = @ShiftSchedule,
                        ShiftLocation = @ShiftLocation,
                        Enabled = @Enabled,
                        CreateShiftsAfter = @CreateShiftsAfter,
                        ShiftStatus = @ShiftStatus,
                        IsDeleted = 0
                    WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", s.Id);
                    cmd.Parameters.AddWithValue("@ErpNextId", s.ErpNextId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Employee", s.Employee ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmployeeName", s.EmployeeName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Company", s.Company ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ShiftSchedule", s.ShiftSchedule ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ShiftLocation", s.ShiftLocation ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Enabled", s.Enabled);
                    cmd.Parameters.AddWithValue("@CreateShiftsAfter", s.CreateShiftsAfter);
                    cmd.Parameters.AddWithValue("@ShiftStatus", s.ShiftStatus ?? "Active");

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void MarkDeletedById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("UPDATE HRShiftScheduleAssignments SET IsDeleted = 1 WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static bool HasColumn(System.Data.Common.DbDataReader reader, string column)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(column, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }
    }
}
