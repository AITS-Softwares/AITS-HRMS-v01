using System;
using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class TravelRequest
    {
        public int Id { get; set; }
        public string TravelRequestCode { get; set; }

        // Top section
        public string TravelType { get; set; }
        public string PurposeOfTravel { get; set; }
        public string TravelFunding { get; set; }
        public string DetailsOfSponsor { get; set; }
        public string CopyOfInvitation { get; set; }

        // Employee Details
        public string Employee { get; set; }
        public string EmployeeName { get; set; }
        public DateTime? DateOfBirth { get; set; }   // <-- correctly placed here
        public string IdentificationDocumentType { get; set; }
        public string ContactNumber { get; set; }
        public string IdentificationDocumentNumber { get; set; }
        public string ContactEmail { get; set; }
        public string PassportNumber { get; set; }
        public string Company { get; set; } = "AITS";

        // Description
        public string OtherDetailsDescription { get; set; }

        // Child tables
        public List<TravelItinerary> Itineraries { get; set; } = new List<TravelItinerary>();
        public List<TravelCosting> Costings { get; set; } = new List<TravelCosting>();

        // Accounting Dimensions
        public string CostCenter { get; set; }

        // Event Details
        public string NameOfOrganizer { get; set; }
        public string AddressOfOrganizer { get; set; }
        public string EventOtherDetails { get; set; }
    }

    public class TravelItinerary
    {
        public int Id { get; set; }
        public int TravelRequestId { get; set; }
        public string TravelFrom { get; set; }
        public string TravelTo { get; set; }
        public string ModeOfTravel { get; set; }
        public DateTime? DepartureDatetime { get; set; }
        // no DateOfBirth here — removed
    }

    public class TravelCosting
    {
        public int Id { get; set; }
        public int TravelRequestId { get; set; }
        public string ExpenseType { get; set; }
        public decimal SponsoredAmount { get; set; }
        public decimal FundedAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }
}