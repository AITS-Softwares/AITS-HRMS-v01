using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class AppraisalCycleService
    {
        private readonly string _connectionString;

        public AppraisalCycleService(string connectionString)
        {
            _connectionString = connectionString;
        }

        private object Db(object value)
        {
            if (value == null)
                return DBNull.Value;

            if (value is string text && string.IsNullOrWhiteSpace(text))
                return DBNull.Value;

            return value;
        }

        public List<AppraisalCycleListRow> GetAll(string idFilter, string company, string status, DateTime? startDate, DateTime? endDate)
        {
            var list = new List<AppraisalCycleListRow>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT Id, ErpNextId, CycleName, Company, Status, StartDate, EndDate
                    FROM AppraisalCycles
                    WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(idFilter)) query += " AND CAST(Id AS VARCHAR) LIKE @IdFilter";
                if (!string.IsNullOrEmpty(company)) query += " AND Company LIKE @Company";
                if (!string.IsNullOrEmpty(status)) query += " AND Status = @Status";
                if (startDate != null) query += " AND StartDate >= @StartDate";
                if (endDate != null) query += " AND EndDate <= @EndDate";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(idFilter)) cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");
                    if (!string.IsNullOrEmpty(company)) cmd.Parameters.AddWithValue("@Company", "%" + company + "%");
                    if (!string.IsNullOrEmpty(status)) cmd.Parameters.AddWithValue("@Status", status);
                    if (startDate != null) cmd.Parameters.AddWithValue("@StartDate", startDate.Value);
                    if (endDate != null) cmd.Parameters.AddWithValue("@EndDate", endDate.Value);

                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new AppraisalCycleListRow
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                ErpNextId = reader["ErpNextId"] == DBNull.Value ? string.Empty : reader["ErpNextId"].ToString(),
                                CycleName = reader["CycleName"] == DBNull.Value ? string.Empty : reader["CycleName"].ToString(),
                                Company = reader["Company"] == DBNull.Value ? string.Empty : reader["Company"].ToString(),
                                Status = reader["Status"] == DBNull.Value ? string.Empty : reader["Status"].ToString(),
                                StartDate = Convert.ToDateTime(reader["StartDate"]),
                                EndDate = Convert.ToDateTime(reader["EndDate"])
                            });
                        }
                    }
                }
            }

            return list;
        }

        public AppraisalCycle GetById(int id)
        {
            AppraisalCycle model = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string query = "SELECT * FROM AppraisalCycles WHERE Id = @Id AND IsDeleted = 0";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model = new AppraisalCycle
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                ErpNextId = reader["ErpNextId"] == DBNull.Value ? string.Empty : reader["ErpNextId"].ToString(),
                                CycleName = reader["CycleName"] == DBNull.Value ? string.Empty : reader["CycleName"].ToString(),
                                Company = reader["Company"] == DBNull.Value ? string.Empty : reader["Company"].ToString(),
                                StartDate = Convert.ToDateTime(reader["StartDate"]),
                                EndDate = Convert.ToDateTime(reader["EndDate"]),
                                Status = reader["Status"] == DBNull.Value ? string.Empty : reader["Status"].ToString(),
                                Description = reader["Description"] == DBNull.Value ? string.Empty : reader["Description"].ToString(),
                                KRAEvaluationMethod = reader["KRAEvaluationMethod"] == DBNull.Value ? string.Empty : reader["KRAEvaluationMethod"].ToString(),
                                CalculateFinalScoreFormula = reader["CalculateFinalScoreFormula"] != DBNull.Value && Convert.ToBoolean(reader["CalculateFinalScoreFormula"]),
                                FinalScoreFormula = reader["FinalScoreFormula"] == DBNull.Value ? string.Empty : reader["FinalScoreFormula"].ToString(),
                                Branch = reader["Branch"] == DBNull.Value ? string.Empty : reader["Branch"].ToString(),
                                Designation = reader["Designation"] == DBNull.Value ? string.Empty : reader["Designation"].ToString(),
                                Department = reader["Department"] == DBNull.Value ? string.Empty : reader["Department"].ToString()
                            };
                        }
                    }
                }

                if (model == null)
                    return null;

                string q2 = @"
                    SELECT Id, AppraisalCycleId, Employee, EmployeeName, AppraisalTemplate, Department, Designation, Branch
                    FROM AppraisalCycleEmployees
                    WHERE AppraisalCycleId = @Id";

                using (var cmd = new SqlCommand(q2, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.Employees.Add(new AppraisalCycleEmployee
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                AppraisalCycleId = Convert.ToInt32(reader["AppraisalCycleId"]),
                                Employee = reader["Employee"] == DBNull.Value ? string.Empty : reader["Employee"].ToString(),
                                EmployeeName = reader["EmployeeName"] == DBNull.Value ? string.Empty : reader["EmployeeName"].ToString(),
                                AppraisalTemplate = reader["AppraisalTemplate"] == DBNull.Value ? string.Empty : reader["AppraisalTemplate"].ToString(),
                                Department = reader["Department"] == DBNull.Value ? string.Empty : reader["Department"].ToString(),
                                Designation = reader["Designation"] == DBNull.Value ? string.Empty : reader["Designation"].ToString(),
                                Branch = reader["Branch"] == DBNull.Value ? string.Empty : reader["Branch"].ToString()
                            });
                        }
                    }
                }
            }

            return model;
        }

        public int Insert(
            AppraisalCycle model,
            List<string> empCodes,
            List<string> empNames,
            List<string> empTemplates,
            List<string> empDepts,
            List<string> empDesigs,
            List<string> empBranches)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string query = @"
                            INSERT INTO AppraisalCycles
                                (ErpNextId, CycleName, Company, StartDate, EndDate, Status, Description,
                                 KRAEvaluationMethod, CalculateFinalScoreFormula, FinalScoreFormula,
                                 Branch, Designation, Department, IsDeleted, CreatedDate)
                            OUTPUT INSERTED.Id
                            VALUES
                                (@ErpNextId, @CycleName, @Company, @StartDate, @EndDate, @Status, @Description,
                                 @KRAEvaluationMethod, @CalculateFinalScoreFormula, @FinalScoreFormula,
                                 @Branch, @Designation, @Department, 0, GETDATE())";

                        int newId;

                        using (var cmd = new SqlCommand(query, conn, transaction))
                        {
                            AddCycleParams(cmd, model);
                            newId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        InsertEmployees(conn, transaction, newId, empCodes, empNames, empTemplates, empDepts, empDesigs, empBranches);

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
                string query = "UPDATE AppraisalCycles SET ErpNextId = @ErpNextId WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@ErpNextId", Db(erpNextId));

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateStatus(int id, string status)
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(@"
                UPDATE AppraisalCycles
                SET Status = @Status
                WHERE Id = @Id AND IsDeleted = 0", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Status", Db(status));
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void UpsertFromErpNext(
            AppraisalCycle model,
            List<string> empCodes,
            List<string> empNames,
            List<string> empTemplates,
            List<string> empDepts,
            List<string> empDesigs,
            List<string> empBranches)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int cycleId = 0;

                        using (var cmd = new SqlCommand("SELECT Id FROM AppraisalCycles WHERE ErpNextId = @ErpNextId", conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ErpNextId", Db(model.ErpNextId));
                            object existingId = cmd.ExecuteScalar();

                            if (existingId != null && existingId != DBNull.Value)
                                cycleId = Convert.ToInt32(existingId);
                        }

                        if (cycleId > 0)
                        {
                            string updateQuery = @"
                                UPDATE AppraisalCycles
                                SET CycleName = @CycleName,
                                    Company = @Company,
                                    StartDate = @StartDate,
                                    EndDate = @EndDate,
                                    Status = @Status,
                                    Description = @Description,
                                    KRAEvaluationMethod = @KRAEvaluationMethod,
                                    CalculateFinalScoreFormula = @CalculateFinalScoreFormula,
                                    FinalScoreFormula = @FinalScoreFormula,
                                    Branch = @Branch,
                                    Designation = @Designation,
                                    Department = @Department,
                                    IsDeleted = 0
                                WHERE Id = @Id";

                            using (var cmd = new SqlCommand(updateQuery, conn, transaction))
                            {
                                AddCycleParams(cmd, model);
                                cmd.Parameters.AddWithValue("@Id", cycleId);
                                cmd.ExecuteNonQuery();
                            }

                            using (var cmd = new SqlCommand("DELETE FROM AppraisalCycleEmployees WHERE AppraisalCycleId = @Id", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Id", cycleId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            string insertQuery = @"
                                INSERT INTO AppraisalCycles
                                    (ErpNextId, CycleName, Company, StartDate, EndDate, Status, Description,
                                     KRAEvaluationMethod, CalculateFinalScoreFormula, FinalScoreFormula,
                                     Branch, Designation, Department, IsDeleted, CreatedDate)
                                OUTPUT INSERTED.Id
                                VALUES
                                    (@ErpNextId, @CycleName, @Company, @StartDate, @EndDate, @Status, @Description,
                                     @KRAEvaluationMethod, @CalculateFinalScoreFormula, @FinalScoreFormula,
                                     @Branch, @Designation, @Department, 0, GETDATE())";

                            using (var cmd = new SqlCommand(insertQuery, conn, transaction))
                            {
                                AddCycleParams(cmd, model);
                                cycleId = Convert.ToInt32(cmd.ExecuteScalar());
                            }
                        }

                        InsertEmployees(conn, transaction, cycleId, empCodes, empNames, empTemplates, empDepts, empDesigs, empBranches);

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

        private void AddCycleParams(SqlCommand cmd, AppraisalCycle model)
        {
            cmd.Parameters.AddWithValue("@ErpNextId", Db(model.ErpNextId));
            cmd.Parameters.AddWithValue("@CycleName", Db(model.CycleName));
            cmd.Parameters.AddWithValue("@Company", Db(model.Company));
            cmd.Parameters.AddWithValue("@StartDate", model.StartDate);
            cmd.Parameters.AddWithValue("@EndDate", model.EndDate);
            cmd.Parameters.AddWithValue("@Status", Db(model.Status));
            cmd.Parameters.AddWithValue("@Description", Db(model.Description));
            cmd.Parameters.AddWithValue("@KRAEvaluationMethod", Db(model.KRAEvaluationMethod));
            cmd.Parameters.AddWithValue("@CalculateFinalScoreFormula", model.CalculateFinalScoreFormula);
            cmd.Parameters.AddWithValue("@FinalScoreFormula", Db(model.FinalScoreFormula));
            cmd.Parameters.AddWithValue("@Branch", Db(model.Branch));
            cmd.Parameters.AddWithValue("@Designation", Db(model.Designation));
            cmd.Parameters.AddWithValue("@Department", Db(model.Department));
        }

        private void InsertEmployees(
            SqlConnection conn,
            SqlTransaction transaction,
            int cycleId,
            List<string> empCodes,
            List<string> empNames,
            List<string> empTemplates,
            List<string> empDepts,
            List<string> empDesigs,
            List<string> empBranches)
        {
            if (empCodes == null)
                return;

            for (int i = 0; i < empCodes.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(empCodes[i]))
                    continue;

                string query = @"
                    INSERT INTO AppraisalCycleEmployees
                        (AppraisalCycleId, Employee, EmployeeName, AppraisalTemplate, Department, Designation, Branch)
                    VALUES
                        (@CycleId, @Employee, @EmployeeName, @Template, @Department, @Designation, @Branch)";

                using (var cmd = new SqlCommand(query, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@CycleId", cycleId);
                    cmd.Parameters.AddWithValue("@Employee", Db(empCodes[i]));
                    cmd.Parameters.AddWithValue("@EmployeeName", Db(empNames != null && i < empNames.Count ? empNames[i] : null));
                    cmd.Parameters.AddWithValue("@Template", Db(empTemplates != null && i < empTemplates.Count ? empTemplates[i] : null));
                    cmd.Parameters.AddWithValue("@Department", Db(empDepts != null && i < empDepts.Count ? empDepts[i] : null));
                    cmd.Parameters.AddWithValue("@Designation", Db(empDesigs != null && i < empDesigs.Count ? empDesigs[i] : null));
                    cmd.Parameters.AddWithValue("@Branch", Db(empBranches != null && i < empBranches.Count ? empBranches[i] : null));

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
