//using System.Collections.Generic;

//namespace Erp_Aits.Models
//{
//    public class ExpenseClaimType
//    {
//        public int Id { get; set; }
//        public string Name { get; set; }
//        public bool DeferredExpenseAccount { get; set; }
//        public string Description { get; set; }
//        public List<ExpenseClaimTypeAccount> Accounts { get; set; } = new List<ExpenseClaimTypeAccount>();
//    }

//    public class ExpenseClaimTypeAccount
//    {
//        public int Id { get; set; }
//        public int ExpenseClaimTypeId { get; set; }
//        public string Company { get; set; }
//        public string DefaultAccount { get; set; }
//    }
//}















//using System.Collections.Generic;

//namespace Erp_Aits.Models
//{
//    public class ExpenseClaimType
//    {
//        public int Id { get; set; }
//        public string ErpNextId { get; set; }
//        public string Name { get; set; }
//        public bool DeferredExpenseAccount { get; set; }
//        public string Description { get; set; }
//        public List<ExpenseClaimTypeAccount> Accounts { get; set; } = new List<ExpenseClaimTypeAccount>();
//    }

//    public class ExpenseClaimTypeAccount
//    {
//        public int Id { get; set; }
//        public int ExpenseClaimTypeId { get; set; }
//        public string Company { get; set; }
//        public string DefaultAccount { get; set; }
//    }
//}











using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class ExpenseClaimType
    {
        public int Id { get; set; }

        // ERPNext document name
        public string ErpNextId { get; set; }

        // ERP field: expense_type
        public string Name { get; set; }

        // ERP field: deferred_expense_account
        public bool DeferredExpenseAccount { get; set; }

        // ERP field: description
        public string Description { get; set; }

        // ERP child table: accounts
        public List<ExpenseClaimTypeAccount> Accounts { get; set; }
            = new List<ExpenseClaimTypeAccount>();
    }


    public class ExpenseClaimTypeAccount
    {
        public int Id { get; set; }

        public int ExpenseClaimTypeId { get; set; }

        // ERP field: company
        public string Company { get; set; }

        // ERP field: default_account
        public string DefaultAccount { get; set; }
    }
}