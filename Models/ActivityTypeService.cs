using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class ActivityTypeService
    {
        private readonly string _connectionString;

        public ActivityTypeService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<ActivityType> GetAll(string idFilter)
        {
            var list = new List<ActivityType>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM ActivityTypes WHERE IsDeleted = 0";

                if (!string.IsNullOrWhiteSpace(idFilter))
                    query += " AND Name LIKE @IdFilter";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrWhiteSpace(idFilter))
                        cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");

                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(MapActivityType(reader));
                        }
                    }
                }
            }

            return list;
        }

        public ActivityType GetById(int id)
        {
            ActivityType model = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand("SELECT * FROM ActivityTypes WHERE Id = @Id AND IsDeleted = 0", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            model = MapActivityType(reader);
                    }
                }
            }

            return model;
        }

        public ActivityType GetByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand("SELECT TOP 1 * FROM ActivityTypes WHERE Name = @Name AND IsDeleted = 0", conn))
                {
                    cmd.Parameters.AddWithValue("@Name", name);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            return MapActivityType(reader);
                    }
                }
            }

            return null;
        }

        public int Insert(ActivityType a)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand(@"
                    INSERT INTO ActivityTypes
                        (Name, DefaultBillingRate, DefaultCostingRate, Disabled, IsDeleted)
                    OUTPUT INSERTED.Id
                    VALUES
                        (@Name, @DefaultBillingRate, @DefaultCostingRate, @Disabled, 0)", conn))
                {
                    cmd.Parameters.AddWithValue("@Name", (object)a.Name ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DefaultBillingRate", (object)a.DefaultBillingRate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DefaultCostingRate", (object)a.DefaultCostingRate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Disabled", a.Disabled);

                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        public void Update(ActivityType a)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand(@"
                    UPDATE ActivityTypes SET
                        Name = @Name,
                        DefaultBillingRate = @DefaultBillingRate,
                        DefaultCostingRate = @DefaultCostingRate,
                        Disabled = @Disabled
                    WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", a.Id);
                    cmd.Parameters.AddWithValue("@Name", (object)a.Name ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DefaultBillingRate", (object)a.DefaultBillingRate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DefaultCostingRate", (object)a.DefaultCostingRate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Disabled", a.Disabled);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void MarkDeletedById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand("UPDATE ActivityTypes SET IsDeleted = 1 WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public int UpsertFromErpNext(ActivityType a)
        {
            var existing = GetByName(a.Name);

            if (existing == null)
                return Insert(a);

            a.Id = existing.Id;
            Update(a);
            return existing.Id;
        }

        private ActivityType MapActivityType(SqlDataReader reader)
        {
            return new ActivityType
            {
                Id = Convert.ToInt32(reader["Id"]),
                Name = reader["Name"]?.ToString() ?? string.Empty,
                DefaultBillingRate = reader["DefaultBillingRate"] != DBNull.Value ? Convert.ToDecimal(reader["DefaultBillingRate"]) : (decimal?)null,
                DefaultCostingRate = reader["DefaultCostingRate"] != DBNull.Value ? Convert.ToDecimal(reader["DefaultCostingRate"]) : (decimal?)null,
                Disabled = reader["Disabled"] != DBNull.Value && Convert.ToBoolean(reader["Disabled"])
            };
        }
    }
}
