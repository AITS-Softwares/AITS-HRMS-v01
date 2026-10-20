using System;

namespace Erp_Aits.Models
{
    public class SalaryRegister
    {
        public string Name { get; set; } = "";
        public int DocStatus { get; set; }
        public DateTime PostingDate { get; set; } = DateTime.Today;
        public string Company { get; set; } = "";
        public string Currency { get; set; } = "INR";
        public decimal ExchangeRate { get; set; } = 1;
        public string PayrollPayableAccount { get; set; } = "";
        public string Status { get; set; } = "Draft";
        public bool SalarySlipBasedOnTimesheet { get; set; }
        public string PayrollFrequency { get; set; } = "Monthly";
        public DateTime StartDate { get; set; } = DateTime.Today;
        public DateTime EndDate { get; set; } = DateTime.Today;
        public bool DeductTaxForUnclaimedEmployeeBenefits { get; set; }
        public bool DeductTaxForUnsubmittedTaxExemptionProof { get; set; }
        public string Branch { get; set; } = "";
        public string Department { get; set; } = "";
        public string Designation { get; set; } = "";
        public string Grade { get; set; } = "";
        public int? NumberOfEmployees { get; set; }
        public bool ValidateAttendance { get; set; }
        public string CostCenter { get; set; } = "";
        public string Project { get; set; } = "";
        public string PaymentAccount { get; set; } = "";
        public string BankAccount { get; set; } = "";
        public bool SalarySlipsCreated { get; set; }
        public bool SalarySlipsSubmitted { get; set; }
        public string ErrorMessage { get; set; } = "";
        public string AmendedFrom { get; set; } = "";
    }

    public class PayrollEmployeeOption
    {
        public string Id { get; set; } = "";
        public string EmployeeName { get; set; } = "";
        public string Department { get; set; } = "";
        public string Designation { get; set; } = "";
        public string Grade { get; set; } = "";
        public string Branch { get; set; } = "";
        public string EmployeeType { get; set; } = "";
        public string Company { get; set; } = "";
        public string DateOfJoining { get; set; } = "";
        public string RelievingDate { get; set; } = "";
        public string Status { get; set; } = "";
    }

    public class SalarySlipListItem
    {
        public string Name { get; set; } = "";
        public string Employee { get; set; } = "";
        public string EmployeeName { get; set; } = "";
        public string Company { get; set; } = "";
        public string SalaryStructure { get; set; } = "";
        public string PostingDate { get; set; } = "";
        public string Status { get; set; } = "";
        public string Modified { get; set; } = "";
    }
}
