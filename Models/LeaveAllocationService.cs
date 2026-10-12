//using System;
//using System.Collections.Generic;
//using Microsoft.Data.SqlClient;
//using Erp_Aits.Models;

//namespace Erp_Aits.Services
//{
//    public class LeaveAllocationService
//    {
//        private readonly string _connectionString;

//        public LeaveAllocationService(string connectionString)
//        {
//            _connectionString = connectionString;
//        }

//        public List<LeaveAllocation> GetAll(string idFilter, string employee, string employeeName, string leaveType, string leavePeriod, string leavePolicy, bool expiredOnly)
//        {
//            var list = new List<LeaveAllocation>();

//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = "SELECT * FROM LeaveAllocations WHERE IsDeleted = 0";

//                if (!string.IsNullOrEmpty(idFilter)) query += " AND CAST(Id AS VARCHAR) LIKE @IdFilter";
//                if (!string.IsNullOrEmpty(employee)) query += " AND Employee LIKE @Employee";
//                if (!string.IsNullOrEmpty(employeeName)) query += " AND EmployeeName LIKE @EmployeeName";
//                if (!string.IsNullOrEmpty(leaveType)) query += " AND LeaveType LIKE @LeaveType";
//                if (!string.IsNullOrEmpty(leavePeriod)) query += " AND LeavePeriod LIKE @LeavePeriod";
//                if (!string.IsNullOrEmpty(leavePolicy)) query += " AND LeavePolicy LIKE @LeavePolicy";
//                if (expiredOnly) query += " AND Expired = 1";

//                query += " ORDER BY Id DESC";

//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    if (!string.IsNullOrEmpty(idFilter)) cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");
//                    if (!string.IsNullOrEmpty(employee)) cmd.Parameters.AddWithValue("@Employee", "%" + employee + "%");
//                    if (!string.IsNullOrEmpty(employeeName)) cmd.Parameters.AddWithValue("@EmployeeName", "%" + employeeName + "%");
//                    if (!string.IsNullOrEmpty(leaveType)) cmd.Parameters.AddWithValue("@LeaveType", "%" + leaveType + "%");
//                    if (!string.IsNullOrEmpty(leavePeriod)) cmd.Parameters.AddWithValue("@LeavePeriod", "%" + leavePeriod + "%");
//                    if (!string.IsNullOrEmpty(leavePolicy)) cmd.Parameters.AddWithValue("@LeavePolicy", "%" + leavePolicy + "%");

//                    conn.Open();
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            list.Add(MapAllocation(reader));
//                        }
//                    }
//                }
//            }

//            return list;
//        }

//        public LeaveAllocation GetById(int id)
//        {
//            LeaveAllocation model = null;

//            using (var conn = new SqlConnection(_connectionString))
//            {
//                conn.Open();
//                using (var cmd = new SqlCommand("SELECT * FROM LeaveAllocations WHERE Id = @Id AND IsDeleted = 0", conn))
//                {
//                    cmd.Parameters.AddWithValue("@Id", id);
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        if (reader.Read())
//                        {
//                            model = MapAllocation(reader);
//                        }
//                    }
//                }
//            }

//            return model;
//        }

//        public int Insert(LeaveAllocation a)
//        {
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                using (var cmd = new SqlCommand(@"
//                    INSERT INTO LeaveAllocations
//                        (Series, Employee, EmployeeName, Company, LeaveType, LeavePeriod, LeavePolicy, FromDate, ToDate,
//                         NewLeavesAllocated, AddUnusedLeaves, Description)
//                    OUTPUT INSERTED.Id
//                    VALUES
//                        (@Series, @Employee, @EmployeeName, @Company, @LeaveType, @LeavePeriod, @LeavePolicy, @FromDate, @ToDate,
//                         @NewLeavesAllocated, @AddUnusedLeaves, @Description)", conn))
//                {
//                    cmd.Parameters.AddWithValue("@Series", a.Series ?? "HR-LAL-.YYYY.-");
//                    cmd.Parameters.AddWithValue("@Employee", a.Employee ?? (object)DBNull.Value);
//                    cmd.Parameters.AddWithValue("@EmployeeName", (object)a.EmployeeName ?? DBNull.Value);
//                    cmd.Parameters.AddWithValue("@Company", a.Company ?? "AITS");
//                    cmd.Parameters.AddWithValue("@LeaveType", a.LeaveType ?? (object)DBNull.Value);
//                    cmd.Parameters.AddWithValue("@LeavePeriod", (object)a.LeavePeriod ?? DBNull.Value);
//                    cmd.Parameters.AddWithValue("@LeavePolicy", (object)a.LeavePolicy ?? DBNull.Value);
//                    cmd.Parameters.AddWithValue("@FromDate", a.FromDate);
//                    cmd.Parameters.AddWithValue("@ToDate", a.ToDate);
//                    cmd.Parameters.AddWithValue("@NewLeavesAllocated", (object)a.NewLeavesAllocated ?? DBNull.Value);
//                    cmd.Parameters.AddWithValue("@AddUnusedLeaves", a.AddUnusedLeaves);
//                    cmd.Parameters.AddWithValue("@Description", (object)a.Description ?? DBNull.Value);

//                    conn.Open();
//                    return (int)cmd.ExecuteScalar();
//                }
//            }
//        }

//        private LeaveAllocation MapAllocation(SqlDataReader reader)
//        {
//            return new LeaveAllocation
//            {
//                Id = Convert.ToInt32(reader["Id"]),
//                Series = reader["Series"]?.ToString() ?? "HR-LAL-.YYYY.-",
//                Employee = reader["Employee"]?.ToString() ?? string.Empty,
//                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
//                Company = reader["Company"]?.ToString() ?? "AITS",
//                LeaveType = reader["LeaveType"]?.ToString() ?? string.Empty,
//                LeavePeriod = reader["LeavePeriod"]?.ToString() ?? string.Empty,
//                LeavePolicy = reader["LeavePolicy"]?.ToString() ?? string.Empty,
//                FromDate = Convert.ToDateTime(reader["FromDate"]),
//                ToDate = Convert.ToDateTime(reader["ToDate"]),
//                NewLeavesAllocated = reader["NewLeavesAllocated"] != DBNull.Value ? Convert.ToDecimal(reader["NewLeavesAllocated"]) : (decimal?)null,
//                AddUnusedLeaves = reader["AddUnusedLeaves"] != DBNull.Value && Convert.ToBoolean(reader["AddUnusedLeaves"]),
//                Description = reader["Description"]?.ToString() ?? string.Empty,
//                Expired = reader["Expired"] != DBNull.Value && Convert.ToBoolean(reader["Expired"])
//            };
//        }
//    }
//}


using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class LeaveAllocationService
    {
        private readonly string _connectionString;

        public LeaveAllocationService(string connectionString)
        {
            _connectionString = connectionString;
            EnsureColumns();
        }

        public List<LeaveAllocation> GetAll(string idFilter, string employee, string employeeName, string leaveType, string leavePeriod, string leavePolicy, bool expiredOnly)
        {
            var list = new List<LeaveAllocation>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM LeaveAllocations WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(idFilter)) query += " AND CAST(Id AS VARCHAR) LIKE @IdFilter";
                if (!string.IsNullOrEmpty(employee)) query += " AND Employee LIKE @Employee";
                if (!string.IsNullOrEmpty(employeeName)) query += " AND EmployeeName LIKE @EmployeeName";
                if (!string.IsNullOrEmpty(leaveType)) query += " AND LeaveType LIKE @LeaveType";
                if (!string.IsNullOrEmpty(leavePeriod)) query += " AND LeavePeriod LIKE @LeavePeriod";
                if (!string.IsNullOrEmpty(leavePolicy)) query += " AND LeavePolicy LIKE @LeavePolicy";
                if (expiredOnly) query += " AND Expired = 1";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(idFilter)) cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");
                    if (!string.IsNullOrEmpty(employee)) cmd.Parameters.AddWithValue("@Employee", "%" + employee + "%");
                    if (!string.IsNullOrEmpty(employeeName)) cmd.Parameters.AddWithValue("@EmployeeName", "%" + employeeName + "%");
                    if (!string.IsNullOrEmpty(leaveType)) cmd.Parameters.AddWithValue("@LeaveType", "%" + leaveType + "%");
                    if (!string.IsNullOrEmpty(leavePeriod)) cmd.Parameters.AddWithValue("@LeavePeriod", "%" + leavePeriod + "%");
                    if (!string.IsNullOrEmpty(leavePolicy)) cmd.Parameters.AddWithValue("@LeavePolicy", "%" + leavePolicy + "%");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(MapAllocation(reader));
                        }
                    }
                }
            }

            return list;
        }

        public LeaveAllocation GetById(int id)
        {
            LeaveAllocation model = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT * FROM LeaveAllocations WHERE Id = @Id AND IsDeleted = 0", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model = MapAllocation(reader);
                        }
                    }
                }
            }

            return model;
        }

        public int Insert(LeaveAllocation a)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand(@"
                    INSERT INTO LeaveAllocations
                        (Series, Employee, EmployeeName, Company, LeaveType, LeavePeriod, LeavePolicy, FromDate, ToDate,
                         NewLeavesAllocated, AddUnusedLeaves, Description, ErpNextId, DocStatus, Status)
                    OUTPUT INSERTED.Id
                    VALUES
                        (@Series, @Employee, @EmployeeName, @Company, @LeaveType, @LeavePeriod, @LeavePolicy, @FromDate, @ToDate,
                         @NewLeavesAllocated, @AddUnusedLeaves, @Description, @ErpNextId, @DocStatus, @Status)", conn))
                {
                    cmd.Parameters.AddWithValue("@Series", a.Series ?? "HR-LAL-.YYYY.-");
                    cmd.Parameters.AddWithValue("@Employee", a.Employee ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmployeeName", (object)a.EmployeeName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Company", a.Company ?? "AITS");
                    cmd.Parameters.AddWithValue("@LeaveType", a.LeaveType ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@LeavePeriod", (object)a.LeavePeriod ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@LeavePolicy", (object)a.LeavePolicy ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FromDate", a.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", a.ToDate);
                    cmd.Parameters.AddWithValue("@NewLeavesAllocated", (object)a.NewLeavesAllocated ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AddUnusedLeaves", a.AddUnusedLeaves);
                    cmd.Parameters.AddWithValue("@Description", (object)a.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ErpNextId", (object)a.ErpNextId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DocStatus", a.DocStatus);
                    cmd.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(a.Status) ? StatusFromDocStatus(a.DocStatus) : a.Status);

                    conn.Open();
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        public void UpdateErpNextId(int id, string erpNextId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE LeaveAllocations SET ErpNextId = @ErpNextId WHERE Id = @Id";
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
                string query = @"SELECT COUNT(*) FROM LeaveAllocations WHERE ErpNextId = @ErpNextId AND IsDeleted = 0";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", erpNextId);
                    conn.Open();
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }

        public void InsertFromErpNext(LeaveAllocation model)
        {
            Insert(model);
        }

        public void UpdateFromErpNext(LeaveAllocation model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"UPDATE LeaveAllocations
                    SET Employee = @Employee,
                        EmployeeName = @EmployeeName,
                        Company = @Company,
                        LeaveType = @LeaveType,
                        LeavePeriod = @LeavePeriod,
                        LeavePolicy = @LeavePolicy,
                        NewLeavesAllocated = @NewLeavesAllocated,
                        AddUnusedLeaves = @AddUnusedLeaves,
                        Description = @Description,
                        FromDate = @FromDate,
                        ToDate = @ToDate,
                        DocStatus = @DocStatus,
                        Status = @Status,
                        IsDeleted = 0
                    WHERE ErpNextId = @ErpNextId";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Employee", model.Employee ?? "");
                    cmd.Parameters.AddWithValue("@EmployeeName", (object)model.EmployeeName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Company", model.Company ?? "AITS");
                    cmd.Parameters.AddWithValue("@LeaveType", model.LeaveType ?? "");
                    cmd.Parameters.AddWithValue("@LeavePeriod", (object)model.LeavePeriod ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@LeavePolicy", (object)model.LeavePolicy ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NewLeavesAllocated", (object)model.NewLeavesAllocated ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AddUnusedLeaves", model.AddUnusedLeaves);
                    cmd.Parameters.AddWithValue("@Description", (object)model.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FromDate", model.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", model.ToDate);
                    cmd.Parameters.AddWithValue("@DocStatus", model.DocStatus);
                    cmd.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(model.Status) ? StatusFromDocStatus(model.DocStatus) : model.Status);
                    cmd.Parameters.AddWithValue("@ErpNextId", model.ErpNextId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private LeaveAllocation MapAllocation(SqlDataReader reader)
        {
            bool hasErpNextId = false;
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals("ErpNextId", StringComparison.OrdinalIgnoreCase))
                {
                    hasErpNextId = true;
                    break;
                }
            }

            return new LeaveAllocation
            {
                Id = Convert.ToInt32(reader["Id"]),
                Series = reader["Series"]?.ToString() ?? "HR-LAL-.YYYY.-",
                Employee = reader["Employee"]?.ToString() ?? string.Empty,
                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
                Company = reader["Company"]?.ToString() ?? "AITS",
                LeaveType = reader["LeaveType"]?.ToString() ?? string.Empty,
                LeavePeriod = reader["LeavePeriod"]?.ToString() ?? string.Empty,
                LeavePolicy = reader["LeavePolicy"]?.ToString() ?? string.Empty,
                FromDate = Convert.ToDateTime(reader["FromDate"]),
                ToDate = Convert.ToDateTime(reader["ToDate"]),
                NewLeavesAllocated = reader["NewLeavesAllocated"] != DBNull.Value ? Convert.ToDecimal(reader["NewLeavesAllocated"]) : (decimal?)null,
                AddUnusedLeaves = reader["AddUnusedLeaves"] != DBNull.Value && Convert.ToBoolean(reader["AddUnusedLeaves"]),
                Description = reader["Description"]?.ToString() ?? string.Empty,
                Expired = reader["Expired"] != DBNull.Value && Convert.ToBoolean(reader["Expired"]),
                ErpNextId = hasErpNextId && reader["ErpNextId"] != DBNull.Value ? reader["ErpNextId"].ToString() : null,
                DocStatus = HasColumn(reader, "DocStatus") && reader["DocStatus"] != DBNull.Value ? Convert.ToInt32(reader["DocStatus"]) : 0,
                Status = HasColumn(reader, "Status") && reader["Status"] != DBNull.Value ? reader["Status"].ToString() : "Draft"
            };
        }

        public LeaveAllocation GetByErpNextId(string erpNextId)
        {
            if (string.IsNullOrWhiteSpace(erpNextId)) return null;

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("SELECT Id FROM LeaveAllocations WHERE ErpNextId = @ErpNextId AND IsDeleted = 0", conn))
            {
                cmd.Parameters.AddWithValue("@ErpNextId", erpNextId);
                conn.Open();
                var value = cmd.ExecuteScalar();
                if (value == null || value == DBNull.Value) return null;
                return GetById(Convert.ToInt32(value));
            }
        }

        public int InsertOrUpdate(LeaveAllocation model)
        {
            var existing = GetByErpNextId(model.ErpNextId);
            if (existing != null)
            {
                model.Id = existing.Id;
                Update(model);
                return existing.Id;
            }

            return Insert(model);
        }

        public void Update(LeaveAllocation model)
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(@"
                UPDATE LeaveAllocations
                SET Series = @Series,
                    Employee = @Employee,
                    EmployeeName = @EmployeeName,
                    Company = @Company,
                    LeaveType = @LeaveType,
                    LeavePeriod = @LeavePeriod,
                    LeavePolicy = @LeavePolicy,
                    FromDate = @FromDate,
                    ToDate = @ToDate,
                    NewLeavesAllocated = @NewLeavesAllocated,
                    AddUnusedLeaves = @AddUnusedLeaves,
                    Description = @Description,
                    ErpNextId = @ErpNextId,
                    DocStatus = @DocStatus,
                    Status = @Status,
                    IsDeleted = 0
                WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Series", model.Series ?? "HR-LAL-.YYYY.-");
                cmd.Parameters.AddWithValue("@Employee", (object)model.Employee ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EmployeeName", (object)model.EmployeeName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Company", model.Company ?? "AITS");
                cmd.Parameters.AddWithValue("@LeaveType", (object)model.LeaveType ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@LeavePeriod", (object)model.LeavePeriod ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@LeavePolicy", (object)model.LeavePolicy ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FromDate", model.FromDate);
                cmd.Parameters.AddWithValue("@ToDate", model.ToDate);
                cmd.Parameters.AddWithValue("@NewLeavesAllocated", (object)model.NewLeavesAllocated ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@AddUnusedLeaves", model.AddUnusedLeaves);
                cmd.Parameters.AddWithValue("@Description", (object)model.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ErpNextId", (object)model.ErpNextId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DocStatus", model.DocStatus);
                cmd.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(model.Status) ? StatusFromDocStatus(model.DocStatus) : model.Status);
                cmd.Parameters.AddWithValue("@Id", model.Id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void MarkDeletedById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("UPDATE LeaveAllocations SET IsDeleted = 1 WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private static bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase)) return true;
            }

            return false;
        }

        private static string StatusFromDocStatus(int docStatus)
        {
            return docStatus == 1
                ? "Submitted"
                : docStatus == 2
                    ? "Cancelled"
                    : "Draft";
        }

        private void EnsureColumns()
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(@"
                IF COL_LENGTH('LeaveAllocations', 'DocStatus') IS NULL
                BEGIN
                    ALTER TABLE LeaveAllocations ADD DocStatus INT NOT NULL CONSTRAINT DF_LeaveAllocations_DocStatus DEFAULT 0
                END

                IF COL_LENGTH('LeaveAllocations', 'Status') IS NULL
                BEGIN
                    ALTER TABLE LeaveAllocations ADD Status NVARCHAR(50) NULL
                END", conn))
            {
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
