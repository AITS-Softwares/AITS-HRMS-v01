using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using Erp_Aits.Models;

namespace Erp_Aits.Controllers
{
    public class LeadTasksController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<LeadTasksController> _logger;

        public LeadTasksController(IConfiguration configuration, ILogger<LeadTasksController> logger)
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
        // INDEX - Show all leads or tasks for a specific lead
        // ============================================================
        public IActionResult Index(int? leadId)
        {
            // Get all leads with task count
            var leads = GetLeadsWithTaskCount();
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
        // GET LEAD TASKS - AJAX
        // ============================================================
        [HttpGet]
        public IActionResult GetLeadTasks(int leadId, string status = null, string priority = null, string search = null)
        {
            try
            {
                var tasks = new List<object>();

                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = @"
                        SELECT t.*, u.Name as AssignedToName 
                        FROM Tasks t
                        LEFT JOIN Users u ON t.AssignedTo = u.Id
                        WHERE t.IsDeleted = 0 AND t.LeadId = @LeadId";

                    if (!string.IsNullOrEmpty(status))
                    {
                        query += " AND t.Status = @Status";
                    }
                    if (!string.IsNullOrEmpty(priority))
                    {
                        query += " AND t.Priority = @Priority";
                    }
                    if (!string.IsNullOrEmpty(search))
                    {
                        query += " AND t.Summary LIKE @Search";
                    }

                    query += " ORDER BY t.TaskDate DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@LeadId", leadId);
                        if (!string.IsNullOrEmpty(status))
                        {
                            cmd.Parameters.AddWithValue("@Status", status);
                        }
                        if (!string.IsNullOrEmpty(priority))
                        {
                            cmd.Parameters.AddWithValue("@Priority", priority);
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
                                tasks.Add(new
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    AssignedTo = reader["AssignedToName"]?.ToString() ?? "Unassigned",
                                    TaskDate = Convert.ToDateTime(reader["TaskDate"]).ToString("dd MMM yyyy HH:mm"),
                                    Summary = reader["Summary"]?.ToString(),
                                    Status = reader["Status"]?.ToString(),
                                    Priority = reader["Priority"]?.ToString(),
                                    AttachmentPath = reader["AttachmentPath"]?.ToString()
                                });
                            }
                        }
                    }
                }

                return Json(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lead tasks");
                return Json(new List<object>());
            }
        }

        // ============================================================
        // GET TASK BY ID - API
        // ============================================================
        [HttpGet]
        public IActionResult GetTask(int id)
        {
            try
            {
                var task = GetTaskById(id);
                if (task == null)
                {
                    return Json(new { success = false, message = "Task not found." });
                }

                return Json(new
                {
                    success = true,
                    task = new
                    {
                        task.Id,
                        task.LeadId,
                        task.OpportunityId,
                        task.AssignedTo,
                        TaskDate = task.TaskDate.ToString("yyyy-MM-ddTHH:mm"),
                        task.Summary,
                        task.Status,
                        task.Priority,
                        task.AttachmentPath
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting task ID: {id}");
                return Json(new { success = false, message = "Error loading task." });
            }
        }

        // ============================================================
        // CREATE TASK - API
        // ============================================================
        [HttpPost]
        public IActionResult Create([FromBody] UserTask task)
        {
            try
            {
                if (string.IsNullOrEmpty(task.Summary))
                {
                    return Json(new { success = false, message = "Summary is required." });
                }

                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = @"
                        INSERT INTO Tasks (
                            LeadId, OpportunityId, AssignedTo, TaskDate, Summary, 
                            Status, Priority, AttachmentPath,
                            CreatedBy, CreatedDate, CompanyId
                        ) VALUES (
                            @LeadId, @OpportunityId, @AssignedTo, @TaskDate, @Summary,
                            @Status, @Priority, @AttachmentPath,
                            @CreatedBy, @CreatedDate, @CompanyId
                        );
                        SELECT SCOPE_IDENTITY();";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@LeadId", task.LeadId ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@OpportunityId", task.OpportunityId ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@AssignedTo", task.AssignedTo ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@TaskDate", task.TaskDate);
                        cmd.Parameters.AddWithValue("@Summary", task.Summary);
                        cmd.Parameters.AddWithValue("@Status", task.Status ?? "Pending");
                        cmd.Parameters.AddWithValue("@Priority", task.Priority ?? "Medium");
                        cmd.Parameters.AddWithValue("@AttachmentPath", task.AttachmentPath ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@CreatedBy", GetCurrentUser());
                        cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                        cmd.Parameters.AddWithValue("@CompanyId", GetCompanyId() ?? (object)DBNull.Value);

                        conn.Open();
                        var newId = Convert.ToInt32(cmd.ExecuteScalar());

                        return Json(new { success = true, message = "Task created successfully!", id = newId });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                return Json(new { success = false, message = "Error creating task: " + ex.Message });
            }
        }

        // ============================================================
        // UPDATE TASK - API
        // ============================================================
        [HttpPut]
        public IActionResult Update([FromBody] UserTask task)
        {
            try
            {
                if (task.Id <= 0)
                {
                    return Json(new { success = false, message = "Invalid task ID." });
                }

                if (string.IsNullOrEmpty(task.Summary))
                {
                    return Json(new { success = false, message = "Summary is required." });
                }

                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = @"
                        UPDATE Tasks SET
                            AssignedTo = @AssignedTo,
                            TaskDate = @TaskDate,
                            Summary = @Summary,
                            Status = @Status,
                            Priority = @Priority,
                            AttachmentPath = @AttachmentPath,
                            ModifiedBy = @ModifiedBy,
                            ModifiedDate = @ModifiedDate
                        WHERE Id = @Id AND IsDeleted = 0";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", task.Id);
                        cmd.Parameters.AddWithValue("@AssignedTo", task.AssignedTo ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@TaskDate", task.TaskDate);
                        cmd.Parameters.AddWithValue("@Summary", task.Summary);
                        cmd.Parameters.AddWithValue("@Status", task.Status ?? "Pending");
                        cmd.Parameters.AddWithValue("@Priority", task.Priority ?? "Medium");
                        cmd.Parameters.AddWithValue("@AttachmentPath", task.AttachmentPath ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ModifiedBy", GetCurrentUser());
                        cmd.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);

                        conn.Open();
                        var rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Json(new { success = true, message = "Task updated successfully!" });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Task not found or no changes made." });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating task ID: {task.Id}");
                return Json(new { success = false, message = "Error updating task: " + ex.Message });
            }
        }

        // ============================================================
        // DELETE TASK - API
        // ============================================================
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = @"
                        UPDATE Tasks SET 
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
                            return Json(new { success = true, message = "Task deleted successfully!" });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Task not found." });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting task ID: {id}");
                return Json(new { success = false, message = "Error deleting task: " + ex.Message });
            }
        }

        // ============================================================
        // UPDATE TASK STATUS - API
        // ============================================================
        [HttpPost]
        public IActionResult UpdateStatus(int id, string status)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = @"
                        UPDATE Tasks SET 
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
                            return Json(new { success = false, message = "Task not found." });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating task status for ID: {id}");
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
        // GET TASK BY ID - Helper
        // ============================================================
        private UserTask? GetTaskById(int id)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT * FROM Tasks WHERE Id = @Id AND IsDeleted = 0";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new UserTask
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                LeadId = reader["LeadId"] != DBNull.Value ? Convert.ToInt32(reader["LeadId"]) : (int?)null,
                                OpportunityId = reader["OpportunityId"] != DBNull.Value ? Convert.ToInt32(reader["OpportunityId"]) : (int?)null,
                                AssignedTo = reader["AssignedTo"] != DBNull.Value ? Convert.ToInt32(reader["AssignedTo"]) : (int?)null,
                                TaskDate = Convert.ToDateTime(reader["TaskDate"]),
                                Summary = reader["Summary"]?.ToString() ?? string.Empty,
                                Status = reader["Status"]?.ToString() ?? "Pending",
                                Priority = reader["Priority"]?.ToString(),
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
        // GET LEADS WITH TASK COUNT - Helper
        // ============================================================
        private List<dynamic> GetLeadsWithTaskCount()
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
                        (SELECT COUNT(*) FROM Tasks WHERE LeadId = l.Id AND IsDeleted = 0) AS TaskCount
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
                                TaskCount = Convert.ToInt32(reader["TaskCount"])
                            });
                        }
                    }
                }
            }

            return leads;
        }
    }
}