using System;

namespace Erp_Aits.Models
{
    public class VehicleExpenseRow
    {
        public string Vehicle { get; set; }
        public string Employee { get; set; }
        public string EmployeeName { get; set; }
        public DateTime? LogDate { get; set; }
        public string FiscalYear { get; set; }
        public decimal FuelExpense { get; set; }
        public decimal ServiceExpense { get; set; }
        public decimal OtherExpense { get; set; }
        public decimal TotalExpense { get; set; }
    }
}
