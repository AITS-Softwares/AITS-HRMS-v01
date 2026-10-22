
//using System.Collections.Generic;
//using System;
//using Erp_Aits.Models;
//using Microsoft.Data.SqlClient;

//namespace Erp_Aits.Services
//{
//    public class ExpenseClaimService
//    {
//        private readonly string _connectionString;

//        public ExpenseClaimService(string connectionString)
//        {
//            _connectionString = connectionString;
//        }

//        public int InsertExpenseClaim(ExpenseClaim claim)
//        {
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                conn.Open();
//                using (var transaction = conn.BeginTransaction())
//                {
//                    try
//                    {
//                        int newId;

//                        string headerQuery = @"
//                            INSERT INTO ExpenseClaims
//                                (ExpenseClaimCode, ExpenseApprover, Employee, EmployeeName, ApprovalStatus, Department, Company,
//                                 PostingDate, ClearanceDate, IsPaid, Remark, PayableAccount, Project, CostCenter,
//                                 Status, Task)
//                            OUTPUT INSERTED.Id
//                            VALUES
//                                (@ExpenseClaimCode, @ExpenseApprover, @Employee, @EmployeeName, @ApprovalStatus, @Department, @Company,
//                                 @PostingDate, @ClearanceDate, @IsPaid, @Remark, @PayableAccount, @Project, @CostCenter,
//                                 @Status, @Task)";

//                        using (var cmd = new SqlCommand(headerQuery, conn, transaction))
//                        {
//                            cmd.Parameters.AddWithValue("@ExpenseClaimCode", claim.ExpenseClaimCode ?? (object)DBNull.Value);
//                            cmd.Parameters.AddWithValue("@ExpenseApprover", claim.ExpenseApprover ?? (object)DBNull.Value);
//                            cmd.Parameters.AddWithValue("@Employee", claim.Employee ?? (object)DBNull.Value);
//                            cmd.Parameters.AddWithValue("@EmployeeName", claim.EmployeeName ?? (object)DBNull.Value);
//                            cmd.Parameters.AddWithValue("@ApprovalStatus", claim.ApprovalStatus ?? "Draft");
//                            cmd.Parameters.AddWithValue("@Department", claim.Department ?? (object)DBNull.Value);
//                            cmd.Parameters.AddWithValue("@Company", claim.Company ?? (object)DBNull.Value);
//                            cmd.Parameters.AddWithValue("@PostingDate", (object)claim.PostingDate ?? DateTime.Now);
//                            cmd.Parameters.AddWithValue("@ClearanceDate", (object)claim.ClearanceDate ?? DBNull.Value);
//                            cmd.Parameters.AddWithValue("@IsPaid", claim.IsPaid);
//                            cmd.Parameters.AddWithValue("@Remark", claim.Remark ?? (object)DBNull.Value);
//                            cmd.Parameters.AddWithValue("@PayableAccount", claim.PayableAccount ?? (object)DBNull.Value);
//                            cmd.Parameters.AddWithValue("@Project", claim.Project ?? (object)DBNull.Value);
//                            cmd.Parameters.AddWithValue("@CostCenter", claim.CostCenter ?? (object)DBNull.Value);
//                            cmd.Parameters.AddWithValue("@Status", claim.Status ?? "Draft");
//                            cmd.Parameters.AddWithValue("@Task", claim.Task ?? (object)DBNull.Value);

//                            newId = (int)cmd.ExecuteScalar();
//                        }

//                        if (claim.Expenses != null)
//                        {
//                            foreach (var e in claim.Expenses)
//                            {
//                                if (string.IsNullOrEmpty(e.ExpenseClaimType)) continue;

//                                string q = @"INSERT INTO ExpenseClaimDetails
//                                             (ExpenseClaimId, ExpenseDate, ExpenseClaimType, Description, Amount, SanctionedAmount)
//                                             VALUES (@ExpenseClaimId, @ExpenseDate, @ExpenseClaimType, @Description, @Amount, @SanctionedAmount)";
//                                using (var cmd = new SqlCommand(q, conn, transaction))
//                                {
//                                    cmd.Parameters.AddWithValue("@ExpenseClaimId", newId);
//                                    cmd.Parameters.AddWithValue("@ExpenseDate", (object)e.ExpenseDate ?? DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@ExpenseClaimType", e.ExpenseClaimType ?? (object)DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@Description", e.Description ?? (object)DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@Amount", e.Amount);
//                                    cmd.Parameters.AddWithValue("@SanctionedAmount", e.SanctionedAmount);
//                                    cmd.ExecuteNonQuery();
//                                }
//                            }
//                        }

//                        if (claim.Taxes != null)
//                        {
//                            foreach (var t in claim.Taxes)
//                            {
//                                if (string.IsNullOrEmpty(t.AccountHead)) continue;

//                                string q = @"INSERT INTO ExpenseTaxesAndCharges
//                                             (ExpenseClaimId, AccountHead, Rate, Amount, Total)
//                                             VALUES (@ExpenseClaimId, @AccountHead, @Rate, @Amount, @Total)";
//                                using (var cmd = new SqlCommand(q, conn, transaction))
//                                {
//                                    cmd.Parameters.AddWithValue("@ExpenseClaimId", newId);
//                                    cmd.Parameters.AddWithValue("@AccountHead", t.AccountHead ?? (object)DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@Rate", t.Rate);
//                                    cmd.Parameters.AddWithValue("@Amount", t.Amount);
//                                    cmd.Parameters.AddWithValue("@Total", t.Total);
//                                    cmd.ExecuteNonQuery();
//                                }
//                            }
//                        }

//                        transaction.Commit();
//                        claim.Id = newId;
//                        return newId;
//                    }
//                    catch
//                    {
//                        transaction.Rollback();
//                        throw;
//                    }
//                }
//            }
//        }

//        public void UpdateErpNextId(int id, string erpNextId)
//        {
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = "UPDATE ExpenseClaims SET ErpNextId = @ErpNextId WHERE Id = @Id";
//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    cmd.Parameters.AddWithValue("@ErpNextId", (object)erpNextId ?? DBNull.Value);
//                    cmd.Parameters.AddWithValue("@Id", id);
//                    conn.Open();
//                    cmd.ExecuteNonQuery();
//                }
//            }
//        }
//        public List<ExpenseClaim> GetAllExpenseClaims(string status = null)
//        {
//            var list = new List<ExpenseClaim>();
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = "SELECT * FROM ExpenseClaims WHERE 1=1";
//                if (!string.IsNullOrEmpty(status))
//                    query += " AND Status = @Status";
//                query += " ORDER BY Id DESC";

//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    if (!string.IsNullOrEmpty(status))
//                        cmd.Parameters.AddWithValue("@Status", status);

//                    conn.Open();
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            list.Add(new ExpenseClaim
//                            {
//                                Id = Convert.ToInt32(reader["Id"]),
//                                ExpenseClaimCode = reader["ExpenseClaimCode"] as string,
//                                Employee = reader["Employee"] as string,
//                                EmployeeName = reader["EmployeeName"] as string,
//                                Company = reader["Company"] as string,
//                                Department = reader["Department"] as string,
//                                Status = reader["Status"] as string,
//                                ApprovalStatus = reader["ApprovalStatus"] as string,
//                                PostingDate = reader["PostingDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["PostingDate"]),
//                                IsPaid = reader["IsPaid"] != DBNull.Value && Convert.ToBoolean(reader["IsPaid"]),
//                                ErpNextId = reader["ErpNextId"] == DBNull.Value ? null : reader["ErpNextId"].ToString()
//                            });
//                        }
//                    }
//                }
//            }
//            return list;
//        }

//        public List<ExpenseClaim> GetUnpaidExpenseClaims(string employee)
//        {
//            var list = new List<ExpenseClaim>();
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = "SELECT * FROM ExpenseClaims WHERE IsPaid = 0 AND Employee = @Employee ORDER BY Id DESC";
//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    cmd.Parameters.AddWithValue("@Employee", employee ?? (object)DBNull.Value);
//                    conn.Open();
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            list.Add(new ExpenseClaim
//                            {
//                                Id = Convert.ToInt32(reader["Id"]),
//                                ExpenseClaimCode = reader["ExpenseClaimCode"] as string,
//                                Employee = reader["Employee"] as string,
//                                EmployeeName = reader["EmployeeName"] as string,
//                                Company = reader["Company"] as string,
//                                Status = reader["Status"] as string,
//                                PostingDate = reader["PostingDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["PostingDate"]),
//                                IsPaid = false
//                            });
//                        }
//                    }
//                }
//            }
//            return list;
//        }

//        public string GetNextExpenseClaimCode()
//        {
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = "SELECT COUNT(*) FROM ExpenseClaims";
//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    conn.Open();
//                    int count = (int)cmd.ExecuteScalar() + 1;
//                    return "HR-EXP-" + count.ToString("00000");
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
    public class ExpenseClaimService
    {
        private readonly string _connectionString;

        public ExpenseClaimService(string connectionString)
        {
            _connectionString = connectionString;
        }

        private bool EnsureAccountingDimensionColumns()
        {
            const string sql = @"
                IF COL_LENGTH('dbo.ExpenseClaims', 'Dunning') IS NULL
                    ALTER TABLE dbo.ExpenseClaims ADD Dunning NVARCHAR(255) NULL;
                IF COL_LENGTH('dbo.ExpenseClaims', 'AiAuditLog') IS NULL
                    ALTER TABLE dbo.ExpenseClaims ADD AiAuditLog NVARCHAR(255) NULL;
                IF COL_LENGTH('dbo.ExpenseClaims', 'AllowedToTransactWith') IS NULL
                    ALTER TABLE dbo.ExpenseClaims ADD AllowedToTransactWith NVARCHAR(255) NULL;
                IF COL_LENGTH('dbo.ExpenseClaims', 'AppraisalTemplate') IS NULL
                    ALTER TABLE dbo.ExpenseClaims ADD AppraisalTemplate NVARCHAR(255) NULL;";

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                try
                {
                    // Do not let a schema lock prevent ordinary page requests.
                    cmd.CommandTimeout = 5;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch (SqlException ex) when (ex.Number == -2)
                {
                    return false;
                }
            }
        }

        private void RequireAccountingDimensionColumns()
        {
            if (!EnsureAccountingDimensionColumns())
                throw new InvalidOperationException(
                    "Expense Claim accounting-dimension fields are waiting for SQL Server. Please retry the save in a few seconds.");
        }

        public HashSet<string> GetExistingErpNextIds()
        {
            var ids = new HashSet<string>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT ErpNextId FROM ExpenseClaims WHERE ErpNextId IS NOT NULL AND ErpNextId != ''";
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ids.Add(reader["ErpNextId"].ToString());
                        }
                    }
                }
            }
            return ids;
        }

        public HashSet<string> GetExistingClaimCodes()
        {
            var codes = new HashSet<string>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT ExpenseClaimCode FROM ExpenseClaims WHERE ExpenseClaimCode IS NOT NULL AND ExpenseClaimCode != ''";
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            codes.Add(reader["ExpenseClaimCode"].ToString());
                        }
                    }
                }
            }
            return codes;
        }

        public int InsertExpenseClaim(ExpenseClaim claim)
        {
            RequireAccountingDimensionColumns();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int newId;

                        string headerQuery = @"
                            INSERT INTO ExpenseClaims
                                (ExpenseClaimCode, ExpenseApprover, Employee, EmployeeName, ApprovalStatus, Department, Company,
                                 PostingDate, ClearanceDate, IsPaid, Remark, PayableAccount, Project, CostCenter, Dunning, AiAuditLog, AllowedToTransactWith, AppraisalTemplate,
                                 Status, Task, ErpNextId)
                            OUTPUT INSERTED.Id
                            VALUES
                                (@ExpenseClaimCode, @ExpenseApprover, @Employee, @EmployeeName, @ApprovalStatus, @Department, @Company,
                                 @PostingDate, @ClearanceDate, @IsPaid, @Remark, @PayableAccount, @Project, @CostCenter, @Dunning, @AiAuditLog, @AllowedToTransactWith, @AppraisalTemplate,
                                 @Status, @Task, @ErpNextId)";

                        using (var cmd = new SqlCommand(headerQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ExpenseClaimCode", claim.ExpenseClaimCode ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@ExpenseApprover", claim.ExpenseApprover ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Employee", claim.Employee ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@EmployeeName", claim.EmployeeName ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@ApprovalStatus", claim.ApprovalStatus ?? "Draft");
                            cmd.Parameters.AddWithValue("@Department", claim.Department ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Company", claim.Company ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@PostingDate", (object)claim.PostingDate ?? DateTime.Now);
                            cmd.Parameters.AddWithValue("@ClearanceDate", (object)claim.ClearanceDate ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@IsPaid", claim.IsPaid);
                            cmd.Parameters.AddWithValue("@Remark", claim.Remark ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@PayableAccount", claim.PayableAccount ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Project", claim.Project ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@CostCenter", claim.CostCenter ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Dunning", claim.Dunning ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@AiAuditLog", claim.AiAuditLog ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@AllowedToTransactWith", claim.AllowedToTransactWith ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@AppraisalTemplate", claim.AppraisalTemplate ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Status", claim.Status ?? "Draft");
                            cmd.Parameters.AddWithValue("@Task", claim.Task ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@ErpNextId", claim.ErpNextId ?? (object)DBNull.Value);

                            newId = (int)cmd.ExecuteScalar();
                        }

                        if (claim.Expenses != null)
                        {
                            foreach (var e in claim.Expenses)
                            {
                                if (string.IsNullOrEmpty(e.ExpenseClaimType)) continue;

                                string q = @"INSERT INTO ExpenseClaimDetails
                                             (ExpenseClaimId, ExpenseDate, ExpenseClaimType, Description, Amount, SanctionedAmount)
                                             VALUES (@ExpenseClaimId, @ExpenseDate, @ExpenseClaimType, @Description, @Amount, @SanctionedAmount)";
                                using (var cmd = new SqlCommand(q, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@ExpenseClaimId", newId);
                                    cmd.Parameters.AddWithValue("@ExpenseDate", (object)e.ExpenseDate ?? DBNull.Value);
                                    cmd.Parameters.AddWithValue("@ExpenseClaimType", e.ExpenseClaimType ?? (object)DBNull.Value);
                                    cmd.Parameters.AddWithValue("@Description", e.Description ?? (object)DBNull.Value);
                                    cmd.Parameters.AddWithValue("@Amount", e.Amount);
                                    cmd.Parameters.AddWithValue("@SanctionedAmount", e.SanctionedAmount);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        if (claim.Taxes != null)
                        {
                            foreach (var t in claim.Taxes)
                            {
                                if (string.IsNullOrEmpty(t.AccountHead)) continue;

                                string q = @"INSERT INTO ExpenseTaxesAndCharges
                                             (ExpenseClaimId, AccountHead, Rate, Amount, Total)
                                             VALUES (@ExpenseClaimId, @AccountHead, @Rate, @Amount, @Total)";
                                using (var cmd = new SqlCommand(q, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@ExpenseClaimId", newId);
                                    cmd.Parameters.AddWithValue("@AccountHead", t.AccountHead ?? (object)DBNull.Value);
                                    cmd.Parameters.AddWithValue("@Rate", t.Rate);
                                    cmd.Parameters.AddWithValue("@Amount", t.Amount);
                                    cmd.Parameters.AddWithValue("@Total", t.Total);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        if (claim.Advances != null)
                        {
                            foreach (var a in claim.Advances)
                            {
                                if (string.IsNullOrEmpty(a.EmployeeAdvance)) continue;

                                string q = @"INSERT INTO ExpenseClaimAdvances
                                             (ExpenseClaimId, EmployeeAdvance, PostingDate, AdvancePaid, UnclaimedAmount, AllocatedAmount)
                                             VALUES (@ExpenseClaimId, @EmployeeAdvance, @PostingDate, @AdvancePaid, @UnclaimedAmount, @AllocatedAmount)";
                                using (var cmd = new SqlCommand(q, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@ExpenseClaimId", newId);
                                    cmd.Parameters.AddWithValue("@EmployeeAdvance", a.EmployeeAdvance ?? (object)DBNull.Value);
                                    cmd.Parameters.AddWithValue("@PostingDate", (object)a.PostingDate ?? DBNull.Value);
                                    cmd.Parameters.AddWithValue("@AdvancePaid", a.AdvancePaid);
                                    cmd.Parameters.AddWithValue("@UnclaimedAmount", a.UnclaimedAmount);
                                    cmd.Parameters.AddWithValue("@AllocatedAmount", a.AllocatedAmount);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                        claim.Id = newId;
                        return newId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void UpdateErpNextId(int id, string erpNextId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE ExpenseClaims SET ErpNextId = @ErpNextId WHERE Id = @Id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", (object)erpNextId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public ExpenseClaim GetExpenseClaimByErpNextId(string erpNextId)
        {
            if (string.IsNullOrWhiteSpace(erpNextId))
                return null;

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT TOP 1 Id FROM ExpenseClaims WHERE ErpNextId = @ErpNextId OR ExpenseClaimCode = @ErpNextId ORDER BY Id DESC";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", erpNextId);
                    conn.Open();
                    object value = cmd.ExecuteScalar();

                    if (value == null || value == DBNull.Value)
                        return null;

                    return GetExpenseClaimById(Convert.ToInt32(value));
                }
            }
        }

        public int InsertOrUpdateExpenseClaim(ExpenseClaim claim)
        {
            RequireAccountingDimensionColumns();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int existingId = 0;
                        string lookup = !string.IsNullOrWhiteSpace(claim.ErpNextId)
                            ? claim.ErpNextId
                            : claim.ExpenseClaimCode;

                        if (!string.IsNullOrWhiteSpace(lookup))
                        {
                            using (var find = new SqlCommand(@"
                                SELECT TOP 1 Id
                                FROM ExpenseClaims
                                WHERE ErpNextId = @Lookup OR ExpenseClaimCode = @Lookup
                                ORDER BY Id DESC",
                                conn,
                                transaction))
                            {
                                find.Parameters.AddWithValue("@Lookup", lookup);
                                object value = find.ExecuteScalar();

                                if (value != null && value != DBNull.Value)
                                    existingId = Convert.ToInt32(value);
                            }
                        }

                        int claimId;

                        if (existingId > 0)
                        {
                            using (var cmd = new SqlCommand(@"
                                UPDATE ExpenseClaims
                                SET ExpenseClaimCode = @ExpenseClaimCode,
                                    ExpenseApprover = @ExpenseApprover,
                                    Employee = @Employee,
                                    EmployeeName = @EmployeeName,
                                    ApprovalStatus = @ApprovalStatus,
                                    Department = @Department,
                                    Company = @Company,
                                    PostingDate = @PostingDate,
                                    ClearanceDate = @ClearanceDate,
                                    IsPaid = @IsPaid,
                                    Remark = @Remark,
                                    PayableAccount = @PayableAccount,
                                    Project = @Project,
                                    CostCenter = @CostCenter,
                                    Dunning = @Dunning,
                                    AiAuditLog = @AiAuditLog,
                                    AllowedToTransactWith = @AllowedToTransactWith,
                                    AppraisalTemplate = @AppraisalTemplate,
                                    Status = @Status,
                                    Task = @Task,
                                    ErpNextId = @ErpNextId
                                WHERE Id = @Id",
                                conn,
                                transaction))
                            {
                                AddHeaderParameters(cmd, claim);
                                cmd.Parameters.AddWithValue("@Id", existingId);
                                cmd.ExecuteNonQuery();
                            }

                            claimId = existingId;

                            DeleteChildRows(conn, transaction, claimId);
                        }
                        else
                        {
                            using (var cmd = new SqlCommand(@"
                                INSERT INTO ExpenseClaims
                                    (ExpenseClaimCode, ExpenseApprover, Employee, EmployeeName, ApprovalStatus, Department, Company,
                                     PostingDate, ClearanceDate, IsPaid, Remark, PayableAccount, Project, CostCenter, Dunning, AiAuditLog, AllowedToTransactWith, AppraisalTemplate,
                                     Status, Task, ErpNextId)
                                OUTPUT INSERTED.Id
                                VALUES
                                    (@ExpenseClaimCode, @ExpenseApprover, @Employee, @EmployeeName, @ApprovalStatus, @Department, @Company,
                                     @PostingDate, @ClearanceDate, @IsPaid, @Remark, @PayableAccount, @Project, @CostCenter, @Dunning, @AiAuditLog, @AllowedToTransactWith, @AppraisalTemplate,
                                     @Status, @Task, @ErpNextId)",
                                conn,
                                transaction))
                            {
                                AddHeaderParameters(cmd, claim);
                                claimId = Convert.ToInt32(cmd.ExecuteScalar());
                            }
                        }

                        InsertChildRows(conn, transaction, claimId, claim);

                        transaction.Commit();
                        claim.Id = claimId;
                        return claimId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private static void AddHeaderParameters(SqlCommand cmd, ExpenseClaim claim)
        {
            cmd.Parameters.AddWithValue("@ExpenseClaimCode", claim.ExpenseClaimCode ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ExpenseApprover", claim.ExpenseApprover ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Employee", claim.Employee ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@EmployeeName", claim.EmployeeName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ApprovalStatus", claim.ApprovalStatus ?? "Draft");
            cmd.Parameters.AddWithValue("@Department", claim.Department ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Company", claim.Company ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@PostingDate", (object)claim.PostingDate ?? DateTime.Now);
            cmd.Parameters.AddWithValue("@ClearanceDate", (object)claim.ClearanceDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsPaid", claim.IsPaid);
            cmd.Parameters.AddWithValue("@Remark", claim.Remark ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@PayableAccount", claim.PayableAccount ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Project", claim.Project ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@CostCenter", claim.CostCenter ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Dunning", claim.Dunning ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@AiAuditLog", claim.AiAuditLog ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@AllowedToTransactWith", claim.AllowedToTransactWith ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@AppraisalTemplate", claim.AppraisalTemplate ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", claim.Status ?? "Draft");
            cmd.Parameters.AddWithValue("@Task", claim.Task ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ErpNextId", claim.ErpNextId ?? (object)DBNull.Value);
        }

        private static void DeleteChildRows(SqlConnection conn, SqlTransaction transaction, int claimId)
        {
            foreach (string table in new[] { "ExpenseClaimDetails", "ExpenseTaxesAndCharges", "ExpenseClaimAdvances" })
            {
                using (var cmd = new SqlCommand($"DELETE FROM {table} WHERE ExpenseClaimId = @ExpenseClaimId", conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@ExpenseClaimId", claimId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void InsertChildRows(SqlConnection conn, SqlTransaction transaction, int claimId, ExpenseClaim claim)
        {
            if (claim.Expenses != null)
            {
                foreach (var e in claim.Expenses)
                {
                    if (string.IsNullOrEmpty(e.ExpenseClaimType)) continue;

                    using (var cmd = new SqlCommand(@"
                        INSERT INTO ExpenseClaimDetails
                            (ExpenseClaimId, ExpenseDate, ExpenseClaimType, Description, Amount, SanctionedAmount)
                        VALUES
                            (@ExpenseClaimId, @ExpenseDate, @ExpenseClaimType, @Description, @Amount, @SanctionedAmount)",
                        conn,
                        transaction))
                    {
                        cmd.Parameters.AddWithValue("@ExpenseClaimId", claimId);
                        cmd.Parameters.AddWithValue("@ExpenseDate", (object)e.ExpenseDate ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ExpenseClaimType", e.ExpenseClaimType ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Description", e.Description ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Amount", e.Amount);
                        cmd.Parameters.AddWithValue("@SanctionedAmount", e.SanctionedAmount);
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            if (claim.Taxes != null)
            {
                foreach (var t in claim.Taxes)
                {
                    if (string.IsNullOrEmpty(t.AccountHead)) continue;

                    using (var cmd = new SqlCommand(@"
                        INSERT INTO ExpenseTaxesAndCharges
                            (ExpenseClaimId, AccountHead, Rate, Amount, Total)
                        VALUES
                            (@ExpenseClaimId, @AccountHead, @Rate, @Amount, @Total)",
                        conn,
                        transaction))
                    {
                        cmd.Parameters.AddWithValue("@ExpenseClaimId", claimId);
                        cmd.Parameters.AddWithValue("@AccountHead", t.AccountHead ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Rate", t.Rate);
                        cmd.Parameters.AddWithValue("@Amount", t.Amount);
                        cmd.Parameters.AddWithValue("@Total", t.Total);
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            if (claim.Advances != null)
            {
                foreach (var a in claim.Advances)
                {
                    if (string.IsNullOrEmpty(a.EmployeeAdvance)) continue;

                    using (var cmd = new SqlCommand(@"
                        INSERT INTO ExpenseClaimAdvances
                            (ExpenseClaimId, EmployeeAdvance, PostingDate, AdvancePaid, UnclaimedAmount, AllocatedAmount)
                        VALUES
                            (@ExpenseClaimId, @EmployeeAdvance, @PostingDate, @AdvancePaid, @UnclaimedAmount, @AllocatedAmount)",
                        conn,
                        transaction))
                    {
                        cmd.Parameters.AddWithValue("@ExpenseClaimId", claimId);
                        cmd.Parameters.AddWithValue("@EmployeeAdvance", a.EmployeeAdvance ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@PostingDate", (object)a.PostingDate ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@AdvancePaid", a.AdvancePaid);
                        cmd.Parameters.AddWithValue("@UnclaimedAmount", a.UnclaimedAmount);
                        cmd.Parameters.AddWithValue("@AllocatedAmount", a.AllocatedAmount);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public List<ExpenseClaim> GetAllExpenseClaims(string status = null)
        {
            var list = new List<ExpenseClaim>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM ExpenseClaims WHERE 1=1";
                if (!string.IsNullOrEmpty(status))
                    query += " AND Status = @Status";
                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(status))
                        cmd.Parameters.AddWithValue("@Status", status);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new ExpenseClaim
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                ExpenseClaimCode = reader["ExpenseClaimCode"] as string,
                                Employee = reader["Employee"] as string,
                                EmployeeName = reader["EmployeeName"] as string,
                                Company = reader["Company"] as string,
                                Department = reader["Department"] as string,
                                Status = reader["Status"] as string,
                                ApprovalStatus = reader["ApprovalStatus"] as string,
                                PostingDate = reader["PostingDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["PostingDate"]),
                                IsPaid = reader["IsPaid"] != DBNull.Value && Convert.ToBoolean(reader["IsPaid"]),
                                ErpNextId = reader["ErpNextId"] == DBNull.Value ? null : reader["ErpNextId"].ToString()
                            });
                        }
                    }
                }
            }
            return list;
        }


        public ExpenseClaim GetExpenseClaimById(int id)
        {
            ExpenseClaim claim = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string headerQuery = "SELECT * FROM ExpenseClaims WHERE Id = @Id";

                using (var cmd = new SqlCommand(headerQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            claim = new ExpenseClaim
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                ExpenseClaimCode = reader["ExpenseClaimCode"]?.ToString(),
                                ExpenseApprover = reader["ExpenseApprover"]?.ToString(),
                                Employee = reader["Employee"]?.ToString(),
                                EmployeeName = reader["EmployeeName"]?.ToString(),
                                ApprovalStatus = reader["ApprovalStatus"]?.ToString(),
                                Department = reader["Department"]?.ToString(),
                                Company = reader["Company"]?.ToString(),
                                ErpNextId = reader["ErpNextId"]?.ToString(),

                                PostingDate = reader["PostingDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["PostingDate"]),
                                ClearanceDate = reader["ClearanceDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["ClearanceDate"]),
                                IsPaid = reader["IsPaid"] != DBNull.Value && Convert.ToBoolean(reader["IsPaid"]),
                                Remark = reader["Remark"]?.ToString(),
                                PayableAccount = reader["PayableAccount"]?.ToString(),
                                Project = reader["Project"]?.ToString(),
                                CostCenter = reader["CostCenter"]?.ToString(),
                                Dunning = GetOptionalString(reader, "Dunning"),
                                AiAuditLog = GetOptionalString(reader, "AiAuditLog"),
                                AllowedToTransactWith = GetOptionalString(reader, "AllowedToTransactWith"),
                                AppraisalTemplate = GetOptionalString(reader, "AppraisalTemplate"),

                                Status = reader["Status"]?.ToString(),
                                Task = reader["Task"]?.ToString()
                            };
                        }
                    }
                }

                if (claim == null)
                {
                    return null;
                }

                string expenseQuery = "SELECT * FROM ExpenseClaimDetails WHERE ExpenseClaimId = @ExpenseClaimId";

                using (var cmd = new SqlCommand(expenseQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@ExpenseClaimId", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            claim.Expenses.Add(new ExpenseClaimDetail
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                ExpenseClaimId = id,
                                ExpenseDate = reader["ExpenseDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["ExpenseDate"]),
                                ExpenseClaimType = reader["ExpenseClaimType"]?.ToString(),
                                Description = reader["Description"]?.ToString(),
                                Amount = reader["Amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Amount"]),
                                SanctionedAmount = reader["SanctionedAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["SanctionedAmount"])
                            });
                        }
                    }
                }

                string taxQuery = "SELECT * FROM ExpenseTaxesAndCharges WHERE ExpenseClaimId = @ExpenseClaimId";

                using (var cmd = new SqlCommand(taxQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@ExpenseClaimId", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            claim.Taxes.Add(new ExpenseTaxAndCharge
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                ExpenseClaimId = id,
                                AccountHead = reader["AccountHead"]?.ToString(),
                                Rate = reader["Rate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Rate"]),
                                Amount = reader["Amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Amount"]),
                                Total = reader["Total"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Total"])
                            });
                        }
                    }
                }

                string advanceQuery = "SELECT * FROM ExpenseClaimAdvances WHERE ExpenseClaimId = @ExpenseClaimId";

                using (var cmd = new SqlCommand(advanceQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@ExpenseClaimId", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            claim.Advances.Add(new ExpenseClaimAdvance
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                ExpenseClaimId = id,
                                EmployeeAdvance = reader["EmployeeAdvance"]?.ToString(),
                                PostingDate = reader["PostingDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["PostingDate"]),
                                AdvancePaid = reader["AdvancePaid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["AdvancePaid"]),
                                UnclaimedAmount = reader["UnclaimedAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["UnclaimedAmount"]),
                                AllocatedAmount = reader["AllocatedAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["AllocatedAmount"])
                            });
                        }
                    }
                }
            }

            return claim;
        }

        private static string GetOptionalString(SqlDataReader reader, string columnName)
        {
            for (int index = 0; index < reader.FieldCount; index++)
            {
                if (string.Equals(reader.GetName(index), columnName, StringComparison.OrdinalIgnoreCase))
                    return reader.IsDBNull(index) ? null : reader.GetValue(index)?.ToString();
            }

            return null;
        }











        public List<ExpenseClaim> GetUnpaidExpenseClaims(string employee)
        {
            var list = new List<ExpenseClaim>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM ExpenseClaims WHERE IsPaid = 0 AND Employee = @Employee ORDER BY Id DESC";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Employee", employee ?? (object)DBNull.Value);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new ExpenseClaim
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                ExpenseClaimCode = reader["ExpenseClaimCode"] as string,
                                Employee = reader["Employee"] as string,
                                EmployeeName = reader["EmployeeName"] as string,
                                Company = reader["Company"] as string,
                                Status = reader["Status"] as string,
                                PostingDate = reader["PostingDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["PostingDate"]),
                                IsPaid = false
                            });
                        }
                    }
                }
            }
            return list;
        }

        public string GetNextExpenseClaimCode()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT COUNT(*) FROM ExpenseClaims";
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    int count = (int)cmd.ExecuteScalar() + 1;
                    return "HR-EXP-" + count.ToString("00000");
                }
            }
        }
    }
}
