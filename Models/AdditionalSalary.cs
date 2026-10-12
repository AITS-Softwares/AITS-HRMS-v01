using System;

namespace Erp_Aits.Models
{
    public class AdditionalSalary
    {
        public int Id { get; set; }
        public string AdditionalSalaryCode { get; set; }

        public string Company { get; set; } = "AITS";
        public string Employee { get; set; }
        public string EmployeeName { get; set; }
        public bool IsRecurring { get; set; }
        public DateTime? PayrollDate { get; set; }

        public string SalaryComponent { get; set; }
        public decimal Amount { get; set; }
        public bool DeductFullTaxOnSelectedPayrollDate { get; set; }
        public bool OverwriteSalaryStructureAmount { get; set; } = true;
    }
}
