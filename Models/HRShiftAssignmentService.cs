using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class HRShiftAssignmentService
    {
        private readonly string _connectionString;

        public HRShiftAssignmentService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<HRShiftAssignment> GetAll(string idFilter, string employee, string employeeName, string shiftType)
        {
            var list = new List<HRShiftAssignment>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM HRShiftAssignments WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(idFilter)) query += " AND CAST(Id AS VARCHAR) LIKE @IdFilter";
                if (!string.IsNullOrEmpty(employee)) query += " AND Employee LIKE @Employee";
                if (!string.IsNullOrEmpty(employeeName)) query += " AND EmployeeName LIKE @EmployeeName";
                if (!string.IsNullOrEmpty(shiftType)) query += " AND ShiftType LIKE @ShiftType";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(idFilter)) cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");
                    if (!string.IsNullOrEmpty(employee)) cmd.Parameters.AddWithValue("@Employee", "%" + employee + "%");
                    if (!string.IsNullOrEmpty(employeeName)) cmd.Parameters.AddWithValue("@EmployeeName", "%" + employeeName + "%");
                    if (!string.IsNullOrEmpty(shiftType)) cmd.Parameters.AddWithValue("@ShiftType", "%" + shiftType + "%");

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

        public HRShiftAssignment GetById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM HRShiftAssignments WHERE Id = @Id AND IsDeleted = 0";
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

        private HRShiftAssignment MapRow(System.Data.Common.DbDataReader reader)
        {
            return new HRShiftAssignment
            {
                Id = Convert.ToInt32(reader["Id"]),
                Employee = reader["Employee"]?.ToString() ?? string.Empty,
                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
                Company = reader["Company"]?.ToString() ?? string.Empty,
                ShiftType = reader["ShiftType"]?.ToString() ?? string.Empty,
                ShiftLocation = reader["ShiftLocation"]?.ToString() ?? string.Empty,
                StartDate = Convert.ToDateTime(reader["StartDate"]),
                EndDate = reader["EndDate"] != DBNull.Value ? Convert.ToDateTime(reader["EndDate"]) : (DateTime?)null,
                Status = reader["Status"]?.ToString() ?? "Active",
                ErpNextId = HasColumn(reader, "ErpNextId") && reader["ErpNextId"] != DBNull.Value ? reader["ErpNextId"].ToString() : string.Empty
            };
        }

        public int Insert(HRShiftAssignment s)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    INSERT INTO HRShiftAssignments
                        (ErpNextId, Employee, EmployeeName, Company, ShiftType, ShiftLocation, StartDate, EndDate, Status)
                    OUTPUT INSERTED.Id
                    VALUES
                        (@ErpNextId, @Employee, @EmployeeName, @Company, @ShiftType, @ShiftLocation, @StartDate, @EndDate, @Status)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", s.ErpNextId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Employee", s.Employee ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmployeeName", s.EmployeeName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Company", s.Company ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ShiftType", s.ShiftType ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ShiftLocation", s.ShiftLocation ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@StartDate", s.StartDate);
                    cmd.Parameters.AddWithValue("@EndDate", (object)s.EndDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", s.Status ?? "Active");

                    conn.Open();
                    int newId = (int)cmd.ExecuteScalar();
                    return newId;
                }
            }
        }

        public int InsertOrUpdate(HRShiftAssignment s)
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

        public HRShiftAssignment GetByErpNextId(string erpNextId)
        {
            if (string.IsNullOrWhiteSpace(erpNextId)) return null;

            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("SELECT TOP 1 * FROM HRShiftAssignments WHERE ErpNextId = @ErpNextId", conn))
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

        public void Update(HRShiftAssignment s)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    UPDATE HRShiftAssignments
                    SET ErpNextId = @ErpNextId,
                        Employee = @Employee,
                        EmployeeName = @EmployeeName,
                        Company = @Company,
                        ShiftType = @ShiftType,
                        ShiftLocation = @ShiftLocation,
                        StartDate = @StartDate,
                        EndDate = @EndDate,
                        Status = @Status,
                        IsDeleted = 0
                    WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", s.Id);
                    cmd.Parameters.AddWithValue("@ErpNextId", s.ErpNextId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Employee", s.Employee ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmployeeName", s.EmployeeName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Company", s.Company ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ShiftType", s.ShiftType ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ShiftLocation", s.ShiftLocation ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@StartDate", s.StartDate);
                    cmd.Parameters.AddWithValue("@EndDate", (object)s.EndDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", s.Status ?? "Active");

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void MarkDeletedById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("UPDATE HRShiftAssignments SET IsDeleted = 1 WHERE Id = @Id", conn))
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
