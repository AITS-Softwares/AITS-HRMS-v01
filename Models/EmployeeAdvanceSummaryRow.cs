using System;

namespace Erp_Aits.Models
{
    public class EmployeeAdvanceSummaryRow
    {
        public string Employee { get; set; }
        public string EmployeeName { get; set; }
        public string Company { get; set; }
        public string Status { get; set; }
        public int NoOfAdvances { get; set; }
        public decimal TotalAdvanceAmount { get; set; }
    }
}
