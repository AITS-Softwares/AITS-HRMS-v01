using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class EmployeeFeedbackCriteriaService
    {
        private readonly string _connectionString;

        public EmployeeFeedbackCriteriaService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<EmployeeFeedbackCriteria> GetAll(string idFilter = null)
        {
            var list = new List<EmployeeFeedbackCriteria>();

            using var conn = new SqlConnection(_connectionString);

            string query = @"
                SELECT Id, CriteriaCode, Criteria
                FROM EmployeeFeedbackCriteria
                WHERE IsDeleted = 0";

            if (!string.IsNullOrWhiteSpace(idFilter))
                query += " AND (CriteriaCode LIKE @IdFilter OR Criteria LIKE @IdFilter)";

            query += " ORDER BY Id DESC";

            using var cmd = new SqlCommand(query, conn);

            if (!string.IsNullOrWhiteSpace(idFilter))
                cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter.Trim() + "%");

            conn.Open();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new EmployeeFeedbackCriteria
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    CriteriaCode = reader["CriteriaCode"]?.ToString() ?? string.Empty,
                    Criteria = reader["Criteria"]?.ToString() ?? string.Empty
                });
            }

            return list;
        }

        public EmployeeFeedbackCriteria GetById(int id)
        {
            using var conn = new SqlConnection(_connectionString);

            string query = @"
                SELECT Id, CriteriaCode, Criteria
                FROM EmployeeFeedbackCriteria
                WHERE Id = @Id AND IsDeleted = 0";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            conn.Open();

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new EmployeeFeedbackCriteria
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    CriteriaCode = reader["CriteriaCode"]?.ToString() ?? string.Empty,
                    Criteria = reader["Criteria"]?.ToString() ?? string.Empty
                };
            }

            return null;
        }

        public bool ExistsCriteria(string criteria, int? excludeId = null)
        {
            using var conn = new SqlConnection(_connectionString);

            string query = @"
                SELECT COUNT(1)
                FROM EmployeeFeedbackCriteria
                WHERE IsDeleted = 0
                AND UPPER(Criteria) = UPPER(@Criteria)
                AND (@ExcludeId IS NULL OR Id <> @ExcludeId)";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Criteria", criteria.Trim());
            cmd.Parameters.AddWithValue("@ExcludeId", excludeId.HasValue ? excludeId.Value : DBNull.Value);

            conn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public int GetLocalIdByCriteriaCode(string criteriaCode)
        {
            if (string.IsNullOrWhiteSpace(criteriaCode))
                return 0;

            using var conn = new SqlConnection(_connectionString);

            const string query = @"
                SELECT TOP 1 Id
                FROM EmployeeFeedbackCriteria
                WHERE CriteriaCode = @CriteriaCode
                  AND IsDeleted = 0";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@CriteriaCode", criteriaCode.Trim());

            conn.Open();

            object result = cmd.ExecuteScalar();

            return result == null || result == DBNull.Value
                ? 0
                : Convert.ToInt32(result);
        }

        public int Insert(EmployeeFeedbackCriteria model)
        {
            Validate(model);

            if (ExistsCriteria(model.Criteria))
                throw new Exception("Criteria already exists.");

            using var conn = new SqlConnection(_connectionString);

            string query = @"
                INSERT INTO EmployeeFeedbackCriteria (CriteriaCode, Criteria, IsDeleted)
                OUTPUT INSERTED.Id
                VALUES (@CriteriaCode, @Criteria, 0)";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@CriteriaCode", string.IsNullOrWhiteSpace(model.CriteriaCode) ? model.Criteria.Trim() : model.CriteriaCode.Trim());
            cmd.Parameters.AddWithValue("@Criteria", model.Criteria.Trim());

            conn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void Update(EmployeeFeedbackCriteria model)
        {
            Validate(model);

            if (ExistsCriteria(model.Criteria, model.Id))
                throw new Exception("Criteria already exists.");

            using var conn = new SqlConnection(_connectionString);

            string query = @"
                UPDATE EmployeeFeedbackCriteria
                SET CriteriaCode = @CriteriaCode,
                    Criteria = @Criteria
                WHERE Id = @Id AND IsDeleted = 0";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", model.Id);
            cmd.Parameters.AddWithValue("@CriteriaCode", string.IsNullOrWhiteSpace(model.CriteriaCode) ? model.Criteria.Trim() : model.CriteriaCode.Trim());
            cmd.Parameters.AddWithValue("@Criteria", model.Criteria.Trim());

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public void UpdateCriteriaCode(int id, string criteriaCode)
        {
            using var conn = new SqlConnection(_connectionString);

            string query = @"
                UPDATE EmployeeFeedbackCriteria
                SET CriteriaCode = @CriteriaCode
                WHERE Id = @Id";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@CriteriaCode", criteriaCode);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public void UpsertFromErpNext(string erpName, string criteria)
        {
            if (string.IsNullOrWhiteSpace(criteria))
                return;

            using var conn = new SqlConnection(_connectionString);

            string query = @"
                IF EXISTS (SELECT 1 FROM EmployeeFeedbackCriteria WHERE CriteriaCode = @CriteriaCode)
                BEGIN
                    UPDATE EmployeeFeedbackCriteria
                    SET Criteria = @Criteria,
                        IsDeleted = 0
                    WHERE CriteriaCode = @CriteriaCode
                END
                ELSE IF EXISTS (SELECT 1 FROM EmployeeFeedbackCriteria WHERE UPPER(Criteria) = UPPER(@Criteria))
                BEGIN
                    UPDATE EmployeeFeedbackCriteria
                    SET CriteriaCode = @CriteriaCode,
                        IsDeleted = 0
                    WHERE UPPER(Criteria) = UPPER(@Criteria)
                END
                ELSE
                BEGIN
                    INSERT INTO EmployeeFeedbackCriteria (CriteriaCode, Criteria, IsDeleted)
                    VALUES (@CriteriaCode, @Criteria, 0)
                END";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@CriteriaCode", erpName.Trim());
            cmd.Parameters.AddWithValue("@Criteria", criteria.Trim());

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public void SoftDelete(int id)
        {
            using var conn = new SqlConnection(_connectionString);

            string query = @"
                UPDATE EmployeeFeedbackCriteria
                SET IsDeleted = 1
                WHERE Id = @Id";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public void SoftDeleteMissingErpRecords(HashSet<string> erpNames)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            if (erpNames == null || erpNames.Count == 0)
            {
                using var cmd = new SqlCommand(@"
                    UPDATE EmployeeFeedbackCriteria
                    SET IsDeleted = 1
                    WHERE IsDeleted = 0
                      AND NULLIF(CriteriaCode, '') IS NOT NULL
                      AND CriteriaCode NOT LIKE 'EFC-%'",
                    conn);

                cmd.ExecuteNonQuery();
                return;
            }

            var parameterNames = new List<string>();
            int index = 0;

            foreach (string erpName in erpNames)
            {
                string parameterName = "@ErpName" + index++;
                parameterNames.Add(parameterName);
            }

            string query = @"
                UPDATE EmployeeFeedbackCriteria
                SET IsDeleted = 1
                WHERE IsDeleted = 0
                  AND NULLIF(CriteriaCode, '') IS NOT NULL
                  AND CriteriaCode NOT LIKE 'EFC-%'
                  AND CriteriaCode NOT IN (" +
                string.Join(",", parameterNames) + ")";

            using var deleteCmd = new SqlCommand(query, conn);

            index = 0;
            foreach (string erpName in erpNames)
            {
                deleteCmd.Parameters.AddWithValue("@ErpName" + index++, erpName);
            }

            deleteCmd.ExecuteNonQuery();
        }

        public string GetNextCode()
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("SELECT COUNT(*) FROM EmployeeFeedbackCriteria", conn);

            conn.Open();
            int count = Convert.ToInt32(cmd.ExecuteScalar()) + 1;
            return $"EFC-{count:D5}";
        }

        private static void Validate(EmployeeFeedbackCriteria model)
        {
            model.Criteria = model.Criteria?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(model.Criteria))
                throw new Exception("Criteria is required.");

            if (model.Criteria.Length > 140)
                throw new Exception("Criteria cannot be more than 140 characters.");
        }
    }
}
