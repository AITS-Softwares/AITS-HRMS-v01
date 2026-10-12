using DocumentFormat.OpenXml.Office.Word;
using Erp_Aits.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace Erp_Aits.Services
{
    public class LeaveApplicationService
    {
        private readonly string _connectionString;

        public LeaveApplicationService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<LeaveApplication> GetAllLeaves(string statusFilter = null)
        {
            var leaves = new List<LeaveApplication>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT Id, LeaveApplicationCode, LeaveType, Employee, EmployeeName, Company, 
                         FromDate, ToDate, HalfDay, Reason, LeaveApprover, PostingDate, Status, ErpNextId 
                         FROM LeaveApplications WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(statusFilter))
                {
                    query += " AND Status = @Status";
                }

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(statusFilter))
                    {
                        cmd.Parameters.AddWithValue("@Status", statusFilter);
                    }

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            leaves.Add(new LeaveApplication
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                LeaveApplicationCode = reader["LeaveApplicationCode"]?.ToString() ?? string.Empty,
                                LeaveType = reader["LeaveType"]?.ToString() ?? string.Empty,
                                Employee = reader["Employee"]?.ToString() ?? string.Empty,
                                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
                                Company = reader["Company"]?.ToString() ?? string.Empty,
                                FromDate = reader["FromDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["FromDate"]),
                                ToDate = reader["ToDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ToDate"]),
                                HalfDay = reader["HalfDay"] != DBNull.Value && Convert.ToBoolean(reader["HalfDay"]),
                                Reason = reader["Reason"]?.ToString() ?? string.Empty,
                                LeaveApprover = reader["LeaveApprover"]?.ToString() ?? string.Empty,
                                PostingDate = reader["PostingDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["PostingDate"]),
                                Status = reader["Status"]?.ToString() ?? "Open",
                                ErpNextId = reader["ErpNextId"] == DBNull.Value ? null : reader["ErpNextId"].ToString()
                            });
                        }
                    }
                }
            }

            return leaves;
        }

        public string GetNextLeaveCode()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT COUNT(*) FROM LeaveApplications";
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    int count = (int)cmd.ExecuteScalar() + 1;
                    return $"HR-LAP-{DateTime.Now.Year}-{count:D5}";
                }
            }
        }

        public int GetLeavesTakenForEmployee(string employeeCode, DateTime fromDate, DateTime toDate)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
            SELECT ISNULL(SUM(DATEDIFF(day, FromDate, ToDate) + 1), 0)
            FROM LeaveApplications
            WHERE IsDeleted = 0
              AND Employee = @Employee
              AND Status = 'Approved'
              AND FromDate >= @FromDate
              AND ToDate <= @ToDate";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Employee", employeeCode);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);

                    conn.Open();
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        public int InsertLeave(LeaveApplication leave)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
    INSERT INTO LeaveApplications
       (LeaveApplicationCode, LeaveType, Employee, EmployeeName, Company,
FromDate, ToDate, HalfDay, Reason,
LeaveApprover, PostingDate, FollowViaEmail, Status,
SalarySlip, LetterHead, Color, ErpNextId, IsDeleted)
    OUTPUT INSERTED.Id
    VALUES
        (@LeaveApplicationCode, @LeaveType, @Employee, @EmployeeName, @Company,
         @FromDate, @ToDate, @HalfDay, @Reason,
         @LeaveApprover, @PostingDate, @FollowViaEmail, @Status,
         @SalarySlip, @LetterHead, @Color, @ErpNextId, 0)";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@LeaveApplicationCode", leave.LeaveApplicationCode ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@LeaveType", leave.LeaveType ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Employee", leave.Employee ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmployeeName", leave.EmployeeName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Company", leave.Company ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FromDate", (object)leave.FromDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", (object)leave.ToDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@HalfDay", leave.HalfDay);
                    cmd.Parameters.AddWithValue("@Reason", leave.Reason ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@LeaveApprover", leave.LeaveApprover ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PostingDate", (object)leave.PostingDate ?? DateTime.Now);
                    cmd.Parameters.AddWithValue("@FollowViaEmail", leave.FollowViaEmail);
                    cmd.Parameters.AddWithValue("@Status", leave.Status ?? "Open");
                    cmd.Parameters.AddWithValue("@SalarySlip", leave.SalarySlip ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@LetterHead", leave.LetterHead ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Color", leave.Color ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ErpNextId", leave.ErpNextId ?? (object)DBNull.Value);


                conn.Open();
                    int newId = (int)cmd.ExecuteScalar();
                    leave.Id = newId;
                    return newId;
                }
            }
        }

        public void UpdateErpNextId(int id, string erpNextId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE LeaveApplications SET ErpNextId = @ErpNextId WHERE Id = @Id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", (object)erpNextId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void MarkDeletedById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE LeaveApplications SET IsDeleted = 1 WHERE Id = @Id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Checks if a local record already exists for a given ERPNext ID (to avoid duplicate imports)
        //public bool ExistsByErpNextId(string erpNextId)
        //{
        //    using (var conn = new SqlConnection(_connectionString))
        //    {
        //        string query = "SELECT COUNT(*) FROM LeaveApplications WHERE ErpNextId = @ErpNextId";
        //        using (var cmd = new SqlCommand(query, conn))
        //        {
        //            cmd.Parameters.AddWithValue("@ErpNextId", erpNextId);
        //            conn.Open();
        //            return (int)cmd.ExecuteScalar() > 0;
        //        }
        //    }
        //}




        public void SyncRecordFromErpNext(int id, string leaveType, string employee, string employeeName, string company, DateTime? fromDate, DateTime? toDate, bool halfDay, string reason, string leaveApprover, string status)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"UPDATE LeaveApplications 
                         SET LeaveType = @LeaveType,
                             Employee = @Employee,
                             EmployeeName = @EmployeeName,
                             Company = @Company,
                             FromDate = @FromDate,
                             ToDate = @ToDate,
                             HalfDay = @HalfDay,
                             Reason = @Reason,
                             LeaveApprover = @LeaveApprover,
                             Status = @Status,
                             IsDeleted = 0
                         WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@LeaveType", (object)leaveType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Employee", (object)employee ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmployeeName", (object)employeeName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Company", (object)company ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate.HasValue ? (object)fromDate.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", toDate.HasValue ? (object)toDate.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@HalfDay", halfDay);
                    cmd.Parameters.AddWithValue("@Reason", (object)reason ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@LeaveApprover", (object)leaveApprover ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", (object)status ?? "Open");

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }



















        public void UpdateFromErpNext(LeaveApplication model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"UPDATE LeaveApplications 
                         SET LeaveType = @LeaveType,
                             Employee = @Employee,
                             EmployeeName = @EmployeeName,
                             Company = @Company,
                             FromDate = @FromDate,
                             ToDate = @ToDate,
                             HalfDay = @HalfDay,
                             Reason = @Reason,
                             LeaveApprover = @LeaveApprover,
                             Status = @Status,
                             IsDeleted = 0
                         WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@LeaveType", string.IsNullOrEmpty(model.LeaveType) ? (object)DBNull.Value : model.LeaveType);
                    cmd.Parameters.AddWithValue("@Employee", string.IsNullOrEmpty(model.Employee) ? (object)DBNull.Value : model.Employee);
                    cmd.Parameters.AddWithValue("@EmployeeName", string.IsNullOrEmpty(model.EmployeeName) ? (object)DBNull.Value : model.EmployeeName);
                    cmd.Parameters.AddWithValue("@Company", string.IsNullOrEmpty(model.Company) ? (object)DBNull.Value : model.Company);
                    cmd.Parameters.AddWithValue("@FromDate", model.FromDate.HasValue ? (object)model.FromDate.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", model.ToDate.HasValue ? (object)model.ToDate.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@HalfDay", model.HalfDay);
                    cmd.Parameters.AddWithValue("@Reason", string.IsNullOrEmpty(model.Reason) ? (object)DBNull.Value : model.Reason);
                    cmd.Parameters.AddWithValue("@LeaveApprover", string.IsNullOrEmpty(model.LeaveApprover) ? (object)DBNull.Value : model.LeaveApprover);
                    cmd.Parameters.AddWithValue("@Status", string.IsNullOrEmpty(model.Status) ? "Open" : model.Status);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }



        public void UpdateByErpNextId(LeaveApplication leave)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
            UPDATE LeaveApplications
            SET LeaveType = @LeaveType,
                Employee = @Employee,
                EmployeeName = @EmployeeName,
                Company = @Company,
                FromDate = @FromDate,
                ToDate = @ToDate,
                HalfDay = @HalfDay,
                Reason = @Reason,
                LeaveApprover = @LeaveApprover,
                PostingDate = @PostingDate,
                FollowViaEmail = @FollowViaEmail,
                Status = @Status,
                ErpNextId = @ErpNextId,
                IsDeleted = 0
            WHERE ErpNextId = @ErpNextId
               OR LeaveApplicationCode = @LeaveApplicationCode";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@LeaveApplicationCode", leave.LeaveApplicationCode ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@LeaveType", leave.LeaveType ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Employee", leave.Employee ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmployeeName", leave.EmployeeName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Company", leave.Company ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FromDate", (object)leave.FromDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", (object)leave.ToDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@HalfDay", leave.HalfDay);
                    cmd.Parameters.AddWithValue("@Reason", leave.Reason ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@LeaveApprover", leave.LeaveApprover ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PostingDate", (object)leave.PostingDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FollowViaEmail", leave.FollowViaEmail);
                    cmd.Parameters.AddWithValue("@Status", leave.Status ?? "Open");
                    cmd.Parameters.AddWithValue("@ErpNextId", leave.ErpNextId ?? (object)DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }







        //public bool ExistsByErpNextId(string erpNextId)
        //{
        //    if (string.IsNullOrWhiteSpace(erpNextId)) return false;

        //    using (var conn = new SqlConnection(_connectionString))
        //    {
        //        // Check BOTH ErpNextId AND LeaveApplicationCode so SQL UNIQUE constraint is never violated
        //        string query = @"
        //    SELECT COUNT(*) 
        //    FROM LeaveApplications 
        //    WHERE ErpNextId = @ErpNextId 
        //       OR LeaveApplicationCode = @ErpNextId";

        //        using (var cmd = new SqlCommand(query, conn))
        //        {
        //            cmd.Parameters.AddWithValue("@ErpNextId", erpNextId);
        //            conn.Open();
        //            return (int)cmd.ExecuteScalar() > 0;
        //        }
        //    }
        //}
        // Inserts a leave application that originated FROM ERPNext (pulled via GET sync)


        public bool ExistsByErpNextId(string erpNextId)
        {
            if (string.IsNullOrWhiteSpace(erpNextId))
                return false;

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
            SELECT COUNT(*)
            FROM LeaveApplications
            WHERE ErpNextId = @ErpNextId
               OR LeaveApplicationCode = @ErpNextId";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", erpNextId);
                    conn.Open();

                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }

        public LeaveApplication GetByErpNextId(string erpNextId)
        {
            if (string.IsNullOrWhiteSpace(erpNextId))
                return null;

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
            SELECT TOP 1 Id, LeaveApplicationCode, LeaveType, Employee, EmployeeName, Company,
                   FromDate, ToDate, HalfDay, Reason, LeaveApprover, PostingDate, Status, ErpNextId
            FROM LeaveApplications
            WHERE ErpNextId = @ErpNextId
               OR LeaveApplicationCode = @ErpNextId
            ORDER BY CASE WHEN ErpNextId = @ErpNextId THEN 0 ELSE 1 END, Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", erpNextId);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            return null;

                        return new LeaveApplication
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            LeaveApplicationCode = reader["LeaveApplicationCode"]?.ToString() ?? string.Empty,
                            LeaveType = reader["LeaveType"]?.ToString() ?? string.Empty,
                            Employee = reader["Employee"]?.ToString() ?? string.Empty,
                            EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
                            Company = reader["Company"]?.ToString() ?? string.Empty,
                            FromDate = reader["FromDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["FromDate"]),
                            ToDate = reader["ToDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ToDate"]),
                            HalfDay = reader["HalfDay"] != DBNull.Value && Convert.ToBoolean(reader["HalfDay"]),
                            Reason = reader["Reason"]?.ToString() ?? string.Empty,
                            LeaveApprover = reader["LeaveApprover"]?.ToString() ?? string.Empty,
                            PostingDate = reader["PostingDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["PostingDate"]),
                            Status = reader["Status"]?.ToString() ?? "Open",
                            ErpNextId = reader["ErpNextId"] == DBNull.Value ? null : reader["ErpNextId"].ToString()
                        };
                    }
                }
            }
        }

        public void SoftDeleteDuplicateSamePeriod(int keepId, LeaveApplication leave)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
            UPDATE LeaveApplications
            SET IsDeleted = 1
            WHERE IsDeleted = 0
              AND Id <> @KeepId
              AND Employee = @Employee
              AND LeaveType = @LeaveType
              AND FromDate = @FromDate
              AND ToDate = @ToDate";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@KeepId", keepId);
                    cmd.Parameters.AddWithValue("@Employee", leave.Employee ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@LeaveType", leave.LeaveType ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FromDate", (object)leave.FromDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", (object)leave.ToDate ?? DBNull.Value);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void InsertFromErpNext(LeaveApplication leave)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    INSERT INTO LeaveApplications
                        (LeaveApplicationCode, LeaveType, Employee, EmployeeName, Company,
                         FromDate, ToDate, HalfDay, Reason,
                         LeaveApprover, PostingDate, FollowViaEmail, Status,
                         SalarySlip, LetterHead, Color, ErpNextId, IsDeleted)
                    VALUES
                        (@LeaveApplicationCode, @LeaveType, @Employee, @EmployeeName, @Company,
                         @FromDate, @ToDate, @HalfDay, @Reason,
                         @LeaveApprover, @PostingDate, @FollowViaEmail, @Status,
                         @SalarySlip, @LetterHead, @Color, @ErpNextId, 0)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@LeaveApplicationCode", leave.LeaveApplicationCode ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@LeaveType", leave.LeaveType ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Employee", leave.Employee ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmployeeName", leave.EmployeeName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Company", leave.Company ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FromDate", (object)leave.FromDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", (object)leave.ToDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@HalfDay", leave.HalfDay);
                    cmd.Parameters.AddWithValue("@Reason", leave.Reason ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@LeaveApprover", leave.LeaveApprover ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PostingDate", (object)leave.PostingDate ?? DateTime.Now);
                    cmd.Parameters.AddWithValue("@FollowViaEmail", leave.FollowViaEmail);
                    cmd.Parameters.AddWithValue("@Status", leave.Status ?? "Open");
                    cmd.Parameters.AddWithValue("@SalarySlip", leave.SalarySlip ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@LetterHead", leave.LetterHead ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Color", leave.Color ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ErpNextId", leave.ErpNextId ?? (object)DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
