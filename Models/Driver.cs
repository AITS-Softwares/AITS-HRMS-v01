using System;
using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class Driver
    {
        public int Id { get; set; }
        public string DriverCode { get; set; }
        public string Series { get; set; } = "HR-DRI-.YYYY.-";
        public string FullName { get; set; }
        public string Employee { get; set; }
        public string CellNumber { get; set; }
        public string Address { get; set; }
        public string Transporter { get; set; }
        public string LicenseNumber { get; set; }
        public DateTime? IssuingDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Status { get; set; } = "Active";
        public List<DriverLicenseCategory> LicenseCategories { get; set; } = new List<DriverLicenseCategory>();
    }

    public class DriverLicenseCategory
    {
        public int Id { get; set; }
        public int DriverId { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public DateTime? IssuingDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}