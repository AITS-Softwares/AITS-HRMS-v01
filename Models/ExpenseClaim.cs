using System;
using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class ExpenseClaim
    {
        public int Id { get; set; }

        public string ExpenseClaimCode { get; set; }

        // ERPNext name
        public string ErpNextId { get; set; }

        // Expenses & Advances tab
        public string ExpenseApprover { get; set; }

        public string Employee { get; set; }

        public string EmployeeName { get; set; }

        public string ApprovalStatus { get; set; } = "Draft";

        public string Department { get; set; }

        public string Company { get; set; }

        public string CompanyGstin { get; set; }

        // Accounting tab
        public DateTime? PostingDate { get; set; }

        public DateTime? ClearanceDate { get; set; }

        public bool IsPaid { get; set; }

        public string Remark { get; set; }

        public string PayableAccount { get; set; }

        public string Project { get; set; }

        public string CostCenter { get; set; }

        // More Info tab
        public string Status { get; set; } = "Draft";

        public string Task { get; set; }

        // Child tables
        public List<ExpenseClaimDetail> Expenses { get; set; }
            = new List<ExpenseClaimDetail>();

        public List<ExpenseTaxAndCharge> Taxes { get; set; }
            = new List<ExpenseTaxAndCharge>();

        public List<ExpenseClaimAdvance> Advances { get; set; }
            = new List<ExpenseClaimAdvance>();
    }

    public class ExpenseClaimDetail
    {
        public int Id { get; set; }

        public int ExpenseClaimId { get; set; }

        public DateTime? ExpenseDate { get; set; }

        // ERPNext child table field: expense_type
        public string ExpenseClaimType { get; set; }

        public string Description { get; set; }

        public decimal Amount { get; set; }

        public decimal SanctionedAmount { get; set; }
    }

    public class ExpenseTaxAndCharge
    {
        public int Id { get; set; }

        public int ExpenseClaimId { get; set; }

        // ERPNext child table field: account_head
        public string AccountHead { get; set; }

        public decimal Rate { get; set; }

        public decimal Amount { get; set; }

        public decimal Total { get; set; }
    }

    public class ExpenseClaimAdvance
    {
        public int Id { get; set; }

        public int ExpenseClaimId { get; set; }

        // ERPNext child table field: employee_advance
        public string EmployeeAdvance { get; set; }

        public DateTime? PostingDate { get; set; }

        public decimal AdvancePaid { get; set; }

        public decimal UnclaimedAmount { get; set; }

        public decimal AllocatedAmount { get; set; }
    }
}