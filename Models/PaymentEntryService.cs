using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class PaymentEntryService
    {
        private readonly string _connectionString;

        public PaymentEntryService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<PaymentEntryListRow> GetAll(
            string idFilter,
            string paymentType,
            string partyType,
            string party,
            string title)
        {
            var list = new List<PaymentEntryListRow>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT
                        Id,
                        ErpNextId,
                        PaymentType,
                        PartyType,
                        Party,
                        PartyName,
                        DocStatus,
                        Status
                    FROM PaymentEntries
                    WHERE IsDeleted = 0";

                if (!string.IsNullOrWhiteSpace(idFilter))
                {
                    query += @"
                        AND
                        (
                            CAST(Id AS VARCHAR) LIKE @IdFilter
                            OR ErpNextId LIKE @IdFilter
                        )";
                }

                if (!string.IsNullOrWhiteSpace(paymentType))
                    query += " AND PaymentType = @PaymentType";

                if (!string.IsNullOrWhiteSpace(partyType))
                    query += " AND PartyType = @PartyType";

                if (!string.IsNullOrWhiteSpace(party))
                    query += " AND Party LIKE @Party";

                if (!string.IsNullOrWhiteSpace(title))
                    query += " AND PartyName LIKE @Title";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrWhiteSpace(idFilter))
                        cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");

                    if (!string.IsNullOrWhiteSpace(paymentType))
                        cmd.Parameters.AddWithValue("@PaymentType", paymentType);

                    if (!string.IsNullOrWhiteSpace(partyType))
                        cmd.Parameters.AddWithValue("@PartyType", partyType);

                    if (!string.IsNullOrWhiteSpace(party))
                        cmd.Parameters.AddWithValue("@Party", "%" + party + "%");

                    if (!string.IsNullOrWhiteSpace(title))
                        cmd.Parameters.AddWithValue("@Title", "%" + title + "%");

                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(
                                new PaymentEntryListRow
                                {
                                    Id = DbInt(reader["Id"]),
                                    ErpNextId = DbString(reader["ErpNextId"]),
                                    PaymentType = DbString(reader["PaymentType"]),
                                    PartyType = DbString(reader["PartyType"]),
                                    Party = DbString(reader["Party"]),
                                    PartyName = DbString(reader["PartyName"]),
                                    DocStatus = DbInt(reader["DocStatus"]),
                                    Status = DbString(reader["Status"])
                                }
                            );
                        }
                    }
                }
            }

            return list;
        }

        public PaymentEntry GetById(int id)
        {
            PaymentEntry model = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand(@"
                    SELECT *
                    FROM PaymentEntries
                    WHERE Id = @Id
                      AND IsDeleted = 0",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model = Map(reader);
                        }
                    }
                }

                if (model != null)
                {
                    LoadTaxes(conn, model);
                }
            }

            return model;
        }

        public PaymentEntry GetByErpNextId(string erpNextId)
        {
            if (string.IsNullOrWhiteSpace(erpNextId))
                return null;

            PaymentEntry model = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand(@"
                    SELECT TOP 1 *
                    FROM PaymentEntries
                    WHERE ErpNextId = @ErpNextId
                      AND IsDeleted = 0",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", erpNextId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model = Map(reader);
                        }
                    }
                }

                if (model != null)
                {
                    LoadTaxes(conn, model);
                }
            }

            return model;
        }

        public HashSet<string> GetExistingErpNextIds()
        {
            var result =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase
                );

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(@"
                SELECT ErpNextId
                FROM PaymentEntries
                WHERE ErpNextId IS NOT NULL
                  AND ErpNextId <> ''
                  AND IsDeleted = 0",
                conn))
            {
                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(
                            reader["ErpNextId"].ToString()
                        );
                    }
                }
            }

            return result;
        }

        public List<PaymentEntry> GetUnsynced()
        {
            var list = new List<PaymentEntry>();
            var ids = new List<int>();

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(@"
                SELECT Id
                FROM PaymentEntries
                WHERE IsDeleted = 0
                  AND
                  (
                      ErpNextId IS NULL
                      OR ErpNextId = ''
                  )
                ORDER BY Id",
                conn))
            {
                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ids.Add(DbInt(reader["Id"]));
                    }
                }
            }

            foreach (int id in ids)
            {
                var item = GetById(id);

                if (item != null)
                    list.Add(item);
            }

            return list;
        }

        public int InsertOrUpdate(PaymentEntry model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        int existingId = 0;

                        if (!string.IsNullOrWhiteSpace(model.ErpNextId))
                        {
                            using (var find = new SqlCommand(@"
                                SELECT TOP 1 Id
                                FROM PaymentEntries
                                WHERE ErpNextId = @ErpNextId",
                                conn,
                                tx))
                            {
                                find.Parameters.AddWithValue("@ErpNextId", model.ErpNextId);

                                object value = find.ExecuteScalar();

                                if (value != null && value != DBNull.Value)
                                {
                                    existingId = Convert.ToInt32(value);
                                }
                            }
                        }

                        int id;

                        if (existingId > 0)
                        {
                            using (var cmd = new SqlCommand(@"
                                UPDATE PaymentEntries
                                SET
                                    ErpNextId = @ErpNextId,
                                    Series = @Series,
                                    PostingDate = @PostingDate,
                                    PaymentType = @PaymentType,
                                    ModeOfPayment = @ModeOfPayment,
                                    Company = @Company,

                                    PartyType = @PartyType,
                                    Party = @Party,
                                    PartyName = @PartyName,
                                    PartyBalance = @PartyBalance,

                                    CompanyBankAccount = @CompanyBankAccount,
                                    PartyBankAccount = @PartyBankAccount,
                                    ContactPerson = @ContactPerson,
                                    ContactEmail = @ContactEmail,

                                    PaidFrom = @PaidFrom,
                                    PaidFromAccountType = @PaidFromAccountType,
                                    PaidFromAccountCurrency = @PaidFromAccountCurrency,
                                    PaidFromAccountBalance = @PaidFromAccountBalance,

                                    PaidTo = @PaidTo,
                                    PaidToAccountType = @PaidToAccountType,
                                    PaidToAccountCurrency = @PaidToAccountCurrency,
                                    PaidToAccountBalance = @PaidToAccountBalance,

                                    PaidAmount = @PaidAmount,
                                    ReceivedAmount = @ReceivedAmount,
                                    SourceExchangeRate = @SourceExchangeRate,
                                    TargetExchangeRate = @TargetExchangeRate,

                                    TotalAllocatedAmount = @TotalAllocatedAmount,
                                    UnallocatedAmount = @UnallocatedAmount,
                                    DifferenceAmount = @DifferenceAmount,

                                    SalesTaxesChargesTemplate = @SalesTaxTemplate,
                                    PurchaseTaxesChargesTemplate = @PurchaseTaxTemplate,

                                    CompanyAddress = @CompanyAddress,
                                    CompanyGstin = @CompanyGstin,
                                    PlaceOfSupply = @PlaceOfSupply,
                                    CustomerAddress = @CustomerAddress,
                                    BillingAddressGstin = @BillingAddressGstin,
                                    GstCategory = @GstCategory,

                                    ReferenceNo = @ReferenceNo,
                                    ReferenceDate = @ReferenceDate,

                                    Project = @Project,
                                    CostCenter = @CostCenter,

                                    DocStatus = @DocStatus,
                                    Status = @Status,

                                    IsDeleted = 0
                                WHERE Id = @Id",
                                conn,
                                tx))
                            {
                                AddParameters(cmd, model);
                                cmd.Parameters.AddWithValue("@Id", existingId);
                                cmd.ExecuteNonQuery();
                            }

                            id = existingId;

                            using (var delete = new SqlCommand(@"
                                DELETE FROM PaymentEntryTaxes
                                WHERE PaymentEntryId = @Id",
                                conn,
                                tx))
                            {
                                delete.Parameters.AddWithValue("@Id", id);
                                delete.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            using (var cmd = new SqlCommand(@"
                                INSERT INTO PaymentEntries
                                (
                                    ErpNextId,
                                    Series,
                                    PostingDate,
                                    PaymentType,
                                    ModeOfPayment,
                                    Company,

                                    PartyType,
                                    Party,
                                    PartyName,
                                    PartyBalance,

                                    CompanyBankAccount,
                                    PartyBankAccount,
                                    ContactPerson,
                                    ContactEmail,

                                    PaidFrom,
                                    PaidFromAccountType,
                                    PaidFromAccountCurrency,
                                    PaidFromAccountBalance,

                                    PaidTo,
                                    PaidToAccountType,
                                    PaidToAccountCurrency,
                                    PaidToAccountBalance,

                                    PaidAmount,
                                    ReceivedAmount,
                                    SourceExchangeRate,
                                    TargetExchangeRate,

                                    TotalAllocatedAmount,
                                    UnallocatedAmount,
                                    DifferenceAmount,

                                    SalesTaxesChargesTemplate,
                                    PurchaseTaxesChargesTemplate,

                                    CompanyAddress,
                                    CompanyGstin,
                                    PlaceOfSupply,
                                    CustomerAddress,
                                    BillingAddressGstin,
                                    GstCategory,

                                    ReferenceNo,
                                    ReferenceDate,

                                    Project,
                                    CostCenter,

                                    DocStatus,
                                    Status,

                                    IsDeleted,
                                    CreatedOn
                                )
                                OUTPUT INSERTED.Id
                                VALUES
                                (
                                    @ErpNextId,
                                    @Series,
                                    @PostingDate,
                                    @PaymentType,
                                    @ModeOfPayment,
                                    @Company,

                                    @PartyType,
                                    @Party,
                                    @PartyName,
                                    @PartyBalance,

                                    @CompanyBankAccount,
                                    @PartyBankAccount,
                                    @ContactPerson,
                                    @ContactEmail,

                                    @PaidFrom,
                                    @PaidFromAccountType,
                                    @PaidFromAccountCurrency,
                                    @PaidFromAccountBalance,

                                    @PaidTo,
                                    @PaidToAccountType,
                                    @PaidToAccountCurrency,
                                    @PaidToAccountBalance,

                                    @PaidAmount,
                                    @ReceivedAmount,
                                    @SourceExchangeRate,
                                    @TargetExchangeRate,

                                    @TotalAllocatedAmount,
                                    @UnallocatedAmount,
                                    @DifferenceAmount,

                                    @SalesTaxTemplate,
                                    @PurchaseTaxTemplate,

                                    @CompanyAddress,
                                    @CompanyGstin,
                                    @PlaceOfSupply,
                                    @CustomerAddress,
                                    @BillingAddressGstin,
                                    @GstCategory,

                                    @ReferenceNo,
                                    @ReferenceDate,

                                    @Project,
                                    @CostCenter,

                                    @DocStatus,
                                    @Status,

                                    0,
                                    GETDATE()
                                )",
                                conn,
                                tx))
                            {
                                AddParameters(cmd, model);
                                id = Convert.ToInt32(cmd.ExecuteScalar());
                            }
                        }

                        if (model.Taxes != null)
                        {
                            foreach (var tax in model.Taxes)
                            {
                                if (string.IsNullOrWhiteSpace(tax.AccountHead))
                                    continue;

                                using (var cmd = new SqlCommand(@"
                                    INSERT INTO PaymentEntryTaxes
                                    (
                                        PaymentEntryId,
                                        Type,
                                        AccountHead,
                                        TaxRate,
                                        Amount,
                                        Total
                                    )
                                    VALUES
                                    (
                                        @PaymentEntryId,
                                        @Type,
                                        @AccountHead,
                                        @TaxRate,
                                        @Amount,
                                        @Total
                                    )",
                                    conn,
                                    tx))
                                {
                                    cmd.Parameters.AddWithValue("@PaymentEntryId", id);
                                    cmd.Parameters.AddWithValue("@Type", Db(tax.Type));
                                    cmd.Parameters.AddWithValue("@AccountHead", Db(tax.AccountHead));

                                    cmd.Parameters.AddWithValue(
                                        "@TaxRate",
                                        tax.TaxRate.HasValue
                                            ? (object)tax.TaxRate.Value
                                            : DBNull.Value
                                    );

                                    cmd.Parameters.AddWithValue(
                                        "@Amount",
                                        tax.Amount.HasValue
                                            ? (object)tax.Amount.Value
                                            : DBNull.Value
                                    );

                                    cmd.Parameters.AddWithValue(
                                        "@Total",
                                        tax.Total.HasValue
                                            ? (object)tax.Total.Value
                                            : DBNull.Value
                                    );

                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        tx.Commit();

                        return id;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public int Insert(PaymentEntry model)
        {
            return InsertOrUpdate(model);
        }

        public void UpdateErpNextId(int id, string erpNextId)
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(@"
                UPDATE PaymentEntries
                SET ErpNextId = @ErpNextId
                WHERE Id = @Id",
                conn))
            {
                cmd.Parameters.AddWithValue("@ErpNextId", Db(erpNextId));
                cmd.Parameters.AddWithValue("@Id", id);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void LoadTaxes(SqlConnection conn, PaymentEntry model)
        {
            model.Taxes = new List<PaymentEntryTax>();

            using (var cmd = new SqlCommand(@"
                SELECT
                    Id,
                    PaymentEntryId,
                    Type,
                    AccountHead,
                    TaxRate,
                    Amount,
                    Total
                FROM PaymentEntryTaxes
                WHERE PaymentEntryId = @Id
                ORDER BY Id",
                conn))
            {
                cmd.Parameters.AddWithValue("@Id", model.Id);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        model.Taxes.Add(
                            new PaymentEntryTax
                            {
                                Id = DbInt(reader["Id"]),
                                PaymentEntryId = model.Id,
                                Type = DbString(reader["Type"]),
                                AccountHead = DbString(reader["AccountHead"]),

                                TaxRate =
                                    reader["TaxRate"] == DBNull.Value
                                        ? (decimal?)null
                                        : Convert.ToDecimal(reader["TaxRate"]),

                                Amount =
                                    reader["Amount"] == DBNull.Value
                                        ? (decimal?)null
                                        : Convert.ToDecimal(reader["Amount"]),

                                Total =
                                    reader["Total"] == DBNull.Value
                                        ? (decimal?)null
                                        : Convert.ToDecimal(reader["Total"])
                            }
                        );
                    }
                }
            }
        }

        private static void AddParameters(SqlCommand cmd, PaymentEntry model)
        {
            cmd.Parameters.AddWithValue("@ErpNextId", Db(model.ErpNextId));
            cmd.Parameters.AddWithValue("@Series", Db(model.Series));
            cmd.Parameters.AddWithValue("@PostingDate", model.PostingDate);
            cmd.Parameters.AddWithValue("@PaymentType", Db(model.PaymentType));
            cmd.Parameters.AddWithValue("@ModeOfPayment", Db(model.ModeOfPayment));
            cmd.Parameters.AddWithValue("@Company", Db(model.Company));

            cmd.Parameters.AddWithValue("@PartyType", Db(model.PartyType));
            cmd.Parameters.AddWithValue("@Party", Db(model.Party));
            cmd.Parameters.AddWithValue("@PartyName", Db(model.PartyName));
            cmd.Parameters.AddWithValue("@PartyBalance", model.PartyBalance);

            cmd.Parameters.AddWithValue("@CompanyBankAccount", Db(model.CompanyBankAccount));
            cmd.Parameters.AddWithValue("@PartyBankAccount", Db(model.PartyBankAccount));
            cmd.Parameters.AddWithValue("@ContactPerson", Db(model.ContactPerson));
            cmd.Parameters.AddWithValue("@ContactEmail", Db(model.ContactEmail));

            cmd.Parameters.AddWithValue("@PaidFrom", Db(model.PaidFrom));
            cmd.Parameters.AddWithValue("@PaidFromAccountType", Db(model.PaidFromAccountType));
            cmd.Parameters.AddWithValue("@PaidFromAccountCurrency", Db(model.PaidFromAccountCurrency));
            cmd.Parameters.AddWithValue("@PaidFromAccountBalance", model.PaidFromAccountBalance);

            cmd.Parameters.AddWithValue("@PaidTo", Db(model.PaidTo));
            cmd.Parameters.AddWithValue("@PaidToAccountType", Db(model.PaidToAccountType));
            cmd.Parameters.AddWithValue("@PaidToAccountCurrency", Db(model.PaidToAccountCurrency));
            cmd.Parameters.AddWithValue("@PaidToAccountBalance", model.PaidToAccountBalance);

            cmd.Parameters.AddWithValue("@PaidAmount", model.PaidAmount);
            cmd.Parameters.AddWithValue("@ReceivedAmount", model.ReceivedAmount);
            cmd.Parameters.AddWithValue("@SourceExchangeRate", model.SourceExchangeRate <= 0 ? 1 : model.SourceExchangeRate);
            cmd.Parameters.AddWithValue("@TargetExchangeRate", model.TargetExchangeRate <= 0 ? 1 : model.TargetExchangeRate);

            cmd.Parameters.AddWithValue("@TotalAllocatedAmount", model.TotalAllocatedAmount);
            cmd.Parameters.AddWithValue("@UnallocatedAmount", model.UnallocatedAmount);
            cmd.Parameters.AddWithValue("@DifferenceAmount", model.DifferenceAmount);

            cmd.Parameters.AddWithValue("@SalesTaxTemplate", Db(model.SalesTaxesChargesTemplate));
            cmd.Parameters.AddWithValue("@PurchaseTaxTemplate", Db(model.PurchaseTaxesChargesTemplate));

            cmd.Parameters.AddWithValue("@CompanyAddress", Db(model.CompanyAddress));
            cmd.Parameters.AddWithValue("@CompanyGstin", Db(model.CompanyGstin));
            cmd.Parameters.AddWithValue("@PlaceOfSupply", Db(model.PlaceOfSupply));
            cmd.Parameters.AddWithValue("@CustomerAddress", Db(model.CustomerAddress));
            cmd.Parameters.AddWithValue("@BillingAddressGstin", Db(model.BillingAddressGstin));
            cmd.Parameters.AddWithValue("@GstCategory", Db(model.GstCategory));

            cmd.Parameters.AddWithValue("@ReferenceNo", Db(model.ReferenceNo));

            cmd.Parameters.AddWithValue(
                "@ReferenceDate",
                model.ReferenceDate.HasValue
                    ? (object)model.ReferenceDate.Value
                    : DBNull.Value
            );

            cmd.Parameters.AddWithValue("@Project", Db(model.Project));
            cmd.Parameters.AddWithValue("@CostCenter", Db(model.CostCenter));

            cmd.Parameters.AddWithValue("@DocStatus", model.DocStatus);
            cmd.Parameters.AddWithValue("@Status", Db(model.Status));
        }

        private PaymentEntry Map(SqlDataReader reader)
        {
            return new PaymentEntry
            {
                Id = DbInt(reader["Id"]),
                ErpNextId = DbString(reader["ErpNextId"]),
                Series = DbString(reader["Series"]),

                PostingDate =
                    reader["PostingDate"] == DBNull.Value
                        ? DateTime.Today
                        : Convert.ToDateTime(reader["PostingDate"]),

                PaymentType = DbString(reader["PaymentType"]),
                ModeOfPayment = DbString(reader["ModeOfPayment"]),
                Company = DbString(reader["Company"]),

                PartyType = DbString(reader["PartyType"]),
                Party = DbString(reader["Party"]),
                PartyName = DbString(reader["PartyName"]),
                PartyBalance = DbDecimal(reader["PartyBalance"]),

                CompanyBankAccount = DbString(reader["CompanyBankAccount"]),
                PartyBankAccount = DbString(reader["PartyBankAccount"]),
                ContactPerson = DbString(reader["ContactPerson"]),
                ContactEmail = DbString(reader["ContactEmail"]),

                PaidFrom = DbString(reader["PaidFrom"]),
                PaidFromAccountType = DbString(reader["PaidFromAccountType"]),
                PaidFromAccountCurrency = DbString(reader["PaidFromAccountCurrency"]),
                PaidFromAccountBalance = DbDecimal(reader["PaidFromAccountBalance"]),

                PaidTo = DbString(reader["PaidTo"]),
                PaidToAccountType = DbString(reader["PaidToAccountType"]),
                PaidToAccountCurrency = DbString(reader["PaidToAccountCurrency"]),
                PaidToAccountBalance = DbDecimal(reader["PaidToAccountBalance"]),

                PaidAmount = DbDecimal(reader["PaidAmount"]),
                ReceivedAmount = DbDecimal(reader["ReceivedAmount"]),
                SourceExchangeRate = DbDecimal(reader["SourceExchangeRate"]),
                TargetExchangeRate = DbDecimal(reader["TargetExchangeRate"]),

                TotalAllocatedAmount = DbDecimal(reader["TotalAllocatedAmount"]),
                UnallocatedAmount = DbDecimal(reader["UnallocatedAmount"]),
                DifferenceAmount = DbDecimal(reader["DifferenceAmount"]),

                SalesTaxesChargesTemplate = DbString(reader["SalesTaxesChargesTemplate"]),
                PurchaseTaxesChargesTemplate = DbString(reader["PurchaseTaxesChargesTemplate"]),

                CompanyAddress = DbString(reader["CompanyAddress"]),
                CompanyGstin = DbString(reader["CompanyGstin"]),
                PlaceOfSupply = DbString(reader["PlaceOfSupply"]),
                CustomerAddress = DbString(reader["CustomerAddress"]),
                BillingAddressGstin = DbString(reader["BillingAddressGstin"]),
                GstCategory = DbString(reader["GstCategory"]),

                ReferenceNo = DbString(reader["ReferenceNo"]),

                ReferenceDate =
                    reader["ReferenceDate"] == DBNull.Value
                        ? (DateTime?)null
                        : Convert.ToDateTime(reader["ReferenceDate"]),

                Project = DbString(reader["Project"]),
                CostCenter = DbString(reader["CostCenter"]),

                DocStatus = DbInt(reader["DocStatus"]),
                Status = DbString(reader["Status"]),

                Taxes = new List<PaymentEntryTax>()
            };
        }

        private static object Db(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? DBNull.Value
                : value;
        }

        private static string DbString(object value)
        {
            return value == null || value == DBNull.Value
                ? ""
                : value.ToString();
        }

        private static int DbInt(object value)
        {
            return value == null || value == DBNull.Value
                ? 0
                : Convert.ToInt32(value);
        }

        private static decimal DbDecimal(object value)
        {
            return value == null || value == DBNull.Value
                ? 0
                : Convert.ToDecimal(value);
        }
    }
}