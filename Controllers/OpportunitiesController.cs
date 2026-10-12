using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using Erp_Aits.Models;

namespace Erp_Aits.Controllers
{
    public class OpportunitiesController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OpportunitiesController> _logger;

        public OpportunitiesController(IConfiguration configuration, ILogger<OpportunitiesController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("DefaultConnection");
        }

        // ============================================================
        // INDEX - List all opportunities with search/filter
        // ============================================================
        public IActionResult Index(OpportunitySearchModel searchModel)
        {
            try
            {
                ViewData["Title"] = "Opportunities";
                ViewData["Module"] = "CRM";
                ViewData["Icon"] = "rocket";

                var opportunities = GetOpportunities(searchModel);

                var viewModel = new OpportunityViewModel
                {
                    Opportunity = new Opportunity(),
                    SalesStageList = GetSalesStages(),
                    OpportunityTypeList = GetOpportunityTypes(),
                    IndustryList = GetIndustries(),
                    CurrencyList = GetCurrencies(),
                    LanguageList = GetLanguages(),
                    MarketSegmentList = GetMarketSegments(),
                    OpportunityFromList = GetOpportunityFrom(),
                    SourceList = GetSourceList(),
                    CountryList = GetCountryList(),
                    TerritoryList = GetTerritoryList()
                };

                ViewBag.Opportunities = opportunities;
                ViewBag.SearchModel = searchModel;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Index action");
                TempData["Error"] = "Error loading opportunities. Please try again.";
                return View(new OpportunityViewModel());
            }
        }

        // ============================================================
        // DETAILS - View single opportunity
        // ============================================================
        public IActionResult Details(int id)
        {
            try
            {
                ViewData["Title"] = "Opportunity Details";
                ViewData["Module"] = "CRM";
                ViewData["Icon"] = "rocket";

                var opportunity = GetOpportunityById(id);
                if (opportunity == null)
                {
                    TempData["Error"] = "Opportunity not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(opportunity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in Details action for ID: {id}");
                TempData["Error"] = "Error loading opportunity details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ============================================================
        // CREATE - GET (With pre-fill from lead)
        // ============================================================
        // ============================================================
        // CREATE - GET (With pre-fill from lead)
        // ============================================================
        public IActionResult Create()
        {
            try
            {
                ViewData["Title"] = "Create Opportunity";
                ViewData["Module"] = "CRM";
                ViewData["Icon"] = "plus-circle";

                var viewModel = new OpportunityViewModel
                {
                    Opportunity = new Opportunity
                    {
                        OpportunityDate = DateTime.Now,
                        Probability = 50,
                        Currency = "INR",
                        PrintLanguage = "English",
                        SalesStage = "Prospecting"
                    },
                    SalesStageList = GetSalesStages(),
                    OpportunityTypeList = GetOpportunityTypes(),
                    IndustryList = GetIndustries(),
                    CurrencyList = GetCurrencies(),
                    LanguageList = GetLanguages(),
                    MarketSegmentList = GetMarketSegments(),
                    OpportunityFromList = GetOpportunityFrom(),
                    SourceList = GetSourceList(),
                    CountryList = GetCountryList(),
                    TerritoryList = GetTerritoryList()
                };

                // Check if coming from Lead (TempData set by CreateFromLead)
                if (TempData["FromLead"] != null && (bool)TempData["FromLead"])
                {
                    // Pre-fill ALL opportunity fields from lead data
                    viewModel.Opportunity.Company = TempData["PreFillCompany"]?.ToString();
                    viewModel.Opportunity.OpportunityFrom = TempData["PreFillOpportunityFrom"]?.ToString();
                    viewModel.Opportunity.Source = TempData["PreFillSource"]?.ToString();
                    viewModel.Opportunity.Employees = TempData["PreFillEmployees"] != null ? Convert.ToInt32(TempData["PreFillEmployees"]) : (int?)null;
                    viewModel.Opportunity.Industry = TempData["PreFillIndustry"]?.ToString();

                    // Convert string back to decimal
                    var annualRevenueStr = TempData["PreFillAnnualRevenue"]?.ToString();
                    viewModel.Opportunity.AnnualRevenue = !string.IsNullOrEmpty(annualRevenueStr) ? Convert.ToDecimal(annualRevenueStr) : (decimal?)null;

                    viewModel.Opportunity.City = TempData["PreFillCity"]?.ToString();
                    viewModel.Opportunity.State = TempData["PreFillState"]?.ToString();
                    viewModel.Opportunity.Country = TempData["PreFillCountry"]?.ToString();
                    viewModel.Opportunity.Website = TempData["PreFillWebsite"]?.ToString();
                    viewModel.Opportunity.Territory = TempData["PreFillTerritory"]?.ToString();
                    viewModel.Opportunity.PrintLanguage = TempData["PreFillPrintLanguage"]?.ToString() ?? "English";
                    viewModel.Opportunity.LeadId = TempData["LeadId"] != null ? Convert.ToInt32(TempData["LeadId"]) : (int?)null;

                    // Store additional data in ViewBag for the form
                    ViewBag.CustomerName = TempData["PreFillCustomerName"]?.ToString();
                    ViewBag.Email = TempData["PreFillEmail"]?.ToString();
                    ViewBag.Phone = TempData["PreFillPhone"]?.ToString();
                    ViewBag.Description = TempData["PreFillDescription"]?.ToString();
                    ViewBag.FromLead = true;

                    // Keep TempData for the POST action
                    TempData.Keep();
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Create GET action");
                TempData["Error"] = "Error loading create form.";
                return RedirectToAction(nameof(Index));
            }
        }
        // ============================================================
        // CREATE - POST
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(OpportunityViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                    {
                        // Remove CompanyId from INSERT query if you don't need it
                        string query = @"INSERT INTO Opportunities (
                    OpportunityFrom, OpportunityType, SalesStage, ExpectedClosingDate, Probability,
                    Employees, Industry, City, State, Country, AnnualRevenue, MarketSegment,
                    Website, Territory, Currency, Amount, Company, OpportunityDate, PrintLanguage,
                    Source, CreatedBy, LeadId
                ) VALUES (
                    @OpportunityFrom, @OpportunityType, @SalesStage, @ExpectedClosingDate, @Probability,
                    @Employees, @Industry, @City, @State, @Country, @AnnualRevenue, @MarketSegment,
                    @Website, @Territory, @Currency, @Amount, @Company, @OpportunityDate, @PrintLanguage,
                    @Source, @CreatedBy, @LeadId
                );
                SELECT SCOPE_IDENTITY();";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            var opp = viewModel.Opportunity;

                            cmd.Parameters.AddWithValue("@OpportunityFrom", (object?)opp.OpportunityFrom ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@OpportunityType", (object?)opp.OpportunityType ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@SalesStage", (object?)opp.SalesStage ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ExpectedClosingDate", (object?)opp.ExpectedClosingDate ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Probability", (object?)opp.Probability ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Employees", (object?)opp.Employees ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Industry", (object?)opp.Industry ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@City", (object?)opp.City ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@State", (object?)opp.State ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Country", (object?)opp.Country ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@AnnualRevenue", (object?)opp.AnnualRevenue ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@MarketSegment", (object?)opp.MarketSegment ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Website", (object?)opp.Website ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Territory", (object?)opp.Territory ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Currency", (object?)opp.Currency ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Amount", (object?)opp.Amount ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Company", (object?)opp.Company ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@OpportunityDate", (object?)opp.OpportunityDate ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@PrintLanguage", (object?)opp.PrintLanguage ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Source", (object?)opp.Source ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@CreatedBy", GetCurrentUser());
                            cmd.Parameters.AddWithValue("@LeadId", (object?)opp.LeadId ?? DBNull.Value);

                            conn.Open();
                            int newId = Convert.ToInt32(cmd.ExecuteScalar());

                            if (newId > 0)
                            {
                                string message = "Opportunity created successfully!";
                                if (viewModel.Opportunity.LeadId.HasValue)
                                {
                                    message = "Opportunity created from lead successfully!";
                                }
                                TempData["Success"] = message;
                                return RedirectToAction(nameof(Index));
                            }
                            else
                            {
                                ModelState.AddModelError("", "Failed to create opportunity. Please try again.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Create POST action");
                ModelState.AddModelError("", "An error occurred while creating the opportunity: " + ex.Message);
            }

            ReloadDropdowns(viewModel);
            return View(viewModel);
        }
        public IActionResult CreateFromLead(int leadId, string company, string customerName,
                                            string email, string phone, string opportunityFrom,
                                            string source, int? employees, string industry,
                                            decimal? annualRevenue, string city, string state,
                                            string country, string website, string territory,
                                            string printLanguage, string description)
        {
            try
            {
                // Store data in TempData - Convert decimal to string
                TempData["LeadId"] = leadId;
                TempData["PreFillCompany"] = company;
                TempData["PreFillCustomerName"] = customerName;
                TempData["PreFillEmail"] = email;
                TempData["PreFillPhone"] = phone;
                TempData["PreFillOpportunityFrom"] = opportunityFrom;
                TempData["PreFillSource"] = source;
                TempData["PreFillEmployees"] = employees;
                TempData["PreFillIndustry"] = industry;
                TempData["PreFillAnnualRevenue"] = annualRevenue?.ToString(); // Convert to string
                TempData["PreFillCity"] = city;
                TempData["PreFillState"] = state;
                TempData["PreFillCountry"] = country;
                TempData["PreFillWebsite"] = website;
                TempData["PreFillTerritory"] = territory;
                TempData["PreFillPrintLanguage"] = printLanguage;
                TempData["PreFillDescription"] = description;
                TempData["FromLead"] = true;

                return RedirectToAction("Create");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateFromLead action");
                TempData["Error"] = "Error copying lead to opportunity.";
                return RedirectToAction("Index", "Leads");
            }
        }

        // ============================================================
        // EDIT - GET
        // ============================================================
        public IActionResult Edit(int id)
        {
            try
            {
                ViewData["Title"] = "Edit Opportunity";
                ViewData["Module"] = "CRM";
                ViewData["Icon"] = "edit";

                var opportunity = GetOpportunityById(id);
                if (opportunity == null)
                {
                    TempData["Error"] = "Opportunity not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new OpportunityViewModel
                {
                    Opportunity = opportunity,
                    SalesStageList = GetSalesStages(),
                    OpportunityTypeList = GetOpportunityTypes(),
                    IndustryList = GetIndustries(),
                    CurrencyList = GetCurrencies(),
                    LanguageList = GetLanguages(),
                    MarketSegmentList = GetMarketSegments(),
                    OpportunityFromList = GetOpportunityFrom(),
                    SourceList = GetSourceList(),
                    CountryList = GetCountryList(),
                    TerritoryList = GetTerritoryList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in Edit GET action for ID: {id}");
                TempData["Error"] = "Error loading edit form.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ============================================================
        // EDIT - POST (Remove CompanyId from UPDATE)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, OpportunityViewModel viewModel)
        {
            try
            {
                if (id != viewModel.Opportunity.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                    {
                        // Remove CompanyId from UPDATE query
                        string query = @"UPDATE Opportunities SET
                    OpportunityFrom = @OpportunityFrom,
                    OpportunityType = @OpportunityType,
                    SalesStage = @SalesStage,
                    ExpectedClosingDate = @ExpectedClosingDate,
                    Probability = @Probability,
                    Employees = @Employees,
                    Industry = @Industry,
                    City = @City,
                    State = @State,
                    Country = @Country,
                    AnnualRevenue = @AnnualRevenue,
                    MarketSegment = @MarketSegment,
                    Website = @Website,
                    Territory = @Territory,
                    Currency = @Currency,
                    Amount = @Amount,
                    Company = @Company,
                    OpportunityDate = @OpportunityDate,
                    PrintLanguage = @PrintLanguage,
                    Source = @Source,
                    ModifiedDate = GETDATE(),
                    ModifiedBy = @ModifiedBy
                WHERE Id = @Id";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            var opp = viewModel.Opportunity;

                            cmd.Parameters.AddWithValue("@Id", id);
                            cmd.Parameters.AddWithValue("@OpportunityFrom", (object?)opp.OpportunityFrom ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@OpportunityType", (object?)opp.OpportunityType ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@SalesStage", (object?)opp.SalesStage ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ExpectedClosingDate", (object?)opp.ExpectedClosingDate ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Probability", (object?)opp.Probability ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Employees", (object?)opp.Employees ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Industry", (object?)opp.Industry ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@City", (object?)opp.City ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@State", (object?)opp.State ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Country", (object?)opp.Country ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@AnnualRevenue", (object?)opp.AnnualRevenue ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@MarketSegment", (object?)opp.MarketSegment ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Website", (object?)opp.Website ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Territory", (object?)opp.Territory ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Currency", (object?)opp.Currency ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Amount", (object?)opp.Amount ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Company", (object?)opp.Company ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@OpportunityDate", (object?)opp.OpportunityDate ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@PrintLanguage", (object?)opp.PrintLanguage ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Source", (object?)opp.Source ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ModifiedBy", GetCurrentUser());

                            conn.Open();
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                TempData["Success"] = "Opportunity updated successfully!";
                                return RedirectToAction(nameof(Index));
                            }
                            else
                            {
                                ModelState.AddModelError("", "Failed to update opportunity. No changes were made.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in Edit POST action for ID: {id}");
                ModelState.AddModelError("", "An error occurred while updating the opportunity: " + ex.Message);
            }

            ReloadDropdowns(viewModel);
            return View(viewModel);
        }

        // ============================================================
        // DELETE - GET (Confirmation)
        // ============================================================
        public IActionResult Delete(int id)
        {
            try
            {
                ViewData["Title"] = "Delete Opportunity";
                ViewData["Module"] = "CRM";
                ViewData["Icon"] = "trash";

                var opportunity = GetOpportunityById(id);
                if (opportunity == null)
                {
                    TempData["Error"] = "Opportunity not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(opportunity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in Delete GET action for ID: {id}");
                TempData["Error"] = "Error loading delete confirmation.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ============================================================
        // DELETE - POST (Confirm)
        // ============================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = @"UPDATE Opportunities SET 
                                    IsDeleted = 1, 
                                    IsActive = 0,
                                    ModifiedDate = GETDATE(),
                                    ModifiedBy = @ModifiedBy 
                                    WHERE Id = @Id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.Parameters.AddWithValue("@ModifiedBy", GetCurrentUser());

                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            TempData["Success"] = "Opportunity deleted successfully!";
                        }
                        else
                        {
                            TempData["Error"] = "Failed to delete opportunity.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in Delete POST action for ID: {id}");
                TempData["Error"] = "An error occurred while deleting the opportunity.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // BULK DELETE
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BulkDelete(int[] ids)
        {
            try
            {
                if (ids == null || ids.Length == 0)
                {
                    TempData["Error"] = "No opportunities selected for deletion.";
                    return RedirectToAction(nameof(Index));
                }

                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = @"UPDATE Opportunities SET 
                                    IsDeleted = 1, 
                                    IsActive = 0,
                                    ModifiedDate = GETDATE(),
                                    ModifiedBy = @ModifiedBy 
                                    WHERE Id IN (" + string.Join(",", ids) + ")";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ModifiedBy", GetCurrentUser());
                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();

                        TempData["Success"] = $"{rowsAffected} opportunities deleted successfully!";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BulkDelete action");
                TempData["Error"] = "An error occurred while deleting opportunities.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // GET STATS - AJAX for dashboard
        // ============================================================
        [HttpGet]
        public IActionResult GetStats()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = @"
                        SELECT 
                            COUNT(*) as Total,
                            SUM(CASE WHEN SalesStage = 'Prospecting' THEN 1 ELSE 0 END) as Prospecting,
                            SUM(CASE WHEN SalesStage = 'Qualification' THEN 1 ELSE 0 END) as Qualification,
                            SUM(CASE WHEN SalesStage = 'Negotiation' THEN 1 ELSE 0 END) as Negotiation,
                            SUM(CASE WHEN SalesStage = 'Closed Won' THEN 1 ELSE 0 END) as ClosedWon,
                            SUM(CASE WHEN SalesStage = 'Closed Lost' THEN 1 ELSE 0 END) as ClosedLost,
                            SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) as Active
                        FROM Opportunities 
                        WHERE IsDeleted = 0";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return Json(new
                                {
                                    Total = reader["Total"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Total"]),
                                    Prospecting = reader["Prospecting"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Prospecting"]),
                                    Qualification = reader["Qualification"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Qualification"]),
                                    Negotiation = reader["Negotiation"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Negotiation"]),
                                    ClosedWon = reader["ClosedWon"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ClosedWon"]),
                                    ClosedLost = reader["ClosedLost"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ClosedLost"]),
                                    Active = reader["Active"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Active"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetStats");
            }

            return Json(new { Total = 0, Prospecting = 0, Qualification = 0, Negotiation = 0, ClosedWon = 0, ClosedLost = 0, Active = 0 });
        }

        // ============================================================
        // EXPORT TO CSV
        // ============================================================
        public IActionResult Export()
        {
            try
            {
                var opportunities = GetOpportunities(new OpportunitySearchModel());

                var csv = new System.Text.StringBuilder();
                csv.AppendLine("ID,Company,Opportunity From,Type,Stage,Amount,Currency,Probability,Source,Country,Website,Territory,Expected Close Date,Created Date");

                foreach (var item in opportunities)
                {
                    csv.AppendLine($"{item.Id},{item.Company},{item.OpportunityFrom},{item.OpportunityType},{item.SalesStage},{item.Amount},{item.Currency},{item.Probability},{item.Source},{item.Country},{item.Website},{item.Territory},{item.ExpectedClosingDate?.ToString("dd/MM/yyyy")},{item.CreatedDate:dd/MM/yyyy}");
                }

                var fileName = $"Opportunities_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Export action");
                TempData["Error"] = "Error exporting opportunities.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ============================================================
        // HELPER METHODS
        // ============================================================

        private List<Opportunity> GetOpportunities(OpportunitySearchModel search = null)
        {
            var opportunities = new List<Opportunity>();

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT * FROM Opportunities WHERE IsDeleted = 0";

                if (search != null)
                {
                    var conditions = new List<string>();

                    if (!string.IsNullOrEmpty(search.SearchTerm))
                    {
                        conditions.Add("(Company LIKE @SearchTerm OR OpportunityFrom LIKE @SearchTerm OR OpportunityType LIKE @SearchTerm OR Source LIKE @SearchTerm)");
                    }
                    if (!string.IsNullOrEmpty(search.SalesStage))
                    {
                        conditions.Add("SalesStage = @SalesStage");
                    }
                    if (!string.IsNullOrEmpty(search.OpportunityType))
                    {
                        conditions.Add("OpportunityType = @OpportunityType");
                    }
                    if (!string.IsNullOrEmpty(search.Company))
                    {
                        conditions.Add("Company LIKE @Company");
                    }
                    if (!string.IsNullOrEmpty(search.Source))
                    {
                        conditions.Add("Source = @Source");
                    }
                    if (!string.IsNullOrEmpty(search.Territory))
                    {
                        conditions.Add("Territory = @Territory");
                    }
                    if (search.FromDate.HasValue)
                    {
                        conditions.Add("CreatedDate >= @FromDate");
                    }
                    if (search.ToDate.HasValue)
                    {
                        conditions.Add("CreatedDate <= @ToDate");
                    }

                    if (conditions.Any())
                    {
                        query += " AND " + string.Join(" AND ", conditions);
                    }
                }

                query += " ORDER BY CreatedDate DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (search != null)
                    {
                        if (!string.IsNullOrEmpty(search.SearchTerm))
                        {
                            cmd.Parameters.AddWithValue("@SearchTerm", $"%{search.SearchTerm}%");
                        }
                        if (!string.IsNullOrEmpty(search.SalesStage))
                        {
                            cmd.Parameters.AddWithValue("@SalesStage", search.SalesStage);
                        }
                        if (!string.IsNullOrEmpty(search.OpportunityType))
                        {
                            cmd.Parameters.AddWithValue("@OpportunityType", search.OpportunityType);
                        }
                        if (!string.IsNullOrEmpty(search.Company))
                        {
                            cmd.Parameters.AddWithValue("@Company", $"%{search.Company}%");
                        }
                        if (!string.IsNullOrEmpty(search.Source))
                        {
                            cmd.Parameters.AddWithValue("@Source", search.Source);
                        }
                        if (!string.IsNullOrEmpty(search.Territory))
                        {
                            cmd.Parameters.AddWithValue("@Territory", search.Territory);
                        }
                        if (search.FromDate.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@FromDate", search.FromDate.Value);
                        }
                        if (search.ToDate.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@ToDate", search.ToDate.Value);
                        }
                    }

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            opportunities.Add(MapDataReaderToOpportunity(reader));
                        }
                    }
                }
            }

            return opportunities;
        }

        private Opportunity? GetOpportunityById(int id)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT * FROM Opportunities WHERE Id = @Id AND IsDeleted = 0";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapDataReaderToOpportunity(reader);
                        }
                    }
                }
            }
            return null;
        }

        private Opportunity MapDataReaderToOpportunity(SqlDataReader reader)
        {
            return new Opportunity
            {
                Id = Convert.ToInt32(reader["Id"]),
                OpportunityFrom = reader["OpportunityFrom"]?.ToString(),
                OpportunityType = reader["OpportunityType"]?.ToString(),
                SalesStage = reader["SalesStage"]?.ToString(),
                ExpectedClosingDate = reader["ExpectedClosingDate"] != DBNull.Value ? Convert.ToDateTime(reader["ExpectedClosingDate"]) : null,
                Probability = reader["Probability"] != DBNull.Value ? Convert.ToInt32(reader["Probability"]) : null,
                Employees = reader["Employees"] != DBNull.Value ? Convert.ToInt32(reader["Employees"]) : null,
                Industry = reader["Industry"]?.ToString(),
                City = reader["City"]?.ToString(),
                State = reader["State"]?.ToString(),
                Country = reader["Country"]?.ToString(),
                AnnualRevenue = reader["AnnualRevenue"] != DBNull.Value ? Convert.ToDecimal(reader["AnnualRevenue"]) : null,
                MarketSegment = reader["MarketSegment"]?.ToString(),
                Website = reader["Website"]?.ToString(),
                Territory = reader["Territory"]?.ToString(),
                Currency = reader["Currency"]?.ToString(),
                Amount = reader["Amount"] != DBNull.Value ? Convert.ToDecimal(reader["Amount"]) : null,
                Company = reader["Company"]?.ToString(),
                OpportunityDate = reader["OpportunityDate"] != DBNull.Value ? Convert.ToDateTime(reader["OpportunityDate"]) : null,
                PrintLanguage = reader["PrintLanguage"]?.ToString(),
                Source = reader["Source"]?.ToString(),
                CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                CreatedBy = reader["CreatedBy"]?.ToString(),
                ModifiedDate = reader["ModifiedDate"] != DBNull.Value ? Convert.ToDateTime(reader["ModifiedDate"]) : null,
                ModifiedBy = reader["ModifiedBy"]?.ToString(),
                IsActive = Convert.ToBoolean(reader["IsActive"]),
                IsDeleted = Convert.ToBoolean(reader["IsDeleted"]),
                CompanyId = reader["CompanyId"] != DBNull.Value ? Convert.ToInt32(reader["CompanyId"]) : null,
                LeadId = reader["LeadId"] != DBNull.Value ? Convert.ToInt32(reader["LeadId"]) : null
            };
        }

        private string GetCurrentUser()
        {
            return HttpContext.Session.GetString("CompanyName") ?? "System";
        }

        private int? GetCompanyId()
        {
            var companyId = HttpContext.Session.GetString("CompanyId");
            if (!string.IsNullOrEmpty(companyId))
            {
                return Convert.ToInt32(companyId);
            }
            return null;
        }

        private void ReloadDropdowns(OpportunityViewModel viewModel)
        {
            viewModel.SalesStageList = GetSalesStages();
            viewModel.OpportunityTypeList = GetOpportunityTypes();
            viewModel.IndustryList = GetIndustries();
            viewModel.CurrencyList = GetCurrencies();
            viewModel.LanguageList = GetLanguages();
            viewModel.MarketSegmentList = GetMarketSegments();
            viewModel.OpportunityFromList = GetOpportunityFrom();
            viewModel.SourceList = GetSourceList();
            viewModel.CountryList = GetCountryList();
            viewModel.TerritoryList = GetTerritoryList();
        }

        // ============================================================
        // DROPDOWN LISTS
        // ============================================================

        private List<string> GetSalesStages()
        {
            return new List<string> { "", "Prospecting", "Qualification", "Needs Analysis", "Value Proposition", "Negotiation", "Closed Won", "Closed Lost" };
        }

        private List<string> GetOpportunityTypes()
        {
            return new List<string> { "", "New Business", "Existing Customer", "Upgrade", "Renewal", "Other" };
        }

        private List<string> GetIndustries()
        {
            return new List<string> { "", "Technology", "IT Services", "Healthcare", "Finance", "Education",
                "Manufacturing", "Retail", "Real Estate", "Transportation", "Hospitality", "Construction", "Other" };
        }

        private List<string> GetCurrencies()
        {
            return new List<string> { "", "INR", "USD", "EUR", "GBP", "AED", "Other" };
        }

        private List<string> GetLanguages()
        {
            return new List<string> { "", "English", "Hindi", "Spanish", "French", "German", "Chinese", "Arabic", "Other" };
        }

        private List<string> GetMarketSegments()
        {
            return new List<string> { "", "Enterprise", "Mid-Market", "SMB", "Startup", "Government", "Non-Profit", "Other" };
        }

        private List<string> GetOpportunityFrom()
        {
            return new List<string> { "", "Website Lead", "Referral", "LinkedIn", "Google", "Email", "Social Media", "Conference", "Cold Call", "Other" };
        }

        // ============================================================
        // ADD THESE MISSING METHODS
        // ============================================================

        private List<string> GetSourceList()
        {
            return new List<string> { "", "Website", "LinkedIn", "Google", "Referral", "Email", "Social Media", "Conference", "Cold Call", "Other" };
        }

        private List<string> GetCountryList()
        {
            return new List<string> { "", "India", "USA", "UK", "Canada", "Australia", "Germany", "France", "UAE", "Singapore", "China", "Japan", "Brazil", "Other" };
        }

        private List<string> GetTerritoryList()
        {
            return new List<string> { "", "North America", "South America", "Europe", "Asia Pacific", "Middle East", "Africa", "Australia", "Other" };
        }
    }
}