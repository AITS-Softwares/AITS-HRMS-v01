using System;

namespace Erp_Aits.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        public string ErpNextId { get; set; }
        public string VehicleCode { get; set; }

        public string LicensePlate { get; set; }
        public string Model { get; set; }
        public string Make { get; set; }

        public decimal OdometerValueLast { get; set; }
        public string ChassisNo { get; set; }
        public DateTime? AcquisitionDate { get; set; }
        public decimal? VehicleValue { get; set; }
        public string Location { get; set; }
        public string Employee { get; set; }

        public string InsuranceCompany { get; set; }
        public DateTime? InsuranceStartDate { get; set; }
        public string PolicyNo { get; set; }
        public DateTime? InsuranceEndDate { get; set; }

        public string FuelType { get; set; } = "Petrol";
        public string Color { get; set; }
        public string FuelUOM { get; set; }
        public int? Wheels { get; set; }
        public DateTime? LastCarbonCheck { get; set; }
        public int? Doors { get; set; }
    }
}
