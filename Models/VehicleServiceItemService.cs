using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class VehicleServiceItemService
    {
        private readonly string _connectionString;

        public VehicleServiceItemService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<VehicleServiceItem> GetAll(string idFilter)
        {
            var list = new List<VehicleServiceItem>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM VehicleServiceItems WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(idFilter)) query += " AND ServiceItem LIKE @IdFilter";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(idFilter)) cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(MapItem(reader));
                        }
                    }
                }
            }

            return list;
        }

        public VehicleServiceItem GetById(int id)
        {
            VehicleServiceItem model = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT * FROM VehicleServiceItems WHERE Id = @Id AND IsDeleted = 0", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model = MapItem(reader);
                        }
                    }
                }
            }

            return model;
        }

        public int Insert(VehicleServiceItem v)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand(@"
                    INSERT INTO VehicleServiceItems (ServiceItem, Rate)
                    OUTPUT INSERTED.Id
                    VALUES (@ServiceItem, @Rate)", conn))
                {
                    cmd.Parameters.AddWithValue("@ServiceItem", v.ServiceItem ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Rate", (object)v.Rate ?? DBNull.Value);

                    conn.Open();
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        private VehicleServiceItem MapItem(SqlDataReader reader)
        {
            return new VehicleServiceItem
            {
                Id = Convert.ToInt32(reader["Id"]),
                ServiceItem = reader["ServiceItem"]?.ToString() ?? string.Empty,
                Rate = reader["Rate"] != DBNull.Value ? Convert.ToDecimal(reader["Rate"]) : (decimal?)null
            };
        }
    }
}
