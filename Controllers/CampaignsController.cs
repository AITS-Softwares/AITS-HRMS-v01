//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Data.SqlClient;
//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.IO;
//using System.Linq;
//using System.Text.Json;
//using System.Threading.Tasks;
//using Erp_Aits.Models;
//using Erp_Aits.Services;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using System.Net;
//using System.Net.Mail;
//using System.Text;
//using ClosedXML.Excel;

//namespace Erp_Aits.Controllers
//{
//    public class CampaignsController : Controller
//    {
//        private readonly IConfiguration _configuration;
//        private readonly ILogger<CampaignsController> _logger;
//        private readonly IServiceScopeFactory _serviceScopeFactory;

//        public CampaignsController(
//            IConfiguration configuration,
//            ILogger<CampaignsController> logger,
//            IServiceScopeFactory serviceScopeFactory)
//        {
//            _configuration = configuration;
//            _logger = logger;
//            _serviceScopeFactory = serviceScopeFactory;
//        }

//        private string GetConnectionString()
//        {
//            return _configuration.GetConnectionString("DefaultConnection");
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

//            try
//            {
//                using (var conn = new SqlConnection(GetConnectionString()))
//                {
//                    var query = "SELECT TOP 1 Id FROM Companies WHERE IsActive = 1";
//                    using (var cmd = new SqlCommand(query, conn))
//                    {
//                        conn.Open();
//                        var result = cmd.ExecuteScalar();
//                        if (result != null)
//                        {
//                            var id = Convert.ToInt32(result);
//                            HttpContext.Session.SetString("CompanyId", id.ToString());
//                            return id;
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"Error getting CompanyId: {ex.Message}");
//            }

//            return 1;
//        }

//        private string GetSmtpUser()
//        {
//            return _configuration["EmailSettings:User"] ?? "pritammore139@gmail.com";
//        }

//        private string GetSmtpPassword()
//        {
//            return _configuration["EmailSettings:Password"] ?? "uuqn vtrq nqqu poqi";
//        }

//        private string GetBaseUrl()
//        {
//            return _configuration["BaseUrl"] ?? "https://localhost:7183";
//        }

//        // ============================================================
//        // INDEX - List all campaigns
//        // ============================================================
//        public IActionResult Index()
//        {
//            try
//            {
//                ViewData["Title"] = "Campaigns";
//                ViewData["Module"] = "CRM";
//                ViewData["Icon"] = "bullhorn";

//                var companyId = GetCompanyId();
//                _logger.LogInformation($"Index action - CompanyId: {companyId}");

//                var campaigns = GetCampaigns();
//                _logger.LogInformation($"Index action - Found {campaigns.Count} campaigns");

//                foreach (var campaign in campaigns)
//                {
//                    var counts = GetCampaignCounts(campaign.Id);
//                    campaign.RecipientCount = counts.RecipientCount;
//                    campaign.OpenCount = counts.OpenCount;
//                }

//                var summary = GetCampaignSummary();

//                ViewBag.Summary = summary;
//                ViewBag.CompanyId = companyId;

//                return View(campaigns);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error in Index action");
//                TempData["Error"] = $"Error loading campaigns: {ex.Message}";
//                return View(new List<Campaign>());
//            }
//        }

//        // ============================================================
//        // DETAILS - View single campaign
//        // ============================================================
//        public IActionResult Details(int id)
//        {
//            try
//            {
//                ViewData["Title"] = "Campaign Details";
//                ViewData["Module"] = "CRM";
//                ViewData["Icon"] = "bullhorn";

//                var campaign = GetCampaignById(id);
//                if (campaign == null)
//                {
//                    TempData["Error"] = "Campaign not found.";
//                    return RedirectToAction(nameof(Index));
//                }

//                var counts = GetCampaignCounts(id);
//                campaign.RecipientCount = counts.RecipientCount;
//                campaign.OpenCount = counts.OpenCount;

//                var stats = GetCampaignStats(id);
//                var logs = GetCampaignLogs(id);

//                ViewBag.Stats = stats;
//                ViewBag.Logs = logs;

//                return View(campaign);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error in Details action for ID: {id}");
//                TempData["Error"] = "Error loading campaign details.";
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
//                ViewData["Title"] = "Campaign Studio";
//                ViewData["Module"] = "CRM";
//                ViewData["Icon"] = "plus";

//                var companyId = GetCompanyId();
//                _logger.LogInformation($"Create GET - CompanyId: {companyId}");

//                ViewBag.Customers = GetCustomers();
//                ViewBag.Leads = GetLeads();
//                ViewBag.EmailMasters = GetActiveEmailMasters();
//                ViewBag.WhatsAppSettings = GetWhatsAppSettings();

//                var campaign = new Campaign
//                {
//                    ScheduledTime = DateTime.Now.AddHours(1),
//                    CampaignChannel = "email",
//                    CompanyId = companyId,
//                    RecipientSource = "segment"
//                };

//                return View(campaign);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error in Create GET action");
//                TempData["Error"] = "Error loading create form.";
//                return RedirectToAction(nameof(Index));
//            }
//        }

//        // ============================================================
//        // CREATE - POST (COMPLETE FIXED VERSION)
//        // ============================================================
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(Campaign campaign, string? manualEmails, string? selectedCustomers, string? selectedLeads, string? whatsappContent, List<IFormFile>? attachments)
//        {
//            try
//            {
//                // ============================================================
//                // STEP 1: CLEAR MODELSTATE AT START
//                // ============================================================
//                ModelState.Clear();
//                _logger.LogInformation("ModelState cleared at start");

//                _logger.LogInformation($"===== CREATE CAMPAIGN =====");
//                _logger.LogInformation($"RecipientSource: {campaign.RecipientSource}");
//                _logger.LogInformation($"CampaignChannel: {campaign.CampaignChannel}");
//                _logger.LogInformation($"ScheduledTime: {campaign.ScheduledTime}");
//                _logger.LogInformation($"manualEmails: {(string.IsNullOrEmpty(manualEmails) ? "NULL" : manualEmails)}");
//                _logger.LogInformation($"selectedCustomers: {(string.IsNullOrEmpty(selectedCustomers) ? "NULL" : selectedCustomers)}");
//                _logger.LogInformation($"selectedLeads: {(string.IsNullOrEmpty(selectedLeads) ? "NULL" : selectedLeads)}");
//                _logger.LogInformation($"whatsappContent: {(string.IsNullOrEmpty(whatsappContent) ? "NULL" : whatsappContent)}");
//                _logger.LogInformation($"Attachments Count: {attachments?.Count ?? 0}");

//                if (attachments != null && attachments.Any())
//                {
//                    foreach (var file in attachments)
//                    {
//                        _logger.LogInformation($"Attachment: {file.FileName}, Size: {file.Length} bytes, Type: {file.ContentType}");
//                    }
//                }

//                // ============================================================
//                // STEP 2: MAP WHATSAPP CONTENT TO CONTENT PROPERTY
//                // ============================================================
//                if (campaign.CampaignChannel == "whatsapp" || campaign.CampaignChannel == "both")
//                {
//                    if (!string.IsNullOrEmpty(whatsappContent))
//                    {
//                        campaign.Content = whatsappContent;
//                        _logger.LogInformation($"WhatsApp content mapped to Content property: {campaign.Content}");
//                    }
//                }

//                // ============================================================
//                // STEP 3: SET SCHEDULED TIME
//                // ============================================================
//                if (!campaign.ScheduledTime.HasValue)
//                {
//                    campaign.ScheduledTime = DateTime.Now.AddHours(1);
//                }
//                else
//                {
//                    var time = campaign.ScheduledTime.Value;
//                    if (time.Kind == DateTimeKind.Utc)
//                    {
//                        campaign.ScheduledTime = time.ToLocalTime();
//                    }
//                    else if (time.Kind == DateTimeKind.Unspecified)
//                    {
//                        campaign.ScheduledTime = DateTime.SpecifyKind(time, DateTimeKind.Local);
//                    }
//                }

//                // ============================================================
//                // STEP 4: MANUALLY VALIDATE ALL REQUIRED FIELDS
//                // ============================================================
//                var validationErrors = new List<string>();

//                // Campaign Name
//                if (string.IsNullOrWhiteSpace(campaign.CampaignName))
//                {
//                    validationErrors.Add("Campaign Name is required");
//                }

//                // Sender
//                if (string.IsNullOrWhiteSpace(campaign.Sender))
//                {
//                    validationErrors.Add("Sender is required");
//                }

//                // Scheduled Time
//                if (!campaign.ScheduledTime.HasValue)
//                {
//                    validationErrors.Add("Scheduled Time is required");
//                }

//                // Content based on channel
//                if (string.IsNullOrWhiteSpace(campaign.Content))
//                {
//                    if (campaign.CampaignChannel == "whatsapp" || campaign.CampaignChannel == "both")
//                    {
//                        validationErrors.Add("WhatsApp message is required. Please add your message.");
//                    }
//                    else
//                    {
//                        validationErrors.Add("Email content is required. Please add your message.");
//                    }
//                }

//                // Recipient Source
//                if (string.IsNullOrWhiteSpace(campaign.RecipientSource))
//                {
//                    validationErrors.Add("Recipient Source is required");
//                }

//                // ============================================================
//                // STEP 5: PROCESS RECIPIENTS
//                // ============================================================
//                bool hasRecipients = true;
//                string errorMessage = "";

//                if (campaign.RecipientSource == "segment")
//                {
//                    var hasCustomers = !string.IsNullOrEmpty(selectedCustomers);
//                    var hasLeads = !string.IsNullOrEmpty(selectedLeads);

//                    if (!hasCustomers && !hasLeads)
//                    {
//                        hasRecipients = false;
//                        errorMessage = "Please select at least one customer or lead.";
//                    }
//                    else
//                    {
//                        var recipients = new List<string>();

//                        if (!string.IsNullOrEmpty(selectedCustomers))
//                        {
//                            var customerIds = selectedCustomers.Split(',').Select(int.Parse).ToList();

//                            if (campaign.CampaignChannel == "both")
//                            {
//                                var emails = GetCustomerEmailsByIds(customerIds);
//                                var phones = GetCustomerPhoneNumbersByIds(customerIds);
//                                recipients.AddRange(emails);
//                                recipients.AddRange(phones);
//                                _logger.LogInformation($"Added {emails.Count} emails and {phones.Count} phones for both channels");
//                            }
//                            else if (campaign.CampaignChannel == "whatsapp")
//                            {
//                                var phoneNumbers = GetCustomerPhoneNumbersByIds(customerIds);
//                                recipients.AddRange(phoneNumbers);
//                                _logger.LogInformation($"Added {phoneNumbers.Count} customer phone numbers for WhatsApp");
//                            }
//                            else
//                            {
//                                var customerEmails = GetCustomerEmailsByIds(customerIds);
//                                recipients.AddRange(customerEmails);
//                                _logger.LogInformation($"Added {customerEmails.Count} customer emails");
//                            }
//                        }

//                        if (!string.IsNullOrEmpty(selectedLeads))
//                        {
//                            var leadIds = selectedLeads.Split(',').Select(int.Parse).ToList();

//                            if (campaign.CampaignChannel == "both")
//                            {
//                                var emails = GetLeadEmailsByIds(leadIds);
//                                var phones = GetLeadPhoneNumbersByIds(leadIds);
//                                recipients.AddRange(emails);
//                                recipients.AddRange(phones);
//                                _logger.LogInformation($"Added {emails.Count} emails and {phones.Count} phones for both channels");
//                            }
//                            else if (campaign.CampaignChannel == "whatsapp")
//                            {
//                                var phoneNumbers = GetLeadPhoneNumbersByIds(leadIds);
//                                recipients.AddRange(phoneNumbers);
//                                _logger.LogInformation($"Added {phoneNumbers.Count} lead phone numbers for WhatsApp");
//                            }
//                            else
//                            {
//                                var leadEmails = GetLeadEmailsByIds(leadIds);
//                                recipients.AddRange(leadEmails);
//                                _logger.LogInformation($"Added {leadEmails.Count} lead emails");
//                            }
//                        }

//                        campaign.RecipientList = JsonSerializer.Serialize(recipients.Distinct());
//                        _logger.LogInformation($"Total segment recipients: {recipients.Distinct().Count()}");
//                    }
//                }
//                else if (campaign.RecipientSource == "manual")
//                {
//                    if (string.IsNullOrEmpty(manualEmails))
//                    {
//                        hasRecipients = false;
//                        errorMessage = "Please enter at least one email address or phone number.";
//                    }
//                    else
//                    {
//                        var rawInputs = manualEmails
//                            .Split(new[] { '\n', ',' }, StringSplitOptions.RemoveEmptyEntries)
//                            .Select(e => e.Trim())
//                            .Where(e => !string.IsNullOrEmpty(e))
//                            .Distinct()
//                            .ToList();

//                        if (!rawInputs.Any())
//                        {
//                            hasRecipients = false;
//                            errorMessage = "Please enter at least one valid email address or phone number.";
//                        }
//                        else
//                        {
//                            if (campaign.CampaignChannel == "whatsapp" || campaign.CampaignChannel == "both")
//                            {
//                                var phoneNumbers = rawInputs
//                                    .Select(r => CleanPhoneNumber(r))
//                                    .Where(r => !string.IsNullOrEmpty(r) && r.Length >= 10)
//                                    .ToList();

//                                if (!phoneNumbers.Any())
//                                {
//                                    hasRecipients = false;
//                                    errorMessage = "Please enter at least one valid phone number (minimum 10 digits).";
//                                }
//                                else
//                                {
//                                    campaign.RecipientManual = string.Join("\n", phoneNumbers);
//                                    _logger.LogInformation($"Added {phoneNumbers.Count} manual phone numbers");
//                                }
//                            }
//                            else
//                            {
//                                var emails = rawInputs
//                                    .Where(e => e.Contains("@") && IsValidEmail(e))
//                                    .ToList();

//                                if (!emails.Any())
//                                {
//                                    hasRecipients = false;
//                                    errorMessage = "Please enter at least one valid email address.";
//                                }
//                                else
//                                {
//                                    campaign.RecipientManual = string.Join("\n", emails);
//                                    _logger.LogInformation($"Added {emails.Count} manual emails");
//                                }
//                            }
//                        }
//                    }
//                }
//                else if (campaign.RecipientSource == "excel")
//                {
//                    if (attachments == null || attachments.Count == 0)
//                    {
//                        hasRecipients = false;
//                        errorMessage = "Please upload an Excel file.";
//                    }
//                    else
//                    {
//                        var file = attachments[0];
//                        if (file.FileName.EndsWith(".xlsx") || file.FileName.EndsWith(".xls") || file.FileName.EndsWith(".csv"))
//                        {
//                            if (campaign.CampaignChannel == "whatsapp" || campaign.CampaignChannel == "both")
//                            {
//                                var phoneData = await ProcessExcelFileForPhoneNumbersAsync(file);
//                                if (!phoneData.Any())
//                                {
//                                    hasRecipients = false;
//                                    errorMessage = "No valid phone numbers found in the Excel file.";
//                                }
//                                else
//                                {
//                                    var phoneNumbers = phoneData.Select(p => p.PhoneNumber).ToList();
//                                    campaign.RecipientExcelEmails = JsonSerializer.Serialize(phoneNumbers);
//                                    _logger.LogInformation($"Added {phoneNumbers.Count} excel phone numbers");
//                                }
//                            }
//                            else
//                            {
//                                var emailData = await ProcessExcelFileWithDetailsAsync(file);
//                                if (!emailData.Any())
//                                {
//                                    hasRecipients = false;
//                                    errorMessage = "No valid email addresses found in the Excel file.";
//                                }
//                                else
//                                {
//                                    var emails = emailData.Select(e => e.Email).ToList();
//                                    campaign.RecipientExcelEmails = JsonSerializer.Serialize(emails);
//                                    _logger.LogInformation($"Added {emails.Count} excel emails");
//                                }
//                            }
//                        }
//                        else
//                        {
//                            var attachmentUrls = new List<string>();
//                            var url = SaveAttachment(file);
//                            if (!string.IsNullOrEmpty(url))
//                            {
//                                attachmentUrls.Add(url);
//                                _logger.LogInformation($"Attachment saved: {url}");
//                            }
//                            campaign.Attachments = JsonSerializer.Serialize(attachmentUrls);

//                            hasRecipients = false;
//                            errorMessage = "Please upload an Excel file (.xlsx or .xls) with email addresses or phone numbers.";
//                        }
//                    }
//                }
//                else
//                {
//                    hasRecipients = false;
//                    errorMessage = "Please select a valid recipient source.";
//                }

//                if (campaign.RecipientSource != "excel" && attachments != null && attachments.Any())
//                {
//                    _logger.LogInformation($"Processing {attachments.Count} regular attachments");
//                    var attachmentUrls = new List<string>();

//                    foreach (var file in attachments)
//                    {
//                        var url = SaveAttachment(file);
//                        if (!string.IsNullOrEmpty(url))
//                        {
//                            attachmentUrls.Add(url);
//                            _logger.LogInformation($"Attachment saved: {url}");
//                        }
//                    }

//                    if (attachmentUrls.Any())
//                    {
//                        campaign.Attachments = JsonSerializer.Serialize(attachmentUrls);
//                        _logger.LogInformation($"Attachments JSON: {campaign.Attachments}");
//                    }
//                }

//                if (!hasRecipients)
//                {
//                    validationErrors.Add(errorMessage);
//                }

//                // ============================================================
//                // STEP 6: CHECK FOR VALIDATION ERRORS
//                // ============================================================
//                if (validationErrors.Any())
//                {
//                    _logger.LogWarning($"Validation errors: {string.Join(" | ", validationErrors)}");
//                    foreach (var error in validationErrors)
//                    {
//                        ModelState.AddModelError("", error);
//                    }

//                    ViewBag.Customers = GetCustomers();
//                    ViewBag.Leads = GetLeads();
//                    ViewBag.EmailMasters = GetActiveEmailMasters();
//                    ViewBag.WhatsAppSettings = GetWhatsAppSettings();
//                    return View(campaign);
//                }

//                // ============================================================
//                // STEP 7: CREATE THE CAMPAIGN
//                // ============================================================
//                campaign.CompanyId = GetCompanyId();
//                campaign.CreatedBy = GetCompanyId();
//                campaign.CreatedDate = DateTime.Now;
//                campaign.IsActive = true;
//                campaign.IsDeleted = false;
//                campaign.Status = "Scheduled";

//                _logger.LogInformation($"Final Content: {(campaign.Content?.Length > 50 ? campaign.Content.Substring(0, 50) + "..." : campaign.Content)}");
//                _logger.LogInformation($"Channel: {campaign.CampaignChannel}, Content Length: {campaign.Content?.Length ?? 0}");

//                using (var conn = new SqlConnection(GetConnectionString()))
//                {
//                    var query = @"
//                        INSERT INTO Campaigns (
//                            CampaignName, ScheduledTime, Channel, Sender, Content,
//                            EmailSubject, CtaText, Attachments, RecipientSource,
//                            RecipientList, RecipientManual, RecipientExcelEmails,
//                            Status, EmailMasterId, CompanyId, CreatedBy, CreatedDate
//                        ) VALUES (
//                            @CampaignName, @ScheduledTime, @Channel, @Sender, @Content,
//                            @EmailSubject, @CtaText, @Attachments, @RecipientSource,
//                            @RecipientList, @RecipientManual, @RecipientExcelEmails,
//                            @Status, @EmailMasterId, @CompanyId, @CreatedBy, @CreatedDate
//                        );
//                        SELECT SCOPE_IDENTITY();";

//                    using (var cmd = new SqlCommand(query, conn))
//                    {
//                        cmd.Parameters.AddWithValue("@CampaignName", campaign.CampaignName ?? (object)DBNull.Value);
//                        cmd.Parameters.AddWithValue("@ScheduledTime", campaign.ScheduledTime ?? (object)DBNull.Value);
//                        cmd.Parameters.AddWithValue("@Channel", campaign.CampaignChannel ?? "email");
//                        cmd.Parameters.AddWithValue("@Sender", campaign.Sender ?? (object)DBNull.Value);
//                        cmd.Parameters.AddWithValue("@Content", campaign.Content ?? (object)DBNull.Value);
//                        cmd.Parameters.AddWithValue("@EmailSubject", campaign.EmailSubject ?? (object)DBNull.Value);
//                        cmd.Parameters.AddWithValue("@CtaText", campaign.CtaText ?? (object)DBNull.Value);
//                        cmd.Parameters.AddWithValue("@Attachments", campaign.Attachments ?? (object)DBNull.Value);
//                        cmd.Parameters.AddWithValue("@RecipientSource", campaign.RecipientSource ?? "segment");
//                        cmd.Parameters.AddWithValue("@RecipientList", campaign.RecipientList ?? (object)DBNull.Value);
//                        cmd.Parameters.AddWithValue("@RecipientManual", campaign.RecipientManual ?? (object)DBNull.Value);
//                        cmd.Parameters.AddWithValue("@RecipientExcelEmails", campaign.RecipientExcelEmails ?? (object)DBNull.Value);
//                        cmd.Parameters.AddWithValue("@Status", campaign.Status);
//                        cmd.Parameters.AddWithValue("@EmailMasterId", campaign.EmailMasterId ?? (object)DBNull.Value);
//                        cmd.Parameters.AddWithValue("@CompanyId", campaign.CompanyId ?? (object)DBNull.Value);
//                        cmd.Parameters.AddWithValue("@CreatedBy", campaign.CreatedBy ?? (object)DBNull.Value);
//                        cmd.Parameters.AddWithValue("@CreatedDate", campaign.CreatedDate);

//                        conn.Open();
//                        var newId = Convert.ToInt32(cmd.ExecuteScalar());

//                        _logger.LogInformation($"Campaign created successfully with ID: {newId}");

//                        var scheduledTime = campaign.ScheduledTime ?? DateTime.Now;
//                        var now = DateTime.Now;
//                        var timeDifference = (scheduledTime - now).TotalMinutes;

//                        if (timeDifference <= 5)
//                        {
//                            _ = Task.Run(async () => await SendCampaignAsync(newId));
//                            TempData["Success"] = "Campaign created and sending started!";
//                        }
//                        else
//                        {
//                            await AddToSchedulerQueueAsync(newId, scheduledTime);
//                            TempData["Success"] = $"Campaign created and scheduled for {scheduledTime:dd MMM yyyy HH:mm}";
//                        }

//                        return RedirectToAction(nameof(Details), new { id = newId });
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error in Create POST action");
//                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
//            }

//            ViewBag.Customers = GetCustomers();
//            ViewBag.Leads = GetLeads();
//            ViewBag.EmailMasters = GetActiveEmailMasters();
//            ViewBag.WhatsAppSettings = GetWhatsAppSettings();

//            return View(campaign);
//        }

//        // ============================================================
//        // SEND CAMPAIGN - POST
//        // ============================================================
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Send(int id)
//        {
//            try
//            {
//                var campaign = GetCampaignById(id);
//                if (campaign == null)
//                {
//                    TempData["Error"] = "Campaign not found.";
//                    return RedirectToAction(nameof(Index));
//                }

//                var scheduledTime = campaign.ScheduledTime ?? DateTime.Now;
//                var now = DateTime.Now;
//                var timeDifference = (scheduledTime - now).TotalMinutes;

//                if (timeDifference <= 5)
//                {
//                    var success = await SendCampaignAsync(id);
//                    if (success)
//                    {
//                        TempData["Success"] = "Campaign sent successfully!";
//                    }
//                    else
//                    {
//                        TempData["Error"] = "Failed to send campaign. Please check logs.";
//                    }
//                }
//                else
//                {
//                    await AddToSchedulerQueueAsync(id, scheduledTime);
//                    UpdateCampaignStatus(id, "Scheduled", null);
//                    TempData["Success"] = $"Campaign scheduled to send on {scheduledTime:dd MMM yyyy HH:mm}";
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error processing campaign {id}");
//                TempData["Error"] = $"Error: {ex.Message}";
//            }

//            return RedirectToAction(nameof(Details), new { id });
//        }

//        // ============================================================
//        // SEND CAMPAIGN ASYNC - Main Entry Point
//        // ============================================================
//        public async Task<bool> SendCampaignAsync(int campaignId)
//        {
//            try
//            {
//                var campaign = GetCampaignById(campaignId);
//                if (campaign == null)
//                {
//                    _logger.LogError($"Campaign {campaignId} not found");
//                    return false;
//                }

//                _logger.LogInformation($"===== SEND CAMPAIGN STARTED - Campaign ID: {campaignId} =====");
//                _logger.LogInformation($"CampaignChannel: {campaign.CampaignChannel}");
//                _logger.LogInformation($"Campaign Name: {campaign.CampaignName}");

//                bool success = false;

//                if (campaign.CampaignChannel == "whatsapp")
//                {
//                    success = await SendWhatsAppCampaignAsync(campaignId);
//                }
//                else if (campaign.CampaignChannel == "email")
//                {
//                    success = await SendEmailCampaignAsync(campaignId);
//                }
//                else if (campaign.CampaignChannel == "both")
//                {
//                    var emailTask = SendEmailCampaignAsync(campaignId);
//                    var whatsappTask = SendWhatsAppCampaignAsync(campaignId);

//                    await Task.WhenAll(emailTask, whatsappTask);
//                    success = emailTask.Result || whatsappTask.Result;
//                }
//                else
//                {
//                    _logger.LogError($"Unknown channel: {campaign.CampaignChannel}");
//                    return false;
//                }

//                if (success)
//                {
//                    UpdateCampaignStatus(campaignId, "Sent", DateTime.Now);
//                    await UpdateSchedulerStatus(campaignId, "Completed");
//                }
//                else
//                {
//                    UpdateCampaignStatus(campaignId, "Failed", DateTime.Now);
//                    await UpdateSchedulerStatus(campaignId, "Failed");
//                }

//                return success;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"Error in SendCampaignAsync: {ex.Message}");
//                UpdateCampaignStatus(campaignId, "Failed", DateTime.Now);
//                await UpdateSchedulerStatus(campaignId, "Error");
//                return false;
//            }
//        }

//        // ============================================================
//        // SEND EMAIL CAMPAIGN
//        // ============================================================
//        private async Task<bool> SendEmailCampaignAsync(int campaignId)
//        {
//            try
//            {
//                var campaign = GetCampaignById(campaignId);
//                if (campaign == null)
//                {
//                    _logger.LogError($"Campaign {campaignId} not found");
//                    return false;
//                }

//                var allRecipients = GetRecipients(campaign);
//                var recipients = allRecipients
//                    .Where(r => r.Contains("@") && IsValidEmail(r))
//                    .ToList();

//                _logger.LogInformation($"Found {recipients.Count} valid email recipients");

//                if (!recipients.Any())
//                {
//                    _logger.LogWarning($"No valid email recipients found for campaign {campaignId}");
//                    return false;
//                }

//                var smtpUser = GetSmtpUser();
//                var smtpPassword = GetSmtpPassword();

//                if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPassword))
//                {
//                    _logger.LogError($"SMTP credentials missing");
//                    return false;
//                }

//                using (var client = new SmtpClient("smtp.gmail.com", 587))
//                {
//                    client.EnableSsl = true;
//                    client.UseDefaultCredentials = false;
//                    client.Credentials = new NetworkCredential(smtpUser, smtpPassword);
//                    client.Timeout = 60000;

//                    int successCount = 0;
//                    int failCount = 0;

//                    foreach (var recipient in recipients)
//                    {
//                        try
//                        {
//                            using (var message = new MailMessage())
//                            {
//                                message.From = new MailAddress(smtpUser, campaign.Sender ?? "Marketing Team");
//                                message.To.Add(recipient);
//                                message.Subject = campaign.EmailSubject ?? "Campaign Update";
//                                message.IsBodyHtml = true;

//                                var body = campaign.Content ?? "";

//                                var trackingPixel = $"<img src='{GetBaseUrl()}/api/track/open?id={campaignId}&email={recipient}' width='1' height='1' style='display:none;' />";
//                                body += trackingPixel;

//                                if (!string.IsNullOrEmpty(campaign.CtaText))
//                                {
//                                    var trackUrl = $"{GetBaseUrl()}/api/track/click?id={campaignId}&email={recipient}&url={System.Net.WebUtility.UrlEncode(campaign.CtaText)}";
//                                    body += $"<br/><br/><a href='{trackUrl}' style='display:inline-block;padding:12px 24px;background:#007bff;color:white;text-decoration:none;border-radius:5px;'>{campaign.CtaText}</a>";
//                                }

//                                message.Body = body;

//                                if (!string.IsNullOrEmpty(campaign.Attachments))
//                                {
//                                    try
//                                    {
//                                        var attachmentList = JsonSerializer.Deserialize<List<string>>(campaign.Attachments);
//                                        if (attachmentList != null && attachmentList.Any())
//                                        {
//                                            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
//                                            foreach (var attachmentPath in attachmentList)
//                                            {
//                                                var cleanPath = attachmentPath.TrimStart('/');
//                                                var fullPath = Path.Combine(basePath, cleanPath);

//                                                if (System.IO.File.Exists(fullPath))
//                                                {
//                                                    var attachment = new Attachment(fullPath);
//                                                    message.Attachments.Add(attachment);
//                                                    _logger.LogInformation($"Attachment added: {Path.GetFileName(fullPath)}");
//                                                }
//                                            }
//                                        }
//                                    }
//                                    catch (Exception ex)
//                                    {
//                                        _logger.LogError($"Error processing attachments: {ex.Message}");
//                                    }
//                                }

//                                await client.SendMailAsync(message);
//                                await LogEmailSend(campaignId, recipient, true);
//                                successCount++;
//                                _logger.LogInformation($"Email sent to {recipient}");
//                            }
//                        }
//                        catch (Exception ex)
//                        {
//                            failCount++;
//                            _logger.LogError($"Failed to send email to {recipient}: {ex.Message}");
//                            await LogEmailSend(campaignId, recipient, false, ex.Message);
//                        }
//                    }

//                    _logger.LogInformation($"Email send complete. Success: {successCount}, Failed: {failCount}");
//                    return successCount > 0;
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"Error sending email campaign: {ex.Message}");
//                return false;
//            }
//        }

//        // ============================================================
//        // SEND WHATSAPP CAMPAIGN
//        // ============================================================
//        // ============================================================
//        // SEND WHATSAPP CAMPAIGN - NO TEMPLATES
//        // ============================================================
//        private async Task<bool> SendWhatsAppCampaignAsync(int campaignId)
//        {
//            try
//            {
//                var campaign = GetCampaignById(campaignId);
//                if (campaign == null)
//                {
//                    _logger.LogError($"Campaign {campaignId} not found");
//                    return false;
//                }

//                var allRecipients = GetRecipients(campaign);
//                var recipients = allRecipients
//                    .Select(r => CleanPhoneNumber(r))
//                    .Where(r => !string.IsNullOrEmpty(r) && r.Length >= 10)
//                    .ToList();

//                _logger.LogInformation($"Found {recipients.Count} valid WhatsApp recipients");

//                if (!recipients.Any())
//                {
//                    _logger.LogWarning($"No valid WhatsApp recipients found");
//                    return false;
//                }

//                int successCount = 0;
//                int failCount = 0;

//                using (var scope = _serviceScopeFactory.CreateScope())
//                {
//                    var whatsAppService = scope.ServiceProvider.GetRequiredService<WhatsAppService>();

//                    foreach (var recipient in recipients)
//                    {
//                        try
//                        {
//                            var message = campaign.Content ?? "";

//                            // Remove HTML tags for WhatsApp
//                            var plainMessage = System.Text.RegularExpressions.Regex.Replace(message, "<.*?>", string.Empty);

//                            // Add sender info
//                            var fullMessage = $"{plainMessage}\n\n- {campaign.Sender ?? "Marketing Team"}";

//                            _logger.LogInformation($"Sending WhatsApp to: {recipient}");

//                            // Send text message (no template)
//                            var success = await whatsAppService.SendTextMessageAsync(recipient, fullMessage);

//                            if (success)
//                            {
//                                successCount++;
//                                _logger.LogInformation($"WhatsApp message sent to {recipient}");
//                                await LogWhatsAppSend(campaignId, recipient, true);
//                            }
//                            else
//                            {
//                                failCount++;
//                                _logger.LogError($"Failed to send WhatsApp message to {recipient}");
//                                await LogWhatsAppSend(campaignId, recipient, false, "Send failed");
//                            }
//                        }
//                        catch (Exception ex)
//                        {
//                            failCount++;
//                            _logger.LogError($"Error sending WhatsApp to {recipient}: {ex.Message}");
//                            await LogWhatsAppSend(campaignId, recipient, false, ex.Message);
//                        }
//                    }
//                }

//                _logger.LogInformation($"WhatsApp send complete. Success: {successCount}, Failed: {failCount}");
//                return successCount > 0;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"Error sending WhatsApp campaign: {ex.Message}");
//                return false;
//            }
//        }

//        // ============================================================
//        // SEND CAMPAIGN IMMEDIATELY - BYPASS SCHEDULER
//        // ============================================================
//        [HttpGet]
//        public async Task<IActionResult> SendNow(int id)
//        {
//            try
//            {
//                var campaign = GetCampaignById(id);
//                if (campaign == null)
//                {
//                    TempData["Error"] = "Campaign not found.";
//                    return RedirectToAction(nameof(Index));
//                }

//                _logger.LogInformation($"Manually sending campaign {id} now");

//                var success = await SendCampaignAsync(id);

//                if (success)
//                {
//                    TempData["Success"] = $"Campaign '{campaign.CampaignName}' sent successfully!";
//                }
//                else
//                {
//                    TempData["Error"] = $"Failed to send campaign '{campaign.CampaignName}'.";
//                }

//                return RedirectToAction(nameof(Details), new { id });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error sending campaign {id} immediately");
//                TempData["Error"] = $"Error: {ex.Message}";
//                return RedirectToAction(nameof(Index));
//            }
//        }
//        // ============================================================
//        // GET WHATSAPP SETTINGS
//        // ============================================================
//        private WhatsAppSettings GetWhatsAppSettings()
//        {
//            try
//            {
//                using (var conn = new SqlConnection(GetConnectionString()))
//                {
//                    var query = "SELECT TOP 1 * FROM WhatsAppSettings WHERE IsActive = 1 AND CompanyId = @CompanyId";
//                    using (var cmd = new SqlCommand(query, conn))
//                    {
//                        cmd.Parameters.AddWithValue("@CompanyId", GetCompanyId() ?? 1);
//                        conn.Open();

//                        using (var reader = cmd.ExecuteReader())
//                        {
//                            if (reader.Read())
//                            {
//                                return new WhatsAppSettings
//                                {
//                                    Id = Convert.ToInt32(reader["Id"]),
//                                    PhoneNumberId = reader["PhoneNumberId"]?.ToString() ?? "",
//                                    BusinessAccountId = reader["BusinessAccountId"]?.ToString() ?? "",
//                                    AccessToken = reader["AccessToken"]?.ToString() ?? "",
//                                    TestNumber = reader["TestNumber"]?.ToString() ?? "",
//                                    PhoneNumber = reader["PhoneNumber"]?.ToString(),
//                                    BusinessName = reader["BusinessName"]?.ToString(),
//                                    IsActive = Convert.ToBoolean(reader["IsActive"])
//                                };
//                            }
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"Error getting WhatsApp settings: {ex.Message}");
//            }
//            return null;
//        }

//        // ============================================================
//        // GET CUSTOMER PHONE NUMBERS BY IDS
//        // ============================================================
//        private List<string> GetCustomerPhoneNumbersByIds(List<int> ids)
//        {
//            var phoneNumbers = new List<string>();

//            if (!ids.Any()) return phoneNumbers;

//            try
//            {
//                var idsStr = string.Join(",", ids);
//                using (var conn = new SqlConnection(GetConnectionString()))
//                {
//                    var query = $"SELECT MobileNumber FROM Customers WHERE Id IN ({idsStr}) AND MobileNumber IS NOT NULL AND MobileNumber != ''";
//                    using (var cmd = new SqlCommand(query, conn))
//                    {
//                        conn.Open();

//                        using (var reader = cmd.ExecuteReader())
//                        {
//                            while (reader.Read())
//                            {
//                                var phone = reader["MobileNumber"]?.ToString();
//                                if (!string.IsNullOrEmpty(phone))
//                                {
//                                    phoneNumbers.Add(CleanPhoneNumber(phone));
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"Error getting customer phone numbers: {ex.Message}");
//            }

//            return phoneNumbers;
//        }

//        // ============================================================
//        // GET LEAD PHONE NUMBERS BY IDS
//        // ============================================================
//        private List<string> GetLeadPhoneNumbersByIds(List<int> ids)
//        {
//            var phoneNumbers = new List<string>();

//            if (!ids.Any()) return phoneNumbers;

//            try
//            {
//                var idsStr = string.Join(",", ids);
//                using (var conn = new SqlConnection(GetConnectionString()))
//                {
//                    var query = $"SELECT MobileNo FROM Leads WHERE Id IN ({idsStr}) AND MobileNo IS NOT NULL AND MobileNo != ''";
//                    using (var cmd = new SqlCommand(query, conn))
//                    {
//                        conn.Open();

//                        using (var reader = cmd.ExecuteReader())
//                        {
//                            while (reader.Read())
//                            {
//                                var phone = reader["MobileNo"]?.ToString();
//                                if (!string.IsNullOrEmpty(phone))
//                                {
//                                    phoneNumbers.Add(CleanPhoneNumber(phone));
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"Error getting lead phone numbers: {ex.Message}");
//            }

//            return phoneNumbers;
//        }

//        // ============================================================
//        // CLEAN PHONE NUMBER
//        // ============================================================
//        private string CleanPhoneNumber(string phone)
//        {
//            if (string.IsNullOrEmpty(phone))
//                return string.Empty;

//            var cleaned = new string(phone.Where(c => char.IsDigit(c)).ToArray());

//            if (cleaned.StartsWith("0"))
//                cleaned = cleaned.Substring(1);

//            return cleaned;
//        }

//        // ============================================================
//        // GET RECIPIENTS
//        // ============================================================
//        private List<string> GetRecipients(Campaign campaign)
//        {
//            var recipients = new List<string>();

//            switch (campaign.RecipientSource)
//            {
//                case "segment":
//                    if (!string.IsNullOrEmpty(campaign.RecipientList))
//                    {
//                        try
//                        {
//                            recipients = JsonSerializer.Deserialize<List<string>>(campaign.RecipientList) ?? new List<string>();
//                            _logger.LogInformation($"Deserialized {recipients.Count} recipients from RecipientList");
//                        }
//                        catch (Exception ex)
//                        {
//                            _logger.LogError($"Error deserializing RecipientList: {ex.Message}");
//                            recipients = new List<string>();
//                        }
//                    }
//                    break;

//                case "manual":
//                    if (!string.IsNullOrEmpty(campaign.RecipientManual))
//                    {
//                        recipients = campaign.RecipientManual
//                            .Split(new[] { '\n', ',' }, StringSplitOptions.RemoveEmptyEntries)
//                            .Select(e => e.Trim())
//                            .Where(e => !string.IsNullOrEmpty(e))
//                            .ToList();
//                        _logger.LogInformation($"Parsed {recipients.Count} recipients from RecipientManual");
//                    }
//                    break;

//                case "excel":
//                    if (!string.IsNullOrEmpty(campaign.RecipientExcelEmails))
//                    {
//                        try
//                        {
//                            recipients = JsonSerializer.Deserialize<List<string>>(campaign.RecipientExcelEmails) ?? new List<string>();
//                            _logger.LogInformation($"Deserialized {recipients.Count} recipients from RecipientExcelEmails");
//                        }
//                        catch (Exception ex)
//                        {
//                            _logger.LogError($"Error deserializing RecipientExcelEmails: {ex.Message}");
//                            recipients = new List<string>();
//                        }
//                    }
//                    break;
//            }

//            return recipients.Distinct().ToList();
//        }

//        // ============================================================
//        // ADD TO SCHEDULER QUEUE
//        // ============================================================
//        public async Task AddToSchedulerQueueAsync(int campaignId, DateTime scheduledTime)
//        {
//            try
//            {
//                using (var conn = new SqlConnection(GetConnectionString()))
//                {
//                    var query = @"
//                        IF NOT EXISTS (SELECT 1 FROM CampaignScheduler WHERE CampaignId = @CampaignId AND Status = 'Pending')
//                        BEGIN
//                            INSERT INTO CampaignScheduler (CampaignId, ScheduledTime, Status, CreatedDate)
//                            VALUES (@CampaignId, @ScheduledTime, 'Pending', GETDATE())
//                        END
//                        ELSE
//                        BEGIN
//                            UPDATE CampaignScheduler 
//                            SET ScheduledTime = @ScheduledTime, 
//                                Status = 'Pending', 
//                                ModifiedDate = GETDATE()
//                            WHERE CampaignId = @CampaignId AND Status = 'Pending'
//                        END";

//                    using (var cmd = new SqlCommand(query, conn))
//                    {
//                        cmd.Parameters.AddWithValue("@CampaignId", campaignId);
//                        cmd.Parameters.AddWithValue("@ScheduledTime", scheduledTime);

//                        conn.Open();
//                        await cmd.ExecuteNonQueryAsync();
//                        _logger.LogInformation($"Added campaign {campaignId} to scheduler queue");
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"Error adding to scheduler queue: {ex.Message}");
//                throw;
//            }
//        }

//        // ============================================================
//        // UPDATE SCHEDULER STATUS
//        // ============================================================
//        private async Task UpdateSchedulerStatus(int campaignId, string status)
//        {
//            try
//            {
//                using (var conn = new SqlConnection(GetConnectionString()))
//                {
//                    var query = @"
//                        UPDATE CampaignScheduler 
//                        SET Status = @Status, 
//                            ProcessedDate = GETDATE(),
//                            ModifiedDate = GETDATE()
//                        WHERE CampaignId = @CampaignId";

//                    using (var cmd = new SqlCommand(query, conn))
//                    {
//                        cmd.Parameters.AddWithValue("@CampaignId", campaignId);
//                        cmd.Parameters.AddWithValue("@Status", status);

//                        conn.Open();
//                        await cmd.ExecuteNonQueryAsync();
//                        _logger.LogInformation($"Updated scheduler status to {status}");
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"Error updating scheduler status: {ex.Message}");
//            }
//        }

//        // ============================================================
//        // LOG WHATSAPP SEND
//        // ============================================================
//        private async Task LogWhatsAppSend(int campaignId, string to, bool success, string error = null)
//        {
//            try
//            {
//                using (var conn = new SqlConnection(GetConnectionString()))
//                {
//                    var query = @"
//                        INSERT INTO WhatsAppLogs (
//                            CampaignId, [To], Status, Error, SentAt
//                        ) VALUES (
//                            @CampaignId, @To, @Status, @Error, @SentAt
//                        )";

//                    using (var cmd = new SqlCommand(query, conn))
//                    {
//                        cmd.Parameters.AddWithValue("@CampaignId", campaignId);
//                        cmd.Parameters.AddWithValue("@To", to);
//                        cmd.Parameters.AddWithValue("@Status", success ? "sent" : "failed");
//                        cmd.Parameters.AddWithValue("@Error", error ?? (object)DBNull.Value);
//                        cmd.Parameters.AddWithValue("@SentAt", success ? DateTime.Now : (object)DBNull.Value);

//                        conn.Open();
//                        await cmd.ExecuteNonQueryAsync();
//                        _logger.LogInformation($"Logged WhatsApp send: {to}, Success: {success}");
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"Error logging WhatsApp send: {ex.Message}");
//            }
//        }

//        // ============================================================
//        // LOG EMAIL SEND
//        // ============================================================
//        private async Task LogEmailSend(int campaignId, string email, bool success, string error = null)
//        {
//            try
//            {
//                using (var conn = new SqlConnection(GetConnectionString()))
//                {
//                    var query = @"
//                        INSERT INTO EmailLogs (
//                            CampaignId, [To], Status, Error, SentAt
//                        ) VALUES (
//                            @CampaignId, @To, @Status, @Error, @SentAt
//                        )";

//                    using (var cmd = new SqlCommand(query, conn))
//                    {
//                        cmd.Parameters.AddWithValue("@CampaignId", campaignId);
//                        cmd.Parameters.AddWithValue("@To", email);
//                        cmd.Parameters.AddWithValue("@Status", success ? "sent" : "failed");
//                        cmd.Parameters.AddWithValue("@Error", error ?? (object)DBNull.Value);
//                        cmd.Parameters.AddWithValue("@SentAt", success ? DateTime.Now : (object)DBNull.Value);

//                        conn.Open();
//                        await cmd.ExecuteNonQueryAsync();
//                        _logger.LogInformation($"Logged email send: {email}, Success: {success}");
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"Error logging email send: {ex.Message}");
//            }
//        }

//        // ============================================================
//        // UPDATE CAMPAIGN STATUS
//        // ============================================================
//        private void UpdateCampaignStatus(int campaignId, string status, DateTime? sentAt = null)
//        {
//            try
//            {
//                using (var conn = new SqlConnection(GetConnectionString()))
//                {
//                    var query = "UPDATE Campaigns SET Status = @Status, SentAt = @SentAt, ModifiedDate = GETDATE() WHERE Id = @Id";

//                    using (var cmd = new SqlCommand(query, conn))
//                    {
//                        cmd.Parameters.AddWithValue("@Status", status);
//                        cmd.Parameters.AddWithValue("@SentAt", sentAt ?? (object)DBNull.Value);
//                        cmd.Parameters.AddWithValue("@Id", campaignId);

//                        conn.Open();
//                        cmd.ExecuteNonQuery();
//                        _logger.LogInformation($"Updated campaign {campaignId} status to {status}");
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"Error updating campaign status: {ex.Message}");
//            }
//        }

//        // ============================================================
//        // SAVE ATTACHMENT
//        // ============================================================
//        private string SaveAttachment(IFormFile file)
//        {
//            try
//            {
//                var baseDirectory = Directory.GetCurrentDirectory();
//                var uploadsFolder = Path.Combine(baseDirectory, "wwwroot", "uploads", "campaigns");

//                if (!Directory.Exists(uploadsFolder))
//                {
//                    Directory.CreateDirectory(uploadsFolder);
//                }

//                var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
//                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

//                using (var stream = new FileStream(filePath, FileMode.Create))
//                {
//                    file.CopyTo(stream);
//                }

//                if (System.IO.File.Exists(filePath))
//                {
//                    return $"/uploads/campaigns/{uniqueFileName}";
//                }

//                return null;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"Error saving attachment: {ex.Message}");
//                return null;
//            }
//        }

//        // ============================================================
//        // PROCESS EXCEL FILE FOR PHONE NUMBERS
//        // ============================================================
//        private async Task<List<(string PhoneNumber, string Name, string Company)>> ProcessExcelFileForPhoneNumbersAsync(IFormFile file)
//        {
//            var phoneList = new List<(string PhoneNumber, string Name, string Company)>();

//            if (file == null || file.Length == 0)
//                return phoneList;

//            try
//            {
//                using (var stream = new MemoryStream())
//                {
//                    await file.CopyToAsync(stream);
//                    stream.Position = 0;

//                    using (var workbook = new XLWorkbook(stream))
//                    {
//                        var worksheet = workbook.Worksheet(1);
//                        var rows = worksheet.RowsUsed();

//                        if (rows.Count() == 0)
//                            return phoneList;

//                        int startRow = 1;
//                        var firstRow = rows.First();
//                        var firstCell = firstRow.Cell(1).GetString().ToLower().Trim();
//                        if (firstCell.Contains("phone") || firstCell.Contains("mobile") || firstCell.Contains("number"))
//                        {
//                            startRow = 2;
//                        }

//                        foreach (var row in rows.Skip(startRow - 1))
//                        {
//                            var phone = CleanPhoneNumber(row.Cell(1).GetString().Trim());
//                            var name = row.Cell(2).GetString().Trim();
//                            var company = row.Cell(3).GetString().Trim();

//                            if (!string.IsNullOrEmpty(phone) && phone.Length >= 10)
//                            {
//                                phoneList.Add((phone, name, company));
//                            }
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"Error processing Excel file for phone numbers: {ex.Message}");
//            }

//            return phoneList;
//        }

//        // ============================================================
//        // PROCESS EXCEL FILE WITH DETAILS
//        // ============================================================
//        private async Task<List<(string Email, string Name, string Company)>> ProcessExcelFileWithDetailsAsync(IFormFile file)
//        {
//            var emailList = new List<(string Email, string Name, string Company)>();

//            if (file == null || file.Length == 0)
//                return emailList;

//            try
//            {
//                using (var stream = new MemoryStream())
//                {
//                    await file.CopyToAsync(stream);
//                    stream.Position = 0;

//                    using (var workbook = new XLWorkbook(stream))
//                    {
//                        var worksheet = workbook.Worksheet(1);
//                        var rows = worksheet.RowsUsed();

//                        if (rows.Count() == 0)
//                            return emailList;

//                        int startRow = 1;
//                        var firstRow = rows.First();
//                        var firstCell = firstRow.Cell(1).GetString().ToLower().Trim();
//                        if (firstCell.Contains("email") || firstCell.Contains("mail") || firstCell == "email*")
//                        {
//                            startRow = 2;
//                        }

//                        foreach (var row in rows.Skip(startRow - 1))
//                        {
//                            var email = row.Cell(1).GetString().Trim();
//                            var name = row.Cell(2).GetString().Trim();
//                            var company = row.Cell(3).GetString().Trim();

//                            if (!string.IsNullOrEmpty(email) && IsValidEmail(email))
//                            {
//                                emailList.Add((email, name, company));
//                            }
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"Error processing Excel file: {ex.Message}");
//            }

//            return emailList;
//        }

//        // ============================================================
//        // VALIDATE EMAIL
//        // ============================================================
//        private bool IsValidEmail(string email)
//        {
//            try
//            {
//                var addr = new System.Net.Mail.MailAddress(email);
//                return addr.Address == email && email.Contains(".");
//            }
//            catch
//            {
//                return false;
//            }
//        }

//        // ============================================================
//        // DOWNLOAD EXCEL TEMPLATE
//        // ============================================================
//        public IActionResult DownloadEmailTemplate()
//        {
//            try
//            {
//                using (var workbook = new XLWorkbook())
//                {
//                    var worksheet = workbook.Worksheets.Add("Email List");

//                    worksheet.Row(1).Cell(1).Value = "Email*";
//                    worksheet.Row(1).Cell(2).Value = "Name";
//                    worksheet.Row(1).Cell(3).Value = "Company";

//                    var headerRange = worksheet.Range(1, 1, 1, 3);
//                    headerRange.Style.Font.Bold = true;
//                    headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(68, 114, 196);
//                    headerRange.Style.Font.FontColor = XLColor.White;
//                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
//                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

//                    var sampleData = new[]
//                    {
//                        new { Email = "john.doe@example.com", Name = "John Doe", Company = "Acme Corp" },
//                        new { Email = "jane.smith@example.com", Name = "Jane Smith", Company = "Tech Solutions" },
//                        new { Email = "bob.johnson@example.com", Name = "Bob Johnson", Company = "Global Enterprises" },
//                        new { Email = "alice.williams@example.com", Name = "Alice Williams", Company = "Innovation Hub" },
//                        new { Email = "", Name = "", Company = "" }
//                    };

//                    for (int i = 0; i < sampleData.Length; i++)
//                    {
//                        var row = i + 2;
//                        worksheet.Cell(row, 1).Value = sampleData[i].Email;
//                        worksheet.Cell(row, 2).Value = sampleData[i].Name;
//                        worksheet.Cell(row, 3).Value = sampleData[i].Company;
//                    }

//                    worksheet.Columns().AdjustToContents();

//                    int instructionsRow = sampleData.Length + 3;
//                    worksheet.Cell(instructionsRow, 1).Value = "INSTRUCTIONS:";
//                    worksheet.Row(instructionsRow).Style.Font.Bold = true;
//                    worksheet.Cell(instructionsRow + 1, 1).Value = "1. Add email addresses in Email column (required)";
//                    worksheet.Cell(instructionsRow + 2, 1).Value = "2. Name and Company are optional";
//                    worksheet.Cell(instructionsRow + 3, 1).Value = "3. Save and upload the file";
//                    worksheet.Cell(instructionsRow + 4, 1).Value = "4. Supports unlimited rows";

//                    var instructionRange = worksheet.Range(instructionsRow, 1, instructionsRow + 4, 1);
//                    instructionRange.Style.Font.FontColor = XLColor.DarkGray;
//                    instructionRange.Style.Font.FontSize = 10;

//                    var noteRow = instructionsRow + 6;
//                    worksheet.Cell(noteRow, 1).Value = "Note: Email column is required. Rows without emails will be ignored.";
//                    worksheet.Row(noteRow).Style.Font.Italic = true;
//                    worksheet.Row(noteRow).Style.Font.FontColor = XLColor.Red;
//                    worksheet.Row(noteRow).Style.Font.FontSize = 10;

//                    using (var stream = new MemoryStream())
//                    {
//                        workbook.SaveAs(stream);
//                        var content = stream.ToArray();
//                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Email_Template.xlsx");
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"Error downloading email template: {ex.Message}");
//                TempData["Error"] = "Error downloading template.";
//                return RedirectToAction(nameof(Create));
//            }
//        }

//        // ============================================================
//        // DOWNLOAD WHATSAPP EXCEL TEMPLATE
//        // ============================================================
//        public IActionResult DownloadWhatsAppTemplate()
//        {
//            try
//            {
//                using (var workbook = new XLWorkbook())
//                {
//                    var worksheet = workbook.Worksheets.Add("WhatsApp List");

//                    worksheet.Row(1).Cell(1).Value = "Phone Number*";
//                    worksheet.Row(1).Cell(2).Value = "Name";
//                    worksheet.Row(1).Cell(3).Value = "Company";

//                    var headerRange = worksheet.Range(1, 1, 1, 3);
//                    headerRange.Style.Font.Bold = true;
//                    headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(46, 160, 67);
//                    headerRange.Style.Font.FontColor = XLColor.White;
//                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
//                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

//                    var sampleData = new[]
//                    {
//                        new { Phone = "9876543210", Name = "John Doe", Company = "Acme Corp" },
//                        new { Phone = "9876543211", Name = "Jane Smith", Company = "Tech Solutions" },
//                        new { Phone = "9876543212", Name = "Bob Johnson", Company = "Global Enterprises" },
//                        new { Phone = "9876543213", Name = "Alice Williams", Company = "Innovation Hub" },
//                        new { Phone = "", Name = "", Company = "" }
//                    };

//                    for (int i = 0; i < sampleData.Length; i++)
//                    {
//                        var row = i + 2;
//                        worksheet.Cell(row, 1).Value = sampleData[i].Phone;
//                        worksheet.Cell(row, 2).Value = sampleData[i].Name;
//                        worksheet.Cell(row, 3).Value = sampleData[i].Company;
//                    }

//                    worksheet.Columns().AdjustToContents();

//                    int instructionsRow = sampleData.Length + 3;
//                    worksheet.Cell(instructionsRow, 1).Value = "INSTRUCTIONS:";
//                    worksheet.Row(instructionsRow).Style.Font.Bold = true;
//                    worksheet.Cell(instructionsRow + 1, 1).Value = "1. Add phone numbers in Phone Number column (required)";
//                    worksheet.Cell(instructionsRow + 2, 1).Value = "2. Name and Company are optional";
//                    worksheet.Cell(instructionsRow + 3, 1).Value = "3. Save and upload the file";
//                    worksheet.Cell(instructionsRow + 4, 1).Value = "4. Supports unlimited rows";
//                    worksheet.Cell(instructionsRow + 5, 1).Value = "5. Phone numbers should be 10 digits or more";

//                    var instructionRange = worksheet.Range(instructionsRow, 1, instructionsRow + 5, 1);
//                    instructionRange.Style.Font.FontColor = XLColor.DarkGray;
//                    instructionRange.Style.Font.FontSize = 10;

//                    var noteRow = instructionsRow + 7;
//                    worksheet.Cell(noteRow, 1).Value = "Note: Phone Number column is required. Rows without phone numbers will be ignored.";
//                    worksheet.Row(noteRow).Style.Font.Italic = true;
//                    worksheet.Row(noteRow).Style.Font.FontColor = XLColor.Red;
//                    worksheet.Row(noteRow).Style.Font.FontSize = 10;

//                    using (var stream = new MemoryStream())
//                    {
//                        workbook.SaveAs(stream);
//                        var content = stream.ToArray();
//                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "WhatsApp_Template.xlsx");
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"Error downloading WhatsApp template: {ex.Message}");
//                TempData["Error"] = "Error downloading template.";
//                return RedirectToAction(nameof(Create));
//            }
//        }

//        // ============================================================
//        // TRIGGER SCHEDULER
//        // ============================================================
//        [HttpGet]
//        public async Task<IActionResult> TriggerScheduler()
//        {
//            try
//            {
//                var dueCampaigns = await GetDueCampaignsFromDb();

//                foreach (var campaignId in dueCampaigns)
//                {
//                    await SendCampaignAsync(campaignId);
//                    await UpdateSchedulerStatus(campaignId, "Completed");
//                }

//                TempData["Success"] = $"Scheduler triggered! Found {dueCampaigns.Count} due campaigns.";
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"Error triggering scheduler: {ex.Message}");
//                TempData["Error"] = $"Error: {ex.Message}";
//            }

//            return RedirectToAction(nameof(Index));
//        }

//        private async Task<List<int>> GetDueCampaignsFromDb()
//        {
//            var campaigns = new List<int>();
//            var now = DateTime.Now;

//            using (var conn = new SqlConnection(GetConnectionString()))
//            {
//                var query = @"
//                    SELECT CampaignId 
//                    FROM CampaignScheduler 
//                    WHERE Status = 'Pending' 
//                    AND ScheduledTime <= @Now
//                    AND IsActive = 1
//                    ORDER BY ScheduledTime ASC";

//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    cmd.Parameters.AddWithValue("@Now", now);
//                    conn.Open();

//                    using (var reader = await cmd.ExecuteReaderAsync())
//                    {
//                        while (await reader.ReadAsync())
//                        {
//                            campaigns.Add(Convert.ToInt32(reader["CampaignId"]));
//                        }
//                    }
//                }
//            }

//            return campaigns;
//        }

//        // ============================================================
//        // ADD RECIPIENTS - POST
//        // ============================================================
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public IActionResult AddRecipients(int id, string manualEmails)
//        {
//            try
//            {
//                var campaign = GetCampaignById(id);
//                if (campaign == null)
//                {
//                    TempData["Error"] = "Campaign not found.";
//                    return RedirectToAction(nameof(Index));
//                }

//                if (!string.IsNullOrEmpty(manualEmails))
//                {
//                    var emails = manualEmails
//                        .Split(new[] { '\n', ',' }, StringSplitOptions.RemoveEmptyEntries)
//                        .Select(e => e.Trim())
//                        .Where(e => !string.IsNullOrEmpty(e) && e.Contains("@"))
//                        .Distinct()
//                        .ToList();

//                    if (emails.Any())
//                    {
//                        var existing = new List<string>();
//                        if (!string.IsNullOrEmpty(campaign.RecipientManual))
//                        {
//                            existing = campaign.RecipientManual
//                                .Split(new[] { '\n', ',' }, StringSplitOptions.RemoveEmptyEntries)
//                                .Select(e => e.Trim())
//                                .Where(e => !string.IsNullOrEmpty(e))
//                                .ToList();
//                        }

//                        var allEmails = existing.Concat(emails).Distinct().ToList();
//                        campaign.RecipientManual = string.Join("\n", allEmails);
//                        campaign.RecipientSource = "manual";

//                        using (var conn = new SqlConnection(GetConnectionString()))
//                        {
//                            var query = @"
//                                UPDATE Campaigns SET 
//                                RecipientManual = @RecipientManual,
//                                RecipientSource = @RecipientSource
//                                WHERE Id = @Id";

//                            using (var cmd = new SqlCommand(query, conn))
//                            {
//                                cmd.Parameters.AddWithValue("@Id", id);
//                                cmd.Parameters.AddWithValue("@RecipientManual", campaign.RecipientManual ?? (object)DBNull.Value);
//                                cmd.Parameters.AddWithValue("@RecipientSource", campaign.RecipientSource);

//                                conn.Open();
//                                cmd.ExecuteNonQuery();
//                            }
//                        }

//                        TempData["Success"] = $"Added {emails.Count} recipients!";
//                    }
//                }

//                return RedirectToAction(nameof(Details), new { id });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error adding recipients: {ex.Message}");
//                TempData["Error"] = $"Error: {ex.Message}";
//                return RedirectToAction(nameof(Details), new { id });
//            }
//        }

//        // ============================================================
//        // DELETE - GET
//        // ============================================================
//        public IActionResult Delete(int id)
//        {
//            try
//            {
//                var campaign = GetCampaignById(id);
//                if (campaign == null)
//                {
//                    TempData["Error"] = "Campaign not found.";
//                    return RedirectToAction(nameof(Index));
//                }

//                return View(campaign);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error in Delete GET: {ex.Message}");
//                TempData["Error"] = "Error loading delete confirmation.";
//                return RedirectToAction(nameof(Index));
//            }
//        }

//        // ============================================================
//        // DELETE - POST
//        // ============================================================
//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public IActionResult DeleteConfirmed(int id)
//        {
//            try
//            {
//                using (var conn = new SqlConnection(GetConnectionString()))
//                {
//                    var query = @"
//                        UPDATE Campaigns SET 
//                        IsDeleted = 1, 
//                        ModifiedDate = GETDATE(),
//                        ModifiedBy = @ModifiedBy 
//                        WHERE Id = @Id";

//                    using (var cmd = new SqlCommand(query, conn))
//                    {
//                        cmd.Parameters.AddWithValue("@Id", id);
//                        cmd.Parameters.AddWithValue("@ModifiedBy", GetCurrentUser());

//                        conn.Open();
//                        var rowsAffected = cmd.ExecuteNonQuery();

//                        if (rowsAffected > 0)
//                        {
//                            TempData["Success"] = "Campaign deleted successfully!";
//                        }
//                        else
//                        {
//                            TempData["Error"] = "Failed to delete campaign.";
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error in Delete POST: {ex.Message}");
//                TempData["Error"] = "An error occurred while deleting.";
//            }

//            return RedirectToAction(nameof(Index));
//        }

//        // ============================================================
//        // REPORT - GET
//        // ============================================================
//        public IActionResult Report(int id)
//        {
//            try
//            {
//                ViewData["Title"] = "Campaign Report";
//                ViewData["Module"] = "CRM";
//                ViewData["Icon"] = "chart-bar";

//                var campaign = GetCampaignById(id);
//                if (campaign == null)
//                {
//                    TempData["Error"] = "Campaign not found.";
//                    return RedirectToAction(nameof(Index));
//                }

//                var counts = GetCampaignCounts(id);
//                campaign.RecipientCount = counts.RecipientCount;
//                campaign.OpenCount = counts.OpenCount;

//                var stats = GetCampaignStats(id);
//                var logs = GetCampaignLogs(id);

//                ViewBag.Stats = stats;
//                ViewBag.Logs = logs;

//                return View(campaign);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error in Report: {ex.Message}");
//                TempData["Error"] = "Error loading campaign report.";
//                return RedirectToAction(nameof(Index));
//            }
//        }

//        // ============================================================
//        // EXPORT REPORT - GET
//        // ============================================================
//        public IActionResult ExportReport(int id)
//        {
//            try
//            {
//                var logs = GetCampaignLogs(id);

//                var csv = "Email,Status,Opened,Open Count,Link Clicked,Attachment Opened,IP,City,Region,Country,Sent At\n";
//                foreach (var log in logs)
//                {
//                    csv += $"{log.To},{log.Status},{log.IsOpened},{log.OpenCount},{log.LinkClicked},{log.AttachmentOpened},{log.Ip},{log.City},{log.Region},{log.Country},{log.SentAt}\n";
//                }

//                var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
//                return File(bytes, "text/csv", $"campaign-report-{id}.csv");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error exporting report: {ex.Message}");
//                TempData["Error"] = "Error exporting report.";
//                return RedirectToAction(nameof(Details), new { id });
//            }
//        }

//        // ============================================================
//        // SCHEDULER STATUS
//        // ============================================================
//        public IActionResult SchedulerStatus()
//        {
//            return Json(new
//            {
//                running = true,
//                message = "Scheduler service is running",
//                lastCheck = DateTime.Now
//            });
//        }

//        // ============================================================
//        // GET CAMPAIGNS
//        // ============================================================
//        private List<Campaign> GetCampaigns()
//        {
//            var campaigns = new List<Campaign>();
//            var companyId = GetCompanyId();

//            using (var conn = new SqlConnection(GetConnectionString()))
//            {
//                var query = @"
//                    SELECT * FROM Campaigns c
//                    WHERE c.CompanyId = @CompanyId AND c.IsDeleted = 0
//                    ORDER BY c.CreatedDate DESC";

//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    cmd.Parameters.AddWithValue("@CompanyId", companyId ?? 1);
//                    conn.Open();

//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            campaigns.Add(MapToCampaign(reader));
//                        }
//                    }
//                }
//            }

//            return campaigns;
//        }

//        // ============================================================
//        // GET CAMPAIGN BY ID
//        // ============================================================
//        private Campaign? GetCampaignById(int id)
//        {
//            using (var conn = new SqlConnection(GetConnectionString()))
//            {
//                var query = "SELECT * FROM Campaigns WHERE Id = @Id AND IsDeleted = 0";
//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    cmd.Parameters.AddWithValue("@Id", id);
//                    conn.Open();

//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        if (reader.Read())
//                        {
//                            return MapToCampaign(reader);
//                        }
//                    }
//                }
//            }
//            return null;
//        }

//        // ============================================================
//        // MAP TO CAMPAIGN
//        // ============================================================
//        private Campaign MapToCampaign(SqlDataReader reader)
//        {
//            return new Campaign
//            {
//                Id = Convert.ToInt32(reader["Id"]),
//                CampaignName = reader["CampaignName"]?.ToString() ?? "",
//                ScheduledTime = reader["ScheduledTime"] != DBNull.Value ? Convert.ToDateTime(reader["ScheduledTime"]) : null,
//                CampaignChannel = reader["Channel"]?.ToString() ?? "",
//                Sender = reader["Sender"]?.ToString() ?? "",
//                Content = reader["Content"]?.ToString() ?? "",
//                EmailSubject = reader["EmailSubject"]?.ToString(),
//                CtaText = reader["CtaText"]?.ToString(),
//                Attachments = reader["Attachments"]?.ToString(),
//                RecipientSource = reader["RecipientSource"]?.ToString() ?? "",
//                RecipientList = reader["RecipientList"]?.ToString(),
//                RecipientManual = reader["RecipientManual"]?.ToString(),
//                RecipientExcelEmails = reader["RecipientExcelEmails"]?.ToString(),
//                Status = reader["Status"]?.ToString() ?? "Scheduled",
//                SentAt = reader["SentAt"] != DBNull.Value ? Convert.ToDateTime(reader["SentAt"]) : null,
//                EmailMasterId = reader["EmailMasterId"] != DBNull.Value ? Convert.ToInt32(reader["EmailMasterId"]) : (int?)null,
//                CompanyId = reader["CompanyId"] != DBNull.Value ? Convert.ToInt32(reader["CompanyId"]) : (int?)null,
//                CreatedBy = reader["CreatedBy"] != DBNull.Value ? Convert.ToInt32(reader["CreatedBy"]) : null,
//                CreatedDate = reader["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedDate"]) : null,
//                ModifiedDate = reader["ModifiedDate"] != DBNull.Value ? Convert.ToDateTime(reader["ModifiedDate"]) : null,
//                ModifiedBy = reader["ModifiedBy"]?.ToString(),
//                IsActive = Convert.ToBoolean(reader["IsActive"]),
//                IsDeleted = Convert.ToBoolean(reader["IsDeleted"]),
//                RecipientCount = 0,
//                OpenCount = 0
//            };
//        }

//        // ============================================================
//        // GET CAMPAIGN COUNTS
//        // ============================================================
//        private (int RecipientCount, int OpenCount) GetCampaignCounts(int campaignId)
//        {
//            int recipientCount = 0;
//            int openCount = 0;

//            using (var conn = new SqlConnection(GetConnectionString()))
//            {
//                var query = @"
//                    SELECT 
//                        (SELECT COUNT(*) FROM EmailLogs WHERE CampaignId = @CampaignId) as RecipientCount,
//                        (SELECT COUNT(*) FROM EmailLogs WHERE CampaignId = @CampaignId AND IsOpened = 1) as OpenCount";

//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    cmd.Parameters.AddWithValue("@CampaignId", campaignId);
//                    conn.Open();

//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        if (reader.Read())
//                        {
//                            recipientCount = reader["RecipientCount"] != DBNull.Value ? Convert.ToInt32(reader["RecipientCount"]) : 0;
//                            openCount = reader["OpenCount"] != DBNull.Value ? Convert.ToInt32(reader["OpenCount"]) : 0;
//                        }
//                    }
//                }
//            }

//            return (recipientCount, openCount);
//        }

//        // ============================================================
//        // GET CAMPAIGN SUMMARY
//        // ============================================================
//        private Dictionary<string, int> GetCampaignSummary()
//        {
//            var summary = new Dictionary<string, int>();

//            using (var conn = new SqlConnection(GetConnectionString()))
//            {
//                var query = @"
//                    SELECT 
//                        COUNT(*) as Total,
//                        SUM(CASE WHEN Status = 'Scheduled' THEN 1 ELSE 0 END) as Scheduled,
//                        SUM(CASE WHEN Status = 'Sent' THEN 1 ELSE 0 END) as Sent,
//                        SUM(CASE WHEN Status = 'Failed' THEN 1 ELSE 0 END) as Failed
//                    FROM Campaigns 
//                    WHERE CompanyId = @CompanyId AND IsDeleted = 0";

//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    cmd.Parameters.AddWithValue("@CompanyId", GetCompanyId() ?? 1);
//                    conn.Open();

//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        if (reader.Read())
//                        {
//                            summary["Total"] = reader["Total"] != DBNull.Value ? Convert.ToInt32(reader["Total"]) : 0;
//                            summary["Scheduled"] = reader["Scheduled"] != DBNull.Value ? Convert.ToInt32(reader["Scheduled"]) : 0;
//                            summary["Sent"] = reader["Sent"] != DBNull.Value ? Convert.ToInt32(reader["Sent"]) : 0;
//                            summary["Failed"] = reader["Failed"] != DBNull.Value ? Convert.ToInt32(reader["Failed"]) : 0;
//                        }
//                    }
//                }
//            }

//            return summary;
//        }

//        // ============================================================
//        // GET CAMPAIGN STATS
//        // ============================================================
//        private CampaignStats GetCampaignStats(int campaignId)
//        {
//            var stats = new CampaignStats();

//            using (var conn = new SqlConnection(GetConnectionString()))
//            {
//                var query = @"
//                    SELECT 
//                        COUNT(*) as Total,
//                        SUM(CASE WHEN IsOpened = 1 THEN 1 ELSE 0 END) as Opens,
//                        SUM(CASE WHEN AttachmentOpened = 1 THEN 1 ELSE 0 END) as Attachments,
//                        SUM(CASE WHEN LinkClicked = 1 THEN 1 ELSE 0 END) as Clicks,
//                        SUM(CASE WHEN Status = 'failed' THEN 1 ELSE 0 END) as Failed
//                    FROM EmailLogs 
//                    WHERE CampaignId = @CampaignId";

//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    cmd.Parameters.AddWithValue("@CampaignId", campaignId);
//                    conn.Open();

//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        if (reader.Read())
//                        {
//                            stats.Total = reader["Total"] != DBNull.Value ? Convert.ToInt32(reader["Total"]) : 0;
//                            stats.Opens = reader["Opens"] != DBNull.Value ? Convert.ToInt32(reader["Opens"]) : 0;
//                            stats.Attachments = reader["Attachments"] != DBNull.Value ? Convert.ToInt32(reader["Attachments"]) : 0;
//                            stats.Clicks = reader["Clicks"] != DBNull.Value ? Convert.ToInt32(reader["Clicks"]) : 0;
//                            stats.Failed = reader["Failed"] != DBNull.Value ? Convert.ToInt32(reader["Failed"]) : 0;
//                        }
//                    }
//                }
//            }

//            return stats;
//        }

//        // ============================================================
//        // GET CAMPAIGN LOGS
//        // ============================================================
//        private List<EmailLog> GetCampaignLogs(int campaignId)
//        {
//            var logs = new List<EmailLog>();

//            using (var conn = new SqlConnection(GetConnectionString()))
//            {
//                var query = "SELECT * FROM EmailLogs WHERE CampaignId = @CampaignId ORDER BY CreatedDate DESC";

//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    cmd.Parameters.AddWithValue("@CampaignId", campaignId);
//                    conn.Open();

//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            logs.Add(new EmailLog
//                            {
//                                Id = Convert.ToInt32(reader["Id"]),
//                                CompanyId = reader["CompanyId"] != DBNull.Value ? Convert.ToInt32(reader["CompanyId"]) : (int?)null,
//                                CampaignId = reader["CampaignId"] != DBNull.Value ? Convert.ToInt32(reader["CampaignId"]) : (int?)null,
//                                EmailMasterId = reader["EmailMasterId"] != DBNull.Value ? Convert.ToInt32(reader["EmailMasterId"]) : (int?)null,
//                                To = reader["To"]?.ToString() ?? "",
//                                Status = reader["Status"]?.ToString() ?? "sending",
//                                Error = reader["Error"]?.ToString(),
//                                IsOpened = Convert.ToBoolean(reader["IsOpened"]),
//                                OpenCount = Convert.ToInt32(reader["OpenCount"]),
//                                FirstOpenedAt = reader["FirstOpenedAt"] != DBNull.Value ? Convert.ToDateTime(reader["FirstOpenedAt"]) : null,
//                                LastOpenedAt = reader["LastOpenedAt"] != DBNull.Value ? Convert.ToDateTime(reader["LastOpenedAt"]) : null,
//                                LinkClicked = Convert.ToBoolean(reader["LinkClicked"]),
//                                ClickCount = Convert.ToInt32(reader["ClickCount"]),
//                                ClickedAt = reader["ClickedAt"] != DBNull.Value ? Convert.ToDateTime(reader["ClickedAt"]) : null,
//                                LastClickUrl = reader["LastClickUrl"]?.ToString(),
//                                AttachmentOpened = Convert.ToBoolean(reader["AttachmentOpened"]),
//                                Ip = reader["Ip"]?.ToString(),
//                                UserAgent = reader["UserAgent"]?.ToString(),
//                                City = reader["City"]?.ToString(),
//                                Region = reader["Region"]?.ToString(),
//                                Country = reader["Country"]?.ToString(),
//                                SentAt = reader["SentAt"] != DBNull.Value ? Convert.ToDateTime(reader["SentAt"]) : null,
//                                CreatedDate = Convert.ToDateTime(reader["CreatedDate"])
//                            });
//                        }
//                    }
//                }
//            }

//            return logs;
//        }

//        // ============================================================
//        // GET CUSTOMERS
//        // ============================================================
//        private List<Customer> GetCustomers()
//        {
//            var customers = new List<Customer>();

//            try
//            {
//                var companyId = GetCompanyId();

//                using (var conn = new SqlConnection(GetConnectionString()))
//                {
//                    var query = @"
//                        SELECT Id, CustomerName, PrimaryEmail, MobileNumber
//                        FROM Customers 
//                        WHERE IsActive = 1 AND (CompanyId = @CompanyId OR CompanyId IS NULL)
//                        ORDER BY CustomerName";

//                    using (var cmd = new SqlCommand(query, conn))
//                    {
//                        cmd.Parameters.AddWithValue("@CompanyId", companyId ?? 1);
//                        conn.Open();

//                        using (var reader = cmd.ExecuteReader())
//                        {
//                            while (reader.Read())
//                            {
//                                customers.Add(new Customer
//                                {
//                                    Id = Convert.ToInt32(reader["Id"]),
//                                    CustomerName = reader["CustomerName"]?.ToString() ?? "",
//                                    PrimaryEmail = reader["PrimaryEmail"]?.ToString(),
//                                    MobileNumber = reader["MobileNumber"]?.ToString() ?? ""
//                                });
//                            }
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"Error getting customers: {ex.Message}");
//            }

//            return customers;
//        }

//        // ============================================================
//        // GET LEADS
//        // ============================================================
//        private List<Lead> GetLeads()
//        {
//            var leads = new List<Lead>();

//            try
//            {
//                var companyId = GetCompanyId();

//                using (var conn = new SqlConnection(GetConnectionString()))
//                {
//                    var query = @"
//                        SELECT Id, FirstName, MiddleName, LastName, Email, MobileNo
//                        FROM Leads 
//                        WHERE IsActive = 1 AND (CompanyId = @CompanyId OR CompanyId IS NULL)
//                        ORDER BY FirstName";

//                    using (var cmd = new SqlCommand(query, conn))
//                    {
//                        cmd.Parameters.AddWithValue("@CompanyId", companyId ?? 1);
//                        conn.Open();

//                        using (var reader = cmd.ExecuteReader())
//                        {
//                            while (reader.Read())
//                            {
//                                leads.Add(new Lead
//                                {
//                                    Id = Convert.ToInt32(reader["Id"]),
//                                    FirstName = reader["FirstName"]?.ToString() ?? "",
//                                    MiddleName = reader["MiddleName"]?.ToString() ?? "",
//                                    LastName = reader["LastName"]?.ToString() ?? "",
//                                    Email = reader["Email"]?.ToString(),
//                                    MobileNo = reader["MobileNo"]?.ToString() ?? ""
//                                });
//                            }
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"Error getting leads: {ex.Message}");
//            }

//            return leads;
//        }

//        // ============================================================
//        // GET CUSTOMER EMAILS BY IDS
//        // ============================================================
//        private List<string> GetCustomerEmailsByIds(List<int> ids)
//        {
//            var emails = new List<string>();

//            if (!ids.Any()) return emails;

//            try
//            {
//                var idsStr = string.Join(",", ids);
//                using (var conn = new SqlConnection(GetConnectionString()))
//                {
//                    var query = $"SELECT PrimaryEmail FROM Customers WHERE Id IN ({idsStr}) AND PrimaryEmail IS NOT NULL AND PrimaryEmail != ''";
//                    using (var cmd = new SqlCommand(query, conn))
//                    {
//                        conn.Open();

//                        using (var reader = cmd.ExecuteReader())
//                        {
//                            while (reader.Read())
//                            {
//                                var email = reader["PrimaryEmail"]?.ToString();
//                                if (!string.IsNullOrEmpty(email))
//                                {
//                                    emails.Add(email);
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"Error getting customer emails: {ex.Message}");
//            }

//            return emails;
//        }

//        // ============================================================
//        // GET LEAD EMAILS BY IDS
//        // ============================================================
//        private List<string> GetLeadEmailsByIds(List<int> ids)
//        {
//            var emails = new List<string>();

//            if (!ids.Any()) return emails;

//            try
//            {
//                var idsStr = string.Join(",", ids);
//                using (var conn = new SqlConnection(GetConnectionString()))
//                {
//                    var query = $"SELECT Email FROM Leads WHERE Id IN ({idsStr}) AND Email IS NOT NULL AND Email != ''";
//                    using (var cmd = new SqlCommand(query, conn))
//                    {
//                        conn.Open();

//                        using (var reader = cmd.ExecuteReader())
//                        {
//                            while (reader.Read())
//                            {
//                                var email = reader["Email"]?.ToString();
//                                if (!string.IsNullOrEmpty(email))
//                                {
//                                    emails.Add(email);
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"Error getting lead emails: {ex.Message}");
//            }

//            return emails;
//        }

//        // ============================================================
//        // GET ACTIVE EMAIL MASTERS
//        // ============================================================
//        private List<EmailMaster> GetActiveEmailMasters()
//        {
//            var masters = new List<EmailMaster>();

//            try
//            {
//                using (var conn = new SqlConnection(GetConnectionString()))
//                {
//                    var query = "SELECT Id, Email, Owner, Service FROM EmailMasters WHERE IsActive = 1";
//                    using (var cmd = new SqlCommand(query, conn))
//                    {
//                        conn.Open();

//                        using (var reader = cmd.ExecuteReader())
//                        {
//                            while (reader.Read())
//                            {
//                                masters.Add(new EmailMaster
//                                {
//                                    Id = Convert.ToInt32(reader["Id"]),
//                                    Email = reader["Email"]?.ToString() ?? "",
//                                    Owner = reader["Owner"]?.ToString(),
//                                    Service = reader["Service"]?.ToString() ?? "gmail"
//                                });
//                            }
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"Error getting email masters: {ex.Message}");
//            }

//            return masters;
//        }
//    }
//}
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Erp_Aits.Models;
using Erp_Aits.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using System.Text;
using ClosedXML.Excel;
using ExcelDataReader;

namespace Erp_Aits.Controllers
{
    public class CampaignsController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CampaignsController> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public CampaignsController(
            IConfiguration configuration,
            ILogger<CampaignsController> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            // Register ExcelDataReader encoding
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("DefaultConnection");
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

            try
            {
                using (var conn = new SqlConnection(GetConnectionString()))
                {
                    var query = "SELECT TOP 1 Id FROM Companies WHERE IsActive = 1";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        var result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            var id = Convert.ToInt32(result);
                            HttpContext.Session.SetString("CompanyId", id.ToString());
                            return id;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting CompanyId: {ex.Message}");
            }

            return 1;
        }

        private string GetSmtpUser()
        {
            return _configuration["EmailSettings:User"] ?? "pritammore139@gmail.com";
        }

        private string GetSmtpPassword()
        {
            return _configuration["EmailSettings:Password"] ?? "uuqn vtrq nqqu poqi";
        }

        private string GetBaseUrl()
        {
            return _configuration["BaseUrl"] ?? "https://localhost:7183";
        }

        // ============================================================
        // INDEX - List all campaigns
        // ============================================================
        public IActionResult Index()
        {
            try
            {
                ViewData["Title"] = "Campaigns";
                ViewData["Module"] = "CRM";
                ViewData["Icon"] = "bullhorn";

                var companyId = GetCompanyId();
                _logger.LogInformation($"Index action - CompanyId: {companyId}");

                var campaigns = GetCampaigns();
                _logger.LogInformation($"Index action - Found {campaigns.Count} campaigns");

                foreach (var campaign in campaigns)
                {
                    var counts = GetCampaignCounts(campaign.Id);
                    campaign.RecipientCount = counts.RecipientCount;
                    campaign.OpenCount = counts.OpenCount;
                }

                var summary = GetCampaignSummary();

                ViewBag.Summary = summary;
                ViewBag.CompanyId = companyId;

                return View(campaigns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Index action");
                TempData["Error"] = $"Error loading campaigns: {ex.Message}";
                return View(new List<Campaign>());
            }
        }

        // ============================================================
        // DETAILS - View single campaign
        // ============================================================
        public IActionResult Details(int id)
        {
            try
            {
                ViewData["Title"] = "Campaign Details";
                ViewData["Module"] = "CRM";
                ViewData["Icon"] = "bullhorn";

                var campaign = GetCampaignById(id);
                if (campaign == null)
                {
                    TempData["Error"] = "Campaign not found.";
                    return RedirectToAction(nameof(Index));
                }

                var counts = GetCampaignCounts(id);
                campaign.RecipientCount = counts.RecipientCount;
                campaign.OpenCount = counts.OpenCount;

                var stats = GetCampaignStats(id);
                var logs = GetCampaignLogs(id);

                ViewBag.Stats = stats;
                ViewBag.Logs = logs;

                return View(campaign);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in Details action for ID: {id}");
                TempData["Error"] = "Error loading campaign details.";
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
                ViewData["Title"] = "Campaign Studio";
                ViewData["Module"] = "CRM";
                ViewData["Icon"] = "plus";

                var companyId = GetCompanyId();
                _logger.LogInformation($"Create GET - CompanyId: {companyId}");

                ViewBag.Customers = GetCustomers();
                ViewBag.Leads = GetLeads();
                ViewBag.EmailMasters = GetActiveEmailMasters();
                ViewBag.WhatsAppSettings = GetWhatsAppSettings();

                var campaign = new Campaign
                {
                    ScheduledTime = DateTime.Now.AddHours(1),
                    CampaignChannel = "email",
                    CompanyId = companyId,
                    RecipientSource = "segment"
                };

                return View(campaign);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Create GET action");
                TempData["Error"] = "Error loading create form.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ============================================================
        // CREATE - POST (FIXED FOR BOTH XLSX AND XLS)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Campaign campaign, string? manualEmails, string? selectedCustomers, string? selectedLeads, string? whatsappContent, List<IFormFile>? attachments)
        {
            try
            {
                // ============================================================
                // STEP 1: CLEAR MODELSTATE
                // ============================================================
                ModelState.Clear();
                _logger.LogInformation("ModelState cleared at start");

                _logger.LogInformation($"===== CREATE CAMPAIGN =====");
                _logger.LogInformation($"RecipientSource: {campaign.RecipientSource}");
                _logger.LogInformation($"CampaignChannel: {campaign.CampaignChannel}");
                _logger.LogInformation($"Attachments Count: {attachments?.Count ?? 0}");

                // ============================================================
                // STEP 2: MAP WHATSAPP CONTENT
                // ============================================================
                if (campaign.CampaignChannel == "whatsapp" || campaign.CampaignChannel == "both")
                {
                    if (!string.IsNullOrEmpty(whatsappContent))
                    {
                        campaign.Content = whatsappContent;
                        _logger.LogInformation($"WhatsApp content mapped: {campaign.Content?.Length ?? 0} chars");
                    }
                }

                // ============================================================
                // STEP 3: SET SCHEDULED TIME
                // ============================================================
                if (!campaign.ScheduledTime.HasValue)
                {
                    campaign.ScheduledTime = DateTime.Now.AddHours(1);
                }

                // ============================================================
                // STEP 4: VALIDATE REQUIRED FIELDS
                // ============================================================
                var validationErrors = new List<string>();

                if (string.IsNullOrWhiteSpace(campaign.CampaignName))
                    validationErrors.Add("Campaign Name is required");

                if (string.IsNullOrWhiteSpace(campaign.Sender))
                    validationErrors.Add("Sender is required");

                if (!campaign.ScheduledTime.HasValue)
                    validationErrors.Add("Scheduled Time is required");

                if (string.IsNullOrWhiteSpace(campaign.Content))
                {
                    if (campaign.CampaignChannel == "whatsapp" || campaign.CampaignChannel == "both")
                        validationErrors.Add("WhatsApp message is required");
                    else
                        validationErrors.Add("Email content is required");
                }

                if (string.IsNullOrWhiteSpace(campaign.RecipientSource))
                    validationErrors.Add("Recipient Source is required");

                // ============================================================
                // STEP 5: PROCESS RECIPIENTS
                // ============================================================
                bool hasRecipients = true;
                string errorMessage = "";

                if (campaign.RecipientSource == "segment")
                {
                    var hasCustomers = !string.IsNullOrEmpty(selectedCustomers);
                    var hasLeads = !string.IsNullOrEmpty(selectedLeads);

                    if (!hasCustomers && !hasLeads)
                    {
                        hasRecipients = false;
                        errorMessage = "Please select at least one customer or lead.";
                    }
                    else
                    {
                        var recipients = new List<string>();

                        if (!string.IsNullOrEmpty(selectedCustomers))
                        {
                            var customerIds = selectedCustomers.Split(',').Select(int.Parse).ToList();

                            if (campaign.CampaignChannel == "both")
                            {
                                recipients.AddRange(GetCustomerEmailsByIds(customerIds));
                                recipients.AddRange(GetCustomerPhoneNumbersByIds(customerIds));
                            }
                            else if (campaign.CampaignChannel == "whatsapp")
                            {
                                recipients.AddRange(GetCustomerPhoneNumbersByIds(customerIds));
                            }
                            else
                            {
                                recipients.AddRange(GetCustomerEmailsByIds(customerIds));
                            }
                        }

                        if (!string.IsNullOrEmpty(selectedLeads))
                        {
                            var leadIds = selectedLeads.Split(',').Select(int.Parse).ToList();

                            if (campaign.CampaignChannel == "both")
                            {
                                recipients.AddRange(GetLeadEmailsByIds(leadIds));
                                recipients.AddRange(GetLeadPhoneNumbersByIds(leadIds));
                            }
                            else if (campaign.CampaignChannel == "whatsapp")
                            {
                                recipients.AddRange(GetLeadPhoneNumbersByIds(leadIds));
                            }
                            else
                            {
                                recipients.AddRange(GetLeadEmailsByIds(leadIds));
                            }
                        }

                        campaign.RecipientList = JsonSerializer.Serialize(recipients.Distinct());
                        _logger.LogInformation($"Total segment recipients: {recipients.Distinct().Count()}");
                    }
                }
                else if (campaign.RecipientSource == "manual")
                {
                    if (string.IsNullOrEmpty(manualEmails))
                    {
                        hasRecipients = false;
                        errorMessage = "Please enter at least one email address or phone number.";
                    }
                    else
                    {
                        var rawInputs = manualEmails
                            .Split(new[] { '\n', ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(e => e.Trim())
                            .Where(e => !string.IsNullOrEmpty(e))
                            .Distinct()
                            .ToList();

                        if (!rawInputs.Any())
                        {
                            hasRecipients = false;
                            errorMessage = "Please enter at least one valid entry.";
                        }
                        else
                        {
                            if (campaign.CampaignChannel == "whatsapp" || campaign.CampaignChannel == "both")
                            {
                                var phoneNumbers = rawInputs
                                    .Select(r => CleanPhoneNumber(r))
                                    .Where(r => !string.IsNullOrEmpty(r) && r.Length >= 10)
                                    .ToList();

                                if (!phoneNumbers.Any())
                                {
                                    hasRecipients = false;
                                    errorMessage = "Please enter at least one valid phone number (minimum 10 digits).";
                                }
                                else
                                {
                                    campaign.RecipientManual = string.Join("\n", phoneNumbers);
                                }
                            }
                            else
                            {
                                var emails = rawInputs
                                    .Where(e => e.Contains("@") && IsValidEmail(e))
                                    .ToList();

                                if (!emails.Any())
                                {
                                    hasRecipients = false;
                                    errorMessage = "Please enter at least one valid email address.";
                                }
                                else
                                {
                                    campaign.RecipientManual = string.Join("\n", emails);
                                }
                            }
                        }
                    }
                }
                else if (campaign.RecipientSource == "excel")
                {
                    if (attachments == null || attachments.Count == 0)
                    {
                        hasRecipients = false;
                        errorMessage = "Please upload an Excel file.";
                    }
                    else
                    {
                        var file = attachments[0];
                        var fileExtension = Path.GetExtension(file.FileName).ToLower();

                        // Check if it's an Excel file
                        if (fileExtension == ".xlsx" || fileExtension == ".xls" || fileExtension == ".csv")
                        {
                            try
                            {
                                // ============================================================
                                // FIXED: PROPER EXCEL PROCESSING FOR BOTH XLSX AND XLS
                                // ============================================================
                                var (emailData, phoneData) = await ProcessExcelFileProperlyAsync(file, campaign.CampaignChannel);

                                if (!emailData.Any() && !phoneData.Any())
                                {
                                    hasRecipients = false;
                                    errorMessage = campaign.CampaignChannel == "whatsapp" || campaign.CampaignChannel == "both"
                                        ? "No valid phone numbers found in the Excel file."
                                        : "No valid email addresses found in the Excel file.";
                                }
                                else
                                {
                                    if (campaign.CampaignChannel == "whatsapp" || campaign.CampaignChannel == "both")
                                    {
                                        campaign.RecipientExcelEmails = JsonSerializer.Serialize(phoneData);
                                        _logger.LogInformation($"Added {phoneData.Count} excel phone numbers");
                                    }
                                    else
                                    {
                                        campaign.RecipientExcelEmails = JsonSerializer.Serialize(emailData);
                                        _logger.LogInformation($"Added {emailData.Count} excel emails");
                                    }
                                }

                                // Handle additional attachments (non-Excel files)
                                if (attachments.Count > 1)
                                {
                                    var attachmentUrls = new List<string>();
                                    for (int i = 1; i < attachments.Count; i++)
                                    {
                                        var url = SaveAttachment(attachments[i]);
                                        if (!string.IsNullOrEmpty(url))
                                        {
                                            attachmentUrls.Add(url);
                                        }
                                    }
                                    if (attachmentUrls.Any())
                                    {
                                        campaign.Attachments = JsonSerializer.Serialize(attachmentUrls);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"Error processing Excel file: {ex.Message}");
                                hasRecipients = false;
                                errorMessage = $"Error processing Excel file: {ex.Message}";
                            }
                        }
                        else
                        {
                            // It's not an Excel file, treat as regular attachment
                            var attachmentUrls = new List<string>();
                            var url = SaveAttachment(file);
                            if (!string.IsNullOrEmpty(url))
                            {
                                attachmentUrls.Add(url);
                            }
                            campaign.Attachments = JsonSerializer.Serialize(attachmentUrls);

                            hasRecipients = false;
                            errorMessage = "Please upload an Excel file (.xlsx, .xls, or .csv) with email addresses or phone numbers.";
                        }
                    }
                }

                // Handle regular attachments (non-Excel)
                if (campaign.RecipientSource != "excel" && attachments != null && attachments.Any())
                {
                    var attachmentUrls = new List<string>();
                    foreach (var file in attachments)
                    {
                        var url = SaveAttachment(file);
                        if (!string.IsNullOrEmpty(url))
                        {
                            attachmentUrls.Add(url);
                        }
                    }
                    if (attachmentUrls.Any())
                    {
                        campaign.Attachments = JsonSerializer.Serialize(attachmentUrls);
                    }
                }

                if (!hasRecipients)
                {
                    validationErrors.Add(errorMessage);
                }

                // ============================================================
                // STEP 6: CHECK VALIDATION ERRORS
                // ============================================================
                if (validationErrors.Any())
                {
                    _logger.LogWarning($"Validation errors: {string.Join(" | ", validationErrors)}");
                    foreach (var error in validationErrors)
                    {
                        ModelState.AddModelError("", error);
                    }

                    ViewBag.Customers = GetCustomers();
                    ViewBag.Leads = GetLeads();
                    ViewBag.EmailMasters = GetActiveEmailMasters();
                    ViewBag.WhatsAppSettings = GetWhatsAppSettings();
                    return View(campaign);
                }

                // ============================================================
                // STEP 7: CREATE CAMPAIGN
                // ============================================================
                campaign.CompanyId = GetCompanyId();
                campaign.CreatedBy = GetCompanyId();
                campaign.CreatedDate = DateTime.Now;
                campaign.IsActive = true;
                campaign.IsDeleted = false;
                campaign.Status = "Scheduled";

                using (var conn = new SqlConnection(GetConnectionString()))
                {
                    var query = @"
                        INSERT INTO Campaigns (
                            CampaignName, ScheduledTime, Channel, Sender, Content,
                            EmailSubject, CtaText, Attachments, RecipientSource,
                            RecipientList, RecipientManual, RecipientExcelEmails,
                            Status, EmailMasterId, CompanyId, CreatedBy, CreatedDate
                        ) VALUES (
                            @CampaignName, @ScheduledTime, @Channel, @Sender, @Content,
                            @EmailSubject, @CtaText, @Attachments, @RecipientSource,
                            @RecipientList, @RecipientManual, @RecipientExcelEmails,
                            @Status, @EmailMasterId, @CompanyId, @CreatedBy, @CreatedDate
                        );
                        SELECT SCOPE_IDENTITY();";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CampaignName", campaign.CampaignName ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ScheduledTime", campaign.ScheduledTime ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Channel", campaign.CampaignChannel ?? "email");
                        cmd.Parameters.AddWithValue("@Sender", campaign.Sender ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Content", campaign.Content ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@EmailSubject", campaign.EmailSubject ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@CtaText", campaign.CtaText ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Attachments", campaign.Attachments ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@RecipientSource", campaign.RecipientSource ?? "segment");
                        cmd.Parameters.AddWithValue("@RecipientList", campaign.RecipientList ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@RecipientManual", campaign.RecipientManual ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@RecipientExcelEmails", campaign.RecipientExcelEmails ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Status", campaign.Status);
                        cmd.Parameters.AddWithValue("@EmailMasterId", campaign.EmailMasterId ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@CompanyId", campaign.CompanyId ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@CreatedBy", campaign.CreatedBy ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@CreatedDate", campaign.CreatedDate);

                        conn.Open();
                        var newId = Convert.ToInt32(cmd.ExecuteScalar());

                        _logger.LogInformation($"Campaign created successfully with ID: {newId}");

                        var scheduledTime = campaign.ScheduledTime ?? DateTime.Now;
                        var timeDifference = (scheduledTime - DateTime.Now).TotalMinutes;

                        if (timeDifference <= 5)
                        {
                            _ = Task.Run(async () => await SendCampaignAsync(newId));
                            TempData["Success"] = "Campaign created and sending started!";
                        }
                        else
                        {
                            await AddToSchedulerQueueAsync(newId, scheduledTime);
                            TempData["Success"] = $"Campaign created and scheduled for {scheduledTime:dd MMM yyyy HH:mm}";
                        }

                        return RedirectToAction(nameof(Details), new { id = newId });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Create POST action");
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
            }

            ViewBag.Customers = GetCustomers();
            ViewBag.Leads = GetLeads();
            ViewBag.EmailMasters = GetActiveEmailMasters();
            ViewBag.WhatsAppSettings = GetWhatsAppSettings();

            return View(campaign);
        }

        // ============================================================
        // FIXED: PROPER EXCEL PROCESSING FOR BOTH XLSX AND XLS
        // ============================================================
        private async Task<(List<string> Emails, List<string> PhoneNumbers)> ProcessExcelFileProperlyAsync(IFormFile file, string channel)
        {
            var emails = new List<string>();
            var phoneNumbers = new List<string>();

            if (file == null || file.Length == 0)
                return (emails, phoneNumbers);

            try
            {
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                bool isWhatsApp = (channel == "whatsapp" || channel == "both");

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    // ============================================================
                    // HANDLE XLSX WITH CLOSEDXML
                    // ============================================================
                    if (fileExtension == ".xlsx" || fileExtension == ".xls")
                    {
                        using (var workbook = new XLWorkbook(stream))
                        {
                            var worksheet = workbook.Worksheet(1);
                            var rows = worksheet.RowsUsed();

                            if (rows.Count() == 0)
                                return (emails, phoneNumbers);

                            // Determine if first row is header
                            int startRow = 1;
                            var firstRow = rows.First();
                            var firstCell = firstRow.Cell(1).GetString().ToLower().Trim();

                            if (firstCell.Contains("email") || firstCell.Contains("mail") ||
                                firstCell.Contains("phone") || firstCell.Contains("mobile") ||
                                firstCell == "email*")
                            {
                                startRow = 2;
                            }

                            foreach (var row in rows.Skip(startRow - 1))
                            {
                                if (row.IsEmpty())
                                    continue;

                                var col1 = row.Cell(1).GetString().Trim();
                                var name = row.Cell(2).GetString().Trim();
                                var company = row.Cell(3).GetString().Trim();

                                if (string.IsNullOrEmpty(col1))
                                    continue;

                                if (isWhatsApp)
                                {
                                    var phone = CleanPhoneNumber(col1);
                                    if (!string.IsNullOrEmpty(phone) && phone.Length >= 10)
                                    {
                                        phoneNumbers.Add(phone);
                                        _logger.LogInformation($"Added phone: {phone}");
                                    }
                                }
                                else
                                {
                                    if (IsValidEmail(col1))
                                    {
                                        emails.Add(col1);
                                        _logger.LogInformation($"Added email: {col1}");
                                    }
                                }
                            }
                        }
                    }
                    // ============================================================
                    // HANDLE CSV WITH STREAM READER
                    // ============================================================
                    else if (fileExtension == ".csv")
                    {
                        using (var reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            string line;
                            int rowIndex = 0;
                            bool isFirstRow = true;

                            while ((line = reader.ReadLine()) != null)
                            {
                                rowIndex++;
                                if (string.IsNullOrWhiteSpace(line))
                                    continue;

                                var parts = line.Split(',');

                                if (isFirstRow)
                                {
                                    isFirstRow = false;
                                    var firstCell = parts.Length > 0 ? parts[0].ToLower().Trim() : "";
                                    if (firstCell.Contains("email") || firstCell.Contains("mail") ||
                                        firstCell.Contains("phone") || firstCell.Contains("mobile"))
                                    {
                                        continue; // Skip header row
                                    }
                                }

                                var col1 = parts.Length > 0 ? parts[0].Trim() : "";
                                var name = parts.Length > 1 ? parts[1].Trim() : "";
                                var company = parts.Length > 2 ? parts[2].Trim() : "";

                                if (string.IsNullOrEmpty(col1))
                                    continue;

                                if (isWhatsApp)
                                {
                                    var phone = CleanPhoneNumber(col1);
                                    if (!string.IsNullOrEmpty(phone) && phone.Length >= 10)
                                    {
                                        phoneNumbers.Add(phone);
                                    }
                                }
                                else
                                {
                                    if (IsValidEmail(col1))
                                    {
                                        emails.Add(col1);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Unsupported file type: {fileExtension}");
                        return (emails, phoneNumbers);
                    }
                }

                _logger.LogInformation($"Excel processing complete. Emails: {emails.Count}, PhoneNumbers: {phoneNumbers.Count}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing Excel/CSV file: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                throw;
            }

            return (emails.Distinct().ToList(), phoneNumbers.Distinct().ToList());
        }

        // ============================================================
        // SEND CAMPAIGN - POST
        // ============================================================
        // ============================================================
        // SEND - POST (Schedule or Send)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(int id)
        {
            try
            {
                var campaign = GetCampaignById(id);
                if (campaign == null)
                {
                    TempData["Error"] = "Campaign not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if campaign is already sent
                if (campaign.Status == "Sent")
                {
                    TempData["Error"] = "This campaign has already been sent.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var scheduledTime = campaign.ScheduledTime ?? DateTime.Now;
                var now = DateTime.Now;
                var timeDifference = (scheduledTime - now).TotalMinutes;

                _logger.LogInformation($"Send action called for campaign {id}. Time difference: {timeDifference} minutes");

                if (timeDifference <= 5)
                {
                    // Send immediately if scheduled within 5 minutes
                    _logger.LogInformation($"Sending campaign {id} immediately (time difference: {timeDifference} minutes)");
                    var success = await SendCampaignAsync(id);
                    if (success)
                    {
                        TempData["Success"] = "Campaign sent successfully!";
                    }
                    else
                    {
                        TempData["Error"] = "Failed to send campaign. Please check logs.";
                    }
                }
                else
                {
                    // Schedule for later
                    _logger.LogInformation($"Scheduling campaign {id} for {scheduledTime}");
                    await AddToSchedulerQueueAsync(id, scheduledTime);
                    UpdateCampaignStatus(id, "Scheduled", null);
                    TempData["Success"] = $"Campaign scheduled to send on {scheduledTime:dd MMM yyyy HH:mm}";
                }

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing campaign {id}");
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // ============================================================
        // SEND CAMPAIGN ASYNC
        // ============================================================
        public async Task<bool> SendCampaignAsync(int campaignId)
        {
            try
            {
                var campaign = GetCampaignById(campaignId);
                if (campaign == null)
                {
                    _logger.LogError($"Campaign {campaignId} not found");
                    return false;
                }

                _logger.LogInformation($"===== SEND CAMPAIGN STARTED - Campaign ID: {campaignId} =====");
                _logger.LogInformation($"CampaignChannel: {campaign.CampaignChannel}");
                _logger.LogInformation($"Campaign Name: {campaign.CampaignName}");

                bool success = false;

                if (campaign.CampaignChannel == "whatsapp")
                {
                    success = await SendWhatsAppCampaignAsync(campaignId);
                }
                else if (campaign.CampaignChannel == "email")
                {
                    success = await SendEmailCampaignAsync(campaignId);
                }
                else if (campaign.CampaignChannel == "both")
                {
                    var emailTask = SendEmailCampaignAsync(campaignId);
                    var whatsappTask = SendWhatsAppCampaignAsync(campaignId);
                    await Task.WhenAll(emailTask, whatsappTask);
                    success = emailTask.Result || whatsappTask.Result;
                }

                if (success)
                {
                    UpdateCampaignStatus(campaignId, "Sent", DateTime.Now);
                    await UpdateSchedulerStatus(campaignId, "Completed");
                }
                else
                {
                    UpdateCampaignStatus(campaignId, "Failed", DateTime.Now);
                    await UpdateSchedulerStatus(campaignId, "Failed");
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in SendCampaignAsync: {ex.Message}");
                UpdateCampaignStatus(campaignId, "Failed", DateTime.Now);
                await UpdateSchedulerStatus(campaignId, "Error");
                return false;
            }
        }

        // ============================================================
        // SEND EMAIL CAMPAIGN
        // ============================================================
        private async Task<bool> SendEmailCampaignAsync(int campaignId)
        {
            try
            {
                var campaign = GetCampaignById(campaignId);
                if (campaign == null)
                {
                    _logger.LogError($"Campaign {campaignId} not found");
                    return false;
                }

                var allRecipients = GetRecipients(campaign);
                var recipients = allRecipients
                    .Where(r => r.Contains("@") && IsValidEmail(r))
                    .ToList();

                _logger.LogInformation($"Found {recipients.Count} valid email recipients");

                if (!recipients.Any())
                {
                    _logger.LogWarning($"No valid email recipients found for campaign {campaignId}");
                    return false;
                }

                var smtpUser = GetSmtpUser();
                var smtpPassword = GetSmtpPassword();

                using (var client = new SmtpClient("smtp.gmail.com", 587))
                {
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(smtpUser, smtpPassword);
                    client.Timeout = 60000;

                    int successCount = 0;
                    int failCount = 0;

                    foreach (var recipient in recipients)
                    {
                        try
                        {
                            using (var message = new MailMessage())
                            {
                                message.From = new MailAddress(smtpUser, campaign.Sender ?? "Marketing Team");
                                message.To.Add(recipient);
                                message.Subject = campaign.EmailSubject ?? "Campaign Update";
                                message.IsBodyHtml = true;

                                var body = campaign.Content ?? "";
                                var trackingPixel = $"<img src='{GetBaseUrl()}/api/track/open?id={campaignId}&email={recipient}' width='1' height='1' style='display:none;' />";
                                body += trackingPixel;

                                if (!string.IsNullOrEmpty(campaign.CtaText))
                                {
                                    var trackUrl = $"{GetBaseUrl()}/api/track/click?id={campaignId}&email={recipient}&url={System.Net.WebUtility.UrlEncode(campaign.CtaText)}";
                                    body += $"<br/><br/><a href='{trackUrl}' style='display:inline-block;padding:12px 24px;background:#007bff;color:white;text-decoration:none;border-radius:5px;'>{campaign.CtaText}</a>";
                                }

                                message.Body = body;

                                if (!string.IsNullOrEmpty(campaign.Attachments))
                                {
                                    try
                                    {
                                        var attachmentList = JsonSerializer.Deserialize<List<string>>(campaign.Attachments);
                                        if (attachmentList != null && attachmentList.Any())
                                        {
                                            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                                            foreach (var attachmentPath in attachmentList)
                                            {
                                                var cleanPath = attachmentPath.TrimStart('/');
                                                var fullPath = Path.Combine(basePath, cleanPath);
                                                if (System.IO.File.Exists(fullPath))
                                                {
                                                    message.Attachments.Add(new Attachment(fullPath));
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError($"Error processing attachments: {ex.Message}");
                                    }
                                }

                                await client.SendMailAsync(message);
                                await LogEmailSend(campaignId, recipient, true);
                                successCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            failCount++;
                            _logger.LogError($"Failed to send email to {recipient}: {ex.Message}");
                            await LogEmailSend(campaignId, recipient, false, ex.Message);
                        }
                    }

                    return successCount > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending email campaign: {ex.Message}");
                return false;
            }
        }

        // ============================================================
        // SEND WHATSAPP CAMPAIGN
        // ============================================================
        private async Task<bool> SendWhatsAppCampaignAsync(int campaignId)
        {
            try
            {
                var campaign = GetCampaignById(campaignId);
                if (campaign == null)
                {
                    _logger.LogError($"Campaign {campaignId} not found");
                    return false;
                }

                var allRecipients = GetRecipients(campaign);
                var recipients = allRecipients
                    .Select(r => CleanPhoneNumber(r))
                    .Where(r => !string.IsNullOrEmpty(r) && r.Length >= 10)
                    .ToList();

                _logger.LogInformation($"Found {recipients.Count} valid WhatsApp recipients");

                if (!recipients.Any())
                    return false;

                int successCount = 0;
                int failCount = 0;

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var whatsAppService = scope.ServiceProvider.GetRequiredService<WhatsAppService>();

                    foreach (var recipient in recipients)
                    {
                        try
                        {
                            var message = campaign.Content ?? "";
                            var plainMessage = System.Text.RegularExpressions.Regex.Replace(message, "<.*?>", string.Empty);
                            var fullMessage = $"{plainMessage}\n\n- {campaign.Sender ?? "Marketing Team"}";

                            var success = await whatsAppService.SendTextMessageAsync(recipient, fullMessage);

                            if (success)
                            {
                                successCount++;
                                await LogWhatsAppSend(campaignId, recipient, true);
                            }
                            else
                            {
                                failCount++;
                                await LogWhatsAppSend(campaignId, recipient, false, "Send failed");
                            }
                        }
                        catch (Exception ex)
                        {
                            failCount++;
                            _logger.LogError($"Error sending WhatsApp to {recipient}: {ex.Message}");
                            await LogWhatsAppSend(campaignId, recipient, false, ex.Message);
                        }
                    }
                }

                return successCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending WhatsApp campaign: {ex.Message}");
                return false;
            }
        }

        // ============================================================
        // HELPER METHODS (KEEP ALL EXISTING HELPER METHODS)
        // ============================================================

        // ... [Keep all your existing helper methods here: 
        // GetWhatsAppSettings, GetCustomerPhoneNumbersByIds, 
        // GetLeadPhoneNumbersByIds, CleanPhoneNumber, GetRecipients,
        // AddToSchedulerQueueAsync, UpdateSchedulerStatus,
        // LogWhatsAppSend, LogEmailSend, UpdateCampaignStatus,
        // SaveAttachment, IsValidEmail, DownloadEmailTemplate,
        // DownloadWhatsAppTemplate, TriggerScheduler, etc.]

        // ============================================================
        // GET RECIPIENTS
        // ============================================================
        private List<string> GetRecipients(Campaign campaign)
        {
            var recipients = new List<string>();

            switch (campaign.RecipientSource)
            {
                case "segment":
                    if (!string.IsNullOrEmpty(campaign.RecipientList))
                    {
                        try
                        {
                            recipients = JsonSerializer.Deserialize<List<string>>(campaign.RecipientList) ?? new List<string>();
                        }
                        catch { recipients = new List<string>(); }
                    }
                    break;

                case "manual":
                    if (!string.IsNullOrEmpty(campaign.RecipientManual))
                    {
                        recipients = campaign.RecipientManual
                            .Split(new[] { '\n', ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(e => e.Trim())
                            .Where(e => !string.IsNullOrEmpty(e))
                            .ToList();
                    }
                    break;

                case "excel":
                    if (!string.IsNullOrEmpty(campaign.RecipientExcelEmails))
                    {
                        try
                        {
                            recipients = JsonSerializer.Deserialize<List<string>>(campaign.RecipientExcelEmails) ?? new List<string>();
                        }
                        catch { recipients = new List<string>(); }
                    }
                    break;
            }

            return recipients.Distinct().ToList();
        }

        // ============================================================
        // ADD TO SCHEDULER QUEUE
        // ============================================================
        public async Task AddToSchedulerQueueAsync(int campaignId, DateTime scheduledTime)
        {
            try
            {
                using (var conn = new SqlConnection(GetConnectionString()))
                {
                    var query = @"
                        IF NOT EXISTS (SELECT 1 FROM CampaignScheduler WHERE CampaignId = @CampaignId AND Status = 'Pending')
                        BEGIN
                            INSERT INTO CampaignScheduler (CampaignId, ScheduledTime, Status, CreatedDate)
                            VALUES (@CampaignId, @ScheduledTime, 'Pending', GETDATE())
                        END
                        ELSE
                        BEGIN
                            UPDATE CampaignScheduler 
                            SET ScheduledTime = @ScheduledTime, 
                                Status = 'Pending', 
                                ModifiedDate = GETDATE()
                            WHERE CampaignId = @CampaignId AND Status = 'Pending'
                        END";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CampaignId", campaignId);
                        cmd.Parameters.AddWithValue("@ScheduledTime", scheduledTime);
                        conn.Open();
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding to scheduler queue: {ex.Message}");
                throw;
            }
        }

        // ============================================================
        // UPDATE SCHEDULER STATUS
        // ============================================================
        private async Task UpdateSchedulerStatus(int campaignId, string status)
        {
            try
            {
                using (var conn = new SqlConnection(GetConnectionString()))
                {
                    var query = @"
                        UPDATE CampaignScheduler 
                        SET Status = @Status, 
                            ProcessedDate = GETDATE(),
                            ModifiedDate = GETDATE()
                        WHERE CampaignId = @CampaignId";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CampaignId", campaignId);
                        cmd.Parameters.AddWithValue("@Status", status);
                        conn.Open();
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating scheduler status: {ex.Message}");
            }
        }

        // ============================================================
        // LOG WHATSAPP SEND
        // ============================================================
        private async Task LogWhatsAppSend(int campaignId, string to, bool success, string error = null)
        {
            try
            {
                using (var conn = new SqlConnection(GetConnectionString()))
                {
                    var query = @"
                        INSERT INTO WhatsAppLogs (CampaignId, [To], Status, Error, SentAt)
                        VALUES (@CampaignId, @To, @Status, @Error, @SentAt)";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CampaignId", campaignId);
                        cmd.Parameters.AddWithValue("@To", to);
                        cmd.Parameters.AddWithValue("@Status", success ? "sent" : "failed");
                        cmd.Parameters.AddWithValue("@Error", error ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@SentAt", success ? DateTime.Now : (object)DBNull.Value);
                        conn.Open();
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error logging WhatsApp send: {ex.Message}");
            }
        }

        // ============================================================
        // LOG EMAIL SEND
        // ============================================================
        private async Task LogEmailSend(int campaignId, string email, bool success, string error = null)
        {
            try
            {
                using (var conn = new SqlConnection(GetConnectionString()))
                {
                    var query = @"
                        INSERT INTO EmailLogs (CampaignId, [To], Status, Error, SentAt)
                        VALUES (@CampaignId, @To, @Status, @Error, @SentAt)";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CampaignId", campaignId);
                        cmd.Parameters.AddWithValue("@To", email);
                        cmd.Parameters.AddWithValue("@Status", success ? "sent" : "failed");
                        cmd.Parameters.AddWithValue("@Error", error ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@SentAt", success ? DateTime.Now : (object)DBNull.Value);
                        conn.Open();
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error logging email send: {ex.Message}");
            }
        }

        // ============================================================
        // UPDATE CAMPAIGN STATUS
        // ============================================================
        private void UpdateCampaignStatus(int campaignId, string status, DateTime? sentAt = null)
        {
            try
            {
                using (var conn = new SqlConnection(GetConnectionString()))
                {
                    var query = "UPDATE Campaigns SET Status = @Status, SentAt = @SentAt, ModifiedDate = GETDATE() WHERE Id = @Id";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Status", status);
                        cmd.Parameters.AddWithValue("@SentAt", sentAt ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Id", campaignId);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating campaign status: {ex.Message}");
            }
        }

        // ============================================================
        // SAVE ATTACHMENT
        // ============================================================
        private string SaveAttachment(IFormFile file)
        {
            try
            {
                var baseDirectory = Directory.GetCurrentDirectory();
                var uploadsFolder = Path.Combine(baseDirectory, "wwwroot", "uploads", "campaigns");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                return $"/uploads/campaigns/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving attachment: {ex.Message}");
                return null;
            }
        }

        // ============================================================
        // VALIDATE EMAIL
        // ============================================================
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email && email.Contains(".");
            }
            catch
            {
                return false;
            }
        }

        // ============================================================
        // CLEAN PHONE NUMBER
        // ============================================================
        private string CleanPhoneNumber(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return string.Empty;

            var cleaned = new string(phone.Where(c => char.IsDigit(c)).ToArray());

            if (cleaned.StartsWith("0"))
                cleaned = cleaned.Substring(1);

            return cleaned;
        }

        // ============================================================
        // GET WHATSAPP SETTINGS
        // ============================================================
        private WhatsAppSettings GetWhatsAppSettings()
        {
            try
            {
                using (var conn = new SqlConnection(GetConnectionString()))
                {
                    var query = "SELECT TOP 1 * FROM WhatsAppSettings WHERE IsActive = 1 AND CompanyId = @CompanyId";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CompanyId", GetCompanyId() ?? 1);
                        conn.Open();

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new WhatsAppSettings
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    PhoneNumberId = reader["PhoneNumberId"]?.ToString() ?? "",
                                    BusinessAccountId = reader["BusinessAccountId"]?.ToString() ?? "",
                                    AccessToken = reader["AccessToken"]?.ToString() ?? "",
                                    TestNumber = reader["TestNumber"]?.ToString() ?? "",
                                    PhoneNumber = reader["PhoneNumber"]?.ToString(),
                                    BusinessName = reader["BusinessName"]?.ToString(),
                                    IsActive = Convert.ToBoolean(reader["IsActive"])
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting WhatsApp settings: {ex.Message}");
            }
            return null;
        }

        // ============================================================
        // GET CUSTOMER PHONE NUMBERS BY IDS
        // ============================================================
        private List<string> GetCustomerPhoneNumbersByIds(List<int> ids)
        {
            var phoneNumbers = new List<string>();
            if (!ids.Any()) return phoneNumbers;

            try
            {
                var idsStr = string.Join(",", ids);
                using (var conn = new SqlConnection(GetConnectionString()))
                {
                    var query = $"SELECT MobileNumber FROM Customers WHERE Id IN ({idsStr}) AND MobileNumber IS NOT NULL AND MobileNumber != ''";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var phone = reader["MobileNumber"]?.ToString();
                                if (!string.IsNullOrEmpty(phone))
                                {
                                    phoneNumbers.Add(CleanPhoneNumber(phone));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting customer phone numbers: {ex.Message}");
            }

            return phoneNumbers;
        }

        // ============================================================
        // GET LEAD PHONE NUMBERS BY IDS
        // ============================================================
        private List<string> GetLeadPhoneNumbersByIds(List<int> ids)
        {
            var phoneNumbers = new List<string>();
            if (!ids.Any()) return phoneNumbers;

            try
            {
                var idsStr = string.Join(",", ids);
                using (var conn = new SqlConnection(GetConnectionString()))
                {
                    var query = $"SELECT MobileNo FROM Leads WHERE Id IN ({idsStr}) AND MobileNo IS NOT NULL AND MobileNo != ''";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var phone = reader["MobileNo"]?.ToString();
                                if (!string.IsNullOrEmpty(phone))
                                {
                                    phoneNumbers.Add(CleanPhoneNumber(phone));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting lead phone numbers: {ex.Message}");
            }

            return phoneNumbers;
        }

        // ============================================================
        // GET CUSTOMER EMAILS BY IDS
        // ============================================================
        private List<string> GetCustomerEmailsByIds(List<int> ids)
        {
            var emails = new List<string>();
            if (!ids.Any()) return emails;

            try
            {
                var idsStr = string.Join(",", ids);
                using (var conn = new SqlConnection(GetConnectionString()))
                {
                    var query = $"SELECT PrimaryEmail FROM Customers WHERE Id IN ({idsStr}) AND PrimaryEmail IS NOT NULL AND PrimaryEmail != ''";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var email = reader["PrimaryEmail"]?.ToString();
                                if (!string.IsNullOrEmpty(email))
                                {
                                    emails.Add(email);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting customer emails: {ex.Message}");
            }

            return emails;
        }

        // ============================================================
        // GET LEAD EMAILS BY IDS
        // ============================================================
        private List<string> GetLeadEmailsByIds(List<int> ids)
        {
            var emails = new List<string>();
            if (!ids.Any()) return emails;

            try
            {
                var idsStr = string.Join(",", ids);
                using (var conn = new SqlConnection(GetConnectionString()))
                {
                    var query = $"SELECT Email FROM Leads WHERE Id IN ({idsStr}) AND Email IS NOT NULL AND Email != ''";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var email = reader["Email"]?.ToString();
                                if (!string.IsNullOrEmpty(email))
                                {
                                    emails.Add(email);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting lead emails: {ex.Message}");
            }

            return emails;
        }

        // ============================================================
        // GET CUSTOMERS
        // ============================================================
        private List<Customer> GetCustomers()
        {
            var customers = new List<Customer>();

            try
            {
                var companyId = GetCompanyId();
                using (var conn = new SqlConnection(GetConnectionString()))
                {
                    var query = "SELECT Id, CustomerName, PrimaryEmail, MobileNumber FROM Customers WHERE IsActive = 1 AND (CompanyId = @CompanyId OR CompanyId IS NULL) ORDER BY CustomerName";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CompanyId", companyId ?? 1);
                        conn.Open();
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                customers.Add(new Customer
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    CustomerName = reader["CustomerName"]?.ToString() ?? "",
                                    PrimaryEmail = reader["PrimaryEmail"]?.ToString(),
                                    MobileNumber = reader["MobileNumber"]?.ToString() ?? ""
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting customers: {ex.Message}");
            }

            return customers;
        }

        // ============================================================
        // GET LEADS
        // ============================================================
        private List<Lead> GetLeads()
        {
            var leads = new List<Lead>();

            try
            {
                var companyId = GetCompanyId();
                using (var conn = new SqlConnection(GetConnectionString()))
                {
                    var query = "SELECT Id, FirstName, MiddleName, LastName, Email, MobileNo FROM Leads WHERE IsActive = 1 AND (CompanyId = @CompanyId OR CompanyId IS NULL) ORDER BY FirstName";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CompanyId", companyId ?? 1);
                        conn.Open();
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                leads.Add(new Lead
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    FirstName = reader["FirstName"]?.ToString() ?? "",
                                    MiddleName = reader["MiddleName"]?.ToString() ?? "",
                                    LastName = reader["LastName"]?.ToString() ?? "",
                                    Email = reader["Email"]?.ToString(),
                                    MobileNo = reader["MobileNo"]?.ToString() ?? ""
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting leads: {ex.Message}");
            }

            return leads;
        }

        // ============================================================
        // GET ACTIVE EMAIL MASTERS
        // ============================================================
        private List<EmailMaster> GetActiveEmailMasters()
        {
            var masters = new List<EmailMaster>();

            try
            {
                using (var conn = new SqlConnection(GetConnectionString()))
                {
                    var query = "SELECT Id, Email, Owner, Service FROM EmailMasters WHERE IsActive = 1";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                masters.Add(new EmailMaster
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Email = reader["Email"]?.ToString() ?? "",
                                    Owner = reader["Owner"]?.ToString(),
                                    Service = reader["Service"]?.ToString() ?? "gmail"
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting email masters: {ex.Message}");
            }

            return masters;
        }

        // ============================================================
        // GET CAMPAIGN COUNTS
        // ============================================================
        private (int RecipientCount, int OpenCount) GetCampaignCounts(int campaignId)
        {
            int recipientCount = 0;
            int openCount = 0;

            using (var conn = new SqlConnection(GetConnectionString()))
            {
                var query = @"
                    SELECT 
                        (SELECT COUNT(*) FROM EmailLogs WHERE CampaignId = @CampaignId) as RecipientCount,
                        (SELECT COUNT(*) FROM EmailLogs WHERE CampaignId = @CampaignId AND IsOpened = 1) as OpenCount";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CampaignId", campaignId);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            recipientCount = reader["RecipientCount"] != DBNull.Value ? Convert.ToInt32(reader["RecipientCount"]) : 0;
                            openCount = reader["OpenCount"] != DBNull.Value ? Convert.ToInt32(reader["OpenCount"]) : 0;
                        }
                    }
                }
            }

            return (recipientCount, openCount);
        }

        // ============================================================
        // GET CAMPAIGN SUMMARY
        // ============================================================
        private Dictionary<string, int> GetCampaignSummary()
        {
            var summary = new Dictionary<string, int>();

            using (var conn = new SqlConnection(GetConnectionString()))
            {
                var query = @"
                    SELECT 
                        COUNT(*) as Total,
                        SUM(CASE WHEN Status = 'Scheduled' THEN 1 ELSE 0 END) as Scheduled,
                        SUM(CASE WHEN Status = 'Sent' THEN 1 ELSE 0 END) as Sent,
                        SUM(CASE WHEN Status = 'Failed' THEN 1 ELSE 0 END) as Failed
                    FROM Campaigns 
                    WHERE CompanyId = @CompanyId AND IsDeleted = 0";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", GetCompanyId() ?? 1);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            summary["Total"] = reader["Total"] != DBNull.Value ? Convert.ToInt32(reader["Total"]) : 0;
                            summary["Scheduled"] = reader["Scheduled"] != DBNull.Value ? Convert.ToInt32(reader["Scheduled"]) : 0;
                            summary["Sent"] = reader["Sent"] != DBNull.Value ? Convert.ToInt32(reader["Sent"]) : 0;
                            summary["Failed"] = reader["Failed"] != DBNull.Value ? Convert.ToInt32(reader["Failed"]) : 0;
                        }
                    }
                }
            }

            return summary;
        }

        // ============================================================
        // GET CAMPAIGN STATS
        // ============================================================
        private CampaignStats GetCampaignStats(int campaignId)
        {
            var stats = new CampaignStats();

            using (var conn = new SqlConnection(GetConnectionString()))
            {
                var query = @"
                    SELECT 
                        COUNT(*) as Total,
                        SUM(CASE WHEN IsOpened = 1 THEN 1 ELSE 0 END) as Opens,
                        SUM(CASE WHEN AttachmentOpened = 1 THEN 1 ELSE 0 END) as Attachments,
                        SUM(CASE WHEN LinkClicked = 1 THEN 1 ELSE 0 END) as Clicks,
                        SUM(CASE WHEN Status = 'failed' THEN 1 ELSE 0 END) as Failed
                    FROM EmailLogs 
                    WHERE CampaignId = @CampaignId";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CampaignId", campaignId);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            stats.Total = reader["Total"] != DBNull.Value ? Convert.ToInt32(reader["Total"]) : 0;
                            stats.Opens = reader["Opens"] != DBNull.Value ? Convert.ToInt32(reader["Opens"]) : 0;
                            stats.Attachments = reader["Attachments"] != DBNull.Value ? Convert.ToInt32(reader["Attachments"]) : 0;
                            stats.Clicks = reader["Clicks"] != DBNull.Value ? Convert.ToInt32(reader["Clicks"]) : 0;
                            stats.Failed = reader["Failed"] != DBNull.Value ? Convert.ToInt32(reader["Failed"]) : 0;
                        }
                    }
                }
            }

            return stats;
        }

        // ============================================================
        // GET CAMPAIGN LOGS
        // ============================================================
        private List<EmailLog> GetCampaignLogs(int campaignId)
        {
            var logs = new List<EmailLog>();

            using (var conn = new SqlConnection(GetConnectionString()))
            {
                var query = "SELECT * FROM EmailLogs WHERE CampaignId = @CampaignId ORDER BY CreatedDate DESC";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CampaignId", campaignId);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            logs.Add(new EmailLog
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                CompanyId = reader["CompanyId"] != DBNull.Value ? Convert.ToInt32(reader["CompanyId"]) : (int?)null,
                                CampaignId = reader["CampaignId"] != DBNull.Value ? Convert.ToInt32(reader["CampaignId"]) : (int?)null,
                                EmailMasterId = reader["EmailMasterId"] != DBNull.Value ? Convert.ToInt32(reader["EmailMasterId"]) : (int?)null,
                                To = reader["To"]?.ToString() ?? "",
                                Status = reader["Status"]?.ToString() ?? "sending",
                                Error = reader["Error"]?.ToString(),
                                IsOpened = Convert.ToBoolean(reader["IsOpened"]),
                                OpenCount = Convert.ToInt32(reader["OpenCount"]),
                                FirstOpenedAt = reader["FirstOpenedAt"] != DBNull.Value ? Convert.ToDateTime(reader["FirstOpenedAt"]) : null,
                                LastOpenedAt = reader["LastOpenedAt"] != DBNull.Value ? Convert.ToDateTime(reader["LastOpenedAt"]) : null,
                                LinkClicked = Convert.ToBoolean(reader["LinkClicked"]),
                                ClickCount = Convert.ToInt32(reader["ClickCount"]),
                                ClickedAt = reader["ClickedAt"] != DBNull.Value ? Convert.ToDateTime(reader["ClickedAt"]) : null,
                                LastClickUrl = reader["LastClickUrl"]?.ToString(),
                                AttachmentOpened = Convert.ToBoolean(reader["AttachmentOpened"]),
                                Ip = reader["Ip"]?.ToString(),
                                UserAgent = reader["UserAgent"]?.ToString(),
                                City = reader["City"]?.ToString(),
                                Region = reader["Region"]?.ToString(),
                                Country = reader["Country"]?.ToString(),
                                SentAt = reader["SentAt"] != DBNull.Value ? Convert.ToDateTime(reader["SentAt"]) : null,
                                CreatedDate = Convert.ToDateTime(reader["CreatedDate"])
                            });
                        }
                    }
                }
            }

            return logs;
        }

        // ============================================================
        // GET CAMPAIGNS
        // ============================================================
        private List<Campaign> GetCampaigns()
        {
            var campaigns = new List<Campaign>();
            var companyId = GetCompanyId();

            using (var conn = new SqlConnection(GetConnectionString()))
            {
                var query = "SELECT * FROM Campaigns c WHERE c.CompanyId = @CompanyId AND c.IsDeleted = 0 ORDER BY c.CreatedDate DESC";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", companyId ?? 1);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            campaigns.Add(new Campaign
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                CampaignName = reader["CampaignName"]?.ToString() ?? "",
                                ScheduledTime = reader["ScheduledTime"] != DBNull.Value ? Convert.ToDateTime(reader["ScheduledTime"]) : null,
                                CampaignChannel = reader["Channel"]?.ToString() ?? "",
                                Sender = reader["Sender"]?.ToString() ?? "",
                                Content = reader["Content"]?.ToString() ?? "",
                                EmailSubject = reader["EmailSubject"]?.ToString(),
                                CtaText = reader["CtaText"]?.ToString(),
                                Attachments = reader["Attachments"]?.ToString(),
                                RecipientSource = reader["RecipientSource"]?.ToString() ?? "",
                                RecipientList = reader["RecipientList"]?.ToString(),
                                RecipientManual = reader["RecipientManual"]?.ToString(),
                                RecipientExcelEmails = reader["RecipientExcelEmails"]?.ToString(),
                                Status = reader["Status"]?.ToString() ?? "Scheduled",
                                SentAt = reader["SentAt"] != DBNull.Value ? Convert.ToDateTime(reader["SentAt"]) : null,
                                EmailMasterId = reader["EmailMasterId"] != DBNull.Value ? Convert.ToInt32(reader["EmailMasterId"]) : (int?)null,
                                CompanyId = reader["CompanyId"] != DBNull.Value ? Convert.ToInt32(reader["CompanyId"]) : (int?)null,
                                CreatedBy = reader["CreatedBy"] != DBNull.Value ? Convert.ToInt32(reader["CreatedBy"]) : null,
                                CreatedDate = reader["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedDate"]) : null,
                                ModifiedDate = reader["ModifiedDate"] != DBNull.Value ? Convert.ToDateTime(reader["ModifiedDate"]) : null,
                                ModifiedBy = reader["ModifiedBy"]?.ToString(),
                                IsActive = Convert.ToBoolean(reader["IsActive"]),
                                IsDeleted = Convert.ToBoolean(reader["IsDeleted"]),
                                RecipientCount = 0,
                                OpenCount = 0
                            });
                        }
                    }
                }
            }

            return campaigns;
        }

        // ============================================================
        // GET CAMPAIGN BY ID
        // ============================================================
        private Campaign? GetCampaignById(int id)
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            {
                var query = "SELECT * FROM Campaigns WHERE Id = @Id AND IsDeleted = 0";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Campaign
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                CampaignName = reader["CampaignName"]?.ToString() ?? "",
                                ScheduledTime = reader["ScheduledTime"] != DBNull.Value ? Convert.ToDateTime(reader["ScheduledTime"]) : null,
                                CampaignChannel = reader["Channel"]?.ToString() ?? "",
                                Sender = reader["Sender"]?.ToString() ?? "",
                                Content = reader["Content"]?.ToString() ?? "",
                                EmailSubject = reader["EmailSubject"]?.ToString(),
                                CtaText = reader["CtaText"]?.ToString(),
                                Attachments = reader["Attachments"]?.ToString(),
                                RecipientSource = reader["RecipientSource"]?.ToString() ?? "",
                                RecipientList = reader["RecipientList"]?.ToString(),
                                RecipientManual = reader["RecipientManual"]?.ToString(),
                                RecipientExcelEmails = reader["RecipientExcelEmails"]?.ToString(),
                                Status = reader["Status"]?.ToString() ?? "Scheduled",
                                SentAt = reader["SentAt"] != DBNull.Value ? Convert.ToDateTime(reader["SentAt"]) : null,
                                EmailMasterId = reader["EmailMasterId"] != DBNull.Value ? Convert.ToInt32(reader["EmailMasterId"]) : (int?)null,
                                CompanyId = reader["CompanyId"] != DBNull.Value ? Convert.ToInt32(reader["CompanyId"]) : (int?)null,
                                CreatedBy = reader["CreatedBy"] != DBNull.Value ? Convert.ToInt32(reader["CreatedBy"]) : null,
                                CreatedDate = reader["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedDate"]) : null,
                                ModifiedDate = reader["ModifiedDate"] != DBNull.Value ? Convert.ToDateTime(reader["ModifiedDate"]) : null,
                                ModifiedBy = reader["ModifiedBy"]?.ToString(),
                                IsActive = Convert.ToBoolean(reader["IsActive"]),
                                IsDeleted = Convert.ToBoolean(reader["IsDeleted"]),
                                RecipientCount = 0,
                                OpenCount = 0
                            };
                        }
                    }
                }
            }
            return null;
        }

        // ============================================================
        // SEND IMMEDIATELY - GET (Bypass Scheduler)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> SendNow(int id)
        {
            try
            {
                var campaign = GetCampaignById(id);
                if (campaign == null)
                {
                    TempData["Error"] = "Campaign not found.";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation($"Manually sending campaign {id} immediately");

                var success = await SendCampaignAsync(id);

                if (success)
                {
                    TempData["Success"] = $"Campaign '{campaign.CampaignName}' sent successfully!";
                }
                else
                {
                    TempData["Error"] = $"Failed to send campaign '{campaign.CampaignName}'. Please check logs.";
                }

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending campaign {id} immediately");
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // ============================================================
        // DOWNLOAD EMAIL TEMPLATE
        // ============================================================
        public IActionResult DownloadEmailTemplate()
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Email List");
                    worksheet.Row(1).Cell(1).Value = "Email*";
                    worksheet.Row(1).Cell(2).Value = "Name";
                    worksheet.Row(1).Cell(3).Value = "Company";

                    var headerRange = worksheet.Range(1, 1, 1, 3);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(68, 114, 196);
                    headerRange.Style.Font.FontColor = XLColor.White;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    var sampleData = new[]
                    {
                        new { Email = "john.doe@example.com", Name = "John Doe", Company = "Acme Corp" },
                        new { Email = "jane.smith@example.com", Name = "Jane Smith", Company = "Tech Solutions" },
                        new { Email = "bob.johnson@example.com", Name = "Bob Johnson", Company = "Global Enterprises" },
                        new { Email = "alice.williams@example.com", Name = "Alice Williams", Company = "Innovation Hub" },
                        new { Email = "", Name = "", Company = "" }
                    };

                    for (int i = 0; i < sampleData.Length; i++)
                    {
                        var row = i + 2;
                        worksheet.Cell(row, 1).Value = sampleData[i].Email;
                        worksheet.Cell(row, 2).Value = sampleData[i].Name;
                        worksheet.Cell(row, 3).Value = sampleData[i].Company;
                    }

                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Email_Template.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error downloading email template: {ex.Message}");
                TempData["Error"] = "Error downloading template.";
                return RedirectToAction(nameof(Create));
            }
        }

        // ============================================================
        // DOWNLOAD WHATSAPP TEMPLATE
        // ============================================================
        public IActionResult DownloadWhatsAppTemplate()
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("WhatsApp List");
                    worksheet.Row(1).Cell(1).Value = "Phone Number*";
                    worksheet.Row(1).Cell(2).Value = "Name";
                    worksheet.Row(1).Cell(3).Value = "Company";

                    var headerRange = worksheet.Range(1, 1, 1, 3);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(46, 160, 67);
                    headerRange.Style.Font.FontColor = XLColor.White;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    var sampleData = new[]
                    {
                        new { Phone = "9876543210", Name = "John Doe", Company = "Acme Corp" },
                        new { Phone = "9876543211", Name = "Jane Smith", Company = "Tech Solutions" },
                        new { Phone = "9876543212", Name = "Bob Johnson", Company = "Global Enterprises" },
                        new { Phone = "9876543213", Name = "Alice Williams", Company = "Innovation Hub" },
                        new { Phone = "", Name = "", Company = "" }
                    };

                    for (int i = 0; i < sampleData.Length; i++)
                    {
                        var row = i + 2;
                        worksheet.Cell(row, 1).Value = sampleData[i].Phone;
                        worksheet.Cell(row, 2).Value = sampleData[i].Name;
                        worksheet.Cell(row, 3).Value = sampleData[i].Company;
                    }

                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "WhatsApp_Template.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error downloading WhatsApp template: {ex.Message}");
                TempData["Error"] = "Error downloading template.";
                return RedirectToAction(nameof(Create));
            }
        }
    }
}