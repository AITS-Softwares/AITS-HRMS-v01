using System;
using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class PaymentEntry
    {
        public int Id { get; set; }

        public string ErpNextId { get; set; }

        public string Series { get; set; } = "ACC-PAY-.YYYY.-";

        public DateTime PostingDate { get; set; } = DateTime.Today;

        public string PaymentType { get; set; } = "Receive";

        public string Company { get; set; }
            = "Vinod Cookware India Private Limited";

        public string ModeOfPayment { get; set; }

        public string PartyType { get; set; }

        public string Party { get; set; }

        public string PartyName { get; set; }

        public decimal PartyBalance { get; set; }

        public string CompanyBankAccount { get; set; }

        public string PartyBankAccount { get; set; }

        public string ContactPerson { get; set; }

        public string ContactEmail { get; set; }

        public string PaidFrom { get; set; }

        public string PaidFromAccountType { get; set; }

        public string PaidFromAccountCurrency { get; set; }

        public decimal PaidFromAccountBalance { get; set; }

        public string PaidTo { get; set; }

        public string PaidToAccountType { get; set; }

        public string PaidToAccountCurrency { get; set; }

        public decimal PaidToAccountBalance { get; set; }

        public string AccountPaidTo
        {
            get => PaidTo;
            set => PaidTo = value;
        }

        public decimal PaidAmount { get; set; }

        public decimal ReceivedAmount { get; set; }

        public decimal SourceExchangeRate { get; set; } = 1;

        public decimal TargetExchangeRate { get; set; } = 1;

        public decimal TotalAllocatedAmount { get; set; }

        public decimal UnallocatedAmount { get; set; }

        public decimal DifferenceAmount { get; set; }

        //public decimal TotalAllocatedAmount { get; set; }

        //public decimal UnallocatedAmount { get; set; }

        //public decimal DifferenceAmount { get; set; }

        public decimal TotalTaxesAndCharges { get; set; }

        public string SalesTaxesChargesTemplate { get; set; }

        public string PurchaseTaxesChargesTemplate { get; set; }

        public string CompanyAddress { get; set; }

        public string CompanyGstin { get; set; }

        public string PlaceOfSupply { get; set; }

        public string CustomerAddress { get; set; }

        public string BillingAddressGstin { get; set; }

        public string GstCategory { get; set; }

        public string ReferenceNo { get; set; }

        public DateTime? ReferenceDate { get; set; }

        public string Project { get; set; }

        public string CostCenter { get; set; }

        public string LetterHead { get; set; }

        public string PrintHeading { get; set; }

        public bool CustomRemarks { get; set; }

        public string Remarks { get; set; }

        public int DocStatus { get; set; }

        public string Status { get; set; } = "Draft";

        public List<PaymentEntryTax> Taxes { get; set; }
            = new List<PaymentEntryTax>();

        public List<PaymentEntryReference> References { get; set; }
            = new List<PaymentEntryReference>();

        public List<PaymentEntryDeduction> Deductions { get; set; }
            = new List<PaymentEntryDeduction>();
    }

    public class PaymentEntryTax
    {
        public int Id { get; set; }

        public int PaymentEntryId { get; set; }

        public string Type { get; set; }

        public string AccountHead { get; set; }

        public decimal? TaxRate { get; set; }

        public decimal? Amount { get; set; }

        public decimal? Total { get; set; }

        public string AddDeductTax { get; set; } = "Add";
        public int? RowId { get; set; }
        public bool IncludedInPaidAmount { get; set; }
    }

    public class PaymentEntryReference
    {
        public int Id { get; set; }

        public int PaymentEntryId { get; set; }

        public string ReferenceDoctype { get; set; }

        public string ReferenceName { get; set; }

        public decimal GrandTotal { get; set; }

        public decimal OutstandingAmount { get; set; }

        public decimal AllocatedAmount { get; set; }

        public string PaymentTerm { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? ExchangeRate { get; set; }
    }

    public class PaymentEntryDeduction
    {
        public int Id { get; set; }

        public int PaymentEntryId { get; set; }

        public string Account { get; set; }

        public string CostCenter { get; set; }

        public decimal Amount { get; set; }
    }

    public class PaymentEntryListRow
    {
        public int Id { get; set; }

        public string ErpNextId { get; set; }

        public string PaymentType { get; set; }

        public string PartyType { get; set; }

        public string Party { get; set; }

        public string PartyName { get; set; }

        public int DocStatus { get; set; }

        public string Status { get; set; }


        public decimal PaidFromAccountBalance { get; set; }
        public decimal PaidToAccountBalance { get; set; }

        public decimal TotalAllocatedAmount { get; set; }
        public decimal UnallocatedAmount { get; set; }
        public decimal DifferenceAmount { get; set; }
        public decimal TotalTaxesAndCharges { get; set; }

        public string? LetterHead { get; set; }
        public string? PrintHeading { get; set; }
        public bool CustomRemarks { get; set; }
        public string? Remarks { get; set; }
        public string? InWords { get; set; }
        public string? BaseInWords { get; set; }
    }
}
