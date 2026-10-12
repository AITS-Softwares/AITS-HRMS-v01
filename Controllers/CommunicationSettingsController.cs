using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Erp_Aits.Models;
using Erp_Aits.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Erp_Aits.Controllers
{
    public class CommunicationSettingsController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CommunicationSettingsController> _logger;
        private readonly WhatsAppService _whatsAppService;

        public CommunicationSettingsController(
            IConfiguration configuration,
            ILogger<CommunicationSettingsController> logger,
            WhatsAppService whatsAppService)
        {
            _configuration = configuration;
            _logger = logger;
            _whatsAppService = whatsAppService;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("DefaultConnection");
        }

        private int? GetCompanyId()
        {
            var companyId = HttpContext.Session.GetString("CompanyId");
            if (!string.IsNullOrEmpty(companyId))
            {
                return Convert.ToInt32(companyId);
            }
            return 1;
        }


      











        private string GetCurrentUser()
        {
            return HttpContext.Session.GetString("CompanyName") ?? "System";
        }

        // ============================================================
        // INDEX - Combined Settings Page
        // ============================================================
        public IActionResult Index()
        {
            try
            {
                ViewData["Title"] = "Communication Settings";
                ViewData["Module"] = "Admin";
                ViewData["Icon"] = "cog";

                var viewModel = new CommunicationSettingsViewModel
                {
                    EmailList = GetEmailList(),
                    WhatsAppList = GetWhatsAppList(),
                    EmailStatusList = GetEmailStatusList(),
                    PurposeList = GetPurposeList(),
                    ServiceList = GetServiceList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Index: {ex.Message}");
                TempData["Error"] = "Error loading communication settings.";
                return View(new CommunicationSettingsViewModel());
            }
        }

        // ============================================================
        // EMAIL - CRUD Operations
        // ============================================================

        // GET Email for Edit
        public IActionResult GetEmail(int id)
        {
            try
            {
                var email = GetEmailById(id);
                if (email == null)
                {
                    return Json(new { success = false, message = "Email not found" });
                }
                return Json(new { success = true, data = email });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting email: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Create Email
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateEmail(CommunicationSettingsViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var email = viewModel.EmailMaster;
                    var companyId = GetCompanyId();

                    using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                    {
                        string query = @"INSERT INTO EmailMasters 
                            (Email, Purpose, Service, RecoveryEmail, Owner, MaskedAppPassword, 
                             EncryptedAppPassword, Status, Notes, CompanyId, LastUpdatedBy, CreatedDate, IsActive)
                            VALUES 
                            (@Email, @Purpose, @Service, @RecoveryEmail, @Owner, @MaskedAppPassword,
                             @EncryptedAppPassword, @Status, @Notes, @CompanyId, @LastUpdatedBy, GETDATE(), @IsActive);
                            SELECT SCOPE_IDENTITY();";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@Email", email.Email);
                            cmd.Parameters.AddWithValue("@Purpose", (object?)email.Purpose ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Service", (object?)email.Service ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@RecoveryEmail", (object?)email.RecoveryEmail ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Owner", (object?)email.Owner ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@MaskedAppPassword", (object?)email.MaskedAppPassword ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@EncryptedAppPassword", (object?)email.EncryptedAppPassword ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Status", (object?)email.Status ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Notes", (object?)email.Notes ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@CompanyId", (object?)companyId ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@LastUpdatedBy", GetCurrentUser());
                            cmd.Parameters.AddWithValue("@IsActive", email.IsActive);

                            conn.Open();
                            int newId = Convert.ToInt32(cmd.ExecuteScalar());

                            if (newId > 0)
                            {
                                TempData["Success"] = "Email setting added successfully!";
                                return RedirectToAction(nameof(Index));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating email: {ex.Message}");
                ModelState.AddModelError("", "Error creating email setting.");
            }

            return RedirectToAction(nameof(Index));
        }

        // Edit Email
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditEmail(CommunicationSettingsViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var email = viewModel.EmailMaster;

                    using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                    {
                        string query = @"UPDATE EmailMasters SET
                            Email = @Email,
                            Purpose = @Purpose,
                            Service = @Service,
                            RecoveryEmail = @RecoveryEmail,
                            Owner = @Owner,
                            MaskedAppPassword = @MaskedAppPassword,
                            EncryptedAppPassword = @EncryptedAppPassword,
                            Status = @Status,
                            Notes = @Notes,
                            LastUpdatedBy = @LastUpdatedBy,
                            ModifiedDate = GETDATE(),
                            IsActive = @IsActive
                            WHERE Id = @Id";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", email.Id);
                            cmd.Parameters.AddWithValue("@Email", email.Email);
                            cmd.Parameters.AddWithValue("@Purpose", (object?)email.Purpose ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Service", (object?)email.Service ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@RecoveryEmail", (object?)email.RecoveryEmail ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Owner", (object?)email.Owner ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@MaskedAppPassword", (object?)email.MaskedAppPassword ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@EncryptedAppPassword", (object?)email.EncryptedAppPassword ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Status", (object?)email.Status ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Notes", (object?)email.Notes ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@LastUpdatedBy", GetCurrentUser());
                            cmd.Parameters.AddWithValue("@IsActive", email.IsActive);

                            conn.Open();
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                TempData["Success"] = "Email setting updated successfully!";
                                return RedirectToAction(nameof(Index));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error editing email: {ex.Message}");
                ModelState.AddModelError("", "Error updating email setting.");
            }

            return RedirectToAction(nameof(Index));
        }

        // Delete Email
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteEmail(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = "DELETE FROM EmailMasters WHERE Id = @Id";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        TempData["Success"] = "Email setting deleted successfully!";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting email: {ex.Message}");
                TempData["Error"] = "Error deleting email setting.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // WHATSAPP - CRUD Operations
        // ============================================================

        // GET WhatsApp for Edit
        public IActionResult GetWhatsApp(int id)
        {
            try
            {
                var whatsapp = GetWhatsAppById(id);
                if (whatsapp == null)
                {
                    return Json(new { success = false, message = "WhatsApp setting not found" });
                }
                return Json(new { success = true, data = whatsapp });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting WhatsApp: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Create WhatsApp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateWhatsApp(CommunicationSettingsViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var whatsapp = viewModel.WhatsAppSetting;
                    var companyId = GetCompanyId();

                    // Test the connection
                    var testResult = await _whatsAppService.SendTextMessageAsync(
                        whatsapp.TestNumber,
                        "🔔 Test message from ERP System! Your WhatsApp integration is working successfully.");

                    if (!testResult)
                    {
                        TempData["Error"] = "Failed to send test message. Please check your credentials.";
                        return RedirectToAction(nameof(Index));
                    }

                    using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                    {
                        string query = @"INSERT INTO WhatsAppSettings 
                            (CompanyId, PhoneNumberId, BusinessAccountId, AccessToken, TestNumber, 
                             PhoneNumber, BusinessName, IsActive, CreatedDate, CreatedBy)
                            VALUES 
                            (@CompanyId, @PhoneNumberId, @BusinessAccountId, @AccessToken, @TestNumber,
                             @PhoneNumber, @BusinessName, @IsActive, GETDATE(), @CreatedBy);
                            SELECT SCOPE_IDENTITY();";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@CompanyId", (object?)companyId ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@PhoneNumberId", whatsapp.PhoneNumberId);
                            cmd.Parameters.AddWithValue("@BusinessAccountId", whatsapp.BusinessAccountId);
                            cmd.Parameters.AddWithValue("@AccessToken", whatsapp.AccessToken);
                            cmd.Parameters.AddWithValue("@TestNumber", whatsapp.TestNumber);
                            cmd.Parameters.AddWithValue("@PhoneNumber", (object?)whatsapp.PhoneNumber ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@BusinessName", (object?)whatsapp.BusinessName ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@IsActive", whatsapp.IsActive);
                            cmd.Parameters.AddWithValue("@CreatedBy", GetCurrentUser());

                            conn.Open();
                            int newId = Convert.ToInt32(cmd.ExecuteScalar());

                            if (newId > 0)
                            {
                                TempData["Success"] = "WhatsApp setting added successfully! Test message sent.";
                                return RedirectToAction(nameof(Index));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating WhatsApp: {ex.Message}");
                TempData["Error"] = $"Error creating WhatsApp setting: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Edit WhatsApp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditWhatsApp(CommunicationSettingsViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var whatsapp = viewModel.WhatsAppSetting;

                    using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                    {
                        string query = @"UPDATE WhatsAppSettings SET
                            PhoneNumberId = @PhoneNumberId,
                            BusinessAccountId = @BusinessAccountId,
                            AccessToken = @AccessToken,
                            TestNumber = @TestNumber,
                            PhoneNumber = @PhoneNumber,
                            BusinessName = @BusinessName,
                            IsActive = @IsActive,
                            ModifiedDate = GETDATE(),
                            ModifiedBy = @ModifiedBy
                            WHERE Id = @Id";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", whatsapp.Id);
                            cmd.Parameters.AddWithValue("@PhoneNumberId", whatsapp.PhoneNumberId);
                            cmd.Parameters.AddWithValue("@BusinessAccountId", whatsapp.BusinessAccountId);
                            cmd.Parameters.AddWithValue("@AccessToken", whatsapp.AccessToken);
                            cmd.Parameters.AddWithValue("@TestNumber", whatsapp.TestNumber);
                            cmd.Parameters.AddWithValue("@PhoneNumber", (object?)whatsapp.PhoneNumber ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@BusinessName", (object?)whatsapp.BusinessName ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@IsActive", whatsapp.IsActive);
                            cmd.Parameters.AddWithValue("@ModifiedBy", GetCurrentUser());

                            conn.Open();
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                TempData["Success"] = "WhatsApp setting updated successfully!";
                                return RedirectToAction(nameof(Index));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error editing WhatsApp: {ex.Message}");
                TempData["Error"] = "Error updating WhatsApp setting.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Delete WhatsApp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteWhatsApp(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = "DELETE FROM WhatsAppSettings WHERE Id = @Id";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        TempData["Success"] = "WhatsApp setting deleted successfully!";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting WhatsApp: {ex.Message}");
                TempData["Error"] = "Error deleting WhatsApp setting.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Test WhatsApp Connection
        [HttpPost]
        public async Task<IActionResult> TestWhatsAppConnection(int id)
        {
            try
            {
                var whatsapp = GetWhatsAppById(id);
                if (whatsapp == null)
                {
                    return Json(new { success = false, message = "Settings not found" });
                }

                var result = await _whatsAppService.SendTextMessageAsync(
                    whatsapp.TestNumber,
                    "✅ Test message from ERP System! Your WhatsApp integration is working successfully."
                );

                return Json(new { success = result, message = result ? "Test message sent successfully!" : "Failed to send test message." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error testing WhatsApp connection: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ============================================================
        // HELPER METHODS
        // ============================================================

        private List<EmailMaster> GetEmailList()
        {
            var emails = new List<EmailMaster>();
            var companyId = GetCompanyId();

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT * FROM EmailMasters WHERE IsActive = 1";
                if (companyId.HasValue)
                {
                    query += " AND (CompanyId = @CompanyId OR CompanyId IS NULL)";
                }
                query += " ORDER BY CreatedDate DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (companyId.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@CompanyId", companyId.Value);
                    }

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            emails.Add(new EmailMaster
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Email = reader["Email"]?.ToString() ?? string.Empty,
                                Purpose = reader["Purpose"]?.ToString(),
                                Service = reader["Service"]?.ToString(),
                                RecoveryEmail = reader["RecoveryEmail"]?.ToString(),
                                Owner = reader["Owner"]?.ToString(),
                                MaskedAppPassword = reader["MaskedAppPassword"]?.ToString(),
                                EncryptedAppPassword = reader["EncryptedAppPassword"]?.ToString(),
                                Status = reader["Status"]?.ToString(),
                                Notes = reader["Notes"]?.ToString(),
                                CompanyId = reader["CompanyId"] != DBNull.Value ? Convert.ToInt32(reader["CompanyId"]) : null,
                                LastUpdatedBy = reader["LastUpdatedBy"]?.ToString(),
                                CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                                ModifiedDate = reader["ModifiedDate"] != DBNull.Value ? Convert.ToDateTime(reader["ModifiedDate"]) : null,
                                IsActive = Convert.ToBoolean(reader["IsActive"])
                            });
                        }
                    }
                }
            }

            return emails;
        }

        private EmailMaster GetEmailById(int id)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT * FROM EmailMasters WHERE Id = @Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new EmailMaster
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Email = reader["Email"]?.ToString() ?? string.Empty,
                                Purpose = reader["Purpose"]?.ToString(),
                                Service = reader["Service"]?.ToString(),
                                RecoveryEmail = reader["RecoveryEmail"]?.ToString(),
                                Owner = reader["Owner"]?.ToString(),
                                MaskedAppPassword = reader["MaskedAppPassword"]?.ToString(),
                                EncryptedAppPassword = reader["EncryptedAppPassword"]?.ToString(),
                                Status = reader["Status"]?.ToString(),
                                Notes = reader["Notes"]?.ToString(),
                                CompanyId = reader["CompanyId"] != DBNull.Value ? Convert.ToInt32(reader["CompanyId"]) : null,
                                LastUpdatedBy = reader["LastUpdatedBy"]?.ToString(),
                                CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                                ModifiedDate = reader["ModifiedDate"] != DBNull.Value ? Convert.ToDateTime(reader["ModifiedDate"]) : null,
                                IsActive = Convert.ToBoolean(reader["IsActive"])
                            };
                        }
                    }
                }
            }
            return null;
        }

        private List<WhatsAppSettings> GetWhatsAppList()
        {
            var whatsapps = new List<WhatsAppSettings>();
            var companyId = GetCompanyId();

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT * FROM WhatsAppSettings WHERE IsActive = 1";
                if (companyId.HasValue)
                {
                    query += " AND (CompanyId = @CompanyId OR CompanyId IS NULL)";
                }
                query += " ORDER BY CreatedDate DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (companyId.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@CompanyId", companyId.Value);
                    }

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            whatsapps.Add(new WhatsAppSettings
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                CompanyId = reader["CompanyId"] != DBNull.Value ? Convert.ToInt32(reader["CompanyId"]) : null,
                                PhoneNumberId = reader["PhoneNumberId"]?.ToString() ?? string.Empty,
                                BusinessAccountId = reader["BusinessAccountId"]?.ToString() ?? string.Empty,
                                AccessToken = reader["AccessToken"]?.ToString() ?? string.Empty,
                                TestNumber = reader["TestNumber"]?.ToString() ?? string.Empty,
                                PhoneNumber = reader["PhoneNumber"]?.ToString(),
                                BusinessName = reader["BusinessName"]?.ToString(),
                                IsActive = Convert.ToBoolean(reader["IsActive"]),
                                CreatedDate = reader["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedDate"]) : null,
                                ModifiedDate = reader["ModifiedDate"] != DBNull.Value ? Convert.ToDateTime(reader["ModifiedDate"]) : null,
                                CreatedBy = reader["CreatedBy"]?.ToString(),
                                ModifiedBy = reader["ModifiedBy"]?.ToString()
                            });
                        }
                    }
                }
            }

            return whatsapps;
        }

        private WhatsAppSettings GetWhatsAppById(int id)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT * FROM WhatsAppSettings WHERE Id = @Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new WhatsAppSettings
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                CompanyId = reader["CompanyId"] != DBNull.Value ? Convert.ToInt32(reader["CompanyId"]) : null,
                                PhoneNumberId = reader["PhoneNumberId"]?.ToString() ?? string.Empty,
                                BusinessAccountId = reader["BusinessAccountId"]?.ToString() ?? string.Empty,
                                AccessToken = reader["AccessToken"]?.ToString() ?? string.Empty,
                                TestNumber = reader["TestNumber"]?.ToString() ?? string.Empty,
                                PhoneNumber = reader["PhoneNumber"]?.ToString(),
                                BusinessName = reader["BusinessName"]?.ToString(),
                                IsActive = Convert.ToBoolean(reader["IsActive"]),
                                CreatedDate = reader["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedDate"]) : null,
                                ModifiedDate = reader["ModifiedDate"] != DBNull.Value ? Convert.ToDateTime(reader["ModifiedDate"]) : null,
                                CreatedBy = reader["CreatedBy"]?.ToString(),
                                ModifiedBy = reader["ModifiedBy"]?.ToString()
                            };
                        }
                    }
                }
            }
            return null;
        }

        private List<string> GetEmailStatusList()
        {
            return new List<string> { "", "Active", "Inactive", "Pending", "Blocked", "Verified", "Unverified" };
        }

        private List<string> GetPurposeList()
        {
            return new List<string> { "", "Sales", "Support", "Marketing", "General", "Accounts", "HR", "IT" };
        }

        private List<string> GetServiceList()
        {
            return new List<string> { "", "Gmail", "Outlook", "Yahoo", "Custom SMTP", "Office 365", "Exchange", "Other" };
        }
    }
}