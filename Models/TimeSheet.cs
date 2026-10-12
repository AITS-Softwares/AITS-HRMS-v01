using System;
using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class TimeSheet
    {
        public int Id { get; set; }
        public string ErpNextId { get; set; }
        public string Series { get; set; } = "TS-.YYYY.-";
        public string Status { get; set; } = "Draft";
        public string Company { get; set; }
        public string Project { get; set; }
        public string Customer { get; set; }
        public string Currency { get; set; } = "INR";
        public decimal ExchangeRate { get; set; } = 1.000m;
        public string Employee { get; set; }
        public decimal TotalWorkingHours { get; set; }
        public decimal TotalBillableHours { get; set; }
        public decimal TotalBillableAmount { get; set; }
        public decimal TotalCostingAmount { get; set; }
        public string Note { get; set; }
        public List<TimeSheetDetail> TimeLogs { get; set; } = new List<TimeSheetDetail>();
    }

    public class TimeSheetDetail
    {
        public int Id { get; set; }
        public int TimeSheetId { get; set; }
        public string ActivityType { get; set; }
        public DateTime? FromTime { get; set; }
        public DateTime? ToTime { get; set; }
        public decimal Hrs { get; set; }
        public string Project { get; set; }
        public string Task { get; set; }
        public bool IsBillable { get; set; }
    }
}
