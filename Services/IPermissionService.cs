using System.Collections.Generic;
using System.Threading.Tasks;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public interface IPermissionService
    {
        // Check if user has specific permission
        Task<bool> HasPermission(int userId, string controller, string action, string permissionType = "View");

        // Get all permissions for a user
        Task<List<UserPermission>> GetUserPermissions(int userId);

        // Get menu permissions for sidebar
        Task<List<Permission>> GetUserMenuPermissions(int userId);

        // Individual permission checks
        Task<bool> CanView(int userId, string controller, string action);
        Task<bool> CanCreate(int userId, string controller, string action);
        Task<bool> CanEdit(int userId, string controller, string action);
        Task<bool> CanDelete(int userId, string controller, string action);

        // Get user roles
        Task<List<string>> GetUserRoles(int userId);

        // Get all permissions
        Task<List<Permission>> GetAllPermissions();

        // Get role permissions
        Task<List<RolePermission>> GetRolePermissions(string roleName);

        // Update role permissions
        Task<bool> UpdateRolePermissions(string roleName, List<RolePermission> permissions);

        // Get user specific permissions
        Task<List<UserPermission>> GetUserSpecificPermissions(int userId);

        // Update user specific permissions
        Task<bool> UpdateUserPermissions(int userId, List<UserPermission> permissions);

        // Page access management
        Task<UserPageAccessViewModel> GetUserPageAccessViewModel(int userId = 0);
        Task<bool> SaveUserPageAccess(int userId, List<int> selectedPageIds);
        bool IsPageAccessible(int userId, string controller, string action);
    }
}