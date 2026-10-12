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
    public class PermissionService : IPermissionService
    {
        private readonly string _connectionString;

        public PermissionService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // ============================================================
        // PERMISSION CHECK METHODS
        // ============================================================

        public async Task<bool> HasPermission(int userId, string controller, string action, string permissionType = "View")
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    string query = @"
                        SELECT 
                            CASE 
                                WHEN EXISTS (
                                    SELECT 1 FROM UserPermissions up
                                    INNER JOIN Permissions p ON up.PermissionId = p.Id
                                    WHERE up.UserId = @UserId 
                                    AND p.ControllerName = @Controller
                                    AND p.ActionName = @Action
                                    AND up.Can" + permissionType + @" = 1
                                ) THEN 1
                                WHEN EXISTS (
                                    SELECT 1 FROM RolePermissions rp
                                    INNER JOIN Permissions p ON rp.PermissionId = p.Id
                                    INNER JOIN Users u ON u.Role = rp.RoleName
                                    WHERE u.Id = @UserId 
                                    AND p.ControllerName = @Controller
                                    AND p.ActionName = @Action
                                    AND rp.Can" + permissionType + @" = 1
                                ) THEN 1
                                ELSE 0
                            END AS HasPermission";

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

        public async Task<bool> CanView(int userId, string controller, string action)
        {
            return await HasPermission(userId, controller, action, "View");
        }

        public async Task<bool> CanCreate(int userId, string controller, string action)
        {
            return await HasPermission(userId, controller, action, "Create");
        }

        public async Task<bool> CanEdit(int userId, string controller, string action)
        {
            return await HasPermission(userId, controller, action, "Edit");
        }

        public async Task<bool> CanDelete(int userId, string controller, string action)
        {
            return await HasPermission(userId, controller, action, "Delete");
        }

        public bool IsPageAccessible(int userId, string controller, string action)
        {
            return HasPermission(userId, controller, action, "View").GetAwaiter().GetResult();
        }

        // ============================================================
        // GET USER PERMISSIONS
        // ============================================================

        public async Task<List<UserPermission>> GetUserPermissions(int userId)
        {
            var permissions = new List<UserPermission>();

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    string query = @"
                        SELECT 
                            p.Id AS PermissionId,
                            p.ControllerName,
                            p.ActionName,
                            p.DisplayName,
                            p.Icon,
                            p.MenuOrder,
                            p.ParentMenu,
                            COALESCE(up.CanView, rp.CanView, 0) AS CanView,
                            COALESCE(up.CanCreate, rp.CanCreate, 0) AS CanCreate,
                            COALESCE(up.CanEdit, rp.CanEdit, 0) AS CanEdit,
                            COALESCE(up.CanDelete, rp.CanDelete, 0) AS CanDelete
                        FROM Permissions p
                        LEFT JOIN UserPermissions up ON p.Id = up.PermissionId AND up.UserId = @UserId
                        LEFT JOIN RolePermissions rp ON p.Id = rp.PermissionId
                        LEFT JOIN Users u ON u.Role = rp.RoleName AND u.Id = @UserId
                        WHERE p.IsActive = 1
                        GROUP BY p.Id, p.ControllerName, p.ActionName, p.DisplayName, p.Icon, p.MenuOrder, p.ParentMenu,
                            up.CanView, up.CanCreate, up.CanEdit, up.CanDelete,
                            rp.CanView, rp.CanCreate, rp.CanEdit, rp.CanDelete
                        ORDER BY p.MenuOrder";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        await conn.OpenAsync();

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                permissions.Add(new UserPermission
                                {
                                    UserId = userId,
                                    PermissionId = Convert.ToInt32(reader["PermissionId"]),
                                    CanView = Convert.ToBoolean(reader["CanView"]),
                                    CanCreate = Convert.ToBoolean(reader["CanCreate"]),
                                    CanEdit = Convert.ToBoolean(reader["CanEdit"]),
                                    CanDelete = Convert.ToBoolean(reader["CanDelete"]),
                                    Permission = new Permission
                                    {
                                        Id = Convert.ToInt32(reader["PermissionId"]),
                                        ControllerName = reader["ControllerName"]?.ToString(),
                                        ActionName = reader["ActionName"]?.ToString(),
                                        DisplayName = reader["DisplayName"]?.ToString(),
                                        Icon = reader["Icon"]?.ToString(),
                                        MenuOrder = reader["MenuOrder"] != DBNull.Value ? Convert.ToInt32(reader["MenuOrder"]) : (int?)null,
                                        ParentMenu = reader["ParentMenu"]?.ToString()
                                    }
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error
            }

            return permissions;
        }

        public async Task<List<Permission>> GetUserMenuPermissions(int userId)
        {
            var permissions = new List<Permission>();

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    string query = @"
                        SELECT DISTINCT
                            p.Id,
                            p.ControllerName,
                            p.ActionName,
                            p.DisplayName,
                            p.Icon,
                            p.MenuOrder,
                            p.ParentMenu
                        FROM Permissions p
                        LEFT JOIN UserPermissions up ON p.Id = up.PermissionId AND up.UserId = @UserId
                        LEFT JOIN RolePermissions rp ON p.Id = rp.PermissionId
                        LEFT JOIN Users u ON u.Role = rp.RoleName AND u.Id = @UserId
                        WHERE p.IsActive = 1
                        AND p.MenuOrder IS NOT NULL
                        AND p.ActionName = 'Index'
                        AND (
                            (up.CanView = 1) OR (rp.CanView = 1)
                        )
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
                                    ControllerName = reader["ControllerName"]?.ToString(),
                                    ActionName = reader["ActionName"]?.ToString(),
                                    DisplayName = reader["DisplayName"]?.ToString(),
                                    Icon = reader["Icon"]?.ToString(),
                                    MenuOrder = reader["MenuOrder"] != DBNull.Value ? Convert.ToInt32(reader["MenuOrder"]) : (int?)null,
                                    ParentMenu = reader["ParentMenu"]?.ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error
            }

            return permissions;
        }

        public async Task<List<string>> GetUserRoles(int userId)
        {
            var roles = new List<string>();

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    string query = "SELECT Role FROM Users WHERE Id = @UserId AND IsActive = 1 AND IsDeleted = 0";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        await conn.OpenAsync();

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var role = reader["Role"]?.ToString();
                                if (!string.IsNullOrEmpty(role))
                                {
                                    roles.Add(role);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error
            }

            return roles;
        }

        // ============================================================
        // PERMISSION MANAGEMENT METHODS
        // ============================================================

        public async Task<List<Permission>> GetAllPermissions()
        {
            var permissions = new List<Permission>();

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
                            permissions.Add(new Permission
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                ControllerName = reader["ControllerName"]?.ToString(),
                                ActionName = reader["ActionName"]?.ToString(),
                                DisplayName = reader["DisplayName"]?.ToString(),
                                Icon = reader["Icon"]?.ToString(),
                                MenuOrder = reader["MenuOrder"] != DBNull.Value ? Convert.ToInt32(reader["MenuOrder"]) : (int?)null,
                                ParentMenu = reader["ParentMenu"]?.ToString(),
                                IsActive = Convert.ToBoolean(reader["IsActive"])
                            });
                        }
                    }
                }
            }

            return permissions;
        }

        public async Task<List<RolePermission>> GetRolePermissions(string roleName)
        {
            var permissions = new List<RolePermission>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT rp.*, p.DisplayName, p.ControllerName, p.ActionName
                    FROM RolePermissions rp
                    INNER JOIN Permissions p ON rp.PermissionId = p.Id
                    WHERE rp.RoleName = @RoleName
                    ORDER BY p.MenuOrder";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@RoleName", roleName);
                    await conn.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            permissions.Add(new RolePermission
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                RoleName = reader["RoleName"]?.ToString(),
                                PermissionId = Convert.ToInt32(reader["PermissionId"]),
                                CanView = Convert.ToBoolean(reader["CanView"]),
                                CanCreate = Convert.ToBoolean(reader["CanCreate"]),
                                CanEdit = Convert.ToBoolean(reader["CanEdit"]),
                                CanDelete = Convert.ToBoolean(reader["CanDelete"]),
                                Permission = new Permission
                                {
                                    DisplayName = reader["DisplayName"]?.ToString(),
                                    ControllerName = reader["ControllerName"]?.ToString(),
                                    ActionName = reader["ActionName"]?.ToString()
                                }
                            });
                        }
                    }
                }
            }

            return permissions;
        }

        public async Task<bool> UpdateRolePermissions(string roleName, List<RolePermission> permissions)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    // Delete existing permissions for this role
                    string deleteQuery = "DELETE FROM RolePermissions WHERE RoleName = @RoleName";
                    using (var deleteCmd = new SqlCommand(deleteQuery, conn))
                    {
                        deleteCmd.Parameters.AddWithValue("@RoleName", roleName);
                        await deleteCmd.ExecuteNonQueryAsync();
                    }

                    // Insert new permissions
                    foreach (var perm in permissions)
                    {
                        string insertQuery = @"
                            INSERT INTO RolePermissions (RoleName, PermissionId, CanView, CanCreate, CanEdit, CanDelete)
                            VALUES (@RoleName, @PermissionId, @CanView, @CanCreate, @CanEdit, @CanDelete)";

                        using (var insertCmd = new SqlCommand(insertQuery, conn))
                        {
                            insertCmd.Parameters.AddWithValue("@RoleName", roleName);
                            insertCmd.Parameters.AddWithValue("@PermissionId", perm.PermissionId);
                            insertCmd.Parameters.AddWithValue("@CanView", perm.CanView);
                            insertCmd.Parameters.AddWithValue("@CanCreate", perm.CanCreate);
                            insertCmd.Parameters.AddWithValue("@CanEdit", perm.CanEdit);
                            insertCmd.Parameters.AddWithValue("@CanDelete", perm.CanDelete);
                            await insertCmd.ExecuteNonQueryAsync();
                        }
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<List<UserPermission>> GetUserSpecificPermissions(int userId)
        {
            var permissions = new List<UserPermission>();

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
                            permissions.Add(new UserPermission
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                UserId = Convert.ToInt32(reader["UserId"]),
                                PermissionId = Convert.ToInt32(reader["PermissionId"]),
                                CanView = Convert.ToBoolean(reader["CanView"]),
                                CanCreate = Convert.ToBoolean(reader["CanCreate"]),
                                CanEdit = Convert.ToBoolean(reader["CanEdit"]),
                                CanDelete = Convert.ToBoolean(reader["CanDelete"])
                            });
                        }
                    }
                }
            }

            return permissions;
        }

        public async Task<bool> UpdateUserPermissions(int userId, List<UserPermission> permissions)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    // Delete existing permissions for this user
                    string deleteQuery = "DELETE FROM UserPermissions WHERE UserId = @UserId";
                    using (var deleteCmd = new SqlCommand(deleteQuery, conn))
                    {
                        deleteCmd.Parameters.AddWithValue("@UserId", userId);
                        await deleteCmd.ExecuteNonQueryAsync();
                    }

                    // Insert new permissions
                    foreach (var perm in permissions)
                    {
                        string insertQuery = @"
                            INSERT INTO UserPermissions (UserId, PermissionId, CanView, CanCreate, CanEdit, CanDelete)
                            VALUES (@UserId, @PermissionId, @CanView, @CanCreate, @CanEdit, @CanDelete)";

                        using (var insertCmd = new SqlCommand(insertQuery, conn))
                        {
                            insertCmd.Parameters.AddWithValue("@UserId", userId);
                            insertCmd.Parameters.AddWithValue("@PermissionId", perm.PermissionId);
                            insertCmd.Parameters.AddWithValue("@CanView", perm.CanView);
                            insertCmd.Parameters.AddWithValue("@CanCreate", perm.CanCreate);
                            insertCmd.Parameters.AddWithValue("@CanEdit", perm.CanEdit);
                            insertCmd.Parameters.AddWithValue("@CanDelete", perm.CanDelete);
                            await insertCmd.ExecuteNonQueryAsync();
                        }
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // ============================================================
        // PAGE ACCESS MANAGEMENT (For UI)
        // ============================================================

        public async Task<UserPageAccessViewModel> GetUserPageAccessViewModel(int userId = 0)
        {
            var model = new UserPageAccessViewModel
            {
                Users = GetUsers(),
                AllPages = await GetAllPermissions(),
                UserAccesses = new List<UserPermission>(),
                SelectedUserId = userId
            };

            if (userId > 0)
            {
                model.UserAccesses = await GetUserSpecificPermissions(userId);
                model.SelectedPageIds = model.UserAccesses.Select(ua => ua.PermissionId).ToList();
            }

            return model;
        }

        public async Task<bool> SaveUserPageAccess(int userId, List<int> selectedPageIds)
        {
            try
            {
                var permissions = new List<UserPermission>();

                foreach (var pageId in selectedPageIds)
                {
                    permissions.Add(new UserPermission
                    {
                        UserId = userId,
                        PermissionId = pageId,
                        CanView = true,
                        CanCreate = false,
                        CanEdit = false,
                        CanDelete = false
                    });
                }

                return await UpdateUserPermissions(userId, permissions);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // ============================================================
        // HELPER METHODS
        // ============================================================

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