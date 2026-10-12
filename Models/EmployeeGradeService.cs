using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class EmployeeGradeService
    {
        private readonly string _connectionString;

        public EmployeeGradeService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<EmployeeGrade> GetAllGrades()
        {
            var grades = new List<EmployeeGrade>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT
                        Id,
                        GradeName,
                        DefaultSalaryStructure,
                        DefaultBasePay,
                        ErpNextId
                    FROM EmployeeGrades
                    WHERE IsDeleted = 0
                    ORDER BY GradeName";

                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            grades.Add(MapReaderToEmployeeGrade(reader));
                        }
                    }
                }
            }

            return grades;
        }

        public EmployeeGrade GetById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(@"
                SELECT TOP 1
                    Id,
                    GradeName,
                    DefaultSalaryStructure,
                    DefaultBasePay,
                    ErpNextId
                FROM EmployeeGrades
                WHERE Id = @Id AND IsDeleted = 0", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read()) return null;

                    return MapReaderToEmployeeGrade(reader);
                }
            }
        }

        public HashSet<string> GetExistingErpNextIds()
        {
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT ErpNextId
                    FROM EmployeeGrades
                    WHERE ErpNextId IS NOT NULL
                      AND ErpNextId != ''
                      AND IsDeleted = 0";

                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ids.Add(reader["ErpNextId"].ToString());
                        }
                    }
                }
            }

            return ids;
        }

        public int InsertGrade(EmployeeGrade grade)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    INSERT INTO EmployeeGrades
                    (
                        ErpNextId,
                        GradeName,
                        DefaultSalaryStructure,
                        DefaultBasePay,
                        IsDeleted
                    )
                    OUTPUT INSERTED.Id
                    VALUES
                    (
                        @ErpNextId,
                        @GradeName,
                        @DefaultSalaryStructure,
                        @DefaultBasePay,
                        0
                    )";

                using (var cmd = new SqlCommand(query, conn))
                {
                    AddEmployeeGradeParameters(cmd, grade);

                    conn.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void Update(EmployeeGrade grade)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    UPDATE EmployeeGrades
                    SET
                        GradeName = @GradeName,
                        DefaultSalaryStructure = @DefaultSalaryStructure,
                        DefaultBasePay = @DefaultBasePay,
                        ErpNextId = @ErpNextId
                    WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", grade.Id);
                    AddEmployeeGradeParameters(cmd, grade);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateErpNextId(int id, string erpNextId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    UPDATE EmployeeGrades
                    SET ErpNextId = @ErpNextId
                    WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", DbValue(erpNextId));
                    cmd.Parameters.AddWithValue("@Id", id);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public int InsertOrUpdateFromErpNext(EmployeeGrade grade)
        {
            if (grade == null || string.IsNullOrWhiteSpace(grade.ErpNextId))
            {
                throw new Exception("ERPNext Employee Grade ID is required.");
            }

            var existing = GetAllGrades()
                .Find(g => string.Equals(g.ErpNextId, grade.ErpNextId, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                grade.Id = existing.Id;
                Update(grade);
                return existing.Id;
            }

            return InsertGrade(grade);
        }

        public int MarkMissingErpNextGradesAsDeleted(HashSet<string> remoteIds)
        {
            int deletedCount = 0;

            remoteIds ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var grade in GetAllGrades())
            {
                if (!string.IsNullOrWhiteSpace(grade.ErpNextId) &&
                    !remoteIds.Contains(grade.ErpNextId))
                {
                    MarkDeletedById(grade.Id);
                    deletedCount++;
                }
            }

            return deletedCount;
        }

        public void MarkDeletedById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    UPDATE EmployeeGrades
                    SET IsDeleted = 1
                    WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void MarkDeletedByErpNextId(string erpNextId)
        {
            if (string.IsNullOrWhiteSpace(erpNextId))
                return;

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    UPDATE EmployeeGrades
                    SET IsDeleted = 1
                    WHERE ErpNextId = @ErpNextId";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", erpNextId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int id)
        {
            MarkDeletedById(id);
        }

        private static EmployeeGrade MapReaderToEmployeeGrade(SqlDataReader reader)
        {
            return new EmployeeGrade
            {
                Id = Convert.ToInt32(reader["Id"]),
                GradeName = reader["GradeName"]?.ToString() ?? string.Empty,
                DefaultSalaryStructure = reader["DefaultSalaryStructure"]?.ToString() ?? string.Empty,
                DefaultBasePay = reader["DefaultBasePay"] == DBNull.Value
                    ? null
                    : Convert.ToDecimal(reader["DefaultBasePay"]),
                ErpNextId = reader["ErpNextId"] == DBNull.Value
                    ? null
                    : reader["ErpNextId"].ToString()
            };
        }

        private static void AddEmployeeGradeParameters(SqlCommand cmd, EmployeeGrade grade)
        {
            cmd.Parameters.AddWithValue("@ErpNextId", DbValue(grade.ErpNextId));
            cmd.Parameters.AddWithValue("@GradeName", DbValue(grade.GradeName));
            cmd.Parameters.AddWithValue("@DefaultSalaryStructure", DbValue(grade.DefaultSalaryStructure));
            cmd.Parameters.AddWithValue("@DefaultBasePay", DbValue(grade.DefaultBasePay));
        }

        private static object DbValue(object value)
        {
            return value ?? DBNull.Value;
        }
    }
}