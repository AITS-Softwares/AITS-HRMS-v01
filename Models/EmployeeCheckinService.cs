//using System;
//using System.Collections.Generic;
//using Microsoft.Data.SqlClient;
//using Erp_Aits.Models;

//namespace Erp_Aits.Services
//{
//    public class EmployeeCheckinService
//    {
//        private readonly string _connectionString;

//        public EmployeeCheckinService(string connectionString)
//        {
//            _connectionString = connectionString;
//        }

//        public List<EmployeeCheckin> GetAllCheckins()
//        {
//            var checkins = new List<EmployeeCheckin>();

//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = @"SELECT Id, EmployeeCheckinCode, Employee, EmployeeName, Time, LogType
//                                  FROM EmployeeCheckins WHERE IsDeleted = 0 ORDER BY Time DESC, Id DESC";

//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    conn.Open();
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            checkins.Add(new EmployeeCheckin
//                            {
//                                Id = Convert.ToInt32(reader["Id"]),
//                                EmployeeCheckinCode = reader["EmployeeCheckinCode"]?.ToString() ?? string.Empty,
//                                Employee = reader["Employee"]?.ToString() ?? string.Empty,
//                                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
//                                Time = reader["Time"] as DateTime?,
//                                LogType = reader["LogType"]?.ToString() ?? string.Empty
//                            });
//                        }
//                    }
//                }
//            }

//            return checkins;
//        }

//        public EmployeeCheckin GetById(int id)
//        {
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = @"SELECT * FROM EmployeeCheckins WHERE Id = @Id AND IsDeleted = 0";

//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    cmd.Parameters.AddWithValue("@Id", id);
//                    conn.Open();
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        if (reader.Read())
//                        {
//                            return new EmployeeCheckin
//                            {
//                                Id = Convert.ToInt32(reader["Id"]),
//                                EmployeeCheckinCode = reader["EmployeeCheckinCode"]?.ToString() ?? string.Empty,
//                                Employee = reader["Employee"]?.ToString() ?? string.Empty,
//                                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
//                                Time = reader["Time"] as DateTime?,
//                                LogType = reader["LogType"]?.ToString() ?? string.Empty,
//                                LocationDeviceId = reader["LocationDeviceId"]?.ToString() ?? string.Empty,
//                                SkipAutoAttendance = Convert.ToBoolean(reader["SkipAutoAttendance"]),
//                                Shift = reader["Shift"]?.ToString() ?? string.Empty
//                            };
//                        }
//                    }
//                }
//            }

//            return null;
//        }

//        public string GetNextCheckinCode()
//        {
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = "SELECT COUNT(*) FROM EmployeeCheckins";
//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    conn.Open();
//                    int count = (int)cmd.ExecuteScalar() + 1;
//                    return $"EMP-CKIN-{DateTime.Now:MM-yyyy}-{count:D6}";
//                }
//            }
//        }

//        public void InsertCheckin(EmployeeCheckin checkin)
//        {
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = @"
//                    INSERT INTO EmployeeCheckins
//                        (EmployeeCheckinCode, Employee, EmployeeName, Time, LogType,
//                         LocationDeviceId, SkipAutoAttendance, Shift)
//                    VALUES
//                        (@EmployeeCheckinCode, @Employee, @EmployeeName, @Time, @LogType,
//                         @LocationDeviceId, @SkipAutoAttendance, @Shift)";

//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    cmd.Parameters.AddWithValue("@EmployeeCheckinCode", checkin.EmployeeCheckinCode ?? (object)DBNull.Value);
//                    cmd.Parameters.AddWithValue("@Employee", checkin.Employee ?? (object)DBNull.Value);
//                    cmd.Parameters.AddWithValue("@EmployeeName", checkin.EmployeeName ?? (object)DBNull.Value);
//                    cmd.Parameters.AddWithValue("@Time", (object)checkin.Time ?? DateTime.Now);
//                    cmd.Parameters.AddWithValue("@LogType", checkin.LogType ?? (object)DBNull.Value);
//                    cmd.Parameters.AddWithValue("@LocationDeviceId", checkin.LocationDeviceId ?? (object)DBNull.Value);
//                    cmd.Parameters.AddWithValue("@SkipAutoAttendance", checkin.SkipAutoAttendance);
//                    cmd.Parameters.AddWithValue("@Shift", checkin.Shift ?? (object)DBNull.Value);

//                    conn.Open();
//                    cmd.ExecuteNonQuery();
//                }
//            }
//        }
//    }
//}










using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class EmployeeCheckinService
    {
        private readonly string _connectionString;

        public EmployeeCheckinService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<EmployeeCheckin> GetAllCheckins()
        {
            var checkins = new List<EmployeeCheckin>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT Id, EmployeeCheckinCode, Employee, EmployeeName, Time, LogType
                                  FROM EmployeeCheckins WHERE IsDeleted = 0 ORDER BY Time DESC, Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            checkins.Add(new EmployeeCheckin
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                EmployeeCheckinCode = reader["EmployeeCheckinCode"]?.ToString() ?? string.Empty,
                                Employee = reader["Employee"]?.ToString() ?? string.Empty,
                                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
                                Time = reader["Time"] as DateTime?,
                                LogType = reader["LogType"]?.ToString() ?? string.Empty
                            });
                        }
                    }
                }
            }

            return checkins;
        }



        public void InsertOrUpdateByCode(EmployeeCheckin checkin)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                bool exists;

                using (var cmd = new SqlCommand(@"
            SELECT COUNT(*)
            FROM EmployeeCheckins
            WHERE EmployeeCheckinCode = @EmployeeCheckinCode", conn))
                {
                    cmd.Parameters.AddWithValue("@EmployeeCheckinCode", checkin.EmployeeCheckinCode ?? "");
                    exists = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }

                if (exists)
                {
                    string query = @"
                UPDATE EmployeeCheckins
                SET Employee = @Employee,
                    EmployeeName = @EmployeeName,
                    Time = @Time,
                    LogType = @LogType,
                    LocationDeviceId = @LocationDeviceId,
                    SkipAutoAttendance = @SkipAutoAttendance,
                    Shift = @Shift,
                    IsDeleted = 0
                WHERE EmployeeCheckinCode = @EmployeeCheckinCode";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        AddCheckinParameters(cmd, checkin);
                        cmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    string query = @"
                INSERT INTO EmployeeCheckins
                    (EmployeeCheckinCode, Employee, EmployeeName, Time, LogType,
                     LocationDeviceId, SkipAutoAttendance, Shift)
                VALUES
                    (@EmployeeCheckinCode, @Employee, @EmployeeName, @Time, @LogType,
                     @LocationDeviceId, @SkipAutoAttendance, @Shift)";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        AddCheckinParameters(cmd, checkin);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public void UpdateCheckinById(EmployeeCheckin checkin)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
            UPDATE EmployeeCheckins
            SET Employee = @Employee,
                EmployeeName = @EmployeeName,
                Time = @Time,
                LogType = @LogType,
                LocationDeviceId = @LocationDeviceId,
                SkipAutoAttendance = @SkipAutoAttendance,
                Shift = @Shift
            WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", checkin.Id);
                    AddCheckinParameters(cmd, checkin);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void MarkDeletedById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE EmployeeCheckins SET IsDeleted = 1 WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void MarkDeletedIfMissingFromErpNext(HashSet<string> erpCodes)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                if (erpCodes == null || erpCodes.Count == 0)
                    return;

                var parameters = new List<string>();

                using (var cmd = conn.CreateCommand())
                {
                    int i = 0;

                    foreach (var code in erpCodes)
                    {
                        if (string.IsNullOrWhiteSpace(code))
                            continue;

                        string param = "@Code" + i;
                        parameters.Add(param);
                        cmd.Parameters.AddWithValue(param, code.Trim());
                        i++;
                    }

                    if (!parameters.Any())
                        return;

                    cmd.CommandText = $@"
                UPDATE EmployeeCheckins
                SET IsDeleted = 1
                WHERE IsDeleted = 0
                AND EmployeeCheckinCode IS NOT NULL
                AND LTRIM(RTRIM(EmployeeCheckinCode)) <> ''
                AND LTRIM(RTRIM(EmployeeCheckinCode)) NOT IN ({string.Join(",", parameters)})";

                    cmd.ExecuteNonQuery();
                }
            }
        }
        private void AddCheckinParameters(SqlCommand cmd, EmployeeCheckin checkin)
        {
            cmd.Parameters.AddWithValue("@EmployeeCheckinCode", checkin.EmployeeCheckinCode ?? "");
            cmd.Parameters.AddWithValue("@Employee", checkin.Employee ?? "");
            cmd.Parameters.AddWithValue("@EmployeeName", checkin.EmployeeName ?? "");
            cmd.Parameters.AddWithValue("@Time", (object)checkin.Time ?? DateTime.Now);
            cmd.Parameters.AddWithValue("@LogType", checkin.LogType ?? "");
            cmd.Parameters.AddWithValue("@LocationDeviceId", checkin.LocationDeviceId ?? "");
            cmd.Parameters.AddWithValue("@SkipAutoAttendance", checkin.SkipAutoAttendance);
            cmd.Parameters.AddWithValue("@Shift", checkin.Shift ?? "");
        }


























        public EmployeeCheckin GetById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT * FROM EmployeeCheckins WHERE Id = @Id AND IsDeleted = 0";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new EmployeeCheckin
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                EmployeeCheckinCode = reader["EmployeeCheckinCode"]?.ToString() ?? string.Empty,
                                Employee = reader["Employee"]?.ToString() ?? string.Empty,
                                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
                                Time = reader["Time"] as DateTime?,
                                LogType = reader["LogType"]?.ToString() ?? string.Empty,
                                LocationDeviceId = reader["LocationDeviceId"]?.ToString() ?? string.Empty,
                                SkipAutoAttendance = Convert.ToBoolean(reader["SkipAutoAttendance"]),
                                Shift = reader["Shift"]?.ToString() ?? string.Empty
                            };
                        }
                    }
                }
            }

            return null;
        }

        public string GetNextCheckinCode()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT COUNT(*) FROM EmployeeCheckins";
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    int count = (int)cmd.ExecuteScalar() + 1;
                    return $"EMP-CKIN-{DateTime.Now:MM-yyyy}-{count:D6}";
                }
            }
        }

        public HashSet<string> GetExistingCodes()
        {
            var existing = new HashSet<string>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT EmployeeCheckinCode FROM EmployeeCheckins WHERE IsDeleted = 0";
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string code = reader["EmployeeCheckinCode"]?.ToString();
                            if (!string.IsNullOrEmpty(code)) existing.Add(code);
                        }
                    }
                }
            }
            return existing;
        }

        public void InsertCheckin(EmployeeCheckin checkin)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    INSERT INTO EmployeeCheckins
                        (EmployeeCheckinCode, Employee, EmployeeName, Time, LogType,
                         LocationDeviceId, SkipAutoAttendance, Shift)
                    VALUES
                        (@EmployeeCheckinCode, @Employee, @EmployeeName, @Time, @LogType,
                         @LocationDeviceId, @SkipAutoAttendance, @Shift)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@EmployeeCheckinCode", checkin.EmployeeCheckinCode ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Employee", checkin.Employee ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmployeeName", checkin.EmployeeName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Time", (object)checkin.Time ?? DateTime.Now);
                    cmd.Parameters.AddWithValue("@LogType", checkin.LogType ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@LocationDeviceId", checkin.LocationDeviceId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SkipAutoAttendance", checkin.SkipAutoAttendance);
                    cmd.Parameters.AddWithValue("@Shift", checkin.Shift ?? (object)DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateCheckinByCode(EmployeeCheckin checkin)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    UPDATE EmployeeCheckins
                    SET Employee = @Employee,
                        EmployeeName = @EmployeeName,
                        Time = @Time,
                        LogType = @LogType,
                        LocationDeviceId = @LocationDeviceId,
                        SkipAutoAttendance = @SkipAutoAttendance,
                        Shift = @Shift,
                        IsDeleted = 0
                    WHERE EmployeeCheckinCode = @EmployeeCheckinCode";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@EmployeeCheckinCode", checkin.EmployeeCheckinCode);
                    cmd.Parameters.AddWithValue("@Employee", checkin.Employee ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmployeeName", checkin.EmployeeName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Time", (object)checkin.Time ?? DateTime.Now);
                    cmd.Parameters.AddWithValue("@LogType", checkin.LogType ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@LocationDeviceId", checkin.LocationDeviceId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SkipAutoAttendance", checkin.SkipAutoAttendance);
                    cmd.Parameters.AddWithValue("@Shift", checkin.Shift ?? (object)DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}