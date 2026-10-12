//namespace Erp_Aits.Models
//{
//    public class Department

//    {
//        public int Id { get; set; }
//        public string DepartmentName { get; set; }
//        public string Company { get; set; }
//        public string ParentDepartment { get; set; }
//        public bool IsGroup { get; set; }
//        public bool Disabled { get; set; }
//        public string PayrollCostCenter { get; set; }
//        public string LeaveBlockList { get; set; }
//        public string Status { get; set; }
//        public string DepartmentCode { get; set; }
//    }
//}



using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class DepartmentApprover
    {
        public int Id { get; set; }
        public int DepartmentId { get; set; }
        public string Approver { get; set; }
    }

    public class Department
    {
        public int Id { get; set; }
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }
        public string Company { get; set; } = "Vinod Cookware India Private Limited";
        public string ParentDepartment { get; set; }
        public bool IsGroup { get; set; }
        public bool Disabled { get; set; }
        public string PayrollCostCenter { get; set; }
        public string LeaveBlockList { get; set; }
        public string Status { get; set; } = "Active";

        // Approver Child Tables
        public List<DepartmentApprover> ShiftRequestApprovers { get; set; } = new List<DepartmentApprover>();
        public List<DepartmentApprover> LeaveApprovers { get; set; } = new List<DepartmentApprover>();
        public List<DepartmentApprover> ExpenseApprovers { get; set; } = new List<DepartmentApprover>();
    }
}