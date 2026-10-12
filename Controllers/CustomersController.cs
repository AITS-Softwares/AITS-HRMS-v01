using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Erp_Aits.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Erp_Aits.Controllers
{
    public class CustomersController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(IConfiguration configuration, ILogger<CustomersController> logger)
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
        // INDEX - List all customers
        // ============================================================
        public IActionResult Index()
        {
            try
            {
                ViewData["Title"] = "Customers";
                ViewData["Module"] = "CRM";
                ViewData["Icon"] = "users";

                var customers = GetCustomers();
                return View(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Index action");
                TempData["Error"] = "Error loading customers. Please try again.";
                return View(new List<Customer>());
            }
        }

        // ============================================================
        // DETAILS - View single customer
        // ============================================================
        public IActionResult Details(int id)
        {
            try
            {
                ViewData["Title"] = "Customer Details";
                ViewData["Module"] = "CRM";
                ViewData["Icon"] = "user";

                var customer = GetCustomerById(id);
                if (customer == null)
                {
                    TempData["Error"] = "Customer not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in Details action for ID: {id}");
                TempData["Error"] = "Error loading customer details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ============================================================
        // CREATE - GET
        // ============================================================
        public IActionResult Create(int? leadId)
        {
            try
            {
                ViewData["Title"] = "Create Customer";
                ViewData["Module"] = "CRM";
                ViewData["Icon"] = "user-plus";

                var customer = new Customer();

                // If coming from Lead
                if (leadId.HasValue)
                {
                    var lead = GetLeadById(leadId.Value);
                    if (lead != null)
                    {
                        // Use FullName (read-only) but don't try to set it
                        // Instead, use the properties to build customer name
                        var fullName = lead.FullName; // This reads the computed property

                        // Set customer properties from lead data
                        customer.CustomerName = fullName; // Use the computed full name
                        customer.ContactPerson = fullName;
                        customer.MobileNumber = lead.MobileNo;
                        customer.PrimaryEmail = lead.Email;
                        customer.Domain = lead.Website;
                        customer.OrganizationName = lead.OrganizationName;
                        customer.City = lead.City;
                        customer.State = lead.State;
                        customer.Country = lead.Country;
                        customer.LeadId = leadId.Value;
                    }
                }

                customer.CustomerCode = GenerateCustomerCode();
                ViewBag.Users = GetActiveUsers();
                return View(customer);
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
        public IActionResult Create(Customer customer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                    {
                        string query = @"INSERT INTO Customers (
                            CustomerCode, CustomerName, CustomerGroup, CustomerType,
                            ContactPerson, MobileNumber, PrimaryEmail, AdditionalEmail,
                            Domain, PanNumber, GstNumber, GstCategory,
                            CommissionRate, AgentId, OrganizationName,
                            Address, City, State, Country, PinCode,
                            LeadId, CreatedBy, CompanyId
                        ) VALUES (
                            @CustomerCode, @CustomerName, @CustomerGroup, @CustomerType,
                            @ContactPerson, @MobileNumber, @PrimaryEmail, @AdditionalEmail,
                            @Domain, @PanNumber, @GstNumber, @GstCategory,
                            @CommissionRate, @AgentId, @OrganizationName,
                            @Address, @City, @State, @Country, @PinCode,
                            @LeadId, @CreatedBy, @CompanyId
                        );
                        SELECT SCOPE_IDENTITY();";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            if (string.IsNullOrEmpty(customer.CustomerCode))
                            {
                                customer.CustomerCode = GenerateCustomerCode();
                            }

                            cmd.Parameters.AddWithValue("@CustomerCode", customer.CustomerCode);
                            cmd.Parameters.AddWithValue("@CustomerName", customer.CustomerName ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@CustomerGroup", customer.CustomerGroup ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@CustomerType", customer.CustomerType ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@ContactPerson", customer.ContactPerson ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@MobileNumber", customer.MobileNumber ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@PrimaryEmail", customer.PrimaryEmail ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@AdditionalEmail", customer.AdditionalEmail ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Domain", customer.Domain ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@PanNumber", customer.PanNumber ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@GstNumber", customer.GstNumber ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@GstCategory", customer.GstCategory ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@CommissionRate", customer.CommissionRate ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@AgentId", customer.AgentId ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@OrganizationName", customer.OrganizationName ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Address", customer.Address ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@City", customer.City ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@State", customer.State ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Country", customer.Country ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@PinCode", customer.PinCode ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@LeadId", customer.LeadId ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@CreatedBy", GetCurrentUser());
                            cmd.Parameters.AddWithValue("@CompanyId", GetCompanyId() ?? (object)DBNull.Value);

                            conn.Open();
                            int newId = Convert.ToInt32(cmd.ExecuteScalar());

                            if (newId > 0)
                            {
                                // Update Lead status if coming from lead
                                if (customer.LeadId.HasValue)
                                {
                                    UpdateLeadStatus(customer.LeadId.Value, "Converted");
                                }

                                TempData["Success"] = "Customer created successfully!";
                                return RedirectToAction(nameof(Details), new { id = newId });
                            }
                            else
                            {
                                ModelState.AddModelError("", "Failed to create customer. Please try again.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Create POST action");
                ModelState.AddModelError("", "An error occurred while creating the customer.");
            }

            ViewBag.Users = GetActiveUsers();
            return View(customer);
        }

        // ============================================================
        // EDIT - GET
        // ============================================================
        public IActionResult Edit(int id)
        {
            try
            {
                ViewData["Title"] = "Edit Customer";
                ViewData["Module"] = "CRM";
                ViewData["Icon"] = "user-edit";

                var customer = GetCustomerById(id);
                if (customer == null)
                {
                    TempData["Error"] = "Customer not found.";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.Users = GetActiveUsers();
                return View(customer);
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
        public IActionResult Edit(int id, Customer customer)
        {
            try
            {
                if (id != customer.Id)
                {
                    return NotFound();
                }

                // Remove validation for these display/audit fields
                ModelState.Remove("AgentName");
                ModelState.Remove("CreatedBy");
                ModelState.Remove("CreatedDate");
                ModelState.Remove("ModifiedDate");

                if (ModelState.IsValid)
                {
                    using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                    {
                        string query = @"UPDATE Customers SET
                    CustomerName = @CustomerName,
                    CustomerGroup = @CustomerGroup,
                    CustomerType = @CustomerType,
                    ContactPerson = @ContactPerson,
                    MobileNumber = @MobileNumber,
                    PrimaryEmail = @PrimaryEmail,
                    AdditionalEmail = @AdditionalEmail,
                    Domain = @Domain,
                    PanNumber = @PanNumber,
                    GstNumber = @GstNumber,
                    GstCategory = @GstCategory,
                    CommissionRate = @CommissionRate,
                    AgentId = @AgentId,
                    OrganizationName = @OrganizationName,
                    Address = @Address,
                    City = @City,
                    State = @State,
                    Country = @Country,
                    PinCode = @PinCode,
                    ModifiedDate = GETDATE(),
                    ModifiedBy = @ModifiedBy
                    WHERE Id = @Id";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", id);
                            cmd.Parameters.AddWithValue("@CustomerName", customer.CustomerName ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@CustomerGroup", customer.CustomerGroup ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@CustomerType", customer.CustomerType ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@ContactPerson", customer.ContactPerson ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@MobileNumber", customer.MobileNumber ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@PrimaryEmail", customer.PrimaryEmail ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@AdditionalEmail", customer.AdditionalEmail ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Domain", customer.Domain ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@PanNumber", customer.PanNumber ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@GstNumber", customer.GstNumber ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@GstCategory", customer.GstCategory ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@CommissionRate", customer.CommissionRate ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@AgentId", customer.AgentId ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@OrganizationName", customer.OrganizationName ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Address", customer.Address ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@City", customer.City ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@State", customer.State ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Country", customer.Country ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@PinCode", customer.PinCode ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@ModifiedBy", GetCurrentUser());

                            conn.Open();
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                TempData["Success"] = "Customer updated successfully!";
                                return RedirectToAction(nameof(Details), new { id = customer.Id });
                            }
                            else
                            {
                                ModelState.AddModelError("", "Failed to update customer. No changes were made.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in Edit POST action for ID: {id}");
                ModelState.AddModelError("", "An error occurred while updating the customer.");
            }

            ViewBag.Users = GetActiveUsers();
            return View(customer);
        }

        // ============================================================
        // DELETE - GET (Confirmation)
        // ============================================================
        public IActionResult Delete(int id)
        {
            try
            {
                ViewData["Title"] = "Delete Customer";
                ViewData["Module"] = "CRM";
                ViewData["Icon"] = "user-slash";

                var customer = GetCustomerById(id);
                if (customer == null)
                {
                    TempData["Error"] = "Customer not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(customer);
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
                    string query = @"UPDATE Customers SET 
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
                            TempData["Success"] = "Customer deleted successfully!";
                        }
                        else
                        {
                            TempData["Error"] = "Failed to delete customer.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in Delete POST action for ID: {id}");
                TempData["Error"] = "An error occurred while deleting the customer.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // GET USERS FOR DROPDOWN - AJAX
        // ============================================================
        [HttpGet]
        public IActionResult GetUsers()
        {
            try
            {
                var users = GetActiveUsers();
                return Json(users.Select(u => new { id = u.Id, name = u.Name }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUsers action");
                return Json(new List<object>());
            }
        }

        // ============================================================
        // HELPER METHODS
        // ============================================================

        private List<Customer> GetCustomers()
        {
            var customers = new List<Customer>();

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                string query = @"SELECT c.*, u.Name as AgentName 
                                FROM Customers c
                                LEFT JOIN Users u ON c.AgentId = u.Id
                                WHERE c.IsActive = 1
                                ORDER BY c.CreatedDate DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var customer = MapDataReaderToCustomer(reader);
                            customer.AgentName = reader["AgentName"]?.ToString();
                            customers.Add(customer);
                        }
                    }
                }
            }

            return customers;
        }

        private Customer? GetCustomerById(int id)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                string query = @"SELECT c.*, u.Name as AgentName 
                                FROM Customers c
                                LEFT JOIN Users u ON c.AgentId = u.Id
                                WHERE c.Id = @Id AND c.IsActive = 1";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var customer = MapDataReaderToCustomer(reader);
                            customer.AgentName = reader["AgentName"]?.ToString();
                            return customer;
                        }
                    }
                }
            }
            return null;
        }

        private Customer MapDataReaderToCustomer(SqlDataReader reader)
        {
            return new Customer
            {
                Id = Convert.ToInt32(reader["Id"]),
                CustomerCode = reader["CustomerCode"]?.ToString() ?? string.Empty,
                CustomerName = reader["CustomerName"]?.ToString() ?? string.Empty,
                CustomerGroup = reader["CustomerGroup"]?.ToString(),
                CustomerType = reader["CustomerType"]?.ToString(),
                ContactPerson = reader["ContactPerson"]?.ToString() ?? string.Empty,
                MobileNumber = reader["MobileNumber"]?.ToString() ?? string.Empty,
                PrimaryEmail = reader["PrimaryEmail"]?.ToString(),
                AdditionalEmail = reader["AdditionalEmail"]?.ToString(),
                Domain = reader["Domain"]?.ToString(),
                PanNumber = reader["PanNumber"]?.ToString(),
                GstNumber = reader["GstNumber"]?.ToString(),
                GstCategory = reader["GstCategory"]?.ToString(),
                CommissionRate = reader["CommissionRate"] != DBNull.Value ? Convert.ToDecimal(reader["CommissionRate"]) : null,
                AgentId = reader["AgentId"] != DBNull.Value ? Convert.ToInt32(reader["AgentId"]) : null,
                OrganizationName = reader["OrganizationName"]?.ToString(),
                Address = reader["Address"]?.ToString(),
                City = reader["City"]?.ToString(),
                State = reader["State"]?.ToString(),
                Country = reader["Country"]?.ToString(),
                PinCode = reader["PinCode"]?.ToString(),
                LeadId = reader["LeadId"] != DBNull.Value ? Convert.ToInt32(reader["LeadId"]) : null,
                CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                CreatedBy = reader["CreatedBy"]?.ToString(),
                ModifiedDate = reader["ModifiedDate"] != DBNull.Value ? Convert.ToDateTime(reader["ModifiedDate"]) : null,
                ModifiedBy = reader["ModifiedBy"]?.ToString(),
                IsActive = Convert.ToBoolean(reader["IsActive"])
            };
        }

        private Lead? GetLeadById(int id)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                string query = @"SELECT Id, FirstName, MiddleName, LastName, Email, MobileNo, 
                                Website, OrganizationName, City, State, Country, Status 
                                FROM Leads WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Lead
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                FirstName = reader["FirstName"]?.ToString() ?? string.Empty,
                                MiddleName = reader["MiddleName"]?.ToString(),
                                LastName = reader["LastName"]?.ToString(),
                                Email = reader["Email"]?.ToString() ?? string.Empty,
                                MobileNo = reader["MobileNo"]?.ToString() ?? string.Empty,
                                Website = reader["Website"]?.ToString(),
                                OrganizationName = reader["OrganizationName"]?.ToString(),
                                City = reader["City"]?.ToString(),
                                State = reader["State"]?.ToString(),
                                Country = reader["Country"]?.ToString(),
                                Status = reader["Status"]?.ToString()
                            };
                        }
                    }
                }
            }
            return null;
        }

        private void UpdateLeadStatus(int leadId, string status)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                string query = @"UPDATE Leads SET 
                                Status = @Status,
                                ModifiedDate = GETDATE(),
                                ModifiedBy = @ModifiedBy 
                                WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", leadId);
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@ModifiedBy", GetCurrentUser());

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private List<User> GetActiveUsers()
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

        private string GenerateCustomerCode()
        {
            string datePart = DateTime.Now.ToString("yyyyMMdd");
            string randomPart = new Random().Next(100, 999).ToString();

            string code = $"CUST-{datePart}-{randomPart}";

            // Check if code exists and regenerate if needed
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT COUNT(*) FROM Customers WHERE CustomerCode = @CustomerCode";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CustomerCode", code);
                    conn.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());

                    while (count > 0)
                    {
                        randomPart = new Random().Next(100, 999).ToString();
                        code = $"CUST-{datePart}-{randomPart}";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@CustomerCode", code);
                        count = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }

            return code;
        }
    }
}