using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using Erp_Aits.Models;

namespace Erp_Aits.Controllers
{
    public class TasksController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TasksController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TasksController(IConfiguration configuration, ILogger<TasksController> logger, IWebHostEnvironment webHostEnvironment)
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

        // GET: Tasks/CreateTaskModal
        public IActionResult CreateTaskModal(int leadId = 0, int opportunityId = 0)
        {
            var users = GetUsers();

            var viewModel = new UserTaskViewModel
            {
                UserTask = new UserTask
                {
                    LeadId = leadId > 0 ? leadId : (int?)null,
                    OpportunityId = opportunityId > 0 ? opportunityId : (int?)null,
                    TaskDate = DateTime.Now,
                    Status = "Pending",
                    Priority = "Medium"
                },
                UserList = users,
                StatusList = new List<string> { "Pending", "In Progress", "Completed", "Cancelled" },
                PriorityList = new List<string> { "Low", "Medium", "High", "Urgent" }
            };

            return PartialView("_CreateTaskModal", viewModel);
        }

        // POST: Tasks/Create - FIXED with file upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(UserTaskViewModel viewModel)
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
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "tasks");
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
                        attachmentPath = "/uploads/tasks/" + uniqueFileName;
                    }

                    using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                    {
                        string query = @"INSERT INTO Tasks (
                            LeadId, OpportunityId, AssignedTo, TaskDate, Summary, 
                            AttachmentPath, Status, Priority, CreatedBy
                        ) VALUES (
                            @LeadId, @OpportunityId, @AssignedTo, @TaskDate, @Summary,
                            @AttachmentPath, @Status, @Priority, @CreatedBy
                        );
                        SELECT SCOPE_IDENTITY();";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            var task = viewModel.UserTask;

                            cmd.Parameters.AddWithValue("@LeadId", (object?)task.LeadId ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@OpportunityId", (object?)task.OpportunityId ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@AssignedTo", (object?)task.AssignedTo ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@TaskDate", task.TaskDate);
                            cmd.Parameters.AddWithValue("@Summary", task.Summary);
                            cmd.Parameters.AddWithValue("@AttachmentPath", (object?)attachmentPath ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Status", task.Status);
                            cmd.Parameters.AddWithValue("@Priority", (object?)task.Priority ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@CreatedBy", GetCurrentUser());

                            conn.Open();
                            int newId = Convert.ToInt32(cmd.ExecuteScalar());

                            if (newId > 0)
                            {
                                return Json(new { success = true, message = "Task created successfully!" });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                return Json(new { success = false, message = "Error creating task: " + ex.Message });
            }

            return Json(new { success = false, message = "Failed to create task." });
        }

        // GET: Tasks/GetTasks
        public IActionResult GetTasks(int leadId = 0, int opportunityId = 0)
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
                        WHERE t.IsDeleted = 0";

                    if (leadId > 0)
                    {
                        query += " AND t.LeadId = @LeadId";
                    }
                    else if (opportunityId > 0)
                    {
                        query += " AND t.OpportunityId = @OpportunityId";
                    }

                    query += " ORDER BY t.TaskDate DESC";

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
                _logger.LogError(ex, "Error getting tasks");
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