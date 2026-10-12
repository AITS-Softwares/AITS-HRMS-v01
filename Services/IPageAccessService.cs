using System.Collections.Generic;
using System.Threading.Tasks;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public interface IPageAccessService
    {
        Task<bool> HasAccess(int userId, string controller, string action);
        Task<List<Permission>> GetUserPages(int userId);
        Task<List<Permission>> GetUserMenuPages(int userId);
        Task<List<UserPermission>> GetUserAccesses(int userId);
        Task<bool> AssignPageAccess(int userId, List<int> pageIds);
        Task<bool> RemovePageAccess(int userId, int pageId);
        Task<bool> HasPageAccess(int userId, int pageId);
        Task<List<Permission>> GetAllPages();
        Task<UserPageAccessViewModel> GetUserPageAccessViewModel(int userId = 0);
        Task<bool> SaveUserPageAccess(int userId, List<int> selectedPageIds);
        bool IsPageAccessible(int userId, string controller, string action);

        // New methods for dynamic navbar
        Task<Dictionary<string, List<Permission>>> GetUserMenuItems(int userId, string loginType, string userRole);
        Task<List<Permission>> GetUserPermissionsList(int userId, string loginType, string userRole);
    }
}