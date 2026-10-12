//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Data.SqlClient;
//using System.Data;
//using Erp_Aits.Models;

//namespace Erp_Aits.Controllers
//{
//    public class LeadsController : Controller
//    {
//        private readonly IConfiguration _configuration;
//        private readonly ILogger<LeadsController> _logger;

//        public LeadsController(IConfiguration configuration, ILogger<LeadsController> logger)
//        {
//            _configuration = configuration;
//            _logger = logger;
//        }

//        private string GetConnectionString()
//        {
//            return _configuration.GetConnectionString("DefaultConnection");
//        }

//        // ============================================================
//        // INDEX - List all leads with search/filter
//        // ============================================================
//        public IActionResult Index(LeadSearchModel searchModel)
//        {
//            try
//            {
//                ViewData["Title"] = "Leads";
//                ViewData["Module"] = "CRM";
//                ViewData["Icon"] = "users";

//                var leads = GetLeads(searchModel);

//                var viewModel = new LeadViewModel
//                {
//                    Lead = new Lead(),
//                    SalutationList = GetSalutations(),
//                    GenderList = GetGenders(),
//                    StatusList = GetStatuses(),
//                    LeadTypeList = GetLeadTypes(),
//                    SourceList = GetSources(),
//                    IndustryList = GetIndustries(),
//                    QualificationStatusList = GetQualificationStatuses(),
//                    RequestTypeList = GetRequestTypes(),
//                    CountryList = GetCountryList(),
//                    TerritoryList = GetTerritoryList(),
//                    PrintLanguageList = GetPrintLanguageList()
//                };

//                ViewBag.Leads = leads;
//                ViewBag.SearchModel = searchModel;

//                return View(viewModel);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error in Index action");
//                TempData["Error"] = "Error loading leads. Please try again.";
//                return View(new LeadViewModel());
//            }
//        }

//        // ============================================================
//        // DETAILS - View single lead
//        // ============================================================
//        public IActionResult Details(int id)
//        {
//            try
//            {
//                ViewData["Title"] = "Lead Details";
//                ViewData["Module"] = "CRM";
//                ViewData["Icon"] = "user";

//                var lead = GetLeadById(id);
//                if (lead == null)
//                {
//                    TempData["Error"] = "Lead not found.";
//                    return RedirectToAction(nameof(Index));
//                }

//                return View(lead);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error in Details action for ID: {id}");
//                TempData["Error"] = "Error loading lead details.";
//                return RedirectToAction(nameof(Index));
//            }
//        }

//        // ============================================================
//        // CREATE - GET
//        // ============================================================
//        public IActionResult Create()
//        {
//            try
//            {
//                ViewData["Title"] = "Create Lead";
//                ViewData["Module"] = "CRM";
//                ViewData["Icon"] = "plus-circle";

//                var viewModel = new LeadViewModel
//                {
//                    Lead = new Lead
//                    {
//                        Status = "New",
//                        IsActive = true,
//                        CreatedDate = DateTime.Now
//                    },
//                    SalutationList = GetSalutations(),
//                    GenderList = GetGenders(),
//                    StatusList = GetStatuses(),
//                    LeadTypeList = GetLeadTypes(),
//                    SourceList = GetSources(),
//                    IndustryList = GetIndustries(),
//                    QualificationStatusList = GetQualificationStatuses(),
//                    RequestTypeList = GetRequestTypes(),
//                    CountryList = GetCountryList(),
//                    TerritoryList = GetTerritoryList(),
//                    PrintLanguageList = GetPrintLanguageList()
//                };

//                return View(viewModel);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error in Create GET action");
//                TempData["Error"] = "Error loading create form.";
//                return RedirectToAction(nameof(Index));
//            }
//        }

//        // ============================================================
//        // CREATE - POST
//        // ============================================================
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public IActionResult Create(LeadViewModel viewModel)
//        {
//            try
//            {
//                if (ModelState.IsValid)
//                {
//                    using (SqlConnection conn = new SqlConnection(GetConnectionString()))
//                    {
//                        string query = @"INSERT INTO Leads (
//                            Salutation, FirstName, MiddleName, LastName, JobTitle, Gender, LeadOwner,
//                            Email, MobileNo, Phone, PhoneExt, WhatsApp, Website, Fax,
//                            OrganizationName, Industry, AnnualRevenue, Employees, MarketSegment, Territory,
//                            City, State, County, Country, PrintLanguage, Source, Status, LeadType, RequestType, 
//                            QualificationStatus, QualifiedBy, QualifiedOn,
//                            CreatedBy, CompanyId, IsActive
//                        ) VALUES (
//                            @Salutation, @FirstName, @MiddleName, @LastName, @JobTitle, @Gender, @LeadOwner,
//                            @Email, @MobileNo, @Phone, @PhoneExt, @WhatsApp, @Website, @Fax,
//                            @OrganizationName, @Industry, @AnnualRevenue, @Employees, @MarketSegment, @Territory,
//                            @City, @State, @County, @Country, @PrintLanguage, @Source, @Status, @LeadType, @RequestType,
//                            @QualificationStatus, @QualifiedBy, @QualifiedOn,
//                            @CreatedBy, @CompanyId, @IsActive
//                        );
//                        SELECT SCOPE_IDENTITY();";

//                        using (SqlCommand cmd = new SqlCommand(query, conn))
//                        {
//                            var lead = viewModel.Lead;
//                            AddLeadParameters(cmd, lead, false);

//                            conn.Open();
//                            int newId = Convert.ToInt32(cmd.ExecuteScalar());

//                            if (newId > 0)
//                            {
//                                TempData["Success"] = "Lead created successfully!";
//                                return RedirectToAction(nameof(Index));
//                            }
//                            else
//                            {
//                                ModelState.AddModelError("", "Failed to create lead. Please try again.");
//                            }
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error in Create POST action");
//                ModelState.AddModelError("", "An error occurred while creating the lead.");
//            }

//            ReloadDropdowns(viewModel);
//            return View(viewModel);
//        }

//        // ============================================================
//        // COPY TO OPPORTUNITY - Redirect to Opportunity Create with ALL lead data
//        // ============================================================
//        public IActionResult CopyToOpportunity(int id)
//        {
//            try
//            {
//                var lead = GetLeadById(id);
//                if (lead == null)
//                {
//                    TempData["Error"] = "Lead not found.";
//                    return RedirectToAction(nameof(Index));
//                }

//                // Redirect to Opportunities CreateFromLead with ALL lead data
//                return RedirectToAction("CreateFromLead", "Opportunities", new
//                {
//                    leadId = lead.Id,
//                    company = lead.OrganizationName ?? "",
//                    customerName = lead.FullName ?? "",
//                    email = lead.Email ?? "",
//                    phone = lead.MobileNo ?? "",
//                    opportunityFrom = lead.Source ?? "",
//                    source = lead.Source ?? "",
//                    employees = lead.Employees,
//                    industry = lead.Industry ?? "",
//                    annualRevenue = lead.AnnualRevenue,
//                    city = lead.City ?? "",
//                    state = lead.State ?? "",
//                    country = lead.Country ?? "",
//                    website = lead.Website ?? "",
//                    territory = lead.Territory ?? "",
//                    printLanguage = lead.PrintLanguage ?? "English",
//                    description = $"Lead: {lead.FullName} - {lead.OrganizationName}"
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error in CopyToOpportunity action for ID: {id}");
//                TempData["Error"] = "Error copying lead to opportunity.";
//                return RedirectToAction(nameof(Index));
//            }
//        }

//        // ============================================================
//        // EDIT - GET
//        // ============================================================
//        public IActionResult Edit(int id)
//        {
//            try
//            {
//                ViewData["Title"] = "Edit Lead";
//                ViewData["Module"] = "CRM";
//                ViewData["Icon"] = "edit";

//                var lead = GetLeadById(id);
//                if (lead == null)
//                {
//                    TempData["Error"] = "Lead not found.";
//                    return RedirectToAction(nameof(Index));
//                }

//                var viewModel = new LeadViewModel
//                {
//                    Lead = lead,
//                    SalutationList = GetSalutations(),
//                    GenderList = GetGenders(),
//                    StatusList = GetStatuses(),
//                    LeadTypeList = GetLeadTypes(),
//                    SourceList = GetSources(),
//                    IndustryList = GetIndustries(),
//                    QualificationStatusList = GetQualificationStatuses(),
//                    RequestTypeList = GetRequestTypes(),
//                    CountryList = GetCountryList(),
//                    TerritoryList = GetTerritoryList(),
//                    PrintLanguageList = GetPrintLanguageList()
//                };

//                return View(viewModel);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error in Edit GET action for ID: {id}");
//                TempData["Error"] = "Error loading edit form.";
//                return RedirectToAction(nameof(Index));
//            }
//        }

//        // ============================================================
//        // EDIT - POST
//        // ============================================================
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public IActionResult Edit(int id, LeadViewModel viewModel)
//        {
//            try
//            {
//                if (id != viewModel.Lead.Id)
//                {
//                    return NotFound();
//                }

//                if (ModelState.IsValid)
//                {
//                    using (SqlConnection conn = new SqlConnection(GetConnectionString()))
//                    {
//                        string query = @"UPDATE Leads SET
//                            Salutation = @Salutation,
//                            FirstName = @FirstName,
//                            MiddleName = @MiddleName,
//                            LastName = @LastName,
//                            JobTitle = @JobTitle,
//                            Gender = @Gender,
//                            LeadOwner = @LeadOwner,
//                            Email = @Email,
//                            MobileNo = @MobileNo,
//                            Phone = @Phone,
//                            PhoneExt = @PhoneExt,
//                            WhatsApp = @WhatsApp,
//                            Website = @Website,
//                            Fax = @Fax,
//                            OrganizationName = @OrganizationName,
//                            Industry = @Industry,
//                            AnnualRevenue = @AnnualRevenue,
//                            Employees = @Employees,
//                            MarketSegment = @MarketSegment,
//                            Territory = @Territory,
//                            City = @City,
//                            State = @State,
//                            County = @County,
//                            Country = @Country,
//                            PrintLanguage = @PrintLanguage,
//                            Source = @Source,
//                            Status = @Status,
//                            LeadType = @LeadType,
//                            RequestType = @RequestType,
//                            QualificationStatus = @QualificationStatus,
//                            QualifiedBy = @QualifiedBy,
//                            QualifiedOn = @QualifiedOn,
//                            IsActive = @IsActive,
//                            ModifiedDate = GETDATE(),
//                            ModifiedBy = @ModifiedBy
//                        WHERE Id = @Id";

//                        using (SqlCommand cmd = new SqlCommand(query, conn))
//                        {
//                            var lead = viewModel.Lead;
//                            AddLeadParameters(cmd, lead, true);
//                            cmd.Parameters.AddWithValue("@Id", id);
//                            cmd.Parameters.AddWithValue("@ModifiedBy", GetCurrentUser());

//                            conn.Open();
//                            int rowsAffected = cmd.ExecuteNonQuery();

//                            if (rowsAffected > 0)
//                            {
//                                TempData["Success"] = "Lead updated successfully!";
//                                return RedirectToAction(nameof(Index));
//                            }
//                            else
//                            {
//                                ModelState.AddModelError("", "Failed to update lead. No changes were made.");
//                            }
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error in Edit POST action for ID: {id}");
//                ModelState.AddModelError("", "An error occurred while updating the lead.");
//            }

//            ReloadDropdowns(viewModel);
//            return View(viewModel);
//        }

//        // ============================================================
//        // DELETE - GET (Confirmation)
//        // ============================================================
//        public IActionResult Delete(int id)
//        {
//            try
//            {
//                ViewData["Title"] = "Delete Lead";
//                ViewData["Module"] = "CRM";
//                ViewData["Icon"] = "trash";

//                var lead = GetLeadById(id);
//                if (lead == null)
//                {
//                    TempData["Error"] = "Lead not found.";
//                    return RedirectToAction(nameof(Index));
//                }

//                return View(lead);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error in Delete GET action for ID: {id}");
//                TempData["Error"] = "Error loading delete confirmation.";
//                return RedirectToAction(nameof(Index));
//            }
//        }

//        // ============================================================
//        // DELETE - POST (Confirm)
//        // ============================================================
//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public IActionResult DeleteConfirmed(int id)
//        {
//            try
//            {
//                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
//                {
//                    string query = @"UPDATE Leads SET 
//                                    IsDeleted = 1, 
//                                    IsActive = 0,
//                                    ModifiedDate = GETDATE(),
//                                    ModifiedBy = @ModifiedBy 
//                                    WHERE Id = @Id";

//                    using (SqlCommand cmd = new SqlCommand(query, conn))
//                    {
//                        cmd.Parameters.AddWithValue("@Id", id);
//                        cmd.Parameters.AddWithValue("@ModifiedBy", GetCurrentUser());

//                        conn.Open();
//                        int rowsAffected = cmd.ExecuteNonQuery();

//                        if (rowsAffected > 0)
//                        {
//                            TempData["Success"] = "Lead deleted successfully!";
//                        }
//                        else
//                        {
//                            TempData["Error"] = "Failed to delete lead.";
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error in Delete POST action for ID: {id}");
//                TempData["Error"] = "An error occurred while deleting the lead.";
//            }

//            return RedirectToAction(nameof(Index));
//        }

//        // ============================================================
//        // GET STATS - AJAX for dashboard
//        // ============================================================
//        [HttpGet]
//        public IActionResult GetStats()
//        {
//            try
//            {
//                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
//                {
//                    string query = @"
//                        SELECT 
//                            COUNT(*) as Total,
//                            SUM(CASE WHEN Status = 'New' THEN 1 ELSE 0 END) as New,
//                            SUM(CASE WHEN Status = 'Qualified' THEN 1 ELSE 0 END) as Qualified,
//                            SUM(CASE WHEN Status = 'Converted' THEN 1 ELSE 0 END) as Converted,
//                            SUM(CASE WHEN Status = 'Lost' THEN 1 ELSE 0 END) as Lost,
//                            SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) as Active
//                        FROM Leads 
//                        WHERE IsDeleted = 0";

//                    using (SqlCommand cmd = new SqlCommand(query, conn))
//                    {
//                        conn.Open();
//                        using (SqlDataReader reader = cmd.ExecuteReader())
//                        {
//                            if (reader.Read())
//                            {
//                                return Json(new
//                                {
//                                    TotalLeads = reader["Total"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Total"]),
//                                    NewLeads = reader["New"] == DBNull.Value ? 0 : Convert.ToInt32(reader["New"]),
//                                    QualifiedLeads = reader["Qualified"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Qualified"]),
//                                    ConvertedLeads = reader["Converted"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Converted"]),
//                                    LostLeads = reader["Lost"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Lost"]),
//                                    ActiveLeads = reader["Active"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Active"])
//                                });
//                            }
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error in GetStats action");
//            }

//            return Json(new { TotalLeads = 0, NewLeads = 0, QualifiedLeads = 0, ConvertedLeads = 0, LostLeads = 0, ActiveLeads = 0 });
//        }

//        // ============================================================
//        // HELPER METHODS
//        // ============================================================

//        private List<Lead> GetLeads(LeadSearchModel search = null)
//        {
//            var leads = new List<Lead>();

//            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
//            {
//                string query = "SELECT * FROM Leads WHERE IsDeleted = 0";

//                if (search != null)
//                {
//                    var conditions = new List<string>();

//                    if (!string.IsNullOrEmpty(search.SearchTerm))
//                    {
//                        conditions.Add("(FirstName LIKE @SearchTerm OR LastName LIKE @SearchTerm OR Email LIKE @SearchTerm OR MobileNo LIKE @SearchTerm OR OrganizationName LIKE @SearchTerm)");
//                    }
//                    if (!string.IsNullOrEmpty(search.Status))
//                    {
//                        conditions.Add("Status = @Status");
//                    }
//                    if (!string.IsNullOrEmpty(search.Source))
//                    {
//                        conditions.Add("Source = @Source");
//                    }
//                    if (!string.IsNullOrEmpty(search.LeadType))
//                    {
//                        conditions.Add("LeadType = @LeadType");
//                    }

//                    if (conditions.Any())
//                    {
//                        query += " AND " + string.Join(" AND ", conditions);
//                    }
//                }

//                query += " ORDER BY CreatedDate DESC";

//                using (SqlCommand cmd = new SqlCommand(query, conn))
//                {
//                    if (search != null)
//                    {
//                        if (!string.IsNullOrEmpty(search.SearchTerm))
//                        {
//                            cmd.Parameters.AddWithValue("@SearchTerm", $"%{search.SearchTerm}%");
//                        }
//                        if (!string.IsNullOrEmpty(search.Status))
//                        {
//                            cmd.Parameters.AddWithValue("@Status", search.Status);
//                        }
//                        if (!string.IsNullOrEmpty(search.Source))
//                        {
//                            cmd.Parameters.AddWithValue("@Source", search.Source);
//                        }
//                        if (!string.IsNullOrEmpty(search.LeadType))
//                        {
//                            cmd.Parameters.AddWithValue("@LeadType", search.LeadType);
//                        }
//                    }

//                    conn.Open();
//                    using (SqlDataReader reader = cmd.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            leads.Add(MapDataReaderToLead(reader));
//                        }
//                    }
//                }
//            }

//            return leads;
//        }

//        private Lead? GetLeadById(int id)
//        {
//            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
//            {
//                string query = "SELECT * FROM Leads WHERE Id = @Id AND IsDeleted = 0";
//                using (SqlCommand cmd = new SqlCommand(query, conn))
//                {
//                    cmd.Parameters.AddWithValue("@Id", id);
//                    conn.Open();

//                    using (SqlDataReader reader = cmd.ExecuteReader())
//                    {
//                        if (reader.Read())
//                        {
//                            return MapDataReaderToLead(reader);
//                        }
//                    }
//                }
//            }
//            return null;
//        }

//        private Lead MapDataReaderToLead(SqlDataReader reader)
//        {
//            return new Lead
//            {
//                Id = Convert.ToInt32(reader["Id"]),
//                Salutation = reader["Salutation"]?.ToString(),
//                FirstName = reader["FirstName"]?.ToString() ?? string.Empty,
//                MiddleName = reader["MiddleName"]?.ToString(),
//                LastName = reader["LastName"]?.ToString(),
//                JobTitle = reader["JobTitle"]?.ToString(),
//                Gender = reader["Gender"]?.ToString(),
//                LeadOwner = reader["LeadOwner"]?.ToString(),
//                Email = reader["Email"]?.ToString() ?? string.Empty,
//                MobileNo = reader["MobileNo"]?.ToString() ?? string.Empty,
//                Phone = reader["Phone"]?.ToString(),
//                PhoneExt = reader["PhoneExt"]?.ToString(),
//                WhatsApp = reader["WhatsApp"]?.ToString(),
//                Website = reader["Website"]?.ToString(),
//                Fax = reader["Fax"]?.ToString(),
//                OrganizationName = reader["OrganizationName"]?.ToString(),
//                Industry = reader["Industry"]?.ToString(),
//                AnnualRevenue = reader["AnnualRevenue"] != DBNull.Value ? Convert.ToDecimal(reader["AnnualRevenue"]) : null,
//                Employees = reader["Employees"] != DBNull.Value ? Convert.ToInt32(reader["Employees"]) : null,
//                MarketSegment = reader["MarketSegment"]?.ToString(),
//                Territory = reader["Territory"]?.ToString(),
//                City = reader["City"]?.ToString(),
//                State = reader["State"]?.ToString(),
//                County = reader["County"]?.ToString(),
//                Country = reader["Country"]?.ToString(),
//                PrintLanguage = reader["PrintLanguage"]?.ToString(),
//                Source = reader["Source"]?.ToString(),
//                Status = reader["Status"]?.ToString(),
//                LeadType = reader["LeadType"]?.ToString(),
//                RequestType = reader["RequestType"]?.ToString(),
//                QualificationStatus = reader["QualificationStatus"]?.ToString(),
//                QualifiedBy = reader["QualifiedBy"]?.ToString(),
//                QualifiedOn = reader["QualifiedOn"] != DBNull.Value ? Convert.ToDateTime(reader["QualifiedOn"]) : null,
//                CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
//                CreatedBy = reader["CreatedBy"]?.ToString(),
//                ModifiedDate = reader["ModifiedDate"] != DBNull.Value ? Convert.ToDateTime(reader["ModifiedDate"]) : null,
//                ModifiedBy = reader["ModifiedBy"]?.ToString(),
//                IsActive = Convert.ToBoolean(reader["IsActive"]),
//                IsDeleted = Convert.ToBoolean(reader["IsDeleted"]),
//                CompanyId = reader["CompanyId"] != DBNull.Value ? Convert.ToInt32(reader["CompanyId"]) : null
//            };
//        }

//        private void AddLeadParameters(SqlCommand cmd, Lead lead, bool includeId = false)
//        {
//            cmd.Parameters.AddWithValue("@Salutation", (object?)lead.Salutation ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@FirstName", lead.FirstName);
//            cmd.Parameters.AddWithValue("@MiddleName", (object?)lead.MiddleName ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@LastName", (object?)lead.LastName ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@JobTitle", (object?)lead.JobTitle ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@Gender", (object?)lead.Gender ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@LeadOwner", (object?)lead.LeadOwner ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@Email", lead.Email);
//            cmd.Parameters.AddWithValue("@MobileNo", lead.MobileNo);
//            cmd.Parameters.AddWithValue("@Phone", (object?)lead.Phone ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@PhoneExt", (object?)lead.PhoneExt ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@WhatsApp", (object?)lead.WhatsApp ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@Website", (object?)lead.Website ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@Fax", (object?)lead.Fax ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@OrganizationName", (object?)lead.OrganizationName ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@Industry", (object?)lead.Industry ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@AnnualRevenue", (object?)lead.AnnualRevenue ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@Employees", (object?)lead.Employees ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@MarketSegment", (object?)lead.MarketSegment ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@Territory", (object?)lead.Territory ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@City", (object?)lead.City ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@State", (object?)lead.State ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@County", (object?)lead.County ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@Country", (object?)lead.Country ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@PrintLanguage", (object?)lead.PrintLanguage ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@Source", (object?)lead.Source ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@Status", (object?)lead.Status ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@LeadType", (object?)lead.LeadType ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@RequestType", (object?)lead.RequestType ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@QualificationStatus", (object?)lead.QualificationStatus ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@QualifiedBy", (object?)lead.QualifiedBy ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@QualifiedOn", (object?)lead.QualifiedOn ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@IsActive", lead.IsActive);

//            if (!includeId)
//            {
//                cmd.Parameters.AddWithValue("@CreatedBy", GetCurrentUser());
//                cmd.Parameters.AddWithValue("@CompanyId", GetCompanyId());
//            }
//        }

//        private string GetCurrentUser()
//        {
//            return HttpContext.Session.GetString("CompanyName") ?? "System";
//        }

//        private int? GetCompanyId()
//        {
//            var companyId = HttpContext.Session.GetString("CompanyId");
//            if (!string.IsNullOrEmpty(companyId))
//            {
//                return Convert.ToInt32(companyId);
//            }
//            return null;
//        }

//        private void ReloadDropdowns(LeadViewModel viewModel)
//        {
//            viewModel.SalutationList = GetSalutations();
//            viewModel.GenderList = GetGenders();
//            viewModel.StatusList = GetStatuses();
//            viewModel.LeadTypeList = GetLeadTypes();
//            viewModel.SourceList = GetSources();
//            viewModel.IndustryList = GetIndustries();
//            viewModel.QualificationStatusList = GetQualificationStatuses();
//            viewModel.RequestTypeList = GetRequestTypes();
//            viewModel.CountryList = GetCountryList();
//            viewModel.TerritoryList = GetTerritoryList();
//            viewModel.PrintLanguageList = GetPrintLanguageList();
//        }

//        // ============================================================
//        // DROPDOWN LISTS
//        // ============================================================

//        private List<string> GetSalutations()
//        {
//            return new List<string> { "", "Mr.", "Ms.", "Mrs.", "Dr.", "Prof.", "Sir", "Madam" };
//        }

//        private List<string> GetGenders()
//        {
//            return new List<string> { "", "Male", "Female", "Other", "Prefer Not to Say" };
//        }

//        private List<string> GetStatuses()
//        {
//            return new List<string> { "", "New", "Open", "Contacted", "Interested", "Qualified", "Converted", "Lost", "Disqualified" };
//        }

//        private List<string> GetLeadTypes()
//        {
//            return new List<string> { "", "Client", "Channel Partner", "Consultant", "Reseller", "Distributor", "Other" };
//        }

//        private List<string> GetSources()
//        {
//            return new List<string> { "", "Website", "LinkedIn", "Google", "Referral", "Email", "Social Media", "Conference", "Cold Call", "Other" };
//        }

//        private List<string> GetIndustries()
//        {
//            return new List<string> { "", "Technology", "IT Services", "Healthcare", "Finance", "Education",
//                "Manufacturing", "Retail", "Real Estate", "Transportation", "Hospitality", "Construction", "Other" };
//        }

//        private List<string> GetQualificationStatuses()
//        {
//            return new List<string> { "", "Pending", "In Progress", "Qualified", "Disqualified", "Rejected" };
//        }

//        private List<string> GetRequestTypes()
//        {
//            return new List<string> { "", "Consultation", "Demo", "Quote", "Support", "Complaint", "Feedback", "Other" };
//        }

//        private List<string> GetCountryList()
//        {
//            return new List<string> { "", "India", "USA", "UK", "Canada", "Australia", "Germany", "France", "UAE", "Singapore", "China", "Japan", "Brazil", "Other" };
//        }

//        private List<string> GetTerritoryList()
//        {
//            return new List<string> { "", "North America", "South America", "Europe", "Asia Pacific", "Middle East", "Africa", "Australia", "Other" };
//        }

//        private List<string> GetPrintLanguageList()
//        {
//            return new List<string> { "", "English", "Hindi", "Spanish", "French", "German", "Chinese", "Arabic", "Other" };
//        }
//    }
//}

using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using Erp_Aits.Models;

namespace Erp_Aits.Controllers
{
    public class LeadsController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<LeadsController> _logger;

        public LeadsController(IConfiguration configuration, ILogger<LeadsController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("DefaultConnection");
        }

        // ============================================================
        // INDEX - List all leads with search/filter
        // ============================================================
        public IActionResult Index(LeadSearchModel searchModel)
        {
            try
            {
                ViewData["Title"] = "Leads";
                ViewData["Module"] = "CRM";
                ViewData["Icon"] = "users";

                var leads = GetLeads(searchModel);

                var viewModel = new LeadViewModel
                {
                    Lead = new Lead(),
                    SalutationList = GetSalutations(),
                    GenderList = GetGenders(),
                    StatusList = GetStatuses(),
                    LeadTypeList = GetLeadTypes(),
                    SourceList = GetSources(),
                    IndustryList = GetIndustries(),
                    QualificationStatusList = GetQualificationStatuses(),
                    RequestTypeList = GetRequestTypes(),
                    CountryList = GetCountryList(),
                    TerritoryList = GetTerritoryList(),
                    AreaList = GetAreaList()  // Changed from GetPrintLanguageList
                };

                ViewBag.Leads = leads;
                ViewBag.SearchModel = searchModel;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Index action");
                TempData["Error"] = "Error loading leads. Please try again.";
                return View(new LeadViewModel());
            }
        }

        // ============================================================
        // DETAILS - View single lead
        // ============================================================
        public IActionResult Details(int id)
        {
            try
            {
                ViewData["Title"] = "Lead Details";
                ViewData["Module"] = "CRM";
                ViewData["Icon"] = "user";

                var lead = GetLeadById(id);
                if (lead == null)
                {
                    TempData["Error"] = "Lead not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(lead);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in Details action for ID: {id}");
                TempData["Error"] = "Error loading lead details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ============================================================
        // CREATE - GET
        // ============================================================
        public IActionResult Create()
        {
            try
            {
                ViewData["Title"] = "Create Lead";
                ViewData["Module"] = "CRM";
                ViewData["Icon"] = "plus-circle";

                var viewModel = new LeadViewModel
                {
                    Lead = new Lead
                    {
                        Status = "New",
                        IsActive = true,
                        CreatedDate = DateTime.Now
                    },
                    SalutationList = GetSalutations(),
                    GenderList = GetGenders(),
                    StatusList = GetStatuses(),
                    LeadTypeList = GetLeadTypes(),
                    SourceList = GetSources(),
                    IndustryList = GetIndustries(),
                    QualificationStatusList = GetQualificationStatuses(),
                    RequestTypeList = GetRequestTypes(),
                    CountryList = GetCountryList(),
                    TerritoryList = GetTerritoryList(),
                    AreaList = GetAreaList()  // Changed from GetPrintLanguageList
                };

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
        public IActionResult Create(LeadViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                    {
                        string query = @"INSERT INTO Leads (
                            Salutation, FirstName, MiddleName, LastName, JobTitle, Gender, LeadOwner,
                            Email, MobileNo, Phone, PhoneExt, WhatsApp, Website, Fax,
                            OrganizationName, Industry, AnnualRevenue, Employees, MarketSegment, Territory,
                            City, State, County, Country, Area, Source, Status, LeadType, RequestType, 
                            QualificationStatus, QualifiedBy, QualifiedOn,
                            CreatedBy, CompanyId, IsActive
                        ) VALUES (
                            @Salutation, @FirstName, @MiddleName, @LastName, @JobTitle, @Gender, @LeadOwner,
                            @Email, @MobileNo, @Phone, @PhoneExt, @WhatsApp, @Website, @Fax,
                            @OrganizationName, @Industry, @AnnualRevenue, @Employees, @MarketSegment, @Territory,
                            @City, @State, @County, @Country, @Area, @Source, @Status, @LeadType, @RequestType,
                            @QualificationStatus, @QualifiedBy, @QualifiedOn,
                            @CreatedBy, @CompanyId, @IsActive
                        );
                        SELECT SCOPE_IDENTITY();";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            var lead = viewModel.Lead;
                            AddLeadParameters(cmd, lead, false);

                            conn.Open();
                            int newId = Convert.ToInt32(cmd.ExecuteScalar());

                            if (newId > 0)
                            {
                                TempData["Success"] = "Lead created successfully!";
                                return RedirectToAction(nameof(Index));
                            }
                            else
                            {
                                ModelState.AddModelError("", "Failed to create lead. Please try again.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Create POST action");
                ModelState.AddModelError("", "An error occurred while creating the lead.");
            }

            ReloadDropdowns(viewModel);
            return View(viewModel);
        }

        // ============================================================
        // COPY TO OPPORTUNITY - Redirect to Opportunity Create with ALL lead data
        // ============================================================
        public IActionResult CopyToOpportunity(int id)
        {
            try
            {
                var lead = GetLeadById(id);
                if (lead == null)
                {
                    TempData["Error"] = "Lead not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Redirect to Opportunities CreateFromLead with ALL lead data
                return RedirectToAction("CreateFromLead", "Opportunities", new
                {
                    leadId = lead.Id,
                    company = lead.OrganizationName ?? "",
                    customerName = lead.FullName ?? "",
                    email = lead.Email ?? "",
                    phone = lead.MobileNo ?? "",
                    opportunityFrom = lead.Source ?? "",
                    source = lead.Source ?? "",
                    employees = lead.Employees,
                    industry = lead.Industry ?? "",
                    annualRevenue = lead.AnnualRevenue,
                    city = lead.City ?? "",
                    state = lead.State ?? "",
                    country = lead.Country ?? "",
                    area = lead.Area ?? "",  // Changed from PrintLanguage to Area
                    website = lead.Website ?? "",
                    territory = lead.Territory ?? "",
                    description = $"Lead: {lead.FullName} - {lead.OrganizationName}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in CopyToOpportunity action for ID: {id}");
                TempData["Error"] = "Error copying lead to opportunity.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ============================================================
        // EDIT - GET
        // ============================================================
        public IActionResult Edit(int id)
        {
            try
            {
                ViewData["Title"] = "Edit Lead";
                ViewData["Module"] = "CRM";
                ViewData["Icon"] = "edit";

                var lead = GetLeadById(id);
                if (lead == null)
                {
                    TempData["Error"] = "Lead not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new LeadViewModel
                {
                    Lead = lead,
                    SalutationList = GetSalutations(),
                    GenderList = GetGenders(),
                    StatusList = GetStatuses(),
                    LeadTypeList = GetLeadTypes(),
                    SourceList = GetSources(),
                    IndustryList = GetIndustries(),
                    QualificationStatusList = GetQualificationStatuses(),
                    RequestTypeList = GetRequestTypes(),
                    CountryList = GetCountryList(),
                    TerritoryList = GetTerritoryList(),
                    AreaList = GetAreaList()  // Changed from GetPrintLanguageList
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
        // EDIT - POST
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, LeadViewModel viewModel)
        {
            try
            {
                if (id != viewModel.Lead.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                    {
                        string query = @"UPDATE Leads SET
                            Salutation = @Salutation,
                            FirstName = @FirstName,
                            MiddleName = @MiddleName,
                            LastName = @LastName,
                            JobTitle = @JobTitle,
                            Gender = @Gender,
                            LeadOwner = @LeadOwner,
                            Email = @Email,
                            MobileNo = @MobileNo,
                            Phone = @Phone,
                            PhoneExt = @PhoneExt,
                            WhatsApp = @WhatsApp,
                            Website = @Website,
                            Fax = @Fax,
                            OrganizationName = @OrganizationName,
                            Industry = @Industry,
                            AnnualRevenue = @AnnualRevenue,
                            Employees = @Employees,
                            MarketSegment = @MarketSegment,
                            Territory = @Territory,
                            City = @City,
                            State = @State,
                            County = @County,
                            Country = @Country,
                            Area = @Area,  -- Changed from PrintLanguage
                            Source = @Source,
                            Status = @Status,
                            LeadType = @LeadType,
                            RequestType = @RequestType,
                            QualificationStatus = @QualificationStatus,
                            QualifiedBy = @QualifiedBy,
                            QualifiedOn = @QualifiedOn,
                            IsActive = @IsActive,
                            ModifiedDate = GETDATE(),
                            ModifiedBy = @ModifiedBy
                        WHERE Id = @Id";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            var lead = viewModel.Lead;
                            AddLeadParameters(cmd, lead, true);
                            cmd.Parameters.AddWithValue("@Id", id);
                            cmd.Parameters.AddWithValue("@ModifiedBy", GetCurrentUser());

                            conn.Open();
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                TempData["Success"] = "Lead updated successfully!";
                                return RedirectToAction(nameof(Index));
                            }
                            else
                            {
                                ModelState.AddModelError("", "Failed to update lead. No changes were made.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in Edit POST action for ID: {id}");
                ModelState.AddModelError("", "An error occurred while updating the lead.");
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
                ViewData["Title"] = "Delete Lead";
                ViewData["Module"] = "CRM";
                ViewData["Icon"] = "trash";

                var lead = GetLeadById(id);
                if (lead == null)
                {
                    TempData["Error"] = "Lead not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(lead);
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
                    string query = @"UPDATE Leads SET 
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
                            TempData["Success"] = "Lead deleted successfully!";
                        }
                        else
                        {
                            TempData["Error"] = "Failed to delete lead.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in Delete POST action for ID: {id}");
                TempData["Error"] = "An error occurred while deleting the lead.";
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
                            SUM(CASE WHEN Status = 'New' THEN 1 ELSE 0 END) as New,
                            SUM(CASE WHEN Status = 'Qualified' THEN 1 ELSE 0 END) as Qualified,
                            SUM(CASE WHEN Status = 'Converted' THEN 1 ELSE 0 END) as Converted,
                            SUM(CASE WHEN Status = 'Lost' THEN 1 ELSE 0 END) as Lost,
                            SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) as Active
                        FROM Leads 
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
                                    TotalLeads = reader["Total"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Total"]),
                                    NewLeads = reader["New"] == DBNull.Value ? 0 : Convert.ToInt32(reader["New"]),
                                    QualifiedLeads = reader["Qualified"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Qualified"]),
                                    ConvertedLeads = reader["Converted"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Converted"]),
                                    LostLeads = reader["Lost"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Lost"]),
                                    ActiveLeads = reader["Active"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Active"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetStats action");
            }

            return Json(new { TotalLeads = 0, NewLeads = 0, QualifiedLeads = 0, ConvertedLeads = 0, LostLeads = 0, ActiveLeads = 0 });
        }

        // ============================================================
        // HELPER METHODS
        // ============================================================

        private List<Lead> GetLeads(LeadSearchModel search = null)
        {
            var leads = new List<Lead>();

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT * FROM Leads WHERE IsDeleted = 0";

                if (search != null)
                {
                    var conditions = new List<string>();

                    if (!string.IsNullOrEmpty(search.SearchTerm))
                    {
                        conditions.Add("(FirstName LIKE @SearchTerm OR LastName LIKE @SearchTerm OR Email LIKE @SearchTerm OR MobileNo LIKE @SearchTerm OR OrganizationName LIKE @SearchTerm)");
                    }
                    if (!string.IsNullOrEmpty(search.Status))
                    {
                        conditions.Add("Status = @Status");
                    }
                    if (!string.IsNullOrEmpty(search.Source))
                    {
                        conditions.Add("Source = @Source");
                    }
                    if (!string.IsNullOrEmpty(search.LeadType))
                    {
                        conditions.Add("LeadType = @LeadType");
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
                        if (!string.IsNullOrEmpty(search.Status))
                        {
                            cmd.Parameters.AddWithValue("@Status", search.Status);
                        }
                        if (!string.IsNullOrEmpty(search.Source))
                        {
                            cmd.Parameters.AddWithValue("@Source", search.Source);
                        }
                        if (!string.IsNullOrEmpty(search.LeadType))
                        {
                            cmd.Parameters.AddWithValue("@LeadType", search.LeadType);
                        }
                    }

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            leads.Add(MapDataReaderToLead(reader));
                        }
                    }
                }
            }

            return leads;
        }

        private Lead? GetLeadById(int id)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT * FROM Leads WHERE Id = @Id AND IsDeleted = 0";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapDataReaderToLead(reader);
                        }
                    }
                }
            }
            return null;
        }

        private Lead MapDataReaderToLead(SqlDataReader reader)
        {
            return new Lead
            {
                Id = Convert.ToInt32(reader["Id"]),
                Salutation = reader["Salutation"]?.ToString(),
                FirstName = reader["FirstName"]?.ToString() ?? string.Empty,
                MiddleName = reader["MiddleName"]?.ToString(),
                LastName = reader["LastName"]?.ToString(),
                JobTitle = reader["JobTitle"]?.ToString(),
                Gender = reader["Gender"]?.ToString(),
                LeadOwner = reader["LeadOwner"]?.ToString(),
                Email = reader["Email"]?.ToString() ?? string.Empty,
                MobileNo = reader["MobileNo"]?.ToString() ?? string.Empty,
                Phone = reader["Phone"]?.ToString(),
                PhoneExt = reader["PhoneExt"]?.ToString(),
                WhatsApp = reader["WhatsApp"]?.ToString(),
                Website = reader["Website"]?.ToString(),
                Fax = reader["Fax"]?.ToString(),
                OrganizationName = reader["OrganizationName"]?.ToString(),
                Industry = reader["Industry"]?.ToString(),
                AnnualRevenue = reader["AnnualRevenue"] != DBNull.Value ? Convert.ToDecimal(reader["AnnualRevenue"]) : null,
                Employees = reader["Employees"] != DBNull.Value ? Convert.ToInt32(reader["Employees"]) : null,
                MarketSegment = reader["MarketSegment"]?.ToString(),
                Territory = reader["Territory"]?.ToString(),
                City = reader["City"]?.ToString(),
                State = reader["State"]?.ToString(),
                County = reader["County"]?.ToString(),
                Country = reader["Country"]?.ToString(),
                Area = reader["Area"]?.ToString(),  // Changed from PrintLanguage
                Source = reader["Source"]?.ToString(),
                Status = reader["Status"]?.ToString(),
                LeadType = reader["LeadType"]?.ToString(),
                RequestType = reader["RequestType"]?.ToString(),
                QualificationStatus = reader["QualificationStatus"]?.ToString(),
                QualifiedBy = reader["QualifiedBy"]?.ToString(),
                QualifiedOn = reader["QualifiedOn"] != DBNull.Value ? Convert.ToDateTime(reader["QualifiedOn"]) : null,
                CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                CreatedBy = reader["CreatedBy"]?.ToString(),
                ModifiedDate = reader["ModifiedDate"] != DBNull.Value ? Convert.ToDateTime(reader["ModifiedDate"]) : null,
                ModifiedBy = reader["ModifiedBy"]?.ToString(),
                IsActive = Convert.ToBoolean(reader["IsActive"]),
                IsDeleted = Convert.ToBoolean(reader["IsDeleted"]),
                CompanyId = reader["CompanyId"] != DBNull.Value ? Convert.ToInt32(reader["CompanyId"]) : null
            };
        }

        private void AddLeadParameters(SqlCommand cmd, Lead lead, bool includeId = false)
        {
            cmd.Parameters.AddWithValue("@Salutation", (object?)lead.Salutation ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FirstName", lead.FirstName);
            cmd.Parameters.AddWithValue("@MiddleName", (object?)lead.MiddleName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LastName", (object?)lead.LastName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@JobTitle", (object?)lead.JobTitle ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Gender", (object?)lead.Gender ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LeadOwner", (object?)lead.LeadOwner ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", lead.Email);
            cmd.Parameters.AddWithValue("@MobileNo", lead.MobileNo);
            cmd.Parameters.AddWithValue("@Phone", (object?)lead.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PhoneExt", (object?)lead.PhoneExt ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@WhatsApp", (object?)lead.WhatsApp ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Website", (object?)lead.Website ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Fax", (object?)lead.Fax ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@OrganizationName", (object?)lead.OrganizationName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Industry", (object?)lead.Industry ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AnnualRevenue", (object?)lead.AnnualRevenue ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Employees", (object?)lead.Employees ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MarketSegment", (object?)lead.MarketSegment ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Territory", (object?)lead.Territory ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@City", (object?)lead.City ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@State", (object?)lead.State ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@County", (object?)lead.County ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Country", (object?)lead.Country ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Area", (object?)lead.Area ?? DBNull.Value);  // Changed from PrintLanguage
            cmd.Parameters.AddWithValue("@Source", (object?)lead.Source ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", (object?)lead.Status ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LeadType", (object?)lead.LeadType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RequestType", (object?)lead.RequestType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@QualificationStatus", (object?)lead.QualificationStatus ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@QualifiedBy", (object?)lead.QualifiedBy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@QualifiedOn", (object?)lead.QualifiedOn ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", lead.IsActive);

            if (!includeId)
            {
                cmd.Parameters.AddWithValue("@CreatedBy", GetCurrentUser());
                cmd.Parameters.AddWithValue("@CompanyId", GetCompanyId());
            }
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

        private void ReloadDropdowns(LeadViewModel viewModel)
        {
            viewModel.SalutationList = GetSalutations();
            viewModel.GenderList = GetGenders();
            viewModel.StatusList = GetStatuses();
            viewModel.LeadTypeList = GetLeadTypes();
            viewModel.SourceList = GetSources();
            viewModel.IndustryList = GetIndustries();
            viewModel.QualificationStatusList = GetQualificationStatuses();
            viewModel.RequestTypeList = GetRequestTypes();
            viewModel.CountryList = GetCountryList();
            viewModel.TerritoryList = GetTerritoryList();
            viewModel.AreaList = GetAreaList();  // Changed from GetPrintLanguageList
        }

        // ============================================================
        // DROPDOWN LISTS
        // ============================================================

        private List<string> GetSalutations()
        {
            return new List<string> { "", "Mr.", "Ms.", "Mrs.", "Dr.", "Prof.", "Sir", "Madam" };
        }

        private List<string> GetGenders()
        {
            return new List<string> { "", "Male", "Female", "Other", "Prefer Not to Say" };
        }

        private List<string> GetStatuses()
        {
            return new List<string> { "", "New", "Open", "Contacted", "Interested", "Qualified", "Converted", "Lost", "Disqualified" };
        }

        private List<string> GetLeadTypes()
        {
            return new List<string> { "", "Client", "Channel Partner", "Consultant", "Reseller", "Distributor", "Other" };
        }

        private List<string> GetSources()
        {
            return new List<string> { "", "Website", "LinkedIn", "Google", "Referral", "Email", "Social Media", "Conference", "Cold Call", "Other" };
        }

        private List<string> GetIndustries()
        {
            return new List<string> { "", "Technology", "IT Services", "Healthcare", "Finance", "Education",
                "Manufacturing", "Retail", "Real Estate", "Transportation", "Hospitality", "Construction", "Other" };
        }

        private List<string> GetQualificationStatuses()
        {
            return new List<string> { "", "Pending", "In Progress", "Qualified", "Disqualified", "Rejected" };
        }

        private List<string> GetRequestTypes()
        {
            return new List<string> { "", "Consultation", "Demo", "Quote", "Support", "Complaint", "Feedback", "Other" };
        }

        private List<string> GetCountryList()
        {
            return new List<string> { "", "India", "USA", "UK", "Canada", "Australia", "Germany", "France", "UAE", "Singapore", "China", "Japan", "Brazil", "Other" };
        }

        private List<string> GetTerritoryList()
        {
            return new List<string> { "", "North America", "South America", "Europe", "Asia Pacific", "Middle East", "Africa", "Australia", "Other" };
        }

        private List<string> GetAreaList()  // Changed from GetPrintLanguageList
        {
            return new List<string> { "", "Downtown", "Uptown", "Midtown", "Eastside", "Westside", "Northside", "Southside", "Central Business District", "Industrial Area", "Residential Area", "Commercial Area", "Other" };
        }
    }
}