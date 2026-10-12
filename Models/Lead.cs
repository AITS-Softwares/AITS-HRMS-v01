//using System.ComponentModel.DataAnnotations;

//namespace Erp_Aits.Models
//{
//    public class Lead
//    {
//        public int Id { get; set; }

//        // Basic Information
//        [Display(Name = "Salutation")]
//        public string? Salutation { get; set; }

//        [Required(ErrorMessage = "First Name is required")]
//        [StringLength(100, MinimumLength = 2, ErrorMessage = "First Name must be between 2 and 100 characters")]
//        [Display(Name = "First Name")]
//        public string FirstName { get; set; } = string.Empty;

//        [Display(Name = "Middle Name")]
//        public string? MiddleName { get; set; }

//        [Display(Name = "Last Name")]
//        public string? LastName { get; set; }

//        [Display(Name = "Job Title")]
//        public string? JobTitle { get; set; }

//        [Display(Name = "Gender")]
//        public string? Gender { get; set; }

//        [Display(Name = "Lead Owner")]
//        public string? LeadOwner { get; set; }

//        // Contact Information
//        [Required(ErrorMessage = "Email is required")]
//        [EmailAddress(ErrorMessage = "Invalid email address")]
//        [Display(Name = "Email Address")]
//        public string Email { get; set; } = string.Empty;

//        [Required(ErrorMessage = "Mobile Number is required")]
//        [Phone(ErrorMessage = "Invalid mobile number")]
//        [StringLength(20, MinimumLength = 10, ErrorMessage = "Mobile number must be at least 10 digits")]
//        [Display(Name = "Mobile Number")]
//        public string MobileNo { get; set; } = string.Empty;

//        [Display(Name = "Phone")]
//        public string? Phone { get; set; }

//        [Display(Name = "Phone Extension")]
//        public string? PhoneExt { get; set; }

//        [Display(Name = "WhatsApp")]
//        public string? WhatsApp { get; set; }

//        [Display(Name = "Website")]
//        public string? Website { get; set; }

//        [Display(Name = "Fax")]
//        public string? Fax { get; set; }

//        // Organization Information
//        [Display(Name = "Organization Name")]
//        public string? OrganizationName { get; set; }

//        [Display(Name = "Industry")]
//        public string? Industry { get; set; }

//        [Display(Name = "Annual Revenue")]
//        [DisplayFormat(DataFormatString = "{0:C}")]
//        public decimal? AnnualRevenue { get; set; }

//        [Display(Name = "Number of Employees")]
//        public int? Employees { get; set; }

//        [Display(Name = "Market Segment")]
//        public string? MarketSegment { get; set; }

//        [Display(Name = "Territory")]
//        public string? Territory { get; set; }

//        // Address Information
//        [Display(Name = "City")]
//        public string? City { get; set; }

//        [Display(Name = "State")]
//        public string? State { get; set; }

//        [Display(Name = "County")]
//        public string? County { get; set; }

//        [Display(Name = "Country")]
//        public string? Country { get; set; }

//        [Display(Name = "Print Language")]
//        public string? PrintLanguage { get; set; }

//        // Lead Classification
//        [Display(Name = "Source")]
//        public string? Source { get; set; }

//        [Display(Name = "Status")]
//        public string? Status { get; set; }

//        [Display(Name = "Lead Type")]
//        public string? LeadType { get; set; }

//        [Display(Name = "Request Type")]
//        public string? RequestType { get; set; }

//        [Display(Name = "Qualification Status")]
//        public string? QualificationStatus { get; set; }

//        [Display(Name = "Qualified By")]
//        public string? QualifiedBy { get; set; }

//        [Display(Name = "Qualified On")]
//        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy}")]
//        public DateTime? QualifiedOn { get; set; }

//        // System Fields
//        public DateTime CreatedDate { get; set; } = DateTime.Now;
//        public string? CreatedBy { get; set; }
//        public DateTime? ModifiedDate { get; set; }
//        public string? ModifiedBy { get; set; }
//        public bool IsActive { get; set; } = true;
//        public bool IsDeleted { get; set; } = false;

//        // Foreign Key
//        public int? CompanyId { get; set; }

//        // Full Name Property
//        [Display(Name = "Full Name")]
//        public string FullName => $"{FirstName} {MiddleName} {LastName}".Trim();
//    }

//    // View Model for Dropdowns
//    // View Model for Dropdowns
//    public class LeadViewModel
//    {
//        public Lead Lead { get; set; } = new Lead();
//        public List<string> SalutationList { get; set; } = new List<string>();
//        public List<string> GenderList { get; set; } = new List<string>();
//        public List<string> StatusList { get; set; } = new List<string>();
//        public List<string> LeadTypeList { get; set; } = new List<string>();
//        public List<string> SourceList { get; set; } = new List<string>();
//        public List<string> IndustryList { get; set; } = new List<string>();
//        public List<string> QualificationStatusList { get; set; } = new List<string>();
//        public List<string> RequestTypeList { get; set; } = new List<string>();
//        public List<string> CountryList { get; set; } = new List<string>();      // Add this
//        public List<string> TerritoryList { get; set; } = new List<string>();    // Add this
//        public List<string> PrintLanguageList { get; set; } = new List<string>(); // Add this
//    }

//    // Search/Filter Model
//    public class LeadSearchModel
//    {
//        public string? SearchTerm { get; set; }
//        public string? Status { get; set; }
//        public string? Source { get; set; }
//        public string? LeadType { get; set; }
//        public DateTime? FromDate { get; set; }
//        public DateTime? ToDate { get; set; }
//    }
//}
using System.ComponentModel.DataAnnotations;

namespace Erp_Aits.Models
{
    public class Lead
    {
        public int Id { get; set; }

        // Basic Information
        [Display(Name = "Salutation")]
        public string? Salutation { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "First Name must be between 2 and 100 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Display(Name = "Middle Name")]
        public string? MiddleName { get; set; }

        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Display(Name = "Job Title")]
        public string? JobTitle { get; set; }

        [Display(Name = "Gender")]
        public string? Gender { get; set; }

        [Display(Name = "Lead Owner")]
        public string? LeadOwner { get; set; }

        // Contact Information
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile Number is required")]
        [Phone(ErrorMessage = "Invalid mobile number")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "Mobile number must be at least 10 digits")]
        [Display(Name = "Mobile Number")]
        public string MobileNo { get; set; } = string.Empty;

        [Display(Name = "Phone")]
        public string? Phone { get; set; }

        [Display(Name = "Phone Extension")]
        public string? PhoneExt { get; set; }

        [Display(Name = "WhatsApp")]
        public string? WhatsApp { get; set; }

        [Display(Name = "Website")]
        public string? Website { get; set; }

        [Display(Name = "Fax")]
        public string? Fax { get; set; }

        // Organization Information
        [Display(Name = "Organization Name")]
        public string? OrganizationName { get; set; }

        [Display(Name = "Industry")]
        public string? Industry { get; set; }

        [Display(Name = "Annual Revenue")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal? AnnualRevenue { get; set; }

        [Display(Name = "Number of Employees")]
        public int? Employees { get; set; }

        [Display(Name = "Market Segment")]
        public string? MarketSegment { get; set; }

        [Display(Name = "Territory")]
        public string? Territory { get; set; }

        // Address Information
        [Display(Name = "City")]
        public string? City { get; set; }

        [Display(Name = "State")]
        public string? State { get; set; }

        [Display(Name = "County")]
        public string? County { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Area/Locality")]
        public string? Area { get; set; }  // Changed from PrintLanguage to Area

        // Lead Classification
        [Display(Name = "Source")]
        public string? Source { get; set; }

        [Display(Name = "Status")]
        public string? Status { get; set; }

        [Display(Name = "Lead Type")]
        public string? LeadType { get; set; }

        [Display(Name = "Request Type")]
        public string? RequestType { get; set; }

        [Display(Name = "Qualification Status")]
        public string? QualificationStatus { get; set; }

        [Display(Name = "Qualified By")]
        public string? QualifiedBy { get; set; }

        [Display(Name = "Qualified On")]
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy}")]
        public DateTime? QualifiedOn { get; set; }

        // System Fields
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        // Foreign Key
        public int? CompanyId { get; set; }

        // Full Name Property
        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {MiddleName} {LastName}".Trim();
    }

    // View Model for Dropdowns
    public class LeadViewModel
    {
        public Lead Lead { get; set; } = new Lead();
        public List<string> SalutationList { get; set; } = new List<string>();
        public List<string> GenderList { get; set; } = new List<string>();
        public List<string> StatusList { get; set; } = new List<string>();
        public List<string> LeadTypeList { get; set; } = new List<string>();
        public List<string> SourceList { get; set; } = new List<string>();
        public List<string> IndustryList { get; set; } = new List<string>();
        public List<string> QualificationStatusList { get; set; } = new List<string>();
        public List<string> RequestTypeList { get; set; } = new List<string>();
        public List<string> CountryList { get; set; } = new List<string>();
        public List<string> TerritoryList { get; set; } = new List<string>();
        public List<string> AreaList { get; set; } = new List<string>();  // Changed from PrintLanguageList to AreaList
    }

    // Search/Filter Model
    public class LeadSearchModel
    {
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public string? Source { get; set; }
        public string? LeadType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}