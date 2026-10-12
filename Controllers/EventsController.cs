using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using Erp_Aits.Models;

namespace Erp_Aits.Controllers
{
    public class EventsController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EventsController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EventsController(IConfiguration configuration, ILogger<EventsController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _configuration = configuration;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("DefaultConnection");
        }

        private string GetCurrentUser()
        {
            return HttpContext.Session.GetString("CompanyName") ?? "System";
        }

        // GET: Events/CreateEventModal
        public IActionResult CreateEventModal(int leadId = 0, int opportunityId = 0)
        {
            var users = GetUsers();

            var viewModel = new EventViewModel
            {
                Event = new Event
                {
                    LeadId = leadId > 0 ? leadId : (int?)null,
                    OpportunityId = opportunityId > 0 ? opportunityId : (int?)null,
                    EventDate = DateTime.Now,
                    EventType = "Call",
                    Status = "Scheduled"
                },
                UserList = users,
                EventTypeList = new List<string> { "Call", "Email", "Meeting", "Demo", "Follow-up" },
                StatusList = new List<string> { "Scheduled", "Completed", "Cancelled", "Postponed" }
            };

            return PartialView("_CreateEventModal", viewModel);
        }

        // POST: Events/Create - FIXED with file upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(EventViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string attachmentPath = null;

                    // Handle file upload
                    if (viewModel.AttachmentFile != null && viewModel.AttachmentFile.Length > 0)
                    {
                        // Create uploads folder if it doesn't exist
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "events");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // Generate unique filename
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + viewModel.AttachmentFile.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        // Save file
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            viewModel.AttachmentFile.CopyTo(fileStream);
                        }

                        // Save relative path to database
                        attachmentPath = "/uploads/events/" + uniqueFileName;
                    }

                    using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                    {
                        string query = @"INSERT INTO Events (
                            LeadId, OpportunityId, AssignedTo, EventType, EventDate, 
                            Summary, AttachmentPath, Status, CreatedBy
                        ) VALUES (
                            @LeadId, @OpportunityId, @AssignedTo, @EventType, @EventDate,
                            @Summary, @AttachmentPath, @Status, @CreatedBy
                        );
                        SELECT SCOPE_IDENTITY();";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            var evt = viewModel.Event;

                            cmd.Parameters.AddWithValue("@LeadId", (object?)evt.LeadId ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@OpportunityId", (object?)evt.OpportunityId ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@AssignedTo", (object?)evt.AssignedTo ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@EventType", evt.EventType);
                            cmd.Parameters.AddWithValue("@EventDate", evt.EventDate);
                            cmd.Parameters.AddWithValue("@Summary", evt.Summary);
                            cmd.Parameters.AddWithValue("@AttachmentPath", (object?)attachmentPath ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Status", evt.Status);
                            cmd.Parameters.AddWithValue("@CreatedBy", GetCurrentUser());

                            conn.Open();
                            int newId = Convert.ToInt32(cmd.ExecuteScalar());

                            if (newId > 0)
                            {
                                return Json(new { success = true, message = "Event created successfully!" });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event");
                return Json(new { success = false, message = "Error creating event: " + ex.Message });
            }

            return Json(new { success = false, message = "Failed to create event." });
        }

        // GET: Events/GetEvents
        public IActionResult GetEvents(int leadId = 0, int opportunityId = 0)
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
                        WHERE e.IsDeleted = 0";

                    if (leadId > 0)
                    {
                        query += " AND e.LeadId = @LeadId";
                    }
                    else if (opportunityId > 0)
                    {
                        query += " AND e.OpportunityId = @OpportunityId";
                    }

                    query += " ORDER BY e.EventDate DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (leadId > 0)
                        {
                            cmd.Parameters.AddWithValue("@LeadId", leadId);
                        }
                        else if (opportunityId > 0)
                        {
                            cmd.Parameters.AddWithValue("@OpportunityId", opportunityId);
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
                _logger.LogError(ex, "Error getting events");
                return Json(new List<object>());
            }
        }

        private List<User> GetUsers()
        {
            var users = new List<User>();

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Id, Name FROM Users WHERE IsActive = 1 AND IsDeleted = 0 ORDER BY Name";
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
                                Name = reader["Name"]?.ToString() ?? string.Empty
                            });
                        }
                    }
                }
            }

            return users;
        }
    }
}