using System.ComponentModel.DataAnnotations;

namespace Erp_Aits.Models
{
    public class Company
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Company name is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Company name must be between 2 and 200 characters")]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact person name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Contact person name must be between 2 and 100 characters")]
        [Display(Name = "Contact Person Name")]
        public string ContactPersonName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Official Email")]
        public string OfficialEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Company address is required")]
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        [Display(Name = "Company Address")]
        public string CompanyAddress { get; set; } = string.Empty;

        [Display(Name = "GST Number")]
        public string? GSTNumber { get; set; }

        [Required(ErrorMessage = "Company type is required")]
        [Display(Name = "Company Type")]
        public string CompanyType { get; set; } = string.Empty;

        [Required(ErrorMessage = "PAN number is required")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "PAN number must be between 10 and 20 characters")]
        [Display(Name = "PAN Number")]
        public string PANNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? PasswordHash { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }

    // REMOVED: LoginModel is now in User.cs to avoid duplication
}