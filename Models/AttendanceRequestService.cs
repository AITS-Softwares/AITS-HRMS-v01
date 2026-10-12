using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class AttendanceRequestService
    {
        private readonly string _connectionString;

        public AttendanceRequestService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<AttendanceRequest> GetAllAttendanceRequests()
        {
            var requests = new List<AttendanceRequest>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT Id, AttendanceRequestCode, Employee, EmployeeName, Company,
                           FromDate, ToDate, ErpNextId, DocStatus,
                           HalfDay, HalfDayDate, IncludeHolidays, Shift, Reason, Explanation
                    FROM AttendanceRequests
                    WHERE IsDeleted = 0
                    ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            requests.Add(ReadAttendanceRequest(reader));
                        }
                    }
                }
            }

            return requests;
        }

        public AttendanceRequest GetById(int id)
        {
            AttendanceRequest model = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT Id, AttendanceRequestCode, Employee, EmployeeName, Company,
                           FromDate, ToDate, ErpNextId, DocStatus,
                           HalfDay, HalfDayDate, IncludeHolidays, Shift, Reason, Explanation
                    FROM AttendanceRequests
                    WHERE Id = @Id AND IsDeleted = 0";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model = ReadAttendanceRequest(reader);
                        }
                    }
                }
            }

            return model;
        }

        public AttendanceRequest GetByErpNextId(string erpNextId)
        {
            if (string.IsNullOrWhiteSpace(erpNextId))
                return null;

            AttendanceRequest model = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT Id, AttendanceRequestCode, Employee, EmployeeName, Company,
                           FromDate, ToDate, ErpNextId, DocStatus,
                           HalfDay, HalfDayDate, IncludeHolidays, Shift, Reason, Explanation
                    FROM AttendanceRequests
                    WHERE ErpNextId = @ErpNextId AND IsDeleted = 0";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", erpNextId);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model = ReadAttendanceRequest(reader);
                        }
                    }
                }
            }

            return model;
        }

        public string GetNextAttendanceRequestCode()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT COUNT(*) FROM AttendanceRequests";

                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar()) + 1;
                    return $"HR-ARQ-{DateTime.Now:yy-MM}-{count:D5}";
                }
            }
        }

        public int InsertAttendanceRequest(AttendanceRequest request)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    INSERT INTO AttendanceRequests
                        (AttendanceRequestCode, Employee, EmployeeName, Company,
                         FromDate, ToDate, ErpNextId, DocStatus,
                         HalfDay, HalfDayDate, IncludeHolidays, Shift,
                         Reason, Explanation, IsDeleted)
                    VALUES
                        (@AttendanceRequestCode, @Employee, @EmployeeName, @Company,
                         @FromDate, @ToDate, @ErpNextId, @DocStatus,
                         @HalfDay, @HalfDayDate, @IncludeHolidays, @Shift,
                         @Reason, @Explanation, 0);

                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                using (var cmd = new SqlCommand(query, conn))
                {
                    AddParameters(cmd, request);

                    conn.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void UpdateAttendanceRequest(AttendanceRequest request)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    UPDATE AttendanceRequests
                    SET AttendanceRequestCode = @AttendanceRequestCode,
                        Employee = @Employee,
                        EmployeeName = @EmployeeName,
                        Company = @Company,
                        FromDate = @FromDate,
                        ToDate = @ToDate,
                        ErpNextId = @ErpNextId,
                        DocStatus = @DocStatus,
                        HalfDay = @HalfDay,
                        HalfDayDate = @HalfDayDate,
                        IncludeHolidays = @IncludeHolidays,
                        Shift = @Shift,
                        Reason = @Reason,
                        Explanation = @Explanation
                    WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", request.Id);
                    AddParameters(cmd, request);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateDocStatus(int id, int docStatus)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    UPDATE AttendanceRequests
                    SET DocStatus = @DocStatus
                    WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@DocStatus", docStatus);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void SoftDeleteById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    UPDATE AttendanceRequests
                    SET IsDeleted = 1
                    WHERE Id = @Id";

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

                string query = @"
                    SELECT Id, ErpNextId, AttendanceRequestCode
                    FROM AttendanceRequests
                    WHERE IsDeleted = 0";

                var rows = new List<(int Id, string ErpNextId, string Code)>();

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rows.Add((
                            Convert.ToInt32(reader["Id"]),
                            reader["ErpNextId"]?.ToString(),
                            reader["AttendanceRequestCode"]?.ToString()
                        ));
                    }
                }

                foreach (var row in rows)
                {
                    string key = !string.IsNullOrWhiteSpace(row.ErpNextId) ? row.ErpNextId : row.Code;

                    if (!string.IsNullOrWhiteSpace(key) && !erpCodes.Contains(key))
                    {
                        using (var cmd = new SqlCommand("UPDATE AttendanceRequests SET IsDeleted = 1 WHERE Id = @Id", conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", row.Id);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        private AttendanceRequest ReadAttendanceRequest(SqlDataReader reader)
        {
            return new AttendanceRequest
            {
                Id = Convert.ToInt32(reader["Id"]),
                AttendanceRequestCode = reader["AttendanceRequestCode"]?.ToString(),
                Employee = reader["Employee"]?.ToString(),
                EmployeeName = reader["EmployeeName"]?.ToString(),
                Company = reader["Company"]?.ToString(),
                FromDate = reader["FromDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["FromDate"]),
                ToDate = reader["ToDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["ToDate"]),
                ErpNextId = reader["ErpNextId"]?.ToString(),
                DocStatus = reader["DocStatus"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DocStatus"]),
                HalfDay = reader["HalfDay"] != DBNull.Value && Convert.ToBoolean(reader["HalfDay"]),
                HalfDayDate = reader["HalfDayDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["HalfDayDate"]),
                IncludeHolidays = reader["IncludeHolidays"] != DBNull.Value && Convert.ToBoolean(reader["IncludeHolidays"]),
                Shift = reader["Shift"]?.ToString(),
                Reason = reader["Reason"]?.ToString(),
                Explanation = reader["Explanation"]?.ToString()
            };
        }

        private void AddParameters(SqlCommand cmd, AttendanceRequest request)
        {
            cmd.Parameters.AddWithValue("@AttendanceRequestCode", (object)request.AttendanceRequestCode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Employee", (object)request.Employee ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EmployeeName", (object)request.EmployeeName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Company", (object)request.Company ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FromDate", (object)request.FromDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ToDate", (object)request.ToDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ErpNextId", (object)request.ErpNextId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DocStatus", request.DocStatus);
            cmd.Parameters.AddWithValue("@HalfDay", request.HalfDay);
            cmd.Parameters.AddWithValue("@HalfDayDate", (object)request.HalfDayDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IncludeHolidays", request.IncludeHolidays);
            cmd.Parameters.AddWithValue("@Shift", (object)request.Shift ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Reason", (object)request.Reason ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Explanation", (object)request.Explanation ?? DBNull.Value);
        }
    }
}