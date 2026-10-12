using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class PurposeOfTravelService
    {
        private readonly string _connectionString;

        public PurposeOfTravelService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<PurposeOfTravel> GetAll(string idFilter)
        {
            var list = new List<PurposeOfTravel>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT Id, ErpNextId, Purpose FROM PurposeOfTravels WHERE IsDeleted = 0";

                if (!string.IsNullOrWhiteSpace(idFilter))
                    query += " AND Purpose LIKE @IdFilter";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrWhiteSpace(idFilter))
                        cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");

                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            list.Add(Map(reader));
                    }
                }
            }

            return list;
        }

        public PurposeOfTravel GetById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT Id, ErpNextId, Purpose FROM PurposeOfTravels WHERE Id = @Id AND IsDeleted = 0";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            return Map(reader);
                    }
                }
            }

            return null;
        }

        public PurposeOfTravel GetByErpNextId(string erpNextId)
        {
            if (string.IsNullOrWhiteSpace(erpNextId))
                return null;

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT TOP 1 Id, ErpNextId, Purpose FROM PurposeOfTravels WHERE ErpNextId = @ErpNextId AND IsDeleted = 0";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", erpNextId);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            return Map(reader);
                    }
                }
            }

            return null;
        }

        public PurposeOfTravel GetByPurpose(string purpose)
        {
            if (string.IsNullOrWhiteSpace(purpose))
                return null;

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT TOP 1 Id, ErpNextId, Purpose FROM PurposeOfTravels WHERE Purpose = @Purpose AND IsDeleted = 0";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Purpose", purpose);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            return Map(reader);
                    }
                }
            }

            return null;
        }

        public int Insert(PurposeOfTravel model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    INSERT INTO PurposeOfTravels (ErpNextId, Purpose)
                    OUTPUT INSERTED.Id
                    VALUES (@ErpNextId, @Purpose)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", (object)model.ErpNextId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Purpose", (object)model.Purpose ?? DBNull.Value);

                    conn.Open();
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        public void Update(PurposeOfTravel model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    UPDATE PurposeOfTravels SET
                        ErpNextId = @ErpNextId,
                        Purpose = @Purpose
                    WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@ErpNextId", (object)model.ErpNextId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Purpose", (object)model.Purpose ?? DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateErpNextId(int id, string erpNextId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE PurposeOfTravels SET ErpNextId = @ErpNextId WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@ErpNextId", (object)erpNextId ?? DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public int UpsertFromErpNext(PurposeOfTravel model)
        {
            var existing = GetByErpNextId(model.ErpNextId) ?? GetByPurpose(model.Purpose);

            if (existing == null)
                return Insert(model);

            model.Id = existing.Id;
            Update(model);
            return existing.Id;
        }

        public void SoftDeleteById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE PurposeOfTravels SET IsDeleted = 1 WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void MarkDeletedIfMissingFromErpNext(HashSet<string> erpIds)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                if (erpIds == null || erpIds.Count == 0)
                {
                    using (var cmd = new SqlCommand(@"
                        UPDATE PurposeOfTravels
                        SET IsDeleted = 1
                        WHERE IsDeleted = 0
                          AND ErpNextId IS NOT NULL
                          AND ErpNextId <> ''", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    return;
                }

                var parameterNames = new List<string>();
                int i = 0;

                foreach (var _ in erpIds)
                {
                    parameterNames.Add("@p" + i);
                    i++;
                }

                string query = @"
                    UPDATE PurposeOfTravels
                    SET IsDeleted = 1
                    WHERE IsDeleted = 0
                      AND ErpNextId IS NOT NULL
                      AND ErpNextId <> ''
                      AND ErpNextId NOT IN (" + string.Join(",", parameterNames) + ")";

                using (var cmd = new SqlCommand(query, conn))
                {
                    i = 0;

                    foreach (var erpId in erpIds)
                    {
                        cmd.Parameters.AddWithValue("@p" + i, erpId);
                        i++;
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }

        private PurposeOfTravel Map(SqlDataReader reader)
        {
            return new PurposeOfTravel
            {
                Id = Convert.ToInt32(reader["Id"]),
                ErpNextId = reader["ErpNextId"]?.ToString() ?? string.Empty,
                Purpose = reader["Purpose"]?.ToString() ?? string.Empty
            };
        }
    }
}
