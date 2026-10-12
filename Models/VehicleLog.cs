using System;
using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class VehicleLog
    {
        public int Id { get; set; }
        public string Series { get; set; } = "HR-VLOG-.YYYY.-";
        public string LicensePlate { get; set; }
        public string Employee { get; set; }
        public DateTime? OdometerDate { get; set; }
        public decimal? CurrentOdometerValue { get; set; }
        public decimal? FuelQty { get; set; }
        public string Supplier { get; set; }
        public decimal? FuelPrice { get; set; }
        public string InvoiceRef { get; set; }
        public List<VehicleLogServiceDetail> ServiceDetails { get; set; } = new List<VehicleLogServiceDetail>();
    }

    public class VehicleLogServiceDetail
    {
        public int Id { get; set; }
        public int VehicleLogId { get; set; }
        public string ServiceItem { get; set; }
        public string Type { get; set; }
        public string Frequency { get; set; }
        public decimal? Expense { get; set; }
    }
}
