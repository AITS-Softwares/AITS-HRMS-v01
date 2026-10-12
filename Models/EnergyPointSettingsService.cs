using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class EnergyPointSettingsService
    {
        private readonly string _connectionString;

        public EnergyPointSettingsService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public EnergyPointSettings GetSettings()
        {
            EnergyPointSettings model = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string query = "SELECT TOP 1 * FROM EnergyPointSettings ORDER BY Id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model = new EnergyPointSettings
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Enabled = reader["Enabled"] as bool? ?? false,
                                PointAllocationPeriodicity = reader["PointAllocationPeriodicity"]?.ToString() ?? "Weekly",
                                LastPointAllocationDate = reader["LastPointAllocationDate"] as DateTime?
                            };
                        }
                    }
                }

                if (model == null)
                {
                    // No settings row exists yet — return defaults (not yet saved)
                    return new EnergyPointSettings { Id = 0, Enabled = false, PointAllocationPeriodicity = "Weekly" };
                }

                string q2 = "SELECT Id, EnergyPointSettingsId, LevelName, Role, ReviewPoints FROM EnergyPointReviewLevels WHERE EnergyPointSettingsId = @Id";
                using (var cmd = new SqlCommand(q2, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.ReviewLevels.Add(new EnergyPointReviewLevel
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                EnergyPointSettingsId = Convert.ToInt32(reader["EnergyPointSettingsId"]),
                                LevelName = reader["LevelName"]?.ToString() ?? string.Empty,
                                Role = reader["Role"]?.ToString() ?? string.Empty,
                                ReviewPoints = Convert.ToInt32(reader["ReviewPoints"])
                            });
                        }
                    }
                }
            }

            return model;
        }

        public void SaveSettings(EnergyPointSettings model, List<string> levelNames, List<string> roles, List<int> reviewPoints)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int settingsId;

                        // Check if a settings row already exists
                        int existingId = 0;
                        using (var cmd = new SqlCommand("SELECT TOP 1 Id FROM EnergyPointSettings ORDER BY Id", conn, transaction))
                        {
                            var result = cmd.ExecuteScalar();
                            if (result != null) existingId = Convert.ToInt32(result);
                        }

                        if (existingId == 0)
                        {
                            string insertQuery = @"
                                INSERT INTO EnergyPointSettings (Enabled, PointAllocationPeriodicity, LastPointAllocationDate)
                                OUTPUT INSERTED.Id
                                VALUES (@Enabled, @Periodicity, @LastDate)";

                            using (var cmd = new SqlCommand(insertQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Enabled", model.Enabled);
                                cmd.Parameters.AddWithValue("@Periodicity", model.PointAllocationPeriodicity ?? "Weekly");
                                cmd.Parameters.AddWithValue("@LastDate", (object)model.LastPointAllocationDate ?? DBNull.Value);
                                settingsId = (int)cmd.ExecuteScalar();
                            }
                        }
                        else
                        {
                            settingsId = existingId;
                            string updateQuery = @"
                                UPDATE EnergyPointSettings SET
                                    Enabled = @Enabled,
                                    PointAllocationPeriodicity = @Periodicity,
                                    LastPointAllocationDate = @LastDate
                                WHERE Id = @Id";

                            using (var cmd = new SqlCommand(updateQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Id", settingsId);
                                cmd.Parameters.AddWithValue("@Enabled", model.Enabled);
                                cmd.Parameters.AddWithValue("@Periodicity", model.PointAllocationPeriodicity ?? "Weekly");
                                cmd.Parameters.AddWithValue("@LastDate", (object)model.LastPointAllocationDate ?? DBNull.Value);
                                cmd.ExecuteNonQuery();
                            }

                            using (var cmd = new SqlCommand("DELETE FROM EnergyPointReviewLevels WHERE EnergyPointSettingsId = @Id", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Id", settingsId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        if (levelNames != null)
                        {
                            for (int i = 0; i < levelNames.Count; i++)
                            {
                                if (string.IsNullOrWhiteSpace(levelNames[i])) continue;

                                string role = (roles != null && i < roles.Count) ? roles[i] : null;
                                int points = (reviewPoints != null && i < reviewPoints.Count) ? reviewPoints[i] : 0;

                                string q = "INSERT INTO EnergyPointReviewLevels (EnergyPointSettingsId, LevelName, Role, ReviewPoints) VALUES (@Sid, @LevelName, @Role, @Points)";
                                using (var cmd = new SqlCommand(q, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@Sid", settingsId);
                                    cmd.Parameters.AddWithValue("@LevelName", levelNames[i]);
                                    cmd.Parameters.AddWithValue("@Role", role ?? (object)DBNull.Value);
                                    cmd.Parameters.AddWithValue("@Points", points);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

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
    }
}
