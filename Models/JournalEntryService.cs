using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;
using System.Linq;

namespace Erp_Aits.Services
{
    public class JournalEntryService
    {
        private readonly string _connectionString;

        public JournalEntryService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<JournalEntryListRow> GetAll(string idFilter, string title, string entryType, string company)
        {
            var list = new List<JournalEntryListRow>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT Id, EntryType, Title, Company, PostingDate FROM JournalEntries WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(idFilter)) query += " AND CAST(Id AS VARCHAR) LIKE @IdFilter";
                if (!string.IsNullOrEmpty(title)) query += " AND Title LIKE @Title";
                if (!string.IsNullOrEmpty(entryType)) query += " AND EntryType = @EntryType";
                if (!string.IsNullOrEmpty(company)) query += " AND Company = @Company";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(idFilter)) cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");
                    if (!string.IsNullOrEmpty(title)) cmd.Parameters.AddWithValue("@Title", "%" + title + "%");
                    if (!string.IsNullOrEmpty(entryType)) cmd.Parameters.AddWithValue("@EntryType", entryType);
                    if (!string.IsNullOrEmpty(company)) cmd.Parameters.AddWithValue("@Company", company);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new JournalEntryListRow
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                EntryType = reader["EntryType"]?.ToString() ?? string.Empty,
                                Title = reader["Title"]?.ToString() ?? string.Empty,
                                Company = reader["Company"]?.ToString() ?? string.Empty,
                                PostingDate = Convert.ToDateTime(reader["PostingDate"])
                            });
                        }
                    }
                }
            }

            return list;
        }

        public JournalEntry GetById(int id)
        {
            JournalEntry model = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string query = "SELECT * FROM JournalEntries WHERE Id = @Id AND IsDeleted = 0";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model = new JournalEntry
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                EntryType = reader["EntryType"]?.ToString() ?? string.Empty,
                                FromTemplate = reader["FromTemplate"]?.ToString() ?? string.Empty,
                                Series = reader["Series"]?.ToString() ?? string.Empty,
                                Company = reader["Company"]?.ToString() ?? string.Empty,
                                PostingDate = Convert.ToDateTime(reader["PostingDate"]),
                                ReferenceNumber = reader["ReferenceNumber"]?.ToString() ?? string.Empty,
                                ReferenceDate = reader["ReferenceDate"] as DateTime?,
                                MultiCurrency = reader["MultiCurrency"] as bool? ?? false,
                                UserRemark = reader["UserRemark"]?.ToString() ?? string.Empty,
                                BillNo = reader["BillNo"]?.ToString() ?? string.Empty,
                                BillDate = reader["BillDate"] as DateTime?,
                                DueDate = reader["DueDate"] as DateTime?,
                                PayToRecdFrom = reader["PayToRecdFrom"]?.ToString() ?? string.Empty,
                                LetterHead = reader["LetterHead"]?.ToString() ?? string.Empty,
                                PrintHeading = reader["PrintHeading"]?.ToString() ?? string.Empty,
                                ModeOfPayment = reader["ModeOfPayment"]?.ToString() ?? string.Empty,
                                IsOpening = reader["IsOpening"]?.ToString() ?? "No",
                                Title = reader["Title"]?.ToString() ?? string.Empty
                            };
                        }
                    }
                }

                if (model == null) return null;

                string q2 = "SELECT Id, JournalEntryId, Account, PartyType, Party, Debit, Credit FROM JournalEntryAccounts WHERE JournalEntryId = @Id";
                using (var cmd = new SqlCommand(q2, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.Accounts.Add(new JournalEntryAccount
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                JournalEntryId = Convert.ToInt32(reader["JournalEntryId"]),
                                Account = reader["Account"]?.ToString() ?? string.Empty,
                                PartyType = reader["PartyType"]?.ToString() ?? string.Empty,
                                Party = reader["Party"]?.ToString() ?? string.Empty,
                                Debit = Convert.ToDecimal(reader["Debit"]),
                                Credit = Convert.ToDecimal(reader["Credit"])
                            });
                        }
                    }
                }
            }

            return model;
        }





        public int UpsertFromErpNext(
    JournalEntry model,
    List<string> accounts,
    List<string> partyTypes,
    List<string> parties,
    List<string> modeOfPayments,
    List<decimal> debits,
    List<decimal> credits)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int journalEntryId = 0;

                        string checkQuery = @"
SELECT TOP 1 Id
FROM JournalEntries
WHERE IsDeleted = 0
  AND ErpNextId = @ErpNextId";

                        using (var cmd = new SqlCommand(checkQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ErpNextId", (object)model.ErpNextId ?? DBNull.Value);

                            object existingId = cmd.ExecuteScalar();

                            if (existingId != null && existingId != DBNull.Value)
                                journalEntryId = Convert.ToInt32(existingId);
                        }

                        if (model.TotalDebit == 0 && debits != null)
                            model.TotalDebit = debits.Sum();

                        if (model.TotalCredit == 0 && credits != null)
                            model.TotalCredit = credits.Sum();

                        model.DifferenceAmount = model.TotalDebit - model.TotalCredit;

                        if (string.IsNullOrWhiteSpace(model.BillNo))
                            model.BillNo = model.ReferenceNumber;

                        if (!model.BillDate.HasValue)
                            model.BillDate = model.ReferenceDate;

                        if (journalEntryId > 0)
                        {
                            string updateQuery = @"
UPDATE JournalEntries
SET
    EntryType = @EntryType,
    FromTemplate = @FromTemplate,
    Series = @Series,
    Company = @Company,
    CompanyGstin = @CompanyGstin,
    PostingDate = @PostingDate,
    ReferenceNumber = @ReferenceNumber,
    ReferenceDate = @ReferenceDate,
    TotalDebit = @TotalDebit,
    TotalCredit = @TotalCredit,
    DifferenceAmount = @DifferenceAmount,
    MultiCurrency = @MultiCurrency,
    UserRemark = @UserRemark,
    BillNo = @BillNo,
    BillDate = @BillDate,
    DueDate = @DueDate,
    PayToRecdFrom = @PayToRecdFrom,
    LetterHead = @LetterHead,
    PrintHeading = @PrintHeading,
    ModeOfPayment = @ModeOfPayment,
    IsOpening = @IsOpening,
    Title = @Title
WHERE Id = @Id";

                            using (var cmd = new SqlCommand(updateQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Id", journalEntryId);
                                AddJournalEntryParameters(cmd, model);
                                cmd.ExecuteNonQuery();
                            }

                            using (var cmd = new SqlCommand(
                                "DELETE FROM JournalEntryAccounts WHERE JournalEntryId = @JournalEntryId",
                                conn,
                                transaction))
                            {
                                cmd.Parameters.AddWithValue("@JournalEntryId", journalEntryId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            string insertQuery = @"
INSERT INTO JournalEntries
    (EntryType, FromTemplate, Series, Company, CompanyGstin, PostingDate,
     ReferenceNumber, ReferenceDate, TotalDebit, TotalCredit, DifferenceAmount,
     MultiCurrency, UserRemark, BillNo, BillDate, DueDate, PayToRecdFrom,
     LetterHead, PrintHeading, ModeOfPayment, IsOpening, Title, ErpNextId,
     IsDeleted, CreatedOn)
OUTPUT INSERTED.Id
VALUES
    (@EntryType, @FromTemplate, @Series, @Company, @CompanyGstin, @PostingDate,
     @ReferenceNumber, @ReferenceDate, @TotalDebit, @TotalCredit, @DifferenceAmount,
     @MultiCurrency, @UserRemark, @BillNo, @BillDate, @DueDate, @PayToRecdFrom,
     @LetterHead, @PrintHeading, @ModeOfPayment, @IsOpening, @Title, @ErpNextId,
     0, GETDATE())";

                            using (var cmd = new SqlCommand(insertQuery, conn, transaction))
                            {
                                AddJournalEntryParameters(cmd, model);
                                journalEntryId = Convert.ToInt32(cmd.ExecuteScalar());
                            }
                        }

                        if (accounts != null)
                        {
                            for (int i = 0; i < accounts.Count; i++)
                            {
                                if (string.IsNullOrWhiteSpace(accounts[i]))
                                    continue;

                                string accountQuery = @"
INSERT INTO JournalEntryAccounts
    (JournalEntryId, Account, PartyType, Party, ModeOfPayment, Debit, Credit)
VALUES
    (@JournalEntryId, @Account, @PartyType, @Party, @ModeOfPayment, @Debit, @Credit)";

                                using (var cmd = new SqlCommand(accountQuery, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@JournalEntryId", journalEntryId);
                                    cmd.Parameters.AddWithValue("@Account", accounts[i]);

                                    cmd.Parameters.AddWithValue("@PartyType",
                                        partyTypes != null && i < partyTypes.Count && !string.IsNullOrWhiteSpace(partyTypes[i])
                                            ? (object)partyTypes[i]
                                            : DBNull.Value);

                                    cmd.Parameters.AddWithValue("@Party",
                                        parties != null && i < parties.Count && !string.IsNullOrWhiteSpace(parties[i])
                                            ? (object)parties[i]
                                            : DBNull.Value);

                                    cmd.Parameters.AddWithValue("@ModeOfPayment",
                                        modeOfPayments != null && i < modeOfPayments.Count && !string.IsNullOrWhiteSpace(modeOfPayments[i])
                                            ? (object)modeOfPayments[i]
                                            : DBNull.Value);

                                    cmd.Parameters.AddWithValue("@Debit",
                                        debits != null && i < debits.Count ? debits[i] : 0);

                                    cmd.Parameters.AddWithValue("@Credit",
                                        credits != null && i < credits.Count ? credits[i] : 0);

                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                        return journalEntryId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }



        private void AddJournalEntryParameters(SqlCommand cmd, JournalEntry model)
        {
            cmd.Parameters.AddWithValue("@EntryType", model.EntryType ?? "Journal Entry");
            cmd.Parameters.AddWithValue("@FromTemplate", (object)model.FromTemplate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Series", model.Series ?? "ACC-JV-.YYYY.-");
            cmd.Parameters.AddWithValue("@Company", model.Company ?? "AITS");
            cmd.Parameters.AddWithValue("@CompanyGstin", (object)model.CompanyGstin ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PostingDate", model.PostingDate == DateTime.MinValue ? DateTime.Now : model.PostingDate);
            cmd.Parameters.AddWithValue("@ReferenceNumber", (object)model.ReferenceNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ReferenceDate", (object)model.ReferenceDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TotalDebit", model.TotalDebit);
            cmd.Parameters.AddWithValue("@TotalCredit", model.TotalCredit);
            cmd.Parameters.AddWithValue("@DifferenceAmount", model.DifferenceAmount);
            cmd.Parameters.AddWithValue("@MultiCurrency", model.MultiCurrency);
            cmd.Parameters.AddWithValue("@UserRemark", (object)model.UserRemark ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@BillNo", (object)model.BillNo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@BillDate", (object)model.BillDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DueDate", (object)model.DueDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PayToRecdFrom", (object)model.PayToRecdFrom ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LetterHead", (object)model.LetterHead ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PrintHeading", (object)model.PrintHeading ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ModeOfPayment", (object)model.ModeOfPayment ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsOpening", model.IsOpening ?? "No");
            cmd.Parameters.AddWithValue("@Title", (object)model.Title ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ErpNextId", (object)model.ErpNextId ?? DBNull.Value);
        }


























        public void UpdateErpNextId(int id, string erpNextId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE JournalEntries SET ErpNextId = @ErpNextId WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@ErpNextId", (object)erpNextId ?? DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public bool ExistsByErpNextId(string erpNextId)
        {
            if (string.IsNullOrWhiteSpace(erpNextId))
                return false;

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
SELECT COUNT(1)
FROM JournalEntries
WHERE IsDeleted = 0
  AND ErpNextId = @ErpNextId";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", erpNextId);

                    conn.Open();

                    int count = Convert.ToInt32(cmd.ExecuteScalar());

                    return count > 0;
                }
            }
        }












        public int Insert(
            JournalEntry model,
            List<string> accounts,
            List<string> partyTypes,
            List<string> parties,
            List<string> modeOfPayments,
            List<decimal> debits,
            List<decimal> credits)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int newId;

                        if (model.TotalDebit == 0 && debits != null)
                            model.TotalDebit = debits.Sum();

                        if (model.TotalCredit == 0 && credits != null)
                            model.TotalCredit = credits.Sum();

                        model.DifferenceAmount = model.TotalDebit - model.TotalCredit;

                        if (string.IsNullOrWhiteSpace(model.BillNo))
                            model.BillNo = model.ReferenceNumber;

                        if (!model.BillDate.HasValue)
                            model.BillDate = model.ReferenceDate;

                        string query = @"
INSERT INTO JournalEntries
    (EntryType, FromTemplate, Series, Company, CompanyGstin, PostingDate,
     ReferenceNumber, ReferenceDate, TotalDebit, TotalCredit, DifferenceAmount,
     MultiCurrency, UserRemark, BillNo, BillDate, DueDate, PayToRecdFrom,
     LetterHead, PrintHeading, ModeOfPayment, IsOpening, Title, ErpNextId,
     IsDeleted, CreatedOn)
OUTPUT INSERTED.Id
VALUES
    (@EntryType, @FromTemplate, @Series, @Company, @CompanyGstin, @PostingDate,
     @ReferenceNumber, @ReferenceDate, @TotalDebit, @TotalCredit, @DifferenceAmount,
     @MultiCurrency, @UserRemark, @BillNo, @BillDate, @DueDate, @PayToRecdFrom,
     @LetterHead, @PrintHeading, @ModeOfPayment, @IsOpening, @Title, @ErpNextId,
     0, GETDATE())";

                        using (var cmd = new SqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@EntryType", model.EntryType ?? "Journal Entry");
                            cmd.Parameters.AddWithValue("@FromTemplate", (object)model.FromTemplate ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Series", model.Series ?? "ACC-JV-.YYYY.-");
                            cmd.Parameters.AddWithValue("@Company", model.Company ?? "AITS");
                            cmd.Parameters.AddWithValue("@CompanyGstin", (object)model.CompanyGstin ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@PostingDate", model.PostingDate == DateTime.MinValue ? DateTime.Now : model.PostingDate);

                            cmd.Parameters.AddWithValue("@ReferenceNumber", (object)model.ReferenceNumber ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ReferenceDate", (object)model.ReferenceDate ?? DBNull.Value);

                            cmd.Parameters.AddWithValue("@TotalDebit", model.TotalDebit);
                            cmd.Parameters.AddWithValue("@TotalCredit", model.TotalCredit);
                            cmd.Parameters.AddWithValue("@DifferenceAmount", model.DifferenceAmount);

                            cmd.Parameters.AddWithValue("@MultiCurrency", model.MultiCurrency);
                            cmd.Parameters.AddWithValue("@UserRemark", (object)model.UserRemark ?? DBNull.Value);

                            cmd.Parameters.AddWithValue("@BillNo", (object)model.BillNo ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@BillDate", (object)model.BillDate ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@DueDate", (object)model.DueDate ?? DBNull.Value);

                            cmd.Parameters.AddWithValue("@PayToRecdFrom", (object)model.PayToRecdFrom ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@LetterHead", (object)model.LetterHead ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@PrintHeading", (object)model.PrintHeading ?? DBNull.Value);

                            cmd.Parameters.AddWithValue("@ModeOfPayment", (object)model.ModeOfPayment ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@IsOpening", model.IsOpening ?? "No");
                            cmd.Parameters.AddWithValue("@Title", (object)model.Title ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ErpNextId", (object)model.ErpNextId ?? DBNull.Value);

                            newId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        if (accounts != null)
                        {
                            for (int i = 0; i < accounts.Count; i++)
                            {
                                if (string.IsNullOrWhiteSpace(accounts[i]))
                                    continue;

                                string accountQuery = @"
INSERT INTO JournalEntryAccounts
    (JournalEntryId, Account, PartyType, Party, ModeOfPayment, Debit, Credit)
VALUES
    (@JournalEntryId, @Account, @PartyType, @Party, @ModeOfPayment, @Debit, @Credit)";

                                using (var cmd = new SqlCommand(accountQuery, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@JournalEntryId", newId);
                                    cmd.Parameters.AddWithValue("@Account", accounts[i]);

                                    cmd.Parameters.AddWithValue("@PartyType",
                                        partyTypes != null &&
                                        i < partyTypes.Count &&
                                        !string.IsNullOrWhiteSpace(partyTypes[i])
                                            ? (object)partyTypes[i]
                                            : DBNull.Value);

                                    cmd.Parameters.AddWithValue("@Party",
                                        parties != null &&
                                        i < parties.Count &&
                                        !string.IsNullOrWhiteSpace(parties[i])
                                            ? (object)parties[i]
                                            : DBNull.Value);

                                    cmd.Parameters.AddWithValue("@ModeOfPayment",
                                        modeOfPayments != null &&
                                        i < modeOfPayments.Count &&
                                        !string.IsNullOrWhiteSpace(modeOfPayments[i])
                                            ? (object)modeOfPayments[i]
                                            : DBNull.Value);

                                    cmd.Parameters.AddWithValue("@Debit",
                                        debits != null && i < debits.Count
                                            ? debits[i]
                                            : 0);

                                    cmd.Parameters.AddWithValue("@Credit",
                                        credits != null && i < credits.Count
                                            ? credits[i]
                                            : 0);

                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
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

    }
}