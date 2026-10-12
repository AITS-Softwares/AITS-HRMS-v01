using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class EnergyPointRuleService
    {
        private readonly string _connectionString;

        public EnergyPointRuleService(string connectionString)
        {
            _connectionString = connectionString;
        }

        private object Db(object value)
        {
            if (value == null) return DBNull.Value;
            if (value is string text && string.IsNullOrWhiteSpace(text)) return DBNull.Value;
            return value;
        }

        public List<EnergyPointRuleListRow> GetAll(string idFilter, string status, bool? enabled, string ruleName, string referenceDocumentType, string condition)
        {
            var list = new List<EnergyPointRuleListRow>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT Id, ErpNextId, RuleName, ReferenceDocumentType, Enabled, Status
                    FROM EnergyPointRules
                    WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(idFilter)) query += " AND CAST(Id AS VARCHAR) LIKE @IdFilter";
                if (!string.IsNullOrEmpty(status)) query += " AND Status = @Status";
                if (enabled != null) query += " AND Enabled = @Enabled";
                if (!string.IsNullOrEmpty(ruleName)) query += " AND RuleName LIKE @RuleName";
                if (!string.IsNullOrEmpty(referenceDocumentType)) query += " AND ReferenceDocumentType LIKE @RefDocType";
                if (!string.IsNullOrEmpty(condition)) query += " AND Condition_ LIKE @Condition";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(idFilter)) cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");
                    if (!string.IsNullOrEmpty(status)) cmd.Parameters.AddWithValue("@Status", status);
                    if (enabled != null) cmd.Parameters.AddWithValue("@Enabled", enabled.Value);
                    if (!string.IsNullOrEmpty(ruleName)) cmd.Parameters.AddWithValue("@RuleName", "%" + ruleName + "%");
                    if (!string.IsNullOrEmpty(referenceDocumentType)) cmd.Parameters.AddWithValue("@RefDocType", "%" + referenceDocumentType + "%");
                    if (!string.IsNullOrEmpty(condition)) cmd.Parameters.AddWithValue("@Condition", "%" + condition + "%");

                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new EnergyPointRuleListRow
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                ErpNextId = reader["ErpNextId"] == DBNull.Value ? string.Empty : reader["ErpNextId"].ToString(),
                                RuleName = reader["RuleName"] == DBNull.Value ? string.Empty : reader["RuleName"].ToString(),
                                ReferenceDocumentType = reader["ReferenceDocumentType"] == DBNull.Value ? string.Empty : reader["ReferenceDocumentType"].ToString(),
                                Enabled = reader["Enabled"] != DBNull.Value && Convert.ToBoolean(reader["Enabled"]),
                                Status = reader["Status"] == DBNull.Value ? string.Empty : reader["Status"].ToString()
                            });
                        }
                    }
                }
            }

            return list;
        }

        public EnergyPointRule GetById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM EnergyPointRules WHERE Id = @Id AND IsDeleted = 0";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new EnergyPointRule
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                ErpNextId = reader["ErpNextId"] == DBNull.Value ? string.Empty : reader["ErpNextId"].ToString(),
                                Enabled = reader["Enabled"] != DBNull.Value && Convert.ToBoolean(reader["Enabled"]),
                                RuleName = reader["RuleName"] == DBNull.Value ? string.Empty : reader["RuleName"].ToString(),
                                ReferenceDocumentType = reader["ReferenceDocumentType"] == DBNull.Value ? string.Empty : reader["ReferenceDocumentType"].ToString(),
                                ForDocumentEvent = reader["ForDocumentEvent"] == DBNull.Value ? string.Empty : reader["ForDocumentEvent"].ToString(),
                                FieldToCheck = reader["FieldToCheck"] == DBNull.Value ? string.Empty : reader["FieldToCheck"].ToString(),
                                Points = reader["Points"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Points"]),
                                ApplyOnlyOnce = reader["ApplyOnlyOnce"] != DBNull.Value && Convert.ToBoolean(reader["ApplyOnlyOnce"]),
                                AllotPointsToAssignedUsers = reader["AllotPointsToAssignedUsers"] != DBNull.Value && Convert.ToBoolean(reader["AllotPointsToAssignedUsers"]),
                                UserField = reader["UserField"] == DBNull.Value ? string.Empty : reader["UserField"].ToString(),
                                MultiplierField = reader["MultiplierField"] == DBNull.Value ? string.Empty : reader["MultiplierField"].ToString(),
                                MaxPoints = reader["MaxPoints"] == DBNull.Value ? null : Convert.ToInt32(reader["MaxPoints"]),
                                Condition = reader["Condition_"] == DBNull.Value ? string.Empty : reader["Condition_"].ToString(),
                                Status = reader["Status"] == DBNull.Value ? string.Empty : reader["Status"].ToString()
                            };
                        }
                    }
                }
            }

            return null;
        }

        public int Insert(EnergyPointRule model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    INSERT INTO EnergyPointRules
                        (ErpNextId, Enabled, RuleName, ReferenceDocumentType, ForDocumentEvent, FieldToCheck, Points,
                         ApplyOnlyOnce, AllotPointsToAssignedUsers, UserField, MultiplierField, MaxPoints, Condition_, Status, IsDeleted)
                    OUTPUT INSERTED.Id
                    VALUES
                        (@ErpNextId, @Enabled, @RuleName, @RefDocType, @ForDocEvent, @FieldToCheck, @Points,
                         @ApplyOnlyOnce, @AllotPoints, @UserField, @MultiplierField, @MaxPoints, @Condition, @Status, 0)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    AddParams(cmd, model);
                    conn.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void UpdateErpNextId(int id, string erpNextId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE EnergyPointRules SET ErpNextId = @ErpNextId WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@ErpNextId", Db(erpNextId));
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpsertFromErpNext(EnergyPointRule model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                int existingId = 0;

                using (var cmd = new SqlCommand("SELECT Id FROM EnergyPointRules WHERE ErpNextId = @ErpNextId", conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", Db(model.ErpNextId));
                    object result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                        existingId = Convert.ToInt32(result);
                }

                if (existingId > 0)
                {
                    string query = @"
                        UPDATE EnergyPointRules
                        SET Enabled = @Enabled,
                            RuleName = @RuleName,
                            ReferenceDocumentType = @RefDocType,
                            ForDocumentEvent = @ForDocEvent,
                            FieldToCheck = @FieldToCheck,
                            Points = @Points,
                            ApplyOnlyOnce = @ApplyOnlyOnce,
                            AllotPointsToAssignedUsers = @AllotPoints,
                            UserField = @UserField,
                            MultiplierField = @MultiplierField,
                            MaxPoints = @MaxPoints,
                            Condition_ = @Condition,
                            Status = @Status,
                            IsDeleted = 0
                        WHERE Id = @Id";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        AddParams(cmd, model);
                        cmd.Parameters.AddWithValue("@Id", existingId);
                        cmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    string query = @"
                        INSERT INTO EnergyPointRules
                            (ErpNextId, Enabled, RuleName, ReferenceDocumentType, ForDocumentEvent, FieldToCheck, Points,
                             ApplyOnlyOnce, AllotPointsToAssignedUsers, UserField, MultiplierField, MaxPoints, Condition_, Status, IsDeleted)
                        VALUES
                            (@ErpNextId, @Enabled, @RuleName, @RefDocType, @ForDocEvent, @FieldToCheck, @Points,
                             @ApplyOnlyOnce, @AllotPoints, @UserField, @MultiplierField, @MaxPoints, @Condition, @Status, 0)";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        AddParams(cmd, model);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private void AddParams(SqlCommand cmd, EnergyPointRule model)
        {
            cmd.Parameters.AddWithValue("@ErpNextId", Db(model.ErpNextId));
            cmd.Parameters.AddWithValue("@Enabled", model.Enabled);
            cmd.Parameters.AddWithValue("@RuleName", Db(model.RuleName));
            cmd.Parameters.AddWithValue("@RefDocType", Db(model.ReferenceDocumentType));
            cmd.Parameters.AddWithValue("@ForDocEvent", Db(model.ForDocumentEvent));
            cmd.Parameters.AddWithValue("@FieldToCheck", Db(model.FieldToCheck));
            cmd.Parameters.AddWithValue("@Points", model.Points);
            cmd.Parameters.AddWithValue("@ApplyOnlyOnce", model.ApplyOnlyOnce);
            cmd.Parameters.AddWithValue("@AllotPoints", model.AllotPointsToAssignedUsers);
            cmd.Parameters.AddWithValue("@UserField", Db(model.UserField));
            cmd.Parameters.AddWithValue("@MultiplierField", Db(model.MultiplierField));
            cmd.Parameters.AddWithValue("@MaxPoints", model.MaxPoints.HasValue ? (object)model.MaxPoints.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@Condition", Db(model.Condition));
            cmd.Parameters.AddWithValue("@Status", Db(model.Status));
        }
    }
}