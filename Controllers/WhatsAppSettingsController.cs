using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Erp_Aits.Models;
using Erp_Aits.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Erp_Aits.Controllers
{
    public class WhatsAppSettingsController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<WhatsAppSettingsController> _logger;
        private readonly WhatsAppService _whatsAppService;

        public WhatsAppSettingsController(
            IConfiguration configuration,
            ILogger<WhatsAppSettingsController> logger,
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

        // ============================================================
        // INDEX - List WhatsApp Settings
        // ============================================================
        public IActionResult Index()
        {
            try
            {
                ViewData["Title"] = "WhatsApp Settings";
                ViewData["Module"] = "Settings";
                ViewData["Icon"] = "fab fa-whatsapp";

                var settings = GetWhatsAppSettings();
                return View(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Index: {ex.Message}");
                TempData["Error"] = "Error loading WhatsApp settings.";
                return View(new List<WhatsAppSettings>());
            }
        }

        // ============================================================
        // CREATE - GET
        // ============================================================
        public IActionResult Create()
        {
            ViewData["Title"] = "Add WhatsApp Configuration";
            ViewData["Module"] = "Settings";
            ViewData["Icon"] = "fab fa-whatsapp";

            var settings = new WhatsAppSettings
            {
                IsActive = true,
                TestNumber = "+1 555 150 2535",
                PhoneNumberId = "1095956276926168",
                BusinessAccountId = "1611618319921364"
            };

            return View(settings);
        }

        // ============================================================
        // CREATE - POST
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WhatsAppSettings settings)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Test the connection
                    var testResult = await _whatsAppService.SendTextMessageAsync(
                        settings.TestNumber,
                        "🔔 Test message from ERP System! Your WhatsApp integration is working successfully.");

                    if (!testResult)
                    {
                        ModelState.AddModelError("", "Failed to send test message. Please check your credentials.");
                        return View(settings);
                    }

                    using (var conn = new SqlConnection(GetConnectionString()))
                    {
                        var query = @"
                            INSERT INTO WhatsAppSettings (
                                PhoneNumberId, BusinessAccountId, AccessToken, 
                                TestNumber, PhoneNumber, BusinessName,
                                IsActive, CompanyId, CreatedDate, CreatedBy
                            ) VALUES (
                                @PhoneNumberId, @BusinessAccountId, @AccessToken,
                                @TestNumber, @PhoneNumber, @BusinessName,
                                @IsActive, @CompanyId, GETDATE(), @CreatedBy
                            );
                            SELECT SCOPE_IDENTITY();";

                        using (var cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@PhoneNumberId", settings.PhoneNumberId);
                            cmd.Parameters.AddWithValue("@BusinessAccountId", settings.BusinessAccountId);
                            cmd.Parameters.AddWithValue("@AccessToken", settings.AccessToken);
                            cmd.Parameters.AddWithValue("@TestNumber", settings.TestNumber);
                            cmd.Parameters.AddWithValue("@PhoneNumber", settings.PhoneNumber ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@BusinessName", settings.BusinessName ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@IsActive", settings.IsActive);
                            cmd.Parameters.AddWithValue("@CompanyId", GetCompanyId() ?? 1);
                            cmd.Parameters.AddWithValue("@CreatedBy", "System");

                            conn.Open();
                            var newId = Convert.ToInt32(cmd.ExecuteScalar());

                            TempData["Success"] = "WhatsApp settings saved successfully! Test message sent.";
                            return RedirectToAction(nameof(Index));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Create: {ex.Message}");
                ModelState.AddModelError("", $"Error: {ex.Message}");
            }

            return View(settings);
        }

        // ============================================================
        // TEST CONNECTION
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> TestConnection(int id)
        {
            try
            {
                var settings = GetWhatsAppSettingById(id);
                if (settings == null)
                {
                    return Json(new { success = false, message = "Settings not found" });
                }

                var result = await _whatsAppService.SendTextMessageAsync(
                    settings.TestNumber,
                    "✅ Test message from ERP System! Your WhatsApp integration is working successfully."
                );

                return Json(new { success = result, message = result ? "Test message sent successfully!" : "Failed to send test message." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error testing connection: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ============================================================
        // DELETE
        // ============================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                using (var conn = new SqlConnection(GetConnectionString()))
                {
                    var query = "DELETE FROM WhatsAppSettings WHERE Id = @Id AND CompanyId = @CompanyId";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.Parameters.AddWithValue("@CompanyId", GetCompanyId() ?? 1);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                TempData["Success"] = "WhatsApp settings deleted successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Delete: {ex.Message}");
                TempData["Error"] = "Error deleting settings.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // HELPER METHODS
        // ============================================================
        private List<WhatsAppSettings> GetWhatsAppSettings()
        {
            var settings = new List<WhatsAppSettings>();

            using (var conn = new SqlConnection(GetConnectionString()))
            {
                var query = "SELECT * FROM WhatsAppSettings WHERE CompanyId = @CompanyId ORDER BY CreatedDate DESC";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", GetCompanyId() ?? 1);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            settings.Add(MapToWhatsAppSettings(reader));
                        }
                    }
                }
            }

            return settings;
        }

        private WhatsAppSettings GetWhatsAppSettingById(int id)
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            {
                var query = "SELECT * FROM WhatsAppSettings WHERE Id = @Id AND CompanyId = @CompanyId";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@CompanyId", GetCompanyId() ?? 1);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToWhatsAppSettings(reader);
                        }
                    }
                }
            }
            return null;
        }

        private WhatsAppSettings MapToWhatsAppSettings(SqlDataReader reader)
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
                IsActive = Convert.ToBoolean(reader["IsActive"]),
                CompanyId = reader["CompanyId"] != DBNull.Value ? Convert.ToInt32(reader["CompanyId"]) : (int?)null,
                CreatedDate = reader["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedDate"]) : null,
                ModifiedDate = reader["ModifiedDate"] != DBNull.Value ? Convert.ToDateTime(reader["ModifiedDate"]) : null,
                CreatedBy = reader["CreatedBy"]?.ToString(),
                ModifiedBy = reader["ModifiedBy"]?.ToString()
            };
        }
    }
}