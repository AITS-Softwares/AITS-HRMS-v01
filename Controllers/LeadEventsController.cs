using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using Erp_Aits.Models;

namespace Erp_Aits.Controllers
{
    public class LeadEventsController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<LeadEventsController> _logger;

        public LeadEventsController(IConfiguration configuration, ILogger<LeadEventsController> logger)
        {
            _configuration = configuration;
            _logger = logger;
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
            return null;
        }

        // ============================================================
        // INDEX - Show all leads or events for a specific lead
        // ============================================================
        public IActionResult Index(int? leadId)
        {
            // Get all leads with event count
            var leads = GetLeadsWithEventCount();
            ViewBag.Leads = leads;

            // Add UserList for dropdowns
            ViewBag.UserList = GetUsers();

            if (leadId.HasValue && leadId.Value > 0)
            {
                ViewBag.LeadId = leadId.Value;

                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = "SELECT FirstName + ' ' + LastName AS FullName FROM Leads WHERE Id = @Id AND IsDeleted = 0";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", leadId.Value);
                        conn.Open();
                        var name = cmd.ExecuteScalar();
                        ViewBag.LeadName = name?.ToString() ?? "Lead";
                    }
                }
            }
            else
            {
                ViewBag.LeadId = 0;
                ViewBag.LeadName = "Select a Lead";
            }

            return View();
        }

        // ============================================================
        // GET LEAD EVENTS - AJAX
        // ============================================================
        [HttpGet]
        public IActionResult GetLeadEvents(int leadId, string eventType = null, string status = null, string search = null)
        {
            try
            {
                var events = new List<object>();

                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = @"
                        SELECT e.*, u.Name as AssignedToName 
                        FROM Events e
                        LEFT JOIN Users u ON e.AssignedTo = u.Id
                        WHERE e.IsDeleted = 0 AND e.LeadId = @LeadId";

                    if (!string.IsNullOrEmpty(eventType))
                    {
                        query += " AND e.EventType = @EventType";
                    }
                    if (!string.IsNullOrEmpty(status))
                    {
                        query += " AND e.Status = @Status";
                    }
                    if (!string.IsNullOrEmpty(search))
                    {
                        query += " AND e.Summary LIKE @Search";
                    }

                    query += " ORDER BY e.EventDate DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@LeadId", leadId);
                        if (!string.IsNullOrEmpty(eventType))
                        {
                            cmd.Parameters.AddWithValue("@EventType", eventType);
                        }
                        if (!string.IsNullOrEmpty(status))
                        {
                            cmd.Parameters.AddWithValue("@Status", status);
                        }
                        if (!string.IsNullOrEmpty(search))
                        {
                            cmd.Parameters.AddWithValue("@Search", $"%{search}%");
                        }

                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                events.Add(new
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    EventType = reader["EventType"]?.ToString(),
                                    AssignedTo = reader["AssignedToName"]?.ToString() ?? "Unassigned",
                                    EventDate = Convert.ToDateTime(reader["EventDate"]).ToString("dd MMM yyyy HH:mm"),
                                    Summary = reader["Summary"]?.ToString(),
                                    Status = reader["Status"]?.ToString(),
                                    AttachmentPath = reader["AttachmentPath"]?.ToString()
                                });
                            }
                        }
                    }
                }

                return Json(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lead events");
                return Json(new List<object>());
            }
        }

        // ============================================================
        // GET EVENT BY ID - API
        // ============================================================
        [HttpGet]
        public IActionResult GetEvent(int id)
        {
            try
            {
                var eventItem = GetEventById(id);
                if (eventItem == null)
                {
                    return Json(new { success = false, message = "Event not found." });
                }

                return Json(new
                {
                    success = true,
                    eventItem = new
                    {
                        eventItem.Id,
                        eventItem.LeadId,
                        eventItem.OpportunityId,
                        eventItem.AssignedTo,
                        EventDate = eventItem.EventDate.ToString("yyyy-MM-ddTHH:mm"),
                        eventItem.EventType,
                        eventItem.Summary,
                        eventItem.Status,
                        eventItem.AttachmentPath
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting event ID: {id}");
                return Json(new { success = false, message = "Error loading event." });
            }
        }

        // ============================================================
        // CREATE EVENT - API
        // ============================================================
        [HttpPost]
        public IActionResult Create([FromBody] Event eventItem)
        {
            try
            {
                if (string.IsNullOrEmpty(eventItem.Summary))
                {
                    return Json(new { success = false, message = "Summary is required." });
                }

                if (string.IsNullOrEmpty(eventItem.EventType))
                {
                    return Json(new { success = false, message = "Event type is required." });
                }

                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = @"
                        INSERT INTO Events (
                            LeadId, OpportunityId, AssignedTo, EventDate, EventType,
                            Summary, Status, AttachmentPath,
                            CreatedBy, CreatedDate, CompanyId
                        ) VALUES (
                            @LeadId, @OpportunityId, @AssignedTo, @EventDate, @EventType,
                            @Summary, @Status, @AttachmentPath,
                            @CreatedBy, @CreatedDate, @CompanyId
                        );
                        SELECT SCOPE_IDENTITY();";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@LeadId", eventItem.LeadId ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@OpportunityId", eventItem.OpportunityId ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@AssignedTo", eventItem.AssignedTo ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@EventDate", eventItem.EventDate);
                        cmd.Parameters.AddWithValue("@EventType", eventItem.EventType);
                        cmd.Parameters.AddWithValue("@Summary", eventItem.Summary);
                        cmd.Parameters.AddWithValue("@Status", eventItem.Status ?? "Scheduled");
                        cmd.Parameters.AddWithValue("@AttachmentPath", eventItem.AttachmentPath ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@CreatedBy", GetCurrentUser());
                        cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                        cmd.Parameters.AddWithValue("@CompanyId", GetCompanyId() ?? (object)DBNull.Value);

                        conn.Open();
                        var newId = Convert.ToInt32(cmd.ExecuteScalar());

                        return Json(new { success = true, message = "Event created successfully!", id = newId });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event");
                return Json(new { success = false, message = "Error creating event: " + ex.Message });
            }
        }

        // ============================================================
        // UPDATE EVENT - API
        // ============================================================
        [HttpPut]
        public IActionResult Update([FromBody] Event eventItem)
        {
            try
            {
                if (eventItem.Id <= 0)
                {
                    return Json(new { success = false, message = "Invalid event ID." });
                }

                if (string.IsNullOrEmpty(eventItem.Summary))
                {
                    return Json(new { success = false, message = "Summary is required." });
                }

                if (string.IsNullOrEmpty(eventItem.EventType))
                {
                    return Json(new { success = false, message = "Event type is required." });
                }

                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = @"
                        UPDATE Events SET
                            AssignedTo = @AssignedTo,
                            EventDate = @EventDate,
                            EventType = @EventType,
                            Summary = @Summary,
                            Status = @Status,
                            AttachmentPath = @AttachmentPath,
                            ModifiedBy = @ModifiedBy,
                            ModifiedDate = @ModifiedDate
                        WHERE Id = @Id AND IsDeleted = 0";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", eventItem.Id);
                        cmd.Parameters.AddWithValue("@AssignedTo", eventItem.AssignedTo ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@EventDate", eventItem.EventDate);
                        cmd.Parameters.AddWithValue("@EventType", eventItem.EventType);
                        cmd.Parameters.AddWithValue("@Summary", eventItem.Summary);
                        cmd.Parameters.AddWithValue("@Status", eventItem.Status ?? "Scheduled");
                        cmd.Parameters.AddWithValue("@AttachmentPath", eventItem.AttachmentPath ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ModifiedBy", GetCurrentUser());
                        cmd.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);

                        conn.Open();
                        var rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Json(new { success = true, message = "Event updated successfully!" });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Event not found or no changes made." });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating event ID: {eventItem.Id}");
                return Json(new { success = false, message = "Error updating event: " + ex.Message });
            }
        }

        // ============================================================
        // DELETE EVENT - API
        // ============================================================
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = @"
                        UPDATE Events SET 
                            IsDeleted = 1,
                            ModifiedBy = @ModifiedBy,
                            ModifiedDate = @ModifiedDate
                        WHERE Id = @Id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.Parameters.AddWithValue("@ModifiedBy", GetCurrentUser());
                        cmd.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);

                        conn.Open();
                        var rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Json(new { success = true, message = "Event deleted successfully!" });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Event not found." });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting event ID: {id}");
                return Json(new { success = false, message = "Error deleting event: " + ex.Message });
            }
        }

        // ============================================================
        // UPDATE EVENT STATUS - API
        // ============================================================
        [HttpPost]
        public IActionResult UpdateStatus(int id, string status)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = @"
                        UPDATE Events SET 
                            Status = @Status,
                            ModifiedBy = @ModifiedBy,
                            ModifiedDate = @ModifiedDate
                        WHERE Id = @Id AND IsDeleted = 0";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.Parameters.AddWithValue("@Status", status);
                        cmd.Parameters.AddWithValue("@ModifiedBy", GetCurrentUser());
                        cmd.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);

                        conn.Open();
                        var rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Json(new { success = true, message = "Status updated successfully!" });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Event not found." });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating event status for ID: {id}");
                return Json(new { success = false, message = "Error updating status: " + ex.Message });
            }
        }

        // ============================================================
        // GET USERS - Helper
        // ============================================================
        private List<User> GetUsers()
        {
            var users = new List<User>();

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Id, Name, Email FROM Users WHERE IsActive = 1 AND IsDeleted = 0 ORDER BY Name";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new User
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Name = reader["Name"]?.ToString() ?? string.Empty,
                                Email = reader["Email"]?.ToString() ?? string.Empty
                            });
                        }
                    }
                }
            }

            return users;
        }

        // ============================================================
        // GET EVENT BY ID - Helper
        // ============================================================
        private Event? GetEventById(int id)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT * FROM Events WHERE Id = @Id AND IsDeleted = 0";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Event
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                LeadId = reader["LeadId"] != DBNull.Value ? Convert.ToInt32(reader["LeadId"]) : (int?)null,
                                OpportunityId = reader["OpportunityId"] != DBNull.Value ? Convert.ToInt32(reader["OpportunityId"]) : (int?)null,
                                AssignedTo = reader["AssignedTo"] != DBNull.Value ? Convert.ToInt32(reader["AssignedTo"]) : (int?)null,
                                EventDate = Convert.ToDateTime(reader["EventDate"]),
                                EventType = reader["EventType"]?.ToString() ?? string.Empty,
                                Summary = reader["Summary"]?.ToString() ?? string.Empty,
                                Status = reader["Status"]?.ToString() ?? "Scheduled",
                                AttachmentPath = reader["AttachmentPath"]?.ToString(),
                                CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                                CreatedBy = reader["CreatedBy"]?.ToString(),
                                ModifiedDate = reader["ModifiedDate"] != DBNull.Value ? Convert.ToDateTime(reader["ModifiedDate"]) : null,
                                ModifiedBy = reader["ModifiedBy"]?.ToString(),
                                IsDeleted = Convert.ToBoolean(reader["IsDeleted"])
                            };
                        }
                    }
                }
            }
            return null;
        }

        // ============================================================
        // GET LEADS WITH EVENT COUNT - Helper
        // ============================================================
        private List<dynamic> GetLeadsWithEventCount()
        {
            var leads = new List<dynamic>();

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                string query = @"
                    SELECT 
                        l.Id, 
                        l.FirstName + ' ' + l.LastName AS FullName,
                        l.OrganizationName AS Company,
                        l.Email,
                        (SELECT COUNT(*) FROM Events WHERE LeadId = l.Id AND IsDeleted = 0) AS EventCount
                    FROM Leads l
                    WHERE l.IsDeleted = 0
                    ORDER BY l.FirstName";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            leads.Add(new
                            {
                                Value = reader["Id"].ToString(),
                                Text = reader["FullName"]?.ToString() ?? "Lead",
                                Company = reader["Company"]?.ToString() ?? "",
                                Email = reader["Email"]?.ToString() ?? "",
                                EventCount = Convert.ToInt32(reader["EventCount"])
                            });
                        }
                    }
                }
            }

            return leads;
        }
    }
}