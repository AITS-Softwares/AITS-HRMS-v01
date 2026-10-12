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

                using (var cmd = new SqlCommand(@"
                    INSERT INTO StaffingPlans (Name, Company, FromDate, ToDate, Department, TotalEstimatedBudget)
                    OUTPUT INSERTED.Id
                    VALUES (@Name, @Company, @FromDate, @ToDate, @Department, @TotalEstimatedBudget)", conn))
                {
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
    }
}