using System;

namespace Erp_Aits.Models
{
    public class EmployeeAdvance
    {
        public int Id { get; set; }
        public string EmployeeAdvanceCode { get; set; }

        public DateTime? PostingDate { get; set; }
        public string Employee { get; set; }
        public string EmployeeName { get; set; }
        public string Company { get; set; } = "AITS";

        public string Purpose { get; set; }
        public decimal AdvanceAmount { get; set; }

        public string AdvanceAccount { get; set; }
        public bool RepayUnclaimedFromSalary { get; set; }
        public string ModeOfPayment { get; set; }

        public string Status { get; set; } = "Draft";
    }
}
