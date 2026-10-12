using System.ComponentModel.DataAnnotations;

namespace Erp_Aits.Models
{
    public class Opportunity
    {
        public int Id { get; set; }

        // Opportunity Details
        [Display(Name = "Opportunity From")]
        public string? OpportunityFrom { get; set; }

        [Display(Name = "Opportunity Type")]
        public string? OpportunityType { get; set; }

        [Display(Name = "Sales Stage")]
        public string? SalesStage { get; set; }

        [Display(Name = "Expected Closing Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? ExpectedClosingDate { get; set; }

        [Display(Name = "Probability (%)")]
        [Range(0, 100, ErrorMessage = "Probability must be between 0 and 100")]
        public int? Probability { get; set; }

        // Organization
        [Display(Name = "No. of Employees")]
        public int? Employees { get; set; }

        [Display(Name = "Industry")]
        public string? Industry { get; set; }

        [Display(Name = "City")]
        public string? City { get; set; }

        [Display(Name = "State")]
        public string? State { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Annual Revenue")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal? AnnualRevenue { get; set; }

        [Display(Name = "Market Segment")]
        public string? MarketSegment { get; set; }

        [Display(Name = "Website")]
        public string? Website { get; set; }

        [Display(Name = "Territory")]
        public string? Territory { get; set; }

        // Opportunity Value
        [Display(Name = "Currency")]
        public string? Currency { get; set; }

        [Display(Name = "Opportunity Amount (INR)")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal? Amount { get; set; }

        // More Information
        [Display(Name = "Company")]
        public string? Company { get; set; }

        [Display(Name = "Opportunity Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? OpportunityDate { get; set; }

        [Display(Name = "Print Language")]
        public string? PrintLanguage { get; set; }

        // Source Information
        [Display(Name = "Source")]
        public string? Source { get; set; }

        // System Fields
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public int? CompanyId { get; set; }
        public int? LeadId { get; set; }

        // Computed Property
        public string DisplayName => $"{Company} - {OpportunityFrom}";
    }

    // View Model for Dropdowns
    public class OpportunityViewModel
    {
        public Opportunity Opportunity { get; set; } = new Opportunity();
        public List<string> SalesStageList { get; set; } = new List<string>();
        public List<string> OpportunityTypeList { get; set; } = new List<string>();
        public List<string> IndustryList { get; set; } = new List<string>();
        public List<string> CurrencyList { get; set; } = new List<string>();
        public List<string> LanguageList { get; set; } = new List<string>();
        public List<string> MarketSegmentList { get; set; } = new List<string>();
        public List<string> OpportunityFromList { get; set; } = new List<string>();
        public List<string> SourceList { get; set; } = new List<string>();
        public List<string> CountryList { get; set; } = new List<string>();
        public List<string> TerritoryList { get; set; } = new List<string>();
    }

    // Search/Filter Model
    public class OpportunitySearchModel
    {
        public string? SearchTerm { get; set; }
        public string? SalesStage { get; set; }
        public string? OpportunityType { get; set; }
        public string? Company { get; set; }
        public string? Source { get; set; }
        public string? Territory { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}