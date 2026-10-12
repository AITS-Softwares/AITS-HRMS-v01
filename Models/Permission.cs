using System.ComponentModel.DataAnnotations;

namespace Erp_Aits.Models
{
    public class Permission
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Controller Name")]
        public string ControllerName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Action Name")]
        public string ActionName { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Icon")]
        public string? Icon { get; set; }

        [Display(Name = "Menu Order")]
        public int? MenuOrder { get; set; }

        [StringLength(100)]
        [Display(Name = "Parent Menu")]
        public string? ParentMenu { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }

        // Navigation Properties
        public List<RolePermission>? RolePermissions { get; set; }
        public List<UserPermission>? UserPermissions { get; set; }
    }

    public class RolePermission
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Role Name")]
        public string RoleName { get; set; } = string.Empty;

        [Required]
        public int PermissionId { get; set; }

        [Display(Name = "Can View")]
        public bool CanView { get; set; } = true;

        [Display(Name = "Can Create")]
        public bool CanCreate { get; set; } = false;

        [Display(Name = "Can Edit")]
        public bool CanEdit { get; set; } = false;

        [Display(Name = "Can Delete")]
        public bool CanDelete { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }

        // Navigation Property
        public Permission? Permission { get; set; }
    }

    public class UserPermission
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int PermissionId { get; set; }

        [Display(Name = "Can View")]
        public bool CanView { get; set; } = true;

        [Display(Name = "Can Create")]
        public bool CanCreate { get; set; } = false;

        [Display(Name = "Can Edit")]
        public bool CanEdit { get; set; } = false;

        [Display(Name = "Can Delete")]
        public bool CanDelete { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }

        // Navigation Properties
        public User? User { get; set; }
        public Permission? Permission { get; set; }
    }

    // View Model for Page Access Management
    public class UserPageAccessViewModel
    {
        public List<User> Users { get; set; } = new List<User>();
        public List<Permission> AllPages { get; set; } = new List<Permission>();
        public List<UserPermission> UserAccesses { get; set; } = new List<UserPermission>();
        public int SelectedUserId { get; set; }
        public List<int> SelectedPageIds { get; set; } = new List<int>();
    }

    public class PermissionAssignment
    {
        public int PermissionId { get; set; }
        public bool CanView { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}