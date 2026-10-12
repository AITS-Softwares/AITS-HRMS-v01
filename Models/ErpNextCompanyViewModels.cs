using System.ComponentModel.DataAnnotations;

namespace Erp_Aits.Models
{
    public class ErpNextCompanyListItem
    {
        public string Name { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string ParentCompany { get; set; } = string.Empty;
    }

    public class ErpNextCompanyListViewModel
    {
        public List<ErpNextCompanyListItem> Companies { get; set; } = new();
        public string Search { get; set; } = string.Empty;
    }

    public class CompanyPrintOptionRow
    {
        public string PrintLabel { get; set; } = string.Empty;
        public string PrintValue { get; set; } = string.Empty;
    }

    public class ErpNextCompanyFormViewModel
    {
        public bool IsEdit { get; set; }
        public string OriginalName { get; set; } = string.Empty;

        [Required]
        public string Company { get; set; } = string.Empty;

        [Required]
        public string Abbr { get; set; } = string.Empty;

        [Required]
        public string DefaultCurrency { get; set; } = string.Empty;

        [Required]
        public string Country { get; set; } = "India";

        public decimal? DefaultGstRate { get; set; } = 18.0m;
        public bool IsGroup { get; set; }
        public string DefaultHolidayList { get; set; } = string.Empty;
        public string DefaultLetterHead { get; set; } = string.Empty;
        public string TaxId { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public DateTime? DateOfEstablishment { get; set; }
        public string ParentCompany { get; set; } = string.Empty;
        public string GstinUin { get; set; } = string.Empty;

        [Required]
        public string GstCategory { get; set; } = "Unregistered";

        public string Pan { get; set; } = string.Empty;
        public string RegistrationDetailsText { get; set; } = string.Empty;

        public string CreateChartOfAccountsBasedOn { get; set; } = string.Empty;
        public string DefaultCustomsDutyExpenseAccount { get; set; } = string.Empty;
        public string DefaultGstExpenseAccount { get; set; } = string.Empty;
        public string DefaultPaymentTermsTemplate { get; set; } = string.Empty;
        public string DefaultFinanceBook { get; set; } = string.Empty;
        public string DefaultCustomsDutyPayableAccount { get; set; } = string.Empty;
        public bool BookAdvancePaymentsInSeparatePartyAccount { get; set; }
        public string ReconciliationTakesEffectOn { get; set; } = "Oldest Of Invoice Or Advance";
        public bool AutoCreateExchangeRateRevaluation { get; set; }
        public string Frequency { get; set; } = "Daily";
        public bool SubmitErrJournals { get; set; }
        public string ExceptionBudgetApproverRole { get; set; } = string.Empty;
        public string SeriesForAssetDepreciationEntry { get; set; } = string.Empty;
        public string ExpensesIncludedInAssetValuation { get; set; } = string.Empty;

        public string DefaultBuyingTerms { get; set; } = string.Empty;
        public decimal? MonthlySalesTarget { get; set; }
        public string DefaultSellingTerms { get; set; } = string.Empty;
        public string DefaultSalesContact { get; set; } = string.Empty;
        public string DefaultWarehouseForSalesReturn { get; set; } = string.Empty;

        public string DefaultEmployeeAdvanceAccount { get; set; } = string.Empty;
        public string BasicComponent { get; set; } = string.Empty;
        public string ArrearComponent { get; set; } = string.Empty;
        public string HraComponent { get; set; } = string.Empty;

        public string DefaultOperatingCostAccount { get; set; } = string.Empty;

        public bool EnablePhysicalSignature { get; set; }
        public List<CompanyPrintOptionRow> BankDetails { get; set; } = new();
        public List<CompanyPrintOptionRow> RegistrationDetails { get; set; } = new();

        public List<string> Companies { get; set; } = new();
        public List<string> Countries { get; set; } = new();
        public List<string> Currencies { get; set; } = new();
        public List<string> LetterHeads { get; set; } = new();
        public List<string> HolidayLists { get; set; } = new();
        public List<string> Accounts { get; set; } = new();
        public List<string> PaymentTermsTemplates { get; set; } = new();
        public List<string> FinanceBooks { get; set; } = new();
        public List<string> Roles { get; set; } = new();
        public List<string> Warehouses { get; set; } = new();
        public List<string> Contacts { get; set; } = new();
        public List<string> SalaryComponents { get; set; } = new();
        public List<string> ChartOfAccountTemplates { get; set; } = new();
        public List<string> TermsAndConditions { get; set; } = new();
        public List<string> PrintLabelOptions { get; set; } = new();
    }
}
