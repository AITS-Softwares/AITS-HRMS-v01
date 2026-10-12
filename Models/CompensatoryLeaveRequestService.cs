
//using System;
//using System.Collections.Generic;
//using Microsoft.Data.SqlClient;
//using Erp_Aits.Models;

//namespace Erp_Aits.Services
//{
//    public class CompensatoryLeaveRequestService
//    {
//        private readonly string _connectionString;

//        public CompensatoryLeaveRequestService(string connectionString)
//        {
//            _connectionString = connectionString;
//        }

//        public List<CompensatoryLeaveRequest> GetAllRequests()
//        {
//            var requests = new List<CompensatoryLeaveRequest>();

//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = "SELECT Id, Employee, EmployeeName FROM CompensatoryLeaveRequests WHERE IsDeleted = 0 ORDER BY Id DESC";

//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    conn.Open();
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            requests.Add(new CompensatoryLeaveRequest
//                            {
//                                Id = Convert.ToInt32(reader["Id"]),
//                                Employee = reader["Employee"]?.ToString() ?? string.Empty,
//                                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty
//                            });
//                        }
//                    }
//                }
//            }

//            return requests;
//        }

//        public void InsertRequest(CompensatoryLeaveRequest r)
//        {
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = @"
//                    INSERT INTO CompensatoryLeaveRequests
//                        (Employee, LeaveType, WorkFromDate, WorkEndDate, HalfDay, Reason)
//                    VALUES
//                        (@Employee, @LeaveType, @WorkFromDate, @WorkEndDate, @HalfDay, @Reason)";

//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    cmd.Parameters.AddWithValue("@Employee", r.Employee ?? (object)DBNull.Value);
//                    cmd.Parameters.AddWithValue("@LeaveType", r.LeaveType ?? (object)DBNull.Value);
//                    cmd.Parameters.AddWithValue("@WorkFromDate", (object)r.WorkFromDate ?? DBNull.Value);
//                    cmd.Parameters.AddWithValue("@WorkEndDate", (object)r.WorkEndDate ?? DBNull.Value);
//                    cmd.Parameters.AddWithValue("@HalfDay", r.HalfDay);
//                    cmd.Parameters.AddWithValue("@Reason", r.Reason ?? (object)DBNull.Value);

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
    public class CompensatoryLeaveRequestService
    {
        private readonly string _connectionString;
        private static bool _statusColumnChecked;
        private static readonly object _statusColumnLock = new object();

        public CompensatoryLeaveRequestService(string connectionString)
        {
            _connectionString = connectionString;
            EnsureStatusColumn();
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
                        string query = @"
                            IF COL_LENGTH('CompensatoryLeaveRequests', 'Status') IS NULL
                            BEGIN
                                ALTER TABLE CompensatoryLeaveRequests ADD Status NVARCHAR(50) NULL
                            END";

                        using (var cmd = new SqlCommand(query, conn))
                        {
                            cmd.CommandTimeout = 5;
                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("CompensatoryLeaveRequests Status column check skipped: " + ex.Message);
                }
                finally
                {
                    _statusColumnChecked = true;
                }
            }
        }

        public void SoftDeleteById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
            UPDATE CompensatoryLeaveRequests
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



        public List<CompensatoryLeaveRequest> GetAllRequests()
        {
            var requests = new List<CompensatoryLeaveRequest>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM CompensatoryLeaveRequests WHERE IsDeleted = 0 ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            requests.Add(Map(reader));
                        }
                    }
                }
            }

            return requests;
        }

        public CompensatoryLeaveRequest GetById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM CompensatoryLeaveRequests WHERE Id = @Id AND IsDeleted = 0";
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

        public int InsertRequest(CompensatoryLeaveRequest r)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    INSERT INTO CompensatoryLeaveRequests
                        (Employee, EmployeeName, LeaveType, WorkFromDate, WorkEndDate, HalfDay, Reason, ErpNextId, Status)
                    OUTPUT INSERTED.Id
                    VALUES
                        (@Employee, @EmployeeName, @LeaveType, @WorkFromDate, @WorkEndDate, @HalfDay, @Reason, @ErpNextId, @Status)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Employee", r.Employee ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmployeeName", (object)r.EmployeeName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@LeaveType", r.LeaveType ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@WorkFromDate", (object)r.WorkFromDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@WorkEndDate", (object)r.WorkEndDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@HalfDay", r.HalfDay);
                    cmd.Parameters.AddWithValue("@Reason", r.Reason ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ErpNextId", (object)r.ErpNextId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(r.Status) ? "Draft" : r.Status);

                    conn.Open();
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        public void InsertFromErpNext(CompensatoryLeaveRequest model)
        {
            InsertRequest(model);
        }

        public void UpdateErpNextId(int id, string erpNextId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE CompensatoryLeaveRequests SET ErpNextId = @ErpNextId WHERE Id = @Id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", (object)erpNextId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public bool ExistsByErpNextId(string erpNextId)
        {
            if (string.IsNullOrWhiteSpace(erpNextId)) return false;

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT COUNT(*) FROM CompensatoryLeaveRequests WHERE ErpNextId = @ErpNextId";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", erpNextId);
                    conn.Open();
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }

        //public void InsertFromErpNext(CompensatoryLeaveRequest model)
        //{
        //    InsertRequest(model);
        //}

        public void UpdateFromErpNext(CompensatoryLeaveRequest model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"UPDATE CompensatoryLeaveRequests
                    SET Employee = @Employee,
                        EmployeeName = @EmployeeName,
                        LeaveType = @LeaveType,
                        WorkFromDate = @WorkFromDate,
                        WorkEndDate = @WorkEndDate,
                        HalfDay = @HalfDay,
                        Reason = @Reason,
                        Status = @Status,
                        IsDeleted = 0
                    WHERE ErpNextId = @ErpNextId";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Employee", model.Employee ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmployeeName", (object)model.EmployeeName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@LeaveType", model.LeaveType ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@WorkFromDate", (object)model.WorkFromDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@WorkEndDate", (object)model.WorkEndDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@HalfDay", model.HalfDay);
                    cmd.Parameters.AddWithValue("@Reason", model.Reason ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(model.Status) ? "Draft" : model.Status);
                    cmd.Parameters.AddWithValue("@ErpNextId", model.ErpNextId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private CompensatoryLeaveRequest Map(SqlDataReader reader)
        {
            bool hasErpNextId = false;
            bool hasEmployeeName = false;
            bool hasStatus = false;
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals("ErpNextId", StringComparison.OrdinalIgnoreCase)) hasErpNextId = true;
                if (reader.GetName(i).Equals("EmployeeName", StringComparison.OrdinalIgnoreCase)) hasEmployeeName = true;
                if (reader.GetName(i).Equals("Status", StringComparison.OrdinalIgnoreCase)) hasStatus = true;
            }

            return new CompensatoryLeaveRequest
            {
                Id = Convert.ToInt32(reader["Id"]),
                Employee = reader["Employee"]?.ToString() ?? string.Empty,
                EmployeeName = hasEmployeeName && reader["EmployeeName"] != DBNull.Value ? reader["EmployeeName"].ToString() : string.Empty,
                LeaveType = reader["LeaveType"]?.ToString(),
                WorkFromDate = reader["WorkFromDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["WorkFromDate"]),
                WorkEndDate = reader["WorkEndDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["WorkEndDate"]),
                HalfDay = reader["HalfDay"] != DBNull.Value && Convert.ToBoolean(reader["HalfDay"]),
                Reason = reader["Reason"]?.ToString(),
                ErpNextId = hasErpNextId && reader["ErpNextId"] != DBNull.Value ? reader["ErpNextId"].ToString() : null,
                Status = hasStatus && reader["Status"] != DBNull.Value ? reader["Status"].ToString() : "Draft"
            };
        }
    }
}
