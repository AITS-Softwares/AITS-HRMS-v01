using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class ActivityCostService
    {
        private readonly string _connectionString;

        public ActivityCostService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<ActivityCost> GetAll(string idFilter, string activityType, string employee)
        {
            var list = new List<ActivityCost>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM ActivityCosts WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(idFilter)) query += " AND CAST(Id AS VARCHAR) LIKE @IdFilter";
                if (!string.IsNullOrEmpty(activityType)) query += " AND ActivityType LIKE @ActivityType";
                if (!string.IsNullOrEmpty(employee)) query += " AND Employee LIKE @Employee";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(idFilter)) cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");
                    if (!string.IsNullOrEmpty(activityType)) cmd.Parameters.AddWithValue("@ActivityType", "%" + activityType + "%");
                    if (!string.IsNullOrEmpty(employee)) cmd.Parameters.AddWithValue("@Employee", "%" + employee + "%");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(MapActivityCost(reader));
                        }
                    }
                }
            }

            return list;
        }


        public void UpdateErpNextId(int id, string erpNextId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand(@"
            UPDATE ActivityCosts
            SET ErpNextId = @ErpNextId
            WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@ErpNextId", (object)erpNextId ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public int UpsertFromErpNext(ActivityCost a)
        {
            var existing = GetByErpNextId(a.ErpNextId);

            if (existing != null)
            {
                a.Id = existing.Id;
                Update(a);
                return existing.Id;
            }

            existing = GetByActivityTypeAndEmployee(a.ActivityType, a.Employee);

            if (existing != null)
            {
                a.Id = existing.Id;
                a.ErpNextId = string.IsNullOrWhiteSpace(a.ErpNextId) ? existing.ErpNextId : a.ErpNextId;
                Update(a);
                return existing.Id;
            }

            return Insert(a);
        }

        public ActivityCost GetByErpNextId(string erpNextId)
        {
            if (string.IsNullOrWhiteSpace(erpNextId))
                return null;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand(@"
            SELECT TOP 1 *
            FROM ActivityCosts
            WHERE ErpNextId = @ErpNextId AND IsDeleted = 0", conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", erpNextId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            return MapActivityCost(reader);
                    }
                }
            }

            return null;
        }

        public ActivityCost GetByActivityTypeAndEmployee(string activityType, string employee)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand(@"
            SELECT TOP 1 *
            FROM ActivityCosts
            WHERE ISNULL(ActivityType, '') = ISNULL(@ActivityType, '')
              AND ISNULL(Employee, '') = ISNULL(@Employee, '')
              AND IsDeleted = 0", conn))
                {
                    cmd.Parameters.AddWithValue("@ActivityType", (object)activityType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Employee", (object)employee ?? DBNull.Value);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            return MapActivityCost(reader);
                    }
                }
            }

            return null;
        }

        public void Update(ActivityCost a)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand(@"
            UPDATE ActivityCosts SET
                ErpNextId = @ErpNextId,
                ActivityType = @ActivityType,
                Employee = @Employee,
                BillingRate = @BillingRate,
                CostingRate = @CostingRate
            WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", a.Id);
                    cmd.Parameters.AddWithValue("@ErpNextId", (object)a.ErpNextId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ActivityType", (object)a.ActivityType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Employee", (object)a.Employee ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@BillingRate", a.BillingRate);
                    cmd.Parameters.AddWithValue("@CostingRate", a.CostingRate);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void MarkDeletedById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand("UPDATE ActivityCosts SET IsDeleted = 1 WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }





        public ActivityCost GetById(int id)
        {
            ActivityCost model = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT * FROM ActivityCosts WHERE Id = @Id AND IsDeleted = 0", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model = MapActivityCost(reader);
                        }
                    }
                }
            }

            return model;
        }

        public int Insert(ActivityCost a)
        {
            int newId;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand(@"
            INSERT INTO ActivityCosts
                (ErpNextId, ActivityType, Employee, BillingRate, CostingRate)
            OUTPUT INSERTED.Id
            VALUES
                (@ErpNextId, @ActivityType, @Employee, @BillingRate, @CostingRate)", conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", (object)a.ErpNextId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ActivityType", (object)a.ActivityType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Employee", (object)a.Employee ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@BillingRate", a.BillingRate);
                    cmd.Parameters.AddWithValue("@CostingRate", a.CostingRate);

                    newId = (int)cmd.ExecuteScalar();
                }
            }

            return newId;
        }
        private ActivityCost MapActivityCost(SqlDataReader reader)
        {
            return new ActivityCost
            {
                Id = Convert.ToInt32(reader["Id"]),
                ActivityType = reader["ActivityType"]?.ToString() ?? string.Empty,
                ErpNextId = reader["ErpNextId"]?.ToString() ?? string.Empty,
                Employee = reader["Employee"]?.ToString() ?? string.Empty,
                BillingRate = reader["BillingRate"] != DBNull.Value ? Convert.ToDecimal(reader["BillingRate"]) : 0,
                CostingRate = reader["CostingRate"] != DBNull.Value ? Convert.ToDecimal(reader["CostingRate"]) : 0
            };
        }
    }
}
