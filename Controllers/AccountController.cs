using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using Erp_Aits.Models;

namespace Erp_Aits.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IConfiguration configuration, ILogger<AccountController> logger)
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

        private bool VerifyPassword(string password, string storedHash)
        {
            string hash = HashPassword(password);
            return hash == storedHash;
        }

        private static bool IsDatabaseConnectionError(Exception ex)
        {
            if (ex is SqlException sqlEx)
            {
                return sqlEx.Number == -2 ||
                       sqlEx.Message.Contains("pre-login handshake", StringComparison.OrdinalIgnoreCase) ||
                       sqlEx.Message.Contains("Cannot open database", StringComparison.OrdinalIgnoreCase) ||
                       sqlEx.Message.Contains("Login failed", StringComparison.OrdinalIgnoreCase);
            }

            return ex.Message.Contains("pre-login handshake", StringComparison.OrdinalIgnoreCase) ||
                   ex.Message.Contains("Connection Timeout", StringComparison.OrdinalIgnoreCase) ||
                   ex.Message.Contains("Cannot open database", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetLoginErrorMessage(Exception ex)
        {
            if (IsDatabaseConnectionError(ex))
            {
                return "Database connection failed. Please make sure SQL Server is running, the Erp database is online, and the login has access.";
            }

            return "Login failed. Please try again.";
        }

        // GET: Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            // Check if user is already logged in
            if (HttpContext.Session.GetString("UserId") != null ||
                HttpContext.Session.GetString("CompanyId") != null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Check for remember me cookie
            var emailCookie = Request.Cookies["RememberEmail"];
            if (!string.IsNullOrEmpty(emailCookie))
            {
                var model = new LoginModel
                {
                    Email = emailCookie,
                    RememberMe = true
                };
                return View(model);
            }

            return View();
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CompanyLogin([Bind("Email,Password,RememberMe")] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Login", model);
            }

            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string query = @"SELECT Id,
                                    CompanyName,
                                    ContactPersonName,
                                    OfficialEmail,
                                    PasswordHash,
                                    IsActive
                             FROM Companies
                             WHERE OfficialEmail = @Email";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Email", model.Email);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                ModelState.AddModelError("", "Invalid email or password.");
                                return View("Login", model);
                            }

                            int companyId = Convert.ToInt32(reader["Id"]);
                            string companyName = reader["CompanyName"].ToString();
                            string contactPerson = reader["ContactPersonName"].ToString();
                            string email = reader["OfficialEmail"].ToString();
                            string passwordHash = reader["PasswordHash"].ToString();
                            bool isActive = Convert.ToBoolean(reader["IsActive"]);

                            if (!isActive)
                            {
                                ModelState.AddModelError("", "Your account is inactive.");
                                return View("Login", model);
                            }

                            if (!VerifyPassword(model.Password, passwordHash))
                            {
                                ModelState.AddModelError("", "Invalid email or password.");
                                return View("Login", model);
                            }

                            HttpContext.Session.SetString("CompanyId", companyId.ToString());
                            HttpContext.Session.SetString("CompanyName", companyName);
                            HttpContext.Session.SetString("ContactPerson", contactPerson);
                            HttpContext.Session.SetString("Email", email);
                            HttpContext.Session.SetString("LoginType", "Company");
                            HttpContext.Session.SetString("UserRole", "Admin");

                            UpdateCompanyLastLogin(companyId);

                            if (model.RememberMe)
                            {
                                Response.Cookies.Append(
                                    "RememberEmail",
                                    model.Email,
                                    new CookieOptions
                                    {
                                        Expires = DateTime.Now.AddDays(30),
                                        HttpOnly = true,
                                        Secure = true,
                                        SameSite = SameSiteMode.Strict
                                    });
                            }
                            else
                            {
                                Response.Cookies.Delete("RememberEmail");
                            }

                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Company Login Error");
                ModelState.AddModelError("", GetLoginErrorMessage(ex));
                return View("Login", model);
            }
        }

        //// POST: Account/CompanyLogin
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult CompanyLogin([Bind("Email,Password,RememberMe")] LoginModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View("Login", model);
        //    }

        //    try
        //    {
        //        using (SqlConnection conn = new SqlConnection(GetConnectionString()))
        //        {
        //            string query = @"SELECT Id, CompanyName, ContactPersonName, OfficialEmail, 
        //                                    PasswordHash, IsActive 
        //                             FROM Companies 
        //                             WHERE OfficialEmail = @Email";

        //            using (SqlCommand cmd = new SqlCommand(query, conn))
        //            {
        //                cmd.Parameters.AddWithValue("@Email", model.Email);
        //                conn.Open();

        //                using (SqlDataReader reader = cmd.ExecuteReader())
        //                {
        //                    if (reader.Read())
        //                    {
        //                        int companyId = reader.GetInt32(0);
        //                        string companyName = reader.GetString(1);
        //                        string contactPerson = reader.GetString(2);
        //                        string email = reader.GetString(3);
        //                        string storedHash = reader.GetString(4);
        //                        bool isActive = reader.GetBoolean(5);

        //                        if (!isActive)
        //                        {
        //                            ModelState.AddModelError("", "Your company account is deactivated. Please contact support.");
        //                            return View("Login", model);
        //                        }

        //                        if (VerifyPassword(model.Password, storedHash))
        //                        {
        //                            // Set session for Company
        //                            HttpContext.Session.SetString("CompanyId", companyId.ToString());
        //                            HttpContext.Session.SetString("CompanyName", companyName);
        //                            HttpContext.Session.SetString("ContactPerson", contactPerson);
        //                            HttpContext.Session.SetString("Email", email);
        //                            HttpContext.Session.SetString("LoginType", "Company");
        //                            HttpContext.Session.SetString("UserRole", "Admin"); // Company acts as Admin

        //                            UpdateCompanyLastLogin(companyId);

        //                            if (model.RememberMe)
        //                            {
        //                                CookieOptions options = new CookieOptions
        //                                {
        //                                    Expires = DateTime.Now.AddDays(30),
        //                                    HttpOnly = true,
        //                                    Secure = true,
        //                                    SameSite = SameSiteMode.Strict
        //                                };
        //                                Response.Cookies.Append("RememberEmail", model.Email, options);
        //                            }
        //                            else
        //                            {
        //                                Response.Cookies.Delete("RememberEmail");
        //                            }

        //                            _logger.LogInformation($"Company {email} logged in successfully.");
        //                            TempData["Success"] = $"Welcome back, {companyName}!";

        //                            return RedirectToAction("Index", "Home");
        //                        }
        //                        else
        //                        {
        //                            ModelState.AddModelError("", "Invalid email or password.");
        //                        }
        //                    }
        //                    else
        //                    {
        //                        ModelState.AddModelError("", "Invalid email or password.");
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Error during company login for email: {model.Email}");
        //        ModelState.AddModelError("", "An error occurred during login. Please try again.");
        //    }

        //    return View("Login", model);
        //}

        // POST: Account/UserLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UserLogin([Bind("Email,Password,RememberMe")] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Login", model);
            }

            try
            {


                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = @"SELECT Id, Name, Email, PasswordHash, Role, IsActive, OnLeave, CompanyId 
                                     FROM Users 
                                     WHERE Email = @Email AND IsDeleted = 0";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", model.Email);
                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int userId = reader.GetInt32(0);
                                string name = reader.GetString(1);
                                string email = reader.GetString(2);
                                string storedHash = reader.GetString(3);
                                string role = reader.GetString(4);
                                bool isActive = reader.GetBoolean(5);
                                bool onLeave = reader.GetBoolean(6);
                                int? companyId = reader.IsDBNull(7) ? null : reader.GetInt32(7);

                                if (!isActive)
                                {
                                    ModelState.AddModelError("", "Your account is deactivated. Please contact administrator.");
                                    return View("Login", model);
                                }

                                if (onLeave)
                                {
                                    ModelState.AddModelError("", "Your account is on leave. Please contact administrator.");
                                    return View("Login", model);
                                }

                                if (VerifyPassword(model.Password, storedHash))
                                {
                                    // Set session for User
                                    HttpContext.Session.SetString("UserId", userId.ToString());
                                    HttpContext.Session.SetString("UserName", name);
                                    HttpContext.Session.SetString("UserEmail", email);
                                    HttpContext.Session.SetString("UserRole", role);
                                    HttpContext.Session.SetString("LoginType", "User");
                                    if (companyId.HasValue)
                                    {
                                        HttpContext.Session.SetString("CompanyId", companyId.Value.ToString());
                                    }

                                    UpdateUserLastLogin(userId);

                                    if (model.RememberMe)
                                    {
                                        CookieOptions options = new CookieOptions
                                        {
                                            Expires = DateTime.Now.AddDays(30),
                                            HttpOnly = true,
                                            Secure = true,
                                            SameSite = SameSiteMode.Strict
                                        };
                                        Response.Cookies.Append("RememberEmail", model.Email, options);
                                    }
                                    else
                                    {
                                        Response.Cookies.Delete("RememberEmail");
                                    }

                                    _logger.LogInformation($"User {email} logged in successfully.");
                                    TempData["Success"] = $"Welcome back, {name}!";

                                    return RedirectToAction("Index", "Home");
                                }
                                else
                                {
                                    ModelState.AddModelError("", "Invalid email or password.");
                                }
                            }
                            else
                            {
                                ModelState.AddModelError("", "Invalid email or password.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during user login for email: {model.Email}");
                ModelState.AddModelError("", GetLoginErrorMessage(ex));
            }

            return View("Login", model);
        }

        // GET: Account/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            try
            {
                HttpContext.Session.Clear();
                Response.Cookies.Delete("RememberEmail");
                TempData["Success"] = "You have been logged out successfully.";
                _logger.LogInformation("User logged out successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
            }

            return RedirectToAction("Login");
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(Company model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (CompanyEmailExists(model.OfficialEmail))
                    {
                        ModelState.AddModelError("OfficialEmail", "Email is already registered.");
                        return View(model);
                    }

                    using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                    {
                        string query = @"INSERT INTO Companies (
                            CompanyName, ContactPersonName, OfficialEmail, PhoneNumber,
                            CompanyAddress, GSTNumber, CompanyType, PANNumber, PasswordHash
                        ) VALUES (
                            @CompanyName, @ContactPersonName, @OfficialEmail, @PhoneNumber,
                            @CompanyAddress, @GSTNumber, @CompanyType, @PANNumber, @PasswordHash
                        );
                        SELECT SCOPE_IDENTITY();";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            string passwordHash = HashPassword(model.Password);

                            cmd.Parameters.AddWithValue("@CompanyName", model.CompanyName);
                            cmd.Parameters.AddWithValue("@ContactPersonName", model.ContactPersonName);
                            cmd.Parameters.AddWithValue("@OfficialEmail", model.OfficialEmail);
                            cmd.Parameters.AddWithValue("@PhoneNumber", model.PhoneNumber);
                            cmd.Parameters.AddWithValue("@CompanyAddress", model.CompanyAddress);
                            cmd.Parameters.AddWithValue("@GSTNumber", string.IsNullOrEmpty(model.GSTNumber) ? DBNull.Value : (object)model.GSTNumber);
                            cmd.Parameters.AddWithValue("@CompanyType", model.CompanyType);
                            cmd.Parameters.AddWithValue("@PANNumber", model.PANNumber);
                            cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);

                            conn.Open();
                            int newId = Convert.ToInt32(cmd.ExecuteScalar());

                            if (newId > 0)
                            {
                                TempData["Success"] = "Registration successful! Please login.";
                                return RedirectToAction("Login");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during registration");
                    ModelState.AddModelError("", "An error occurred during registration. Please try again.");
                }
            }

            return View(model);
        }

        // GET: Account/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Helper Methods
        private bool CompanyEmailExists(string email)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT COUNT(*) FROM Companies WHERE OfficialEmail = @Email";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    conn.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        private void UpdateCompanyLastLogin(int companyId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = "UPDATE Companies SET LastLoginDate = GETDATE() WHERE Id = @Id";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", companyId);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to update last login for company ID: {companyId}");
            }
        }

        private void UpdateUserLastLogin(int userId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = @"UPDATE Users SET 
                                    ModifiedDate = GETDATE() 
                                    WHERE Id = @Id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", userId);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to update last login for user ID: {userId}");
            }
        }
    }
}
