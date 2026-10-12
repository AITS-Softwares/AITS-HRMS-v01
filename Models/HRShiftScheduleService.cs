using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class HRShiftScheduleService
    {
        private readonly string _connectionString;
        private static bool _statusColumnChecked;
        private static readonly object _statusColumnLock = new object();

        public HRShiftScheduleService(string connectionString)
        {
            _connectionString = connectionString;
            EnsureStatusColumn();
        }

        public List<HRShiftSchedule> GetAll(string idFilter, string shiftType)
        {
            var list = new List<HRShiftSchedule>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT Id, Name, ShiftType, Frequency, Status FROM HRShiftSchedules WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(idFilter)) query += " AND CAST(Id AS VARCHAR) LIKE @IdFilter";
                if (!string.IsNullOrEmpty(shiftType)) query += " AND ShiftType LIKE @ShiftType";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(idFilter)) cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");
                    if (!string.IsNullOrEmpty(shiftType)) cmd.Parameters.AddWithValue("@ShiftType", "%" + shiftType + "%");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new HRShiftSchedule
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Name = reader["Name"]?.ToString() ?? string.Empty,
                                ShiftType = reader["ShiftType"]?.ToString() ?? string.Empty,
                                Frequency = reader["Frequency"]?.ToString() ?? "Every Week",
                                Status = HasColumn(reader, "Status") ? reader["Status"]?.ToString() ?? "Draft" : "Draft"
                            });
                        }
                    }
                }
            }

            return list;
        }

        public HRShiftSchedule GetById(int id)
        {
            HRShiftSchedule schedule = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand("SELECT * FROM HRShiftSchedules WHERE Id = @Id AND IsDeleted = 0", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            schedule = new HRShiftSchedule
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Name = reader["Name"]?.ToString() ?? string.Empty,
                                ShiftType = reader["ShiftType"]?.ToString() ?? string.Empty,
                                Frequency = reader["Frequency"]?.ToString() ?? "Every Week",
                                Status = HasColumn(reader, "Status") ? reader["Status"]?.ToString() ?? "Draft" : "Draft"
                            };
                        }
                    }
                }

                if (schedule != null)
                {
                    using (var cmd = new SqlCommand("SELECT * FROM HRShiftScheduleDays WHERE ShiftScheduleId = @Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                schedule.Days.Add(new HRShiftScheduleDay
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    ShiftScheduleId = id,
                                    Day = reader["Day"]?.ToString() ?? string.Empty
                                });
                            }
                        }
                    }
                }
            }

            return schedule;
        }

        public int Insert(HRShiftSchedule s)
        {
            int newId;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand(@"
                    INSERT INTO HRShiftSchedules (Name, ShiftType, Frequency, Status)
                    OUTPUT INSERTED.Id
                    VALUES (@Name, @ShiftType, @Frequency, @Status)", conn))
                {
                    cmd.Parameters.AddWithValue("@Name", s.Name ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ShiftType", s.ShiftType ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Frequency", s.Frequency ?? "Every Week");
                    cmd.Parameters.AddWithValue("@Status", s.Status ?? "Draft");

                    newId = (int)cmd.ExecuteScalar();
                }

                foreach (var day in s.Days)
                {
                    if (string.IsNullOrWhiteSpace(day.Day)) continue;

                    using (var cmd = new SqlCommand(@"
                        INSERT INTO HRShiftScheduleDays (ShiftScheduleId, Day)
                        VALUES (@ShiftScheduleId, @Day)", conn))
                    {
                        cmd.Parameters.AddWithValue("@ShiftScheduleId", newId);
                        cmd.Parameters.AddWithValue("@Day", day.Day);

                        cmd.ExecuteNonQuery();
                    }
                }
            }

            return newId;
        }

        public int InsertOrUpdate(HRShiftSchedule s)
        {
            var existing = GetByName(s.Name);
            if (existing == null)
            {
                return Insert(s);
            }

            s.Id = existing.Id;
            Update(s);
            return existing.Id;
        }

        public HRShiftSchedule GetByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT TOP 1 * FROM HRShiftSchedules WHERE Name = @Name", conn))
                {
                    cmd.Parameters.AddWithValue("@Name", name);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new HRShiftSchedule
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Name = reader["Name"]?.ToString() ?? string.Empty,
                                ShiftType = reader["ShiftType"]?.ToString() ?? string.Empty,
                                Frequency = reader["Frequency"]?.ToString() ?? "Every Week",
                                Status = HasColumn(reader, "Status") ? reader["Status"]?.ToString() ?? "Draft" : "Draft"
                            };
                        }
                    }
                }
            }

            return null;
        }

        public void Update(HRShiftSchedule s)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand(@"
                    UPDATE HRShiftSchedules
                    SET Name = @Name,
                        ShiftType = @ShiftType,
                        Frequency = @Frequency,
                        Status = @Status,
                        IsDeleted = 0
                    WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", s.Id);
                    cmd.Parameters.AddWithValue("@Name", s.Name ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ShiftType", s.ShiftType ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Frequency", s.Frequency ?? "Every Week");
                    cmd.Parameters.AddWithValue("@Status", s.Status ?? "Draft");
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new SqlCommand("DELETE FROM HRShiftScheduleDays WHERE ShiftScheduleId = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", s.Id);
                    cmd.ExecuteNonQuery();
                }

                foreach (var day in s.Days)
                {
                    if (string.IsNullOrWhiteSpace(day.Day)) continue;

                    using (var cmd = new SqlCommand(@"
                        INSERT INTO HRShiftScheduleDays (ShiftScheduleId, Day)
                        VALUES (@ShiftScheduleId, @Day)", conn))
                    {
                        cmd.Parameters.AddWithValue("@ShiftScheduleId", s.Id);
                        cmd.Parameters.AddWithValue("@Day", day.Day);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public void MarkDeletedById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("UPDATE HRShiftSchedules SET IsDeleted = 1 WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void EnsureStatusColumn()
        {
            if (_statusColumnChecked) return;

            lock (_statusColumnLock)
            {
                if (_statusColumnChecked) return;

                try
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        using (var cmd = new SqlCommand(@"
                            IF COL_LENGTH('HRShiftSchedules', 'Status') IS NULL
                            BEGIN
                                ALTER TABLE HRShiftSchedules ADD Status NVARCHAR(50) NULL;
                            END", conn))
                        {
                            cmd.CommandTimeout = 5;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("HRShiftSchedules Status column check skipped: " + ex.Message);
                }
                finally
                {
                    _statusColumnChecked = true;
                }
            }
        }

        private static bool HasColumn(System.Data.Common.DbDataReader reader, string column)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(column, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }
    }
}
