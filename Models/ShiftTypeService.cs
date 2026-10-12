using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class ShiftTypeService
    {
        private readonly string _connectionString;

        public ShiftTypeService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<ShiftType> GetAll(string idFilter = null)
        {
            var list = new List<ShiftType>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
            SELECT Id, ShiftTypeCode, Name, StartTime, EndTime, HolidayList, RosterColor, EnableAutoAttendance
            FROM ShiftTypes
            WHERE IsDeleted = 0";

                if (!string.IsNullOrWhiteSpace(idFilter))
                {
                    query += @" AND (
                ShiftTypeCode LIKE @IdFilter
                OR Name LIKE @IdFilter
                OR HolidayList LIKE @IdFilter
            )";
                }

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
                            list.Add(new ShiftType
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                ShiftTypeCode = reader["ShiftTypeCode"]?.ToString() ?? "",
                                Name = reader["Name"]?.ToString() ?? "",
                                StartTime = reader["StartTime"]?.ToString() ?? "",
                                EndTime = reader["EndTime"]?.ToString() ?? "",
                                HolidayList = reader["HolidayList"]?.ToString() ?? "",
                                RosterColor = reader["RosterColor"]?.ToString() ?? "Blue",
                                EnableAutoAttendance = reader["EnableAutoAttendance"] != DBNull.Value &&
                                                       Convert.ToBoolean(reader["EnableAutoAttendance"])
                            });
                        }
                    }
                }
            }

            return list;
        }
        public ShiftType GetById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT Id, ShiftTypeCode, Name, StartTime, EndTime, HolidayList, RosterColor, EnableAutoAttendance
                    FROM ShiftTypes
                    WHERE Id = @Id AND IsDeleted = 0";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            return ReadShiftType(reader);
                    }
                }
            }

            return null;
        }

        public ShiftType GetByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT Id, ShiftTypeCode, Name, StartTime, EndTime, HolidayList, RosterColor, EnableAutoAttendance
                    FROM ShiftTypes
                    WHERE Name = @Name AND IsDeleted = 0";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", name);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            return ReadShiftType(reader);
                    }
                }
            }

            return null;
        }

        public ShiftType GetByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT Id, ShiftTypeCode, Name, StartTime, EndTime, HolidayList, RosterColor, EnableAutoAttendance
                    FROM ShiftTypes
                    WHERE ShiftTypeCode = @Code AND IsDeleted = 0";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Code", code);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            return ReadShiftType(reader);
                    }
                }
            }

            return null;
        }

        public string GetNextCode()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT COUNT(*) FROM ShiftTypes";

                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar()) + 1;
                    return $"ST-{count:D5}";
                }
            }
        }

        public int Insert(ShiftType model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    INSERT INTO ShiftTypes
                        (ShiftTypeCode, Name, StartTime, EndTime, HolidayList, RosterColor, EnableAutoAttendance)
                    OUTPUT INSERTED.Id
                    VALUES
                        (@ShiftTypeCode, @Name, @StartTime, @EndTime, @HolidayList, @RosterColor, @EnableAutoAttendance)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ShiftTypeCode", model.ShiftTypeCode ?? "");
                    cmd.Parameters.AddWithValue("@Name", model.Name ?? "");
                    cmd.Parameters.AddWithValue("@StartTime", model.StartTime ?? "");
                    cmd.Parameters.AddWithValue("@EndTime", model.EndTime ?? "");
                    cmd.Parameters.AddWithValue("@HolidayList", model.HolidayList ?? "");
                    cmd.Parameters.AddWithValue("@RosterColor", model.RosterColor ?? "Blue");
                    cmd.Parameters.AddWithValue("@EnableAutoAttendance", model.EnableAutoAttendance);

                    conn.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void Update(ShiftType model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    UPDATE ShiftTypes
                    SET ShiftTypeCode = @ShiftTypeCode,
                        Name = @Name,
                        StartTime = @StartTime,
                        EndTime = @EndTime,
                        HolidayList = @HolidayList,
                        RosterColor = @RosterColor,
                        EnableAutoAttendance = @EnableAutoAttendance
                    WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@ShiftTypeCode", model.ShiftTypeCode ?? "");
                    cmd.Parameters.AddWithValue("@Name", model.Name ?? "");
                    cmd.Parameters.AddWithValue("@StartTime", model.StartTime ?? "");
                    cmd.Parameters.AddWithValue("@EndTime", model.EndTime ?? "");
                    cmd.Parameters.AddWithValue("@HolidayList", model.HolidayList ?? "");
                    cmd.Parameters.AddWithValue("@RosterColor", model.RosterColor ?? "Blue");
                    cmd.Parameters.AddWithValue("@EnableAutoAttendance", model.EnableAutoAttendance);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void InsertOrUpdateFromErpNext(ShiftType model)
        {
            var existing = GetByName(model.Name) ?? GetByCode(model.ShiftTypeCode);

            if (existing == null)
            {
                Insert(model);
                return;
            }

            model.Id = existing.Id;
            Update(model);
        }

        public int UpsertFromErpNextBatch(List<ShiftType> items, HashSet<string> erpNames)
        {
            if (items == null || items.Count == 0) return 0;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                var existingByName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                var existingByCode = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                using (var cmd = new SqlCommand(@"
                    SELECT Id, ShiftTypeCode, Name
                    FROM ShiftTypes
                    WHERE IsDeleted = 0", conn))
                {
                    cmd.CommandTimeout = 60;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var id = Convert.ToInt32(reader["Id"]);
                            var code = reader["ShiftTypeCode"]?.ToString() ?? string.Empty;
                            var name = reader["Name"]?.ToString() ?? string.Empty;

                            if (!string.IsNullOrWhiteSpace(name) && !existingByName.ContainsKey(name))
                                existingByName[name] = id;

                            if (!string.IsNullOrWhiteSpace(code) && !existingByCode.ContainsKey(code))
                                existingByCode[code] = id;
                        }
                    }
                }

                int changed = 0;
                foreach (var model in items)
                {
                    int id = 0;
                    if (!string.IsNullOrWhiteSpace(model.Name))
                        existingByName.TryGetValue(model.Name, out id);

                    if (id == 0 && !string.IsNullOrWhiteSpace(model.ShiftTypeCode))
                        existingByCode.TryGetValue(model.ShiftTypeCode, out id);

                    if (id == 0)
                    {
                        using (var cmd = new SqlCommand(@"
                            INSERT INTO ShiftTypes
                                (ShiftTypeCode, Name, StartTime, EndTime, HolidayList, RosterColor, EnableAutoAttendance, IsDeleted)
                            VALUES
                                (@ShiftTypeCode, @Name, @StartTime, @EndTime, @HolidayList, @RosterColor, @EnableAutoAttendance, 0)", conn))
                        {
                            cmd.CommandTimeout = 60;
                            AddShiftTypeParameters(cmd, model);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        using (var cmd = new SqlCommand(@"
                            UPDATE ShiftTypes
                            SET ShiftTypeCode = @ShiftTypeCode,
                                Name = @Name,
                                StartTime = @StartTime,
                                EndTime = @EndTime,
                                HolidayList = @HolidayList,
                                RosterColor = @RosterColor,
                                EnableAutoAttendance = @EnableAutoAttendance,
                                IsDeleted = 0
                            WHERE Id = @Id", conn))
                        {
                            cmd.CommandTimeout = 60;
                            cmd.Parameters.AddWithValue("@Id", id);
                            AddShiftTypeParameters(cmd, model);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    changed++;
                }

                if (erpNames != null && erpNames.Count > 0)
                {
                    var parameters = erpNames.Select((_, i) => "@Name" + i).ToList();
                    using (var cmd = new SqlCommand($@"
                        UPDATE ShiftTypes
                        SET IsDeleted = 1
                        WHERE IsDeleted = 0
                          AND Name NOT IN ({string.Join(",", parameters)})", conn))
                    {
                        cmd.CommandTimeout = 60;
                        int i = 0;
                        foreach (var name in erpNames)
                        {
                            cmd.Parameters.AddWithValue("@Name" + i, name);
                            i++;
                        }
                        cmd.ExecuteNonQuery();
                    }
                }

                return changed;
            }
        }

        public void MarkDeletedById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE ShiftTypes SET IsDeleted = 1 WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static ShiftType ReadShiftType(SqlDataReader reader)
        {
            return new ShiftType
            {
                Id = Convert.ToInt32(reader["Id"]),
                ShiftTypeCode = reader["ShiftTypeCode"]?.ToString() ?? "",
                Name = reader["Name"]?.ToString() ?? "",
                StartTime = reader["StartTime"]?.ToString() ?? "",
                EndTime = reader["EndTime"]?.ToString() ?? "",
                HolidayList = reader["HolidayList"]?.ToString() ?? "",
                RosterColor = reader["RosterColor"]?.ToString() ?? "Blue",
                EnableAutoAttendance = reader["EnableAutoAttendance"] != DBNull.Value &&
                                       Convert.ToBoolean(reader["EnableAutoAttendance"])
            };
        }

        private static void AddShiftTypeParameters(SqlCommand cmd, ShiftType model)
        {
            cmd.Parameters.AddWithValue("@ShiftTypeCode", model.ShiftTypeCode ?? "");
            cmd.Parameters.AddWithValue("@Name", model.Name ?? "");
            cmd.Parameters.AddWithValue("@StartTime", model.StartTime ?? "");
            cmd.Parameters.AddWithValue("@EndTime", model.EndTime ?? "");
            cmd.Parameters.AddWithValue("@HolidayList", model.HolidayList ?? "");
            cmd.Parameters.AddWithValue("@RosterColor", model.RosterColor ?? "Blue");
            cmd.Parameters.AddWithValue("@EnableAutoAttendance", model.EnableAutoAttendance);
        }
    }
}
