using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class DepartmentService
    {
        private readonly string _connectionString;

        public DepartmentService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Department> GetAll()
        {
            var list = new List<Department>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT * FROM Departments WHERE IsDeleted = 0 ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(MapDepartment(reader));
                        }
                    }
                }
            }

            return list;
        }

        // Alias for HRController
        public List<Department> GetAllDepartments() => GetAll();

        public Department GetById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT * FROM Departments WHERE Id = @Id AND IsDeleted = 0";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapDepartment(reader);
                        }
                    }
                }
            }

            return null;
        }

        public void UpdateDepartmentById(Department dep)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
            UPDATE Departments
            SET DepartmentCode = @DepartmentCode,
                DepartmentName = @DepartmentName,
                Company = @Company,
                ParentDepartment = @ParentDepartment,
                IsGroup = @IsGroup,
                Disabled = @Disabled,
                PayrollCostCenter = @PayrollCostCenter,
                LeaveBlockList = @LeaveBlockList,
                Status = @Status,
                IsDeleted = 0
            WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", dep.Id);
                    cmd.Parameters.AddWithValue("@DepartmentCode", (object)dep.DepartmentCode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DepartmentName", (object)dep.DepartmentName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Company", (object)dep.Company ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ParentDepartment", (object)dep.ParentDepartment ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsGroup", dep.IsGroup);
                    cmd.Parameters.AddWithValue("@Disabled", dep.Disabled);
                    cmd.Parameters.AddWithValue("@PayrollCostCenter", (object)dep.PayrollCostCenter ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@LeaveBlockList", (object)dep.LeaveBlockList ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", dep.Disabled ? "Disabled" : "Active");

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void MarkDeletedById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"UPDATE Departments SET IsDeleted = 1 WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void MarkDeletedByCode(string departmentCode)
        {
            if (string.IsNullOrWhiteSpace(departmentCode))
                return;

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
            UPDATE Departments
            SET IsDeleted = 1
            WHERE DepartmentCode = @DepartmentCode";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@DepartmentCode", departmentCode);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }


















        public HashSet<string> GetExistingCodes()
        {
            var codes = new HashSet<string>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT DepartmentCode FROM Departments WHERE IsDeleted = 0 AND DepartmentCode IS NOT NULL";
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string code = reader["DepartmentCode"]?.ToString();
                            if (!string.IsNullOrEmpty(code)) codes.Add(code);
                        }
                    }
                }
            }
            return codes;
        }

        // Aliases for HRController
        public HashSet<string> GetExistingDepartmentNames() => GetExistingCodes();

        public int Insert(Department dep)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    INSERT INTO Departments
                        (DepartmentCode, DepartmentName, Company, ParentDepartment, IsGroup, Disabled, PayrollCostCenter, LeaveBlockList, Status)
                    OUTPUT INSERTED.Id
                    VALUES
                        (@DepartmentCode, @DepartmentName, @Company, @ParentDepartment, @IsGroup, @Disabled, @PayrollCostCenter, @LeaveBlockList, @Status)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@DepartmentCode", (object)dep.DepartmentCode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DepartmentName", (object)dep.DepartmentName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Company", dep.Company ?? "Vinod Cookware India Private Limited");
                    cmd.Parameters.AddWithValue("@ParentDepartment", (object)dep.ParentDepartment ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsGroup", dep.IsGroup);
                    cmd.Parameters.AddWithValue("@Disabled", dep.Disabled);
                    cmd.Parameters.AddWithValue("@PayrollCostCenter", (object)dep.PayrollCostCenter ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@LeaveBlockList", (object)dep.LeaveBlockList ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", dep.Disabled ? "Disabled" : "Active");

                    conn.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        // Alias for HRController
        public int InsertDepartment(Department dep) => Insert(dep);

        public void UpdateByCode(Department dep)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    UPDATE Departments
                    SET DepartmentName = @DepartmentName,
                        Company = @Company,
                        ParentDepartment = @ParentDepartment,
                        IsGroup = @IsGroup,
                        Disabled = @Disabled,
                        PayrollCostCenter = @PayrollCostCenter,
                        LeaveBlockList = @LeaveBlockList,
                        Status = @Status,
                        IsDeleted = 0
                    WHERE DepartmentCode = @DepartmentCode OR DepartmentName = @DepartmentName";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@DepartmentCode", (object)dep.DepartmentCode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DepartmentName", (object)dep.DepartmentName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Company", dep.Company ?? "Vinod Cookware India Private Limited");
                    cmd.Parameters.AddWithValue("@ParentDepartment", (object)dep.ParentDepartment ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsGroup", dep.IsGroup);
                    cmd.Parameters.AddWithValue("@Disabled", dep.Disabled);
                    cmd.Parameters.AddWithValue("@PayrollCostCenter", (object)dep.PayrollCostCenter ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@LeaveBlockList", (object)dep.LeaveBlockList ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", dep.Disabled ? "Disabled" : "Active");

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Alias for HRController
        public void UpdateDepartmentByName(Department dep) => UpdateByCode(dep);

        private Department MapDepartment(SqlDataReader reader)
        {
            string code = string.Empty;
            try { code = reader["DepartmentCode"]?.ToString() ?? string.Empty; } catch { }

            bool isDisabled = reader["Disabled"] != DBNull.Value && Convert.ToBoolean(reader["Disabled"]);

            return new Department
            {
                Id = Convert.ToInt32(reader["Id"]),
                DepartmentCode = code,
                DepartmentName = reader["DepartmentName"]?.ToString() ?? string.Empty,
                Company = reader["Company"]?.ToString() ?? "Vinod Cookware India Private Limited",
                ParentDepartment = reader["ParentDepartment"]?.ToString() ?? string.Empty,
                IsGroup = reader["IsGroup"] != DBNull.Value && Convert.ToBoolean(reader["IsGroup"]),
                Disabled = isDisabled,
                PayrollCostCenter = reader["PayrollCostCenter"]?.ToString() ?? string.Empty,
                LeaveBlockList = reader["LeaveBlockList"]?.ToString() ?? string.Empty,
                Status = isDisabled ? "Disabled" : "Active"
            };
        }
    }
}