using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class EmployeePromotionService
    {
        private readonly string _connectionString;

        public EmployeePromotionService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<EmployeePromotionListRow> GetAll(string idFilter, string employeeName)
        {
            var list = new List<EmployeePromotionListRow>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT Id, ErpNextId, Employee, EmployeeName, PromotionDate
                    FROM EmployeePromotions
                    WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(idFilter))
                    query += " AND CAST(Id AS VARCHAR) LIKE @IdFilter";

                if (!string.IsNullOrEmpty(employeeName))
                    query += " AND EmployeeName LIKE @EmployeeName";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(idFilter))
                        cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");

                    if (!string.IsNullOrEmpty(employeeName))
                        cmd.Parameters.AddWithValue("@EmployeeName", "%" + employeeName + "%");

                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new EmployeePromotionListRow
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                ErpNextId = reader["ErpNextId"] == DBNull.Value ? string.Empty : reader["ErpNextId"].ToString(),
                                Employee = reader["Employee"] == DBNull.Value ? string.Empty : reader["Employee"].ToString(),
                                EmployeeName = reader["EmployeeName"] == DBNull.Value ? string.Empty : reader["EmployeeName"].ToString(),
                                PromotionDate = Convert.ToDateTime(reader["PromotionDate"])
                            });
                        }
                    }
                }
            }

            return list;
        }

        public EmployeePromotion GetById(int id)
        {
            EmployeePromotion model = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string query = "SELECT * FROM EmployeePromotions WHERE Id = @Id AND IsDeleted = 0";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            decimal ctcValue;
                            decimal? currentCtc = reader["CurrentCTC"] != DBNull.Value && decimal.TryParse(reader["CurrentCTC"].ToString(), out ctcValue)
                                ? ctcValue
                                : null;

                            model = new EmployeePromotion
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                ErpNextId = reader["ErpNextId"] == DBNull.Value ? string.Empty : reader["ErpNextId"].ToString(),
                                Employee = reader["Employee"] == DBNull.Value ? string.Empty : reader["Employee"].ToString(),
                                EmployeeName = reader["EmployeeName"] == DBNull.Value ? string.Empty : reader["EmployeeName"].ToString(),
                                PromotionDate = Convert.ToDateTime(reader["PromotionDate"]),
                                SalaryCurrency = reader["SalaryCurrency"] == DBNull.Value ? "INR" : reader["SalaryCurrency"].ToString(),
                                Company = reader["Company"] == DBNull.Value ? string.Empty : reader["Company"].ToString(),
                                CurrentCTC = currentCtc
                            };
                        }
                    }
                }

                if (model == null)
                    return null;

                string detailQuery = @"
                    SELECT Id, EmployeePromotionId, FieldName, Property, CurrentValue, NewValue
                    FROM EmployeePromotionDetails
                    WHERE EmployeePromotionId = @Id";

                using (var cmd = new SqlCommand(detailQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.Details.Add(new EmployeePromotionDetail
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                EmployeePromotionId = Convert.ToInt32(reader["EmployeePromotionId"]),
                                FieldName = reader["FieldName"] == DBNull.Value ? string.Empty : reader["FieldName"].ToString(),
                                Property = reader["Property"] == DBNull.Value ? string.Empty : reader["Property"].ToString(),
                                CurrentValue = reader["CurrentValue"] == DBNull.Value ? string.Empty : reader["CurrentValue"].ToString(),
                                NewValue = reader["NewValue"] == DBNull.Value ? string.Empty : reader["NewValue"].ToString()
                            });
                        }
                    }
                }
            }

            return model;
        }

        public int Insert(
            EmployeePromotion model,
            List<string> fieldNames,
            List<string> properties,
            List<string> currentValues,
            List<string> newValues)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string query = @"
                            INSERT INTO EmployeePromotions
                                (ErpNextId, Employee, EmployeeName, PromotionDate, SalaryCurrency, Company, CurrentCTC, IsDeleted, CreatedDate)
                            OUTPUT INSERTED.Id
                            VALUES
                                (@ErpNextId, @Employee, @EmployeeName, @PromotionDate, @SalaryCurrency, @Company, @CurrentCTC, 0, GETDATE())";

                        int newId;

                        using (var cmd = new SqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ErpNextId", (object)model.ErpNextId ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Employee", (object)model.Employee ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@EmployeeName", (object)model.EmployeeName ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@PromotionDate", model.PromotionDate);
                            cmd.Parameters.AddWithValue("@SalaryCurrency", (object)model.SalaryCurrency ?? "INR");
                            cmd.Parameters.AddWithValue("@Company", (object)model.Company ?? "AITS");
                            cmd.Parameters.AddWithValue("@CurrentCTC", (object)model.CurrentCTC ?? DBNull.Value);

                            newId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        InsertDetails(conn, transaction, newId, fieldNames, properties, currentValues, newValues);

                        transaction.Commit();
                        return newId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void UpdateErpNextId(int id, string erpNextId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE EmployeePromotions SET ErpNextId = @ErpNextId WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@ErpNextId", (object)erpNextId ?? DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpsertFromErpNext(
            EmployeePromotion model,
            List<string> fieldNames,
            List<string> properties,
            List<string> currentValues,
            List<string> newValues)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int promotionId = 0;

                        string findQuery = "SELECT Id FROM EmployeePromotions WHERE ErpNextId = @ErpNextId";

                        using (var cmd = new SqlCommand(findQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ErpNextId", (object)model.ErpNextId ?? DBNull.Value);
                            object existingId = cmd.ExecuteScalar();

                            if (existingId != null && existingId != DBNull.Value)
                                promotionId = Convert.ToInt32(existingId);
                        }

                        if (promotionId > 0)
                        {
                            string updateQuery = @"
                                UPDATE EmployeePromotions
                                SET Employee = @Employee,
                                    EmployeeName = @EmployeeName,
                                    PromotionDate = @PromotionDate,
                                    SalaryCurrency = @SalaryCurrency,
                                    Company = @Company,
                                    CurrentCTC = @CurrentCTC,
                                    IsDeleted = 0
                                WHERE Id = @Id";

                            using (var cmd = new SqlCommand(updateQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Id", promotionId);
                                cmd.Parameters.AddWithValue("@Employee", (object)model.Employee ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@EmployeeName", (object)model.EmployeeName ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@PromotionDate", model.PromotionDate);
                                cmd.Parameters.AddWithValue("@SalaryCurrency", (object)model.SalaryCurrency ?? "INR");
                                cmd.Parameters.AddWithValue("@Company", (object)model.Company ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@CurrentCTC", (object)model.CurrentCTC ?? DBNull.Value);
                                cmd.ExecuteNonQuery();
                            }

                            using (var cmd = new SqlCommand("DELETE FROM EmployeePromotionDetails WHERE EmployeePromotionId = @Id", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Id", promotionId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            string insertQuery = @"
                                INSERT INTO EmployeePromotions
                                    (ErpNextId, Employee, EmployeeName, PromotionDate, SalaryCurrency, Company, CurrentCTC, IsDeleted, CreatedDate)
                                OUTPUT INSERTED.Id
                                VALUES
                                    (@ErpNextId, @Employee, @EmployeeName, @PromotionDate, @SalaryCurrency, @Company, @CurrentCTC, 0, GETDATE())";

                            using (var cmd = new SqlCommand(insertQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@ErpNextId", (object)model.ErpNextId ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@Employee", (object)model.Employee ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@EmployeeName", (object)model.EmployeeName ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@PromotionDate", model.PromotionDate);
                                cmd.Parameters.AddWithValue("@SalaryCurrency", (object)model.SalaryCurrency ?? "INR");
                                cmd.Parameters.AddWithValue("@Company", (object)model.Company ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@CurrentCTC", (object)model.CurrentCTC ?? DBNull.Value);

                                promotionId = Convert.ToInt32(cmd.ExecuteScalar());
                            }
                        }

                        InsertDetails(conn, transaction, promotionId, fieldNames, properties, currentValues, newValues);

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private void InsertDetails(
            SqlConnection conn,
            SqlTransaction transaction,
            int promotionId,
            List<string> fieldNames,
            List<string> properties,
            List<string> currentValues,
            List<string> newValues)
        {
            if (properties == null)
                return;

            for (int i = 0; i < properties.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(properties[i]))
                    continue;

                object fieldNameValue = DBNull.Value;
                if (fieldNames != null && i < fieldNames.Count && !string.IsNullOrWhiteSpace(fieldNames[i]))
                    fieldNameValue = fieldNames[i];

                object currentValue = DBNull.Value;
                if (currentValues != null && i < currentValues.Count && !string.IsNullOrWhiteSpace(currentValues[i]))
                    currentValue = currentValues[i];

                object newValue = DBNull.Value;
                if (newValues != null && i < newValues.Count && !string.IsNullOrWhiteSpace(newValues[i]))
                    newValue = newValues[i];

                string query = @"
                    INSERT INTO EmployeePromotionDetails
                        (EmployeePromotionId, FieldName, Property, CurrentValue, NewValue)
                    VALUES
                        (@EmployeePromotionId, @FieldName, @Property, @CurrentValue, @NewValue)";

                using (var cmd = new SqlCommand(query, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@EmployeePromotionId", promotionId);
                    cmd.Parameters.AddWithValue("@FieldName", fieldNameValue);
                    cmd.Parameters.AddWithValue("@Property", properties[i]);
                    cmd.Parameters.AddWithValue("@CurrentValue", currentValue);
                    cmd.Parameters.AddWithValue("@NewValue", newValue);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}