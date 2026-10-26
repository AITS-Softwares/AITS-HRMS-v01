using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class StaffingPlanService
    {
        private readonly string _connectionString;

        public StaffingPlanService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<StaffingPlan> GetAll(string idFilter)
        {
            var list = new List<StaffingPlan>();

            using (var conn = new SqlConnection(_connectionString))
            {
                EnsureErpNextIdColumn(conn);
                string query = "SELECT * FROM StaffingPlans WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(idFilter))
                    query += " AND CAST(Id AS VARCHAR) LIKE @IdFilter";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(idFilter))
                        cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new StaffingPlan
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                ErpNextId = reader["ErpNextId"] == DBNull.Value ? null : reader["ErpNextId"]?.ToString(),
                                DocStatus = reader["DocStatus"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DocStatus"]),
                                Name = reader["Name"]?.ToString() ?? string.Empty,
                                Company = reader["Company"]?.ToString() ?? string.Empty,
                                FromDate = Convert.ToDateTime(reader["FromDate"]),
                                ToDate = Convert.ToDateTime(reader["ToDate"]),
                                Department = reader["Department"]?.ToString() ?? string.Empty,
                                TotalEstimatedBudget = Convert.ToDecimal(reader["TotalEstimatedBudget"])
                            });
                        }
                    }
                }
            }

            return list;
        }

        public StaffingPlan GetById(int id)
        {
            StaffingPlan plan = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                EnsureErpNextIdColumn(conn);

                using (var cmd = new SqlCommand("SELECT * FROM StaffingPlans WHERE Id = @Id AND IsDeleted = 0", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            plan = new StaffingPlan
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                ErpNextId = reader["ErpNextId"] == DBNull.Value ? null : reader["ErpNextId"]?.ToString(),
                                DocStatus = reader["DocStatus"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DocStatus"]),
                                Name = reader["Name"]?.ToString() ?? string.Empty,
                                Company = reader["Company"]?.ToString() ?? string.Empty,
                                FromDate = Convert.ToDateTime(reader["FromDate"]),
                                ToDate = Convert.ToDateTime(reader["ToDate"]),
                                Department = reader["Department"]?.ToString() ?? string.Empty,
                                TotalEstimatedBudget = Convert.ToDecimal(reader["TotalEstimatedBudget"])
                            };
                        }
                    }
                }

                if (plan != null)
                {
                    using (var cmd = new SqlCommand("SELECT * FROM StaffingPlanDetails WHERE StaffingPlanId = @Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                plan.Details.Add(new StaffingPlanDetail
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    StaffingPlanId = id,
                                    Designation = reader["Designation"]?.ToString() ?? string.Empty,
                                    Vacancies = Convert.ToInt32(reader["Vacancies"]),
                                    EstimatedCostPerPosition = Convert.ToDecimal(reader["EstimatedCostPerPosition"]),
                                    TotalEstimatedCost = Convert.ToDecimal(reader["TotalEstimatedCost"]),
                                    NumberOfPositions = Convert.ToInt32(reader["NumberOfPositions"])
                                });
                            }
                        }
                    }
                }
            }

            return plan;
        }

        public int Insert(StaffingPlan plan)
        {
            plan.TotalEstimatedBudget = plan.Details.Sum(d => d.Vacancies * d.EstimatedCostPerPosition);

            int newId;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                EnsureErpNextIdColumn(conn);

                using (var cmd = new SqlCommand(@"
                    INSERT INTO StaffingPlans (ErpNextId, Name, Company, FromDate, ToDate, Department, TotalEstimatedBudget)
                    OUTPUT INSERTED.Id
                    VALUES (@ErpNextId, @Name, @Company, @FromDate, @ToDate, @Department, @TotalEstimatedBudget)", conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", plan.ErpNextId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Name", plan.Name ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Company", plan.Company ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FromDate", plan.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", plan.ToDate);
                    cmd.Parameters.AddWithValue("@Department", plan.Department ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@TotalEstimatedBudget", plan.TotalEstimatedBudget);

                    newId = (int)cmd.ExecuteScalar();
                }

                foreach (var detail in plan.Details)
                {
                    if (string.IsNullOrWhiteSpace(detail.Designation)) continue;

                    decimal total = detail.Vacancies * detail.EstimatedCostPerPosition;

                    using (var cmd = new SqlCommand(@"
                        INSERT INTO StaffingPlanDetails
                            (StaffingPlanId, Designation, Vacancies, EstimatedCostPerPosition, TotalEstimatedCost, NumberOfPositions)
                        VALUES
                            (@StaffingPlanId, @Designation, @Vacancies, @EstimatedCostPerPosition, @TotalEstimatedCost, @NumberOfPositions)", conn))
                    {
                        cmd.Parameters.AddWithValue("@StaffingPlanId", newId);
                        cmd.Parameters.AddWithValue("@Designation", detail.Designation ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Vacancies", detail.Vacancies);
                        cmd.Parameters.AddWithValue("@EstimatedCostPerPosition", detail.EstimatedCostPerPosition);
                        cmd.Parameters.AddWithValue("@TotalEstimatedCost", total);
                        cmd.Parameters.AddWithValue("@NumberOfPositions", detail.NumberOfPositions);

                        cmd.ExecuteNonQuery();
                    }
                }
            }

            return newId;
        }

        public void Update(StaffingPlan plan)
        {
            plan.TotalEstimatedBudget = plan.Details.Sum(d => d.Vacancies * d.EstimatedCostPerPosition);
            using var conn = new SqlConnection(_connectionString); conn.Open(); EnsureErpNextIdColumn(conn);
            using (var cmd = new SqlCommand("UPDATE StaffingPlans SET Name=@Name, Company=@Company, FromDate=@FromDate, ToDate=@ToDate, Department=@Department, TotalEstimatedBudget=@Budget, DocStatus=@DocStatus WHERE Id=@Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", plan.Id); cmd.Parameters.AddWithValue("@Name", plan.Name ?? ""); cmd.Parameters.AddWithValue("@Company", plan.Company ?? ""); cmd.Parameters.AddWithValue("@FromDate", plan.FromDate); cmd.Parameters.AddWithValue("@ToDate", plan.ToDate); cmd.Parameters.AddWithValue("@Department", plan.Department ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@Budget", plan.TotalEstimatedBudget); cmd.Parameters.AddWithValue("@DocStatus", plan.DocStatus); cmd.ExecuteNonQuery();
            }
            using (var delete = new SqlCommand("DELETE FROM StaffingPlanDetails WHERE StaffingPlanId=@Id", conn)) { delete.Parameters.AddWithValue("@Id", plan.Id); delete.ExecuteNonQuery(); }
            foreach (var detail in plan.Details.Where(d => !string.IsNullOrWhiteSpace(d.Designation)))
            {
                using var insert = new SqlCommand("INSERT INTO StaffingPlanDetails (StaffingPlanId, Designation, Vacancies, EstimatedCostPerPosition, TotalEstimatedCost, NumberOfPositions) VALUES (@PlanId,@Designation,@Vacancies,@Cost,@Total,@Positions)", conn);
                insert.Parameters.AddWithValue("@PlanId", plan.Id); insert.Parameters.AddWithValue("@Designation", detail.Designation); insert.Parameters.AddWithValue("@Vacancies", detail.Vacancies); insert.Parameters.AddWithValue("@Cost", detail.EstimatedCostPerPosition); insert.Parameters.AddWithValue("@Total", detail.Vacancies * detail.EstimatedCostPerPosition); insert.Parameters.AddWithValue("@Positions", detail.NumberOfPositions); insert.ExecuteNonQuery();
            }
        }

        private static void EnsureErpNextIdColumn(SqlConnection conn)
        {
            var closeWhenDone = conn.State != System.Data.ConnectionState.Open;
            if (closeWhenDone) conn.Open();
            using var cmd = new SqlCommand("IF COL_LENGTH('dbo.StaffingPlans','ErpNextId') IS NULL ALTER TABLE dbo.StaffingPlans ADD ErpNextId NVARCHAR(255) NULL; IF COL_LENGTH('dbo.StaffingPlans','DocStatus') IS NULL ALTER TABLE dbo.StaffingPlans ADD DocStatus INT NOT NULL CONSTRAINT DF_StaffingPlans_DocStatus DEFAULT(0);", conn);
            cmd.ExecuteNonQuery();
            if (closeWhenDone) conn.Close();
        }
    }
}
