using Erp_Aits.Infrastructure.ErpNext;
using Erp_Aits.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Erp_Aits.Controllers
{
    [Route("HR")]
    public class CompanyController : Controller
    {
        private readonly IErpNextClient _erpNextClient;
        private readonly ILogger<CompanyController> _logger;

        public CompanyController(IErpNextClient erpNextClient, ILogger<CompanyController> logger)
        {
            _erpNextClient = erpNextClient;
            _logger = logger;
        }

        [HttpGet("Company")]
        [HttpGet("Companies")]
        public async Task<IActionResult> Company(string? search, CancellationToken cancellationToken)
        {
            var model = new ErpNextCompanyListViewModel
            {
                Search = search ?? string.Empty
            };

            try
            {
                string json = await _erpNextClient.GetListAsync(
                    "Company",
                    new[] { "name", "company_name", "country", "parent_company" },
                    500,
                    cancellationToken);

                var rows = JObject.Parse(json)["data"] as JArray ?? new JArray();
                model.Companies = rows
                    .Select(row => new ErpNextCompanyListItem
                    {
                        Name = ReadString(row, "name"),
                        CompanyName = ReadString(row, "company_name"),
                        Country = ReadString(row, "country"),
                        ParentCompany = ReadString(row, "parent_company")
                    })
                    .Where(row => string.IsNullOrWhiteSpace(model.Search) ||
                                  row.Name.Contains(model.Search, StringComparison.OrdinalIgnoreCase) ||
                                  row.CompanyName.Contains(model.Search, StringComparison.OrdinalIgnoreCase) ||
                                  row.Country.Contains(model.Search, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(row => row.Name)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Company list fetch failed from ERPNext.");
                TempData["CompanyError"] = "Company data could not be fetched from ERPNext. " + ex.Message;
            }

            return View("~/Views/HR/Company.cshtml", model);
        }

        [HttpGet("CreateCompany")]
        public async Task<IActionResult> CreateCompany(CancellationToken cancellationToken)
        {
            var model = new ErpNextCompanyFormViewModel();
            await LoadDropdownsAsync(model, cancellationToken);
            return View("~/Views/HR/CreateCompany.cshtml", model);
        }

        [HttpGet("CompanyDetails")]
        public async Task<IActionResult> CompanyDetails(string name, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["CompanyError"] = "Please select a company first.";
                return RedirectToAction(nameof(Company));
            }

            var model = new ErpNextCompanyFormViewModel
            {
                IsEdit = true,
                OriginalName = name
            };

            try
            {
                string json = await _erpNextClient.GetDocumentAsync("Company", name, cancellationToken);
                var data = JObject.Parse(json)["data"];
                if (data != null)
                {
                    FillModelFromErpNext(model, data);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Company detail fetch failed from ERPNext for {Company}.", name);
                TempData["CompanyError"] = "Company details could not be fetched from ERPNext. " + ex.Message;
                return RedirectToAction(nameof(Company));
            }

            await LoadDropdownsAsync(model, cancellationToken);
            return View("~/Views/HR/CreateCompany.cshtml", model);
        }

        [HttpPost("CreateCompany")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCompany(ErpNextCompanyFormViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(model, cancellationToken);
                return View("~/Views/HR/CreateCompany.cshtml", model);
            }

            try
            {
                await _erpNextClient.CreateDocumentAsync("Company", BuildPayload(model), cancellationToken);
                TempData["CompanySuccess"] = "Company created successfully in ERPNext.";
                return RedirectToAction(nameof(CompanyDetails), new { name = model.Company });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Company create failed in ERPNext.");
                TempData["CompanyError"] = "ERPNext rejected the company save. " + ex.Message;
                await LoadDropdownsAsync(model, cancellationToken);
                return View("~/Views/HR/CreateCompany.cshtml", model);
            }
        }

        [HttpPost("UpdateCompany")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCompany(ErpNextCompanyFormViewModel model, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(model.OriginalName))
            {
                TempData["CompanyError"] = "Company name is missing for update.";
                return RedirectToAction(nameof(Company));
            }

            if (!ModelState.IsValid)
            {
                model.IsEdit = true;
                await LoadDropdownsAsync(model, cancellationToken);
                return View("~/Views/HR/CreateCompany.cshtml", model);
            }

            try
            {
                await _erpNextClient.UpdateDocumentAsync("Company", model.OriginalName, BuildPayload(model), cancellationToken);
                TempData["CompanySuccess"] = "Company updated successfully in ERPNext.";
                return RedirectToAction(nameof(CompanyDetails), new { name = string.IsNullOrWhiteSpace(model.Company) ? model.OriginalName : model.Company });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Company update failed in ERPNext for {Company}.", model.OriginalName);
                TempData["CompanyError"] = "ERPNext rejected the company update. " + ex.Message;
                model.IsEdit = true;
                await LoadDropdownsAsync(model, cancellationToken);
                return View("~/Views/HR/CreateCompany.cshtml", model);
            }
        }

        [HttpPost("DeleteCompany")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCompany(string name, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["CompanyError"] = "Company name is missing for delete.";
                return RedirectToAction(nameof(Company));
            }

            try
            {
                await _erpNextClient.DeleteDocumentAsync("Company", name, cancellationToken);
                TempData["CompanySuccess"] = "Company deleted successfully from ERPNext.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Company delete failed in ERPNext for {Company}.", name);
                TempData["CompanyError"] = "ERPNext did not allow delete. This usually happens when the company is used in other records. " + ex.Message;
            }

            return RedirectToAction(nameof(Company));
        }

        private async Task LoadDropdownsAsync(ErpNextCompanyFormViewModel model, CancellationToken cancellationToken)
        {
            model.Companies = await GetNameOptionsAsync("Company", cancellationToken);
            model.Countries = await GetNameOptionsAsync("Country", cancellationToken);
            model.Currencies = await GetNameOptionsAsync("Currency", cancellationToken);
            model.LetterHeads = await GetNameOptionsAsync("Letter Head", cancellationToken);
            model.HolidayLists = await GetNameOptionsAsync("Holiday List", cancellationToken);
            model.Accounts = await GetNameOptionsAsync("Account", cancellationToken);
            model.PaymentTermsTemplates = await GetNameOptionsAsync("Payment Terms Template", cancellationToken);
            model.FinanceBooks = await GetNameOptionsAsync("Finance Book", cancellationToken);
            model.Roles = await GetNameOptionsAsync("Role", cancellationToken);
            model.Warehouses = await GetNameOptionsAsync("Warehouse", cancellationToken);
            model.Contacts = await GetNameOptionsAsync("Contact", cancellationToken);
            model.SalaryComponents = await GetNameOptionsAsync("Salary Component", cancellationToken);
            model.ChartOfAccountTemplates = new List<string> { "Standard Template", "Existing Company" };
            model.TermsAndConditions = await GetNameOptionsAsync("Terms and Conditions", cancellationToken);
            model.PrintLabelOptions = await GetPrintLabelOptionsAsync(cancellationToken);
        }

        private async Task<List<string>> GetNameOptionsAsync(string doctype, CancellationToken cancellationToken)
        {
            try
            {
                string json = await _erpNextClient.GetListAsync(doctype, new[] { "name" }, 500, cancellationToken);
                var data = JObject.Parse(json)["data"] as JArray ?? new JArray();
                return data
                    .Select(row => ReadString(row, "name"))
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(value => value)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Dropdown fetch failed for ERPNext doctype {DocType}.", doctype);
                return new List<string>();
            }
        }

        private static Dictionary<string, object?> BuildPayload(ErpNextCompanyFormViewModel model)
        {
            var payload = new Dictionary<string, object?>
            {
                ["company_name"] = model.Company,
                ["abbr"] = model.Abbr,
                ["default_currency"] = model.DefaultCurrency,
                ["country"] = model.Country,
                ["is_group"] = model.IsGroup ? 1 : 0,
                ["default_holiday_list"] = EmptyToNull(model.DefaultHolidayList),
                ["default_letter_head"] = EmptyToNull(model.DefaultLetterHead),
                ["tax_id"] = EmptyToNull(model.TaxId),
                ["domain"] = EmptyToNull(model.Domain),
                ["date_of_establishment"] = model.DateOfEstablishment?.ToString("yyyy-MM-dd"),
                ["parent_company"] = EmptyToNull(model.ParentCompany),
                ["gstin"] = EmptyToNull(model.GstinUin),
                ["gst_category"] = EmptyToNull(model.GstCategory),
                ["pan"] = EmptyToNull(model.Pan),
                ["default_gst_rate"] = model.DefaultGstRate,
                ["registration_details"] = EmptyToNull(model.RegistrationDetailsText),
                ["chart_of_accounts"] = EmptyToNull(model.CreateChartOfAccountsBasedOn),
                ["default_customs_duty_expense_account"] = EmptyToNull(model.DefaultCustomsDutyExpenseAccount),
                ["default_gst_expense_account"] = EmptyToNull(model.DefaultGstExpenseAccount),
                ["default_payment_terms_template"] = EmptyToNull(model.DefaultPaymentTermsTemplate),
                ["default_finance_book"] = EmptyToNull(model.DefaultFinanceBook),
                ["default_customs_duty_payable_account"] = EmptyToNull(model.DefaultCustomsDutyPayableAccount),
                ["book_advance_payments_in_separate_party_account"] = model.BookAdvancePaymentsInSeparatePartyAccount ? 1 : 0,
                ["reconciliation_takes_effect_on"] = EmptyToNull(model.ReconciliationTakesEffectOn),
                ["auto_create_exchange_rate_revaluation"] = model.AutoCreateExchangeRateRevaluation ? 1 : 0,
                ["frequency"] = EmptyToNull(model.Frequency),
                ["submit_err_journals"] = model.SubmitErrJournals ? 1 : 0,
                ["exception_budget_approver_role"] = EmptyToNull(model.ExceptionBudgetApproverRole),
                ["series_for_depreciation_entry"] = EmptyToNull(model.SeriesForAssetDepreciationEntry),
                ["expenses_included_in_asset_valuation"] = EmptyToNull(model.ExpensesIncludedInAssetValuation),
                ["default_buying_terms"] = EmptyToNull(model.DefaultBuyingTerms),
                ["monthly_sales_target"] = model.MonthlySalesTarget,
                ["default_selling_terms"] = EmptyToNull(model.DefaultSellingTerms),
                ["default_sales_contact"] = EmptyToNull(model.DefaultSalesContact),
                ["default_warehouse_for_sales_return"] = EmptyToNull(model.DefaultWarehouseForSalesReturn),
                ["default_employee_advance_account"] = EmptyToNull(model.DefaultEmployeeAdvanceAccount),
                ["basic_component"] = EmptyToNull(model.BasicComponent),
                ["arrear_component"] = EmptyToNull(model.ArrearComponent),
                ["hra_component"] = EmptyToNull(model.HraComponent),
                ["default_operating_cost_account"] = EmptyToNull(model.DefaultOperatingCostAccount),
                ["enable_physical_signature"] = model.EnablePhysicalSignature ? 1 : 0
            };

            var bankRows = BuildPrintRows(model.BankDetails);
            if (bankRows.Count > 0)
            {
                payload["bank_details"] = bankRows;
            }

            var registrationRows = BuildPrintRows(model.RegistrationDetails);
            if (registrationRows.Count > 0)
            {
                payload["registration_details_table"] = registrationRows;
            }

            return payload
                .Where(item => item.Value != null)
                .ToDictionary(item => item.Key, item => item.Value);
        }

        private static List<Dictionary<string, string>> BuildPrintRows(IEnumerable<CompanyPrintOptionRow>? rows)
        {
            return rows?
                .Where(row => !string.IsNullOrWhiteSpace(row.PrintLabel) || !string.IsNullOrWhiteSpace(row.PrintValue))
                .Select(row => new Dictionary<string, string>
                {
                    ["print_label"] = row.PrintLabel ?? string.Empty,
                    ["print_value"] = row.PrintValue ?? string.Empty
                })
                .ToList() ?? new List<Dictionary<string, string>>();
        }

        private static void FillModelFromErpNext(ErpNextCompanyFormViewModel model, JToken data)
        {
            model.Company = ReadString(data, "company_name", "name");
            model.Abbr = ReadString(data, "abbr");
            model.DefaultCurrency = ReadString(data, "default_currency");
            model.Country = ReadString(data, "country");
            model.DefaultGstRate = ReadDecimal(data, "default_gst_rate");
            model.IsGroup = ReadBool(data, "is_group");
            model.DefaultHolidayList = ReadString(data, "default_holiday_list");
            model.DefaultLetterHead = ReadString(data, "default_letter_head");
            model.TaxId = ReadString(data, "tax_id");
            model.Domain = ReadString(data, "domain");
            model.DateOfEstablishment = ReadDate(data, "date_of_establishment");
            model.ParentCompany = ReadString(data, "parent_company");
            model.GstinUin = ReadString(data, "gstin");
            model.GstCategory = ReadString(data, "gst_category");
            model.Pan = ReadString(data, "pan");
            model.RegistrationDetailsText = ReadString(data, "registration_details");
            model.CreateChartOfAccountsBasedOn = ReadString(data, "chart_of_accounts");
            model.DefaultCustomsDutyExpenseAccount = ReadString(data, "default_customs_duty_expense_account");
            model.DefaultGstExpenseAccount = ReadString(data, "default_gst_expense_account");
            model.DefaultPaymentTermsTemplate = ReadString(data, "default_payment_terms_template");
            model.DefaultFinanceBook = ReadString(data, "default_finance_book");
            model.DefaultCustomsDutyPayableAccount = ReadString(data, "default_customs_duty_payable_account");
            model.BookAdvancePaymentsInSeparatePartyAccount = ReadBool(data, "book_advance_payments_in_separate_party_account");
            model.ReconciliationTakesEffectOn = ReadString(data, "reconciliation_takes_effect_on");
            model.AutoCreateExchangeRateRevaluation = ReadBool(data, "auto_create_exchange_rate_revaluation");
            model.Frequency = ReadString(data, "frequency");
            model.SubmitErrJournals = ReadBool(data, "submit_err_journals");
            model.ExceptionBudgetApproverRole = ReadString(data, "exception_budget_approver_role");
            model.SeriesForAssetDepreciationEntry = ReadString(data, "series_for_depreciation_entry");
            model.ExpensesIncludedInAssetValuation = ReadString(data, "expenses_included_in_asset_valuation");
            model.DefaultBuyingTerms = ReadString(data, "default_buying_terms");
            model.MonthlySalesTarget = ReadDecimal(data, "monthly_sales_target");
            model.DefaultSellingTerms = ReadString(data, "default_selling_terms");
            model.DefaultSalesContact = ReadString(data, "default_sales_contact");
            model.DefaultWarehouseForSalesReturn = ReadString(data, "default_warehouse_for_sales_return");
            model.DefaultEmployeeAdvanceAccount = ReadString(data, "default_employee_advance_account");
            model.BasicComponent = ReadString(data, "basic_component");
            model.ArrearComponent = ReadString(data, "arrear_component");
            model.HraComponent = ReadString(data, "hra_component");
            model.DefaultOperatingCostAccount = ReadString(data, "default_operating_cost_account");
            model.EnablePhysicalSignature = ReadBool(data, "enable_physical_signature");
            model.BankDetails = ReadPrintRows(data, "bank_details");
            model.RegistrationDetails = ReadPrintRows(data, "registration_details_table");
        }

        private async Task<List<string>> GetPrintLabelOptionsAsync(CancellationToken cancellationToken)
        {
            var options = new List<string>();

            options.AddRange(await GetNameOptionsAsync("Print Heading", cancellationToken));
            options.AddRange(new[]
            {
                "Bank Name",
                "Account No",
                "Account Number",
                "IFSC Code",
                "Branch",
                "GSTIN / UIN",
                "PAN",
                "CIN",
                "Address",
                "Phone",
                "Email"
            });

            return options
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value)
                .ToList();
        }

        private static List<CompanyPrintOptionRow> ReadPrintRows(JToken data, string propertyName)
        {
            var rows = data[propertyName] as JArray;
            if (rows == null)
            {
                return new List<CompanyPrintOptionRow>();
            }

            return rows.Select(row => new CompanyPrintOptionRow
            {
                PrintLabel = ReadString(row, "print_label"),
                PrintValue = ReadString(row, "print_value")
            }).ToList();
        }

        private static string ReadString(JToken? token, params string[] names)
        {
            foreach (string name in names)
            {
                string? value = token?[name]?.ToString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return string.Empty;
        }

        private static bool ReadBool(JToken? token, string name)
        {
            string value = ReadString(token, name);
            return value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        private static decimal? ReadDecimal(JToken? token, string name)
        {
            return decimal.TryParse(ReadString(token, name), out decimal value) ? value : null;
        }

        private static DateTime? ReadDate(JToken? token, string name)
        {
            return DateTime.TryParse(ReadString(token, name), out DateTime value) ? value : null;
        }

        private static string? EmptyToNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
