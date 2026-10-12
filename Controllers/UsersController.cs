using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using Erp_Aits.Models;

namespace Erp_Aits.Controllers
{
    public class UsersController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IConfiguration configuration, ILogger<UsersController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("DefaultConnection");
        }

        private string HashPassword(string password)
        {
            string salt = _configuration["AppSettings:Salt"] ?? "DefaultSalt";
            using (SHA256 sha256 = SHA256.Create())
            {
                string saltedPassword = password + salt;
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                return Convert.ToBase64String(bytes);
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

        // ============================================================
        // INDEX - List all users
        // ============================================================
        public IActionResult Index(UserSearchModel searchModel)
        {
            try
            {
                ViewData["Title"] = "Users";
                ViewData["Module"] = "Administration";
                ViewData["Icon"] = "users-gear";

                var users = GetUsers(searchModel);

                var viewModel = new UserViewModel
                {
                    User = new User(),
                    RoleList = GetRoles()
                };

                ViewBag.Users = users;
                ViewBag.SearchModel = searchModel;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Index action");
                TempData["Error"] = "Error loading users. Please try again.";
                return View(new UserViewModel());
            }
        }

        // ============================================================
        // DETAILS - View single user
        // ============================================================
        public IActionResult Details(int id)
        {
            try
            {
                ViewData["Title"] = "User Details";
                ViewData["Module"] = "Administration";
                ViewData["Icon"] = "user";

                var user = GetUserById(id);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in Details action for ID: {id}");
                TempData["Error"] = "Error loading user details.";
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
                ViewData["Title"] = "Create User";
                ViewData["Module"] = "Administration";
                ViewData["Icon"] = "user-plus";

                var viewModel = new UserViewModel
                {
                    User = new User
                    {
                        IsActive = true,
                        OnLeave = false,
                        Role = "User"
                    },
                    RoleList = GetRoles()
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
        public IActionResult Create(UserViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if email already exists
                    if (EmailExists(viewModel.User.Email))
                    {
                        ModelState.AddModelError("User.Email", "Email is already registered.");
                        viewModel.RoleList = GetRoles();
                        return View(viewModel);
                    }

                    using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                    {
                        string query = @"INSERT INTO Users (
                            Name, Email, PasswordHash, Role, IsActive, OnLeave,
                            CreatedBy, CompanyId
                        ) VALUES (
                            @Name, @Email, @PasswordHash, @Role, @IsActive, @OnLeave,
                            @CreatedBy, @CompanyId
                        );
                        SELECT SCOPE_IDENTITY();";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            var user = viewModel.User;
                            string passwordHash = HashPassword(user.Password);

                            cmd.Parameters.AddWithValue("@Name", user.Name);
                            cmd.Parameters.AddWithValue("@Email", user.Email);
                            cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                            cmd.Parameters.AddWithValue("@Role", user.Role);
                            cmd.Parameters.AddWithValue("@IsActive", user.IsActive);
                            cmd.Parameters.AddWithValue("@OnLeave", user.OnLeave);
                            cmd.Parameters.AddWithValue("@CreatedBy", GetCurrentUser());
                            cmd.Parameters.AddWithValue("@CompanyId", GetCompanyId() ?? (object)DBNull.Value);

                            conn.Open();
                            int newId = Convert.ToInt32(cmd.ExecuteScalar());

                            if (newId > 0)
                            {
                                TempData["Success"] = "User created successfully!";
                                return RedirectToAction(nameof(Index));
                            }
                            else
                            {
                                ModelState.AddModelError("", "Failed to create user. Please try again.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Create POST action");
                ModelState.AddModelError("", "An error occurred while creating the user.");
            }

            viewModel.RoleList = GetRoles();
            return View(viewModel);
        }

        // ============================================================
        // EDIT - GET
        // ============================================================
        public IActionResult Edit(int id)
        {
            try
            {
                ViewData["Title"] = "Edit User";
                ViewData["Module"] = "Administration";
                ViewData["Icon"] = "user-edit";

                var user = GetUserById(id);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Clear password for security
                user.Password = string.Empty;
                user.ConfirmPassword = string.Empty;

                var viewModel = new UserViewModel
                {
                    User = user,
                    RoleList = GetRoles()
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
        // ============================================================
        // EDIT - POST
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, UserViewModel viewModel)
        {
            try
            {
                if (id != viewModel.User.Id)
                {
                    return NotFound();
                }

                // ============================================================
                // IMPORTANT: Remove password validation from ModelState
                // ============================================================
                ModelState.Remove("User.Password");
                ModelState.Remove("User.ConfirmPassword");

                // Also remove any validation errors for password fields
                if (ModelState.ContainsKey("User.Password"))
                {
                    ModelState["User.Password"].Errors.Clear();
                }
                if (ModelState.ContainsKey("User.ConfirmPassword"))
                {
                    ModelState["User.ConfirmPassword"].Errors.Clear();
                }

                // Check if ModelState is valid (excluding password)
                if (ModelState.IsValid)
                {
                    using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                    {
                        // Check if email already exists (excluding current user)
                        if (EmailExists(viewModel.User.Email, id))
                        {
                            ModelState.AddModelError("User.Email", "Email is already registered.");
                            viewModel.RoleList = GetRoles();
                            return View(viewModel);
                        }

                        // Build update query - only update password if provided
                        string query = @"UPDATE Users SET
                            Name = @Name,
                            Email = @Email,
                            Role = @Role,
                            IsActive = @IsActive,
                            OnLeave = @OnLeave,
                            ModifiedDate = GETDATE(),
                            ModifiedBy = @ModifiedBy";

                        // Add password update only if password is provided
                        if (!string.IsNullOrEmpty(viewModel.User.Password))
                        {
                            query += ", PasswordHash = @PasswordHash";
                        }

                        query += " WHERE Id = @Id";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            var user = viewModel.User;

                            cmd.Parameters.AddWithValue("@Id", id);
                            cmd.Parameters.AddWithValue("@Name", user.Name);
                            cmd.Parameters.AddWithValue("@Email", user.Email);
                            cmd.Parameters.AddWithValue("@Role", user.Role);
                            cmd.Parameters.AddWithValue("@IsActive", user.IsActive);
                            cmd.Parameters.AddWithValue("@OnLeave", user.OnLeave);
                            cmd.Parameters.AddWithValue("@ModifiedBy", GetCurrentUser());

                            // Only add password parameter if provided
                            if (!string.IsNullOrEmpty(user.Password))
                            {
                                string passwordHash = HashPassword(user.Password);
                                cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                            }

                            conn.Open();
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                TempData["Success"] = "User updated successfully!";
                                return RedirectToAction(nameof(Index));
                            }
                            else
                            {
                                ModelState.AddModelError("", "Failed to update user. No changes were made.");
                            }
                        }
                    }
                }
                else
                {
                    // Log validation errors for debugging
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    _logger.LogWarning($"Validation errors: {string.Join(", ", errors)}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in Edit POST action for ID: {id}");
                ModelState.AddModelError("", "An error occurred while updating the user.");
            }

            viewModel.RoleList = GetRoles();
            return View(viewModel);
        }
        // ============================================================
        // DELETE - GET (Confirmation)
        // ============================================================
        public IActionResult Delete(int id)
        {
            try
            {
                ViewData["Title"] = "Delete User";
                ViewData["Module"] = "Administration";
                ViewData["Icon"] = "user-slash";

                var user = GetUserById(id);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(user);
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
                    string query = @"UPDATE Users SET 
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
                            TempData["Success"] = "User deleted successfully!";
                        }
                        else
                        {
                            TempData["Error"] = "Failed to delete user.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in Delete POST action for ID: {id}");
                TempData["Error"] = "An error occurred while deleting the user.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // TOGGLE ACTIVE STATUS - AJAX
        // ============================================================
        [HttpPost]
        public IActionResult ToggleActive(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = @"UPDATE Users SET 
                                    IsActive = ~IsActive,
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
                            return Json(new { success = true, message = "Status updated successfully!" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in ToggleActive action for ID: {id}");
                return Json(new { success = false, message = "Error updating status." });
            }

            return Json(new { success = false, message = "Failed to update status." });
        }

        // ============================================================
        // TOGGLE LEAVE STATUS - AJAX
        // ============================================================
        [HttpPost]
        public IActionResult ToggleLeave(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = @"UPDATE Users SET 
                                    OnLeave = ~OnLeave,
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
                            return Json(new { success = true, message = "Leave status updated successfully!" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in ToggleLeave action for ID: {id}");
                return Json(new { success = false, message = "Error updating leave status." });
            }

            return Json(new { success = false, message = "Failed to update leave status." });
        }

        // ============================================================
        // HELPER METHODS
        // ============================================================

        private List<User> GetUsers(UserSearchModel search = null)
        {
            var users = new List<User>();

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT * FROM Users WHERE IsDeleted = 0";

                if (search != null)
                {
                    var conditions = new List<string>();

                    if (!string.IsNullOrEmpty(search.SearchTerm))
                    {
                        conditions.Add("(Name LIKE @SearchTerm OR Email LIKE @SearchTerm)");
                    }
                    if (!string.IsNullOrEmpty(search.Role))
                    {
                        conditions.Add("Role = @Role");
                    }
                    if (search.IsActive.HasValue)
                    {
                        conditions.Add("IsActive = @IsActive");
                    }
                    if (search.OnLeave.HasValue)
                    {
                        conditions.Add("OnLeave = @OnLeave");
                    }

                    if (conditions.Any())
                    {
                        query += " AND " + string.Join(" AND ", conditions);
                    }
                }

                query += " ORDER BY Name ASC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (search != null)
                    {
                        if (!string.IsNullOrEmpty(search.SearchTerm))
                        {
                            cmd.Parameters.AddWithValue("@SearchTerm", $"%{search.SearchTerm}%");
                        }
                        if (!string.IsNullOrEmpty(search.Role))
                        {
                            cmd.Parameters.AddWithValue("@Role", search.Role);
                        }
                        if (search.IsActive.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@IsActive", search.IsActive.Value);
                        }
                        if (search.OnLeave.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@OnLeave", search.OnLeave.Value);
                        }
                    }

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(MapDataReaderToUser(reader));
                        }
                    }
                }
            }

            return users;
        }

        private User? GetUserById(int id)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT * FROM Users WHERE Id = @Id AND IsDeleted = 0";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapDataReaderToUser(reader);
                        }
                    }
                }
            }
            return null;
        }

        private User MapDataReaderToUser(SqlDataReader reader)
        {
            return new User
            {
                Id = Convert.ToInt32(reader["Id"]),
                Name = reader["Name"]?.ToString() ?? string.Empty,
                Email = reader["Email"]?.ToString() ?? string.Empty,
                PasswordHash = reader["PasswordHash"]?.ToString(),
                Role = reader["Role"]?.ToString() ?? "User",
                IsActive = Convert.ToBoolean(reader["IsActive"]),
                OnLeave = Convert.ToBoolean(reader["OnLeave"]),
                CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                CreatedBy = reader["CreatedBy"]?.ToString(),
                ModifiedDate = reader["ModifiedDate"] != DBNull.Value ? Convert.ToDateTime(reader["ModifiedDate"]) : null,
                ModifiedBy = reader["ModifiedBy"]?.ToString(),
                IsDeleted = Convert.ToBoolean(reader["IsDeleted"]),
                CompanyId = reader["CompanyId"] != DBNull.Value ? Convert.ToInt32(reader["CompanyId"]) : null
            };
        }

        private bool EmailExists(string email, int? excludeId = null)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT COUNT(*) FROM Users WHERE Email = @Email AND IsDeleted = 0";
                if (excludeId.HasValue)
                {
                    query += " AND Id != @ExcludeId";
                }

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    if (excludeId.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@ExcludeId", excludeId.Value);
                    }

                    conn.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        private List<string> GetRoles()
        {
            return new List<string> { "Admin", "Manager", "User", "Viewer" };
        }
    }
}