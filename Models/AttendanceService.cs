////using System;
////using System.Collections.Generic;
////using Microsoft.Data.SqlClient;
////using Erp_Aits.Models;

////namespace Erp_Aits.Services
////{
////    public class AttendanceService
////    {
////        private readonly string _connectionString;

////        public AttendanceService(string connectionString)
////        {
////            _connectionString = connectionString;
////        }

////        public List<Attendance> GetAllAttendance(string employeeFilter = null, string statusFilter = null)
////        {
////            var records = new List<Attendance>();

////            using (var conn = new SqlConnection(_connectionString))
////            {
////                string query = "SELECT Id, AttendanceCode, Employee, EmployeeName, AttendanceDate, Status FROM Attendances WHERE IsDeleted = 0";

////                if (!string.IsNullOrEmpty(employeeFilter))
////                    query += " AND Employee = @Employee";

////                if (!string.IsNullOrEmpty(statusFilter))
////                    query += " AND Status = @Status";

////                query += " ORDER BY AttendanceDate DESC";

////                using (var cmd = new SqlCommand(query, conn))
////                {
////                    if (!string.IsNullOrEmpty(employeeFilter))
////                        cmd.Parameters.AddWithValue("@Employee", employeeFilter);

////                    if (!string.IsNullOrEmpty(statusFilter))
////                        cmd.Parameters.AddWithValue("@Status", statusFilter);

////                    conn.Open();
////                    using (var reader = cmd.ExecuteReader())
////                    {
////                        while (reader.Read())
////                        {
////                            records.Add(new Attendance
////                            {
////                                Id = Convert.ToInt32(reader["Id"]),
////                                AttendanceCode = reader["AttendanceCode"]?.ToString() ?? string.Empty,
////                                Employee = reader["Employee"]?.ToString() ?? string.Empty,
////                                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
////                                AttendanceDate = Convert.ToDateTime(reader["AttendanceDate"]),
////                                Status = reader["Status"]?.ToString() ?? "Present"
////                            });
////                        }
////                    }
////                }
////            }

////            return records;
////        }

////        public string GetNextAttendanceCode()
////        {
////            using (var conn = new SqlConnection(_connectionString))
////            {
////                string query = "SELECT COUNT(*) FROM Attendances";
////                using (var cmd = new SqlCommand(query, conn))
////                {
////                    conn.Open();
////                    int count = (int)cmd.ExecuteScalar() + 1;
////                    return $"HR-ATT-{DateTime.Now.Year}-{count:D5}";
////                }
////            }
////        }

////        public void InsertAttendance(Attendance a)
////        {
////            using (var conn = new SqlConnection(_connectionString))
////            {
////                string query = @"
////                    INSERT INTO Attendances
////                        (AttendanceCode, Employee, EmployeeName, Company, AttendanceDate, Status, Shift, LateEntry, EarlyExit)
////                    VALUES
////                        (@AttendanceCode, @Employee, @EmployeeName, @Company, @AttendanceDate, @Status, @Shift, @LateEntry, @EarlyExit)";

////                using (var cmd = new SqlCommand(query, conn))
////                {
////                    cmd.Parameters.AddWithValue("@AttendanceCode", a.AttendanceCode ?? (object)DBNull.Value);
////                    cmd.Parameters.AddWithValue("@Employee", a.Employee ?? (object)DBNull.Value);
////                    cmd.Parameters.AddWithValue("@EmployeeName", a.EmployeeName ?? (object)DBNull.Value);
////                    cmd.Parameters.AddWithValue("@Company", a.Company ?? (object)DBNull.Value);
////                    cmd.Parameters.AddWithValue("@AttendanceDate", a.AttendanceDate);
////                    cmd.Parameters.AddWithValue("@Status", a.Status ?? "Present");
////                    cmd.Parameters.AddWithValue("@Shift", a.Shift ?? (object)DBNull.Value);
////                    cmd.Parameters.AddWithValue("@LateEntry", a.LateEntry);
////                    cmd.Parameters.AddWithValue("@EarlyExit", a.EarlyExit);

////                    conn.Open();
////                    cmd.ExecuteNonQuery();
////                }
////            }
////        }
////    }
////}


//using System;
//using System.Collections.Generic;
//using Microsoft.Data.SqlClient;
//using Erp_Aits.Models;

//namespace Erp_Aits.Services
//{
//    public class AttendanceService
//    {
//        private readonly string _connectionString;

//        public AttendanceService(string connectionString)
//        {
//            _connectionString = connectionString;
//        }

//        public List<Attendance> GetAllAttendance(string employeeFilter = null, string statusFilter = null)
//        {
//            var records = new List<Attendance>();

//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = "SELECT Id, AttendanceCode, Employee, EmployeeName, Company, AttendanceDate, Status, Shift, LateEntry, EarlyExit FROM Attendances WHERE IsDeleted = 0";

//                if (!string.IsNullOrEmpty(employeeFilter))
//                    query += " AND (Employee LIKE @Employee OR EmployeeName LIKE @Employee)";

//                if (!string.IsNullOrEmpty(statusFilter))
//                    query += " AND Status = @Status";

//                query += " ORDER BY AttendanceDate DESC";

//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    if (!string.IsNullOrEmpty(employeeFilter))
//                        cmd.Parameters.AddWithValue("@Employee", $"%{employeeFilter}%");

//                    if (!string.IsNullOrEmpty(statusFilter))
//                        cmd.Parameters.AddWithValue("@Status", statusFilter);

//                    conn.Open();
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            records.Add(new Attendance
//                            {
//                                Id = Convert.ToInt32(reader["Id"]),
//                                AttendanceCode = reader["AttendanceCode"]?.ToString() ?? string.Empty,
//                                Employee = reader["Employee"]?.ToString() ?? string.Empty,
//                                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
//                                Company = reader["Company"]?.ToString() ?? string.Empty,
//                                AttendanceDate = Convert.ToDateTime(reader["AttendanceDate"]),
//                                Status = reader["Status"]?.ToString() ?? "Present",
//                                Shift = reader["Shift"]?.ToString() ?? string.Empty,
//                                LateEntry = reader["LateEntry"] != DBNull.Value && Convert.ToBoolean(reader["LateEntry"]),
//                                EarlyExit = reader["EarlyExit"] != DBNull.Value && Convert.ToBoolean(reader["EarlyExit"])
//                            });
//                        }
//                    }
//                }
//            }

//            return records;
//        }

//        public HashSet<string> GetExistingAttendanceCodes()
//        {
//            var codes = new HashSet<string>();
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = "SELECT AttendanceCode FROM Attendances WHERE AttendanceCode IS NOT NULL AND AttendanceCode != ''";
//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    conn.Open();
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            codes.Add(reader["AttendanceCode"].ToString());
//                        }
//                    }
//                }
//            }
//            return codes;
//        }

//        //public string GetNextAttendanceCode()
//        //{
//        //    using (var conn = new SqlConnection(_connectionString))
//        //    {
//        //        string query = "SELECT COUNT(*) FROM Attendances";
//        //        using (var cmd = new SqlCommand(query, conn))
//        //        {
//        //            conn.Open();
//        //            int count = (int)cmd.ExecuteScalar() + 1;
//        //            return $"HR-ATT-{DateTime.Now.Year}-{count:D5}";
//        //        }
//        //    }
//        //}


//        public string GetNextAttendanceCode()
//        {
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                conn.Open();

//                string query = "SELECT TOP 1 AttendanceCode FROM Attendances WHERE AttendanceCode LIKE @YearPattern ORDER BY Id DESC";
//                string lastCode = null;

//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    cmd.Parameters.AddWithValue("@YearPattern", $"HR-ATT-{DateTime.Now.Year}-%");
//                    lastCode = cmd.ExecuteScalar()?.ToString();
//                }

//                int nextNumber = 1;
//                if (!string.IsNullOrEmpty(lastCode))
//                {
//                    string parts = lastCode.Substring(lastCode.LastIndexOf('-') + 1);
//                    if (int.TryParse(parts, out int parsedNum))
//                    {
//                        nextNumber = parsedNum + 1;
//                    }
//                }
//                else
//                {
//                    string countQuery = "SELECT COUNT(*) FROM Attendances";
//                    using (var countCmd = new SqlCommand(countQuery, conn))
//                    {
//                        nextNumber = (int)countCmd.ExecuteScalar() + 1;
//                    }
//                }

//                string generatedCode;
//                int safetyCounter = 0;

//                // Loop and check uniqueness. If it collides, append a unique tick/suffix so it never fails.
//                do
//                {
//                    generatedCode = $"HR-ATT-{DateTime.Now.Year}-{nextNumber + safetyCounter:D5}";
//                    string checkQuery = "SELECT COUNT(*) FROM Attendances WHERE AttendanceCode = @Code";
//                    using (var checkCmd = new SqlCommand(checkQuery, conn))
//                    {
//                        checkCmd.Parameters.AddWithValue("@Code", generatedCode);
//                        if ((int)checkCmd.ExecuteScalar() == 0) break;
//                    }
//                    safetyCounter++;
//                } while (safetyCounter < 100);

//                // Ultimate fallback if sequence is completely bottlenecked
//                if (safetyCounter >= 100)
//                {
//                    generatedCode = $"HR-ATT-{DateTime.Now.Year}-{Guid.NewGuid().ToString().Substring(0, 5).ToUpper()}";
//                }

//                return generatedCode;
//            }
//        }


















//        public void InsertAttendance(Attendance a)
//        {
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = @"
//                    INSERT INTO Attendances
//                        (AttendanceCode, Employee, EmployeeName, Company, AttendanceDate, Status, Shift, LateEntry, EarlyExit, IsDeleted)
//                    VALUES
//                        (@AttendanceCode, @Employee, @EmployeeName, @Company, @AttendanceDate, @Status, @Shift, @LateEntry, @EarlyExit, 0)";

//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    cmd.Parameters.AddWithValue("@AttendanceCode", a.AttendanceCode ?? (object)DBNull.Value);
//                    cmd.Parameters.AddWithValue("@Employee", a.Employee ?? (object)DBNull.Value);
//                    cmd.Parameters.AddWithValue("@EmployeeName", a.EmployeeName ?? (object)DBNull.Value);
//                    cmd.Parameters.AddWithValue("@Company", a.Company ?? (object)DBNull.Value);
//                    cmd.Parameters.AddWithValue("@AttendanceDate", a.AttendanceDate);
//                    cmd.Parameters.AddWithValue("@Status", a.Status ?? "Present");
//                    cmd.Parameters.AddWithValue("@Shift", a.Shift ?? (object)DBNull.Value);
//                    cmd.Parameters.AddWithValue("@LateEntry", a.LateEntry);
//                    cmd.Parameters.AddWithValue("@EarlyExit", a.EarlyExit);

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
    public class AttendanceService
    {
        private readonly string _connectionString;

        public AttendanceService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Attendance> GetAllAttendance(string employeeFilter = null, string statusFilter = null)
        {
            var records = new List<Attendance>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT Id, ErpNextId, DocStatus, AttendanceCode, Employee, EmployeeName, Company, AttendanceDate, Status, Shift, LateEntry, EarlyExit FROM Attendances WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(employeeFilter))
                    query += " AND (Employee LIKE @Employee OR EmployeeName LIKE @Employee)";

                if (!string.IsNullOrEmpty(statusFilter))
                    query += " AND Status = @Status";

                query += " ORDER BY AttendanceDate DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(employeeFilter))
                        cmd.Parameters.AddWithValue("@Employee", $"%{employeeFilter}%");

                    if (!string.IsNullOrEmpty(statusFilter))
                        cmd.Parameters.AddWithValue("@Status", statusFilter);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            records.Add(new Attendance
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                ErpNextId = reader["ErpNextId"]?.ToString(),
                                DocStatus = reader["DocStatus"] != DBNull.Value ? Convert.ToInt32(reader["DocStatus"]) : 0,
                                AttendanceCode = reader["AttendanceCode"]?.ToString() ?? string.Empty,
                                Employee = reader["Employee"]?.ToString() ?? string.Empty,
                                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
                                Company = reader["Company"]?.ToString() ?? string.Empty,
                                AttendanceDate = Convert.ToDateTime(reader["AttendanceDate"]),
                                Status = reader["Status"]?.ToString() ?? "Present",
                                Shift = reader["Shift"]?.ToString() ?? string.Empty,
                                LateEntry = reader["LateEntry"] != DBNull.Value && Convert.ToBoolean(reader["LateEntry"]),
                                EarlyExit = reader["EarlyExit"] != DBNull.Value && Convert.ToBoolean(reader["EarlyExit"])
                            });
                        }
                    }
                }
            }
            return records;
        }


        public Attendance GetByErpNextId(string erpNextId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
            SELECT *
            FROM Attendances
            WHERE ErpNextId = @ErpNextId OR AttendanceCode = @ErpNextId";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", erpNextId);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Attendance
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                ErpNextId = reader["ErpNextId"]?.ToString(),
                                DocStatus = reader["DocStatus"] != DBNull.Value ? Convert.ToInt32(reader["DocStatus"]) : 0,
                                AttendanceCode = reader["AttendanceCode"]?.ToString() ?? string.Empty,
                                Employee = reader["Employee"]?.ToString() ?? string.Empty,
                                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
                                Company = reader["Company"]?.ToString() ?? string.Empty,
                                AttendanceDate = Convert.ToDateTime(reader["AttendanceDate"]),
                                Status = reader["Status"]?.ToString() ?? "Present",
                                Shift = reader["Shift"]?.ToString() ?? string.Empty,
                                LateEntry = reader["LateEntry"] != DBNull.Value && Convert.ToBoolean(reader["LateEntry"]),
                                EarlyExit = reader["EarlyExit"] != DBNull.Value && Convert.ToBoolean(reader["EarlyExit"])
                            };
                        }
                    }
                }
            }

            return null;
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

                    if (parameters.Count == 0)
                        return;

                    cmd.CommandText = $@"
                UPDATE Attendances
                SET IsDeleted = 1
                WHERE IsDeleted = 0
                  AND AttendanceCode IS NOT NULL
                  AND LTRIM(RTRIM(AttendanceCode)) <> ''
                  AND LTRIM(RTRIM(AttendanceCode)) NOT IN ({string.Join(",", parameters)})";

                    cmd.ExecuteNonQuery();
                }
            }
        }













        public Attendance GetById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Attendances WHERE Id = @Id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Attendance
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                ErpNextId = reader["ErpNextId"]?.ToString(),
                                DocStatus = reader["DocStatus"] != DBNull.Value ? Convert.ToInt32(reader["DocStatus"]) : 0,
                                AttendanceCode = reader["AttendanceCode"]?.ToString() ?? string.Empty,
                                Employee = reader["Employee"]?.ToString() ?? string.Empty,
                                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
                                Company = reader["Company"]?.ToString() ?? string.Empty,
                                AttendanceDate = Convert.ToDateTime(reader["AttendanceDate"]),
                                Status = reader["Status"]?.ToString() ?? "Present",
                                Shift = reader["Shift"]?.ToString() ?? string.Empty,
                                LateEntry = reader["LateEntry"] != DBNull.Value && Convert.ToBoolean(reader["LateEntry"]),
                                EarlyExit = reader["EarlyExit"] != DBNull.Value && Convert.ToBoolean(reader["EarlyExit"])
                            };
                        }
                    }
                }
            }
            return null;
        }

        public int InsertAttendance(Attendance a)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    INSERT INTO Attendances
                        (AttendanceCode, Employee, EmployeeName, Company, AttendanceDate, Status, Shift, LateEntry, EarlyExit, DocStatus, ErpNextId, IsDeleted)
                    VALUES
                        (@AttendanceCode, @Employee, @EmployeeName, @Company, @AttendanceDate, @Status, @Shift, @LateEntry, @EarlyExit, @DocStatus, @ErpNextId, 0);
                    SELECT SCOPE_IDENTITY();";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@AttendanceCode", a.AttendanceCode ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Employee", a.Employee ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmployeeName", a.EmployeeName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Company", a.Company ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AttendanceDate", a.AttendanceDate);
                    cmd.Parameters.AddWithValue("@Status", a.Status ?? "Present");
                    cmd.Parameters.AddWithValue("@Shift", a.Shift ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@LateEntry", a.LateEntry);
                    cmd.Parameters.AddWithValue("@EarlyExit", a.EarlyExit);
                    cmd.Parameters.AddWithValue("@DocStatus", a.DocStatus);
                    cmd.Parameters.AddWithValue("@ErpNextId", string.IsNullOrEmpty(a.ErpNextId) ? (object)DBNull.Value : a.ErpNextId);

                    conn.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void UpdateAttendance(Attendance a)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
            UPDATE Attendances SET 
                Employee = @Employee,
                EmployeeName = @EmployeeName,
                Company = @Company,
                AttendanceCode = @AttendanceCode,
                ErpNextId = @ErpNextId,
                AttendanceDate = @AttendanceDate,
                Status = @Status,
                Shift = @Shift,
                LateEntry = @LateEntry,
                EarlyExit = @EarlyExit,
                DocStatus = @DocStatus,
                IsDeleted = 0
            WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", a.Id);
                    cmd.Parameters.AddWithValue("@Employee", string.IsNullOrWhiteSpace(a.Employee) ? (object)DBNull.Value : a.Employee);
                    cmd.Parameters.AddWithValue("@EmployeeName", string.IsNullOrWhiteSpace(a.EmployeeName) ? (object)DBNull.Value : a.EmployeeName);
                    cmd.Parameters.AddWithValue("@Company", string.IsNullOrWhiteSpace(a.Company) ? (object)DBNull.Value : a.Company);
                    cmd.Parameters.AddWithValue("@AttendanceCode", string.IsNullOrWhiteSpace(a.AttendanceCode) ? (object)DBNull.Value : a.AttendanceCode);
                    cmd.Parameters.AddWithValue("@ErpNextId", string.IsNullOrWhiteSpace(a.ErpNextId) ? (object)DBNull.Value : a.ErpNextId);
                    cmd.Parameters.AddWithValue("@AttendanceDate", a.AttendanceDate);
                    cmd.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(a.Status) ? "Present" : a.Status);
                    cmd.Parameters.AddWithValue("@Shift", string.IsNullOrWhiteSpace(a.Shift) ? (object)DBNull.Value : a.Shift);
                    cmd.Parameters.AddWithValue("@LateEntry", a.LateEntry);
                    cmd.Parameters.AddWithValue("@EarlyExit", a.EarlyExit);
                    cmd.Parameters.AddWithValue("@DocStatus", a.DocStatus);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateDocStatus(int id, int docStatus)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE Attendances SET DocStatus = @DocStatus WHERE Id = @Id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@DocStatus", docStatus);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }


        public void UpdateErpNextLink(int id, string erpNextId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
            UPDATE Attendances
            SET ErpNextId = @ErpNextId,
                AttendanceCode = @ErpNextId
            WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@ErpNextId", erpNextId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }







        public HashSet<string> GetExistingAttendanceCodes()
        {
            var codes = new HashSet<string>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT AttendanceCode FROM Attendances WHERE AttendanceCode IS NOT NULL AND AttendanceCode != ''";
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            codes.Add(reader["AttendanceCode"].ToString());
                        }
                    }
                }
            }
            return codes;
        }



        public bool ExistsByErpNextId(string erpNextId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
            SELECT COUNT(*)
            FROM Attendances
            WHERE ErpNextId = @ErpNextId OR AttendanceCode = @ErpNextId";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", erpNextId);
                    conn.Open();
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }

        public void SoftDeleteById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE Attendances SET IsDeleted = 1 WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void SoftDeleteByErpNextId(string erpNextId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
            UPDATE Attendances
            SET IsDeleted = 1
            WHERE ErpNextId = @ErpNextId OR AttendanceCode = @ErpNextId";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", erpNextId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }


        //public void SoftDeleteById(int id)
        //{
        //    using (var conn = new SqlConnection(_connectionString))
        //    {
        //        string query = "UPDATE Attendances SET IsDeleted = 1 WHERE Id = @Id";

        //        using (var cmd = new SqlCommand(query, conn))
        //        {
        //            cmd.Parameters.AddWithValue("@Id", id);
        //            conn.Open();
        //            cmd.ExecuteNonQuery();
        //        }
        //    }
        //}




        public string GetNextAttendanceCode()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "SELECT TOP 1 AttendanceCode FROM Attendances WHERE AttendanceCode LIKE @YearPattern ORDER BY Id DESC";
                string lastCode = null;

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@YearPattern", $"HR-ATT-{DateTime.Now.Year}-%");
                    lastCode = cmd.ExecuteScalar()?.ToString();
                }

                int nextNumber = 1;
                if (!string.IsNullOrEmpty(lastCode))
                {
                    string parts = lastCode.Substring(lastCode.LastIndexOf('-') + 1);
                    if (int.TryParse(parts, out int parsedNum)) nextNumber = parsedNum + 1;
                }
                else
                {
                    string countQuery = "SELECT COUNT(*) FROM Attendances";
                    using (var countCmd = new SqlCommand(countQuery, conn))
                    {
                        nextNumber = (int)countCmd.ExecuteScalar() + 1;
                    }
                }

                return $"HR-ATT-{DateTime.Now.Year}-{nextNumber:D5}";
            }
        }
    }
}