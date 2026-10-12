using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class PageAccessService : IPageAccessService
    {
        private readonly string _connectionString;

        public PageAccessService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<bool> HasAccess(int userId, string controller, string action)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    string query = @"
                        SELECT CASE 
                            WHEN EXISTS (
                                SELECT 1 FROM UserPermissions up
                                INNER JOIN Permissions p ON up.PermissionId = p.Id
                                WHERE up.UserId = @UserId 
                                AND p.ControllerName = @Controller
                                AND p.ActionName = @Action
                                AND up.CanView = 1
                            ) THEN 1
                            ELSE 0
                        END AS HasAccess";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@Controller", controller);
                        cmd.Parameters.AddWithValue("@Action", action);

                        await conn.OpenAsync();
                        var result = await cmd.ExecuteScalarAsync();
                        return Convert.ToInt32(result) == 1;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool IsPageAccessible(int userId, string controller, string action)
        {
            return HasAccess(userId, controller, action).GetAwaiter().GetResult();
        }

        public async Task<List<Permission>> GetUserPages(int userId)
        {
            var pages = new List<Permission>();

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    string query = @"
                        SELECT p.* 
                        FROM Permissions p
                        INNER JOIN UserPermissions up ON p.Id = up.PermissionId
                        WHERE up.UserId = @UserId 
                        AND up.CanView = 1
                        AND p.IsActive = 1
                        ORDER BY p.MenuOrder";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        await conn.OpenAsync();

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                pages.Add(MapToPermission(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUserPages: {ex.Message}");
            }

            return pages;
        }

        public async Task<List<Permission>> GetUserMenuPages(int userId)
        {
            var pages = await GetUserPages(userId);
            return pages.Where(p => p.MenuOrder.HasValue).OrderBy(p => p.MenuOrder).ToList();
        }

        public async Task<List<UserPermission>> GetUserAccesses(int userId)
        {
            var accesses = new List<UserPermission>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM UserPermissions WHERE UserId = @UserId";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    await conn.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            accesses.Add(new UserPermission
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                UserId = Convert.ToInt32(reader["UserId"]),
                                PermissionId = Convert.ToInt32(reader["PermissionId"]),
                                CanView = Convert.ToBoolean(reader["CanView"]),
                                CanCreate = Convert.ToBoolean(reader["CanCreate"]),
                                CanEdit = Convert.ToBoolean(reader["CanEdit"]),
                                CanDelete = Convert.ToBoolean(reader["CanDelete"]),
                                CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                                CreatedBy = reader["CreatedBy"]?.ToString()
                            });
                        }
                    }
                }
            }

            return accesses;
        }

        // FIXED: AssignPageAccess - Properly sets CanCreate, CanEdit, CanDelete based on ActionName
        public async Task<bool> AssignPageAccess(int userId, List<int> pageIds)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    // First, get all permissions to know which ones are Create/Edit/Delete
                    string getPermissionsQuery = "SELECT Id, ActionName FROM Permissions WHERE IsActive = 1";
                    var permissionActions = new Dictionary<int, string>();
                    using (var cmd = new SqlCommand(getPermissionsQuery, conn))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                permissionActions[Convert.ToInt32(reader["Id"])] = reader["ActionName"]?.ToString() ?? "";
                            }
                        }
                    }

                    // Delete existing permissions for this user
                    string deleteQuery = "DELETE FROM UserPermissions WHERE UserId = @UserId";
                    using (var deleteCmd = new SqlCommand(deleteQuery, conn))
                    {
                        deleteCmd.Parameters.AddWithValue("@UserId", userId);
                        await deleteCmd.ExecuteNonQueryAsync();
                        Console.WriteLine($"Deleted existing permissions for user {userId}");
                    }

                    // Insert new permissions with proper values
                    foreach (var pageId in pageIds)
                    {
                        bool canView = true;
                        bool canCreate = false;
                        bool canEdit = false;
                        bool canDelete = false;

                        // Set permissions based on action name
                        if (permissionActions.TryGetValue(pageId, out string actionName))
                        {
                            switch (actionName.ToLower())
                            {
                                case "create":
                                    canCreate = true;
                                    break;
                                case "edit":
                                    canEdit = true;
                                    break;
                                case "delete":
                                    canDelete = true;
                                    break;
                                case "index":
                                case "details":
                                    canView = true;
                                    break;
                                default:
                                    canView = true;
                                    break;
                            }
                        }

                        string insertQuery = @"
                            INSERT INTO UserPermissions (UserId, PermissionId, CanView, CanCreate, CanEdit, CanDelete, CreatedDate)
                            VALUES (@UserId, @PermissionId, @CanView, @CanCreate, @CanEdit, @CanDelete, GETDATE())";

                        using (var insertCmd = new SqlCommand(insertQuery, conn))
                        {
                            insertCmd.Parameters.AddWithValue("@UserId", userId);
                            insertCmd.Parameters.AddWithValue("@PermissionId", pageId);
                            insertCmd.Parameters.AddWithValue("@CanView", canView);
                            insertCmd.Parameters.AddWithValue("@CanCreate", canCreate);
                            insertCmd.Parameters.AddWithValue("@CanEdit", canEdit);
                            insertCmd.Parameters.AddWithValue("@CanDelete", canDelete);
                            await insertCmd.ExecuteNonQueryAsync();
                            Console.WriteLine($"Inserted permission {pageId} for user {userId} - View:{canView}, Create:{canCreate}, Edit:{canEdit}, Delete:{canDelete}");
                        }
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AssignPageAccess: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RemovePageAccess(int userId, int pageId)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    string query = "DELETE FROM UserPermissions WHERE UserId = @UserId AND PermissionId = @PermissionId";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@PermissionId", pageId);
                        await conn.OpenAsync();
                        var rowsAffected = await cmd.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> HasPageAccess(int userId, int pageId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT COUNT(*) FROM UserPermissions WHERE UserId = @UserId AND PermissionId = @PermissionId AND CanView = 1";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@PermissionId", pageId);
                    await conn.OpenAsync();
                    var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    return count > 0;
                }
            }
        }

        public async Task<List<Permission>> GetAllPages()
        {
            var pages = new List<Permission>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Permissions WHERE IsActive = 1 ORDER BY MenuOrder";
                using (var cmd = new SqlCommand(query, conn))
                {
                    await conn.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            pages.Add(MapToPermission(reader));
                        }
                    }
                }
            }

            return pages;
        }

        public async Task<UserPageAccessViewModel> GetUserPageAccessViewModel(int userId = 0)
        {
            var model = new UserPageAccessViewModel
            {
                Users = GetUsers(),
                AllPages = await GetAllPages(),
                UserAccesses = new List<UserPermission>(),
                SelectedUserId = userId
            };

            if (userId > 0)
            {
                model.UserAccesses = await GetUserAccesses(userId);
                model.SelectedPageIds = model.UserAccesses.Select(ua => ua.PermissionId).ToList();
                Console.WriteLine($"Loaded {model.SelectedPageIds.Count} permissions for user {userId}");
            }

            return model;
        }

        public async Task<bool> SaveUserPageAccess(int userId, List<int> selectedPageIds)
        {
            try
            {
                Console.WriteLine($"Saving page access for UserId: {userId}, Pages: {string.Join(",", selectedPageIds)}");

                if (selectedPageIds == null)
                {
                    selectedPageIds = new List<int>();
                }

                var result = await AssignPageAccess(userId, selectedPageIds);
                Console.WriteLine($"Save result: {result}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SaveUserPageAccess: {ex.Message}");
                return false;
            }
        }

        private Permission MapToPermission(SqlDataReader reader)
        {
            return new Permission
            {
                Id = Convert.ToInt32(reader["Id"]),
                ControllerName = reader["ControllerName"]?.ToString() ?? string.Empty,
                ActionName = reader["ActionName"]?.ToString() ?? string.Empty,
                DisplayName = reader["DisplayName"]?.ToString() ?? string.Empty,
                Icon = reader["Icon"]?.ToString(),
                MenuOrder = reader["MenuOrder"] != DBNull.Value ? Convert.ToInt32(reader["MenuOrder"]) : (int?)null,
                ParentMenu = reader["ParentMenu"]?.ToString(),
                IsActive = Convert.ToBoolean(reader["IsActive"])
            };
        }

        // GetUserMenuItems for dynamic navbar
        public async Task<Dictionary<string, List<Permission>>> GetUserMenuItems(int userId, string loginType, string userRole)
        {
            var menuItems = new Dictionary<string, List<Permission>>();

            try
            {
                if (loginType == "Company" || userRole == "Admin")
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        string query = @"
                            SELECT 
                                Id,
                                ControllerName,
                                ActionName,
                                DisplayName,
                                Icon,
                                MenuOrder,
                                ParentMenu
                            FROM Permissions
                            WHERE IsActive = 1
                            AND MenuOrder IS NOT NULL
                            ORDER BY MenuOrder";

                        using (var cmd = new SqlCommand(query, conn))
                        {
                            await conn.OpenAsync();

                            using (var reader = await cmd.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    var permission = new Permission
                                    {
                                        Id = Convert.ToInt32(reader["Id"]),
                                        ControllerName = reader["ControllerName"]?.ToString() ?? string.Empty,
                                        ActionName = reader["ActionName"]?.ToString() ?? string.Empty,
                                        DisplayName = reader["DisplayName"]?.ToString() ?? string.Empty,
                                        Icon = reader["Icon"]?.ToString(),
                                        MenuOrder = reader["MenuOrder"] != DBNull.Value ? Convert.ToInt32(reader["MenuOrder"]) : (int?)null,
                                        ParentMenu = reader["ParentMenu"]?.ToString()
                                    };

                                    var parentMenu = permission.ParentMenu ?? "Main";

                                    if (!menuItems.ContainsKey(parentMenu))
                                    {
                                        menuItems[parentMenu] = new List<Permission>();
                                    }
                                    menuItems[parentMenu].Add(permission);
                                }
                            }
                        }
                    }
                }
                else
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        string query = @"
                            SELECT 
                                p.Id,
                                p.ControllerName,
                                p.ActionName,
                                p.DisplayName,
                                p.Icon,
                                p.MenuOrder,
                                p.ParentMenu
                            FROM Permissions p
                            INNER JOIN UserPermissions up ON p.Id = up.PermissionId
                            WHERE up.UserId = @UserId 
                            AND up.CanView = 1
                            AND p.IsActive = 1
                            AND p.MenuOrder IS NOT NULL
                            ORDER BY p.MenuOrder";

                        using (var cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@UserId", userId);
                            await conn.OpenAsync();

                            using (var reader = await cmd.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    var permission = new Permission
                                    {
                                        Id = Convert.ToInt32(reader["Id"]),
                                        ControllerName = reader["ControllerName"]?.ToString() ?? string.Empty,
                                        ActionName = reader["ActionName"]?.ToString() ?? string.Empty,
                                        DisplayName = reader["DisplayName"]?.ToString() ?? string.Empty,
                                        Icon = reader["Icon"]?.ToString(),
                                        MenuOrder = reader["MenuOrder"] != DBNull.Value ? Convert.ToInt32(reader["MenuOrder"]) : (int?)null,
                                        ParentMenu = reader["ParentMenu"]?.ToString()
                                    };

                                    var parentMenu = permission.ParentMenu ?? "Main";

                                    if (!menuItems.ContainsKey(parentMenu))
                                    {
                                        menuItems[parentMenu] = new List<Permission>();
                                    }
                                    menuItems[parentMenu].Add(permission);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user menu items: {ex.Message}");
            }

            return menuItems;
        }

        public async Task<List<Permission>> GetUserPermissionsList(int userId, string loginType, string userRole)
        {
            var permissions = new List<Permission>();

            try
            {
                if (loginType == "Company" || userRole == "Admin")
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        string query = @"
                            SELECT 
                                Id,
                                ControllerName,
                                ActionName,
                                DisplayName,
                                Icon,
                                MenuOrder,
                                ParentMenu
                            FROM Permissions
                            WHERE IsActive = 1
                            ORDER BY MenuOrder";

                        using (var cmd = new SqlCommand(query, conn))
                        {
                            await conn.OpenAsync();

                            using (var reader = await cmd.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    permissions.Add(new Permission
                                    {
                                        Id = Convert.ToInt32(reader["Id"]),
                                        ControllerName = reader["ControllerName"]?.ToString() ?? string.Empty,
                                        ActionName = reader["ActionName"]?.ToString() ?? string.Empty,
                                        DisplayName = reader["DisplayName"]?.ToString() ?? string.Empty,
                                        Icon = reader["Icon"]?.ToString(),
                                        MenuOrder = reader["MenuOrder"] != DBNull.Value ? Convert.ToInt32(reader["MenuOrder"]) : (int?)null,
                                        ParentMenu = reader["ParentMenu"]?.ToString()
                                    });
                                }
                            }
                        }
                    }
                }
                else
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        string query = @"
                            SELECT 
                                p.Id,
                                p.ControllerName,
                                p.ActionName,
                                p.DisplayName,
                                p.Icon,
                                p.MenuOrder,
                                p.ParentMenu
                            FROM Permissions p
                            INNER JOIN UserPermissions up ON p.Id = up.PermissionId
                            WHERE up.UserId = @UserId 
                            AND up.CanView = 1
                            AND p.IsActive = 1
                            ORDER BY p.MenuOrder";

                        using (var cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@UserId", userId);
                            await conn.OpenAsync();

                            using (var reader = await cmd.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    permissions.Add(new Permission
                                    {
                                        Id = Convert.ToInt32(reader["Id"]),
                                        ControllerName = reader["ControllerName"]?.ToString() ?? string.Empty,
                                        ActionName = reader["ActionName"]?.ToString() ?? string.Empty,
                                        DisplayName = reader["DisplayName"]?.ToString() ?? string.Empty,
                                        Icon = reader["Icon"]?.ToString(),
                                        MenuOrder = reader["MenuOrder"] != DBNull.Value ? Convert.ToInt32(reader["MenuOrder"]) : (int?)null,
                                        ParentMenu = reader["ParentMenu"]?.ToString()
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user permissions: {ex.Message}");
            }

            return permissions;
        }

        private List<User> GetUsers()
        {
            var users = new List<User>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT Id, Name, Email, Role, IsActive FROM Users WHERE IsDeleted = 0 ORDER BY Name";
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new User
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Name = reader["Name"]?.ToString() ?? string.Empty,
                                Email = reader["Email"]?.ToString() ?? string.Empty,
                                Role = reader["Role"]?.ToString() ?? "User",
                                IsActive = Convert.ToBoolean(reader["IsActive"])
                            });
                        }
                    }
                }
            }

            return users;
        }
    }
}