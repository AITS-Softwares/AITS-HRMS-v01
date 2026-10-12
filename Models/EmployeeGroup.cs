////namespace Erp_Aits.Models
////{
////    public class EmployeeGroup
////    {
////        public int Id { get; set; }
////        public string GroupName { get; set; }
////    }
////}

//using System.Collections.Generic;

//namespace Erp_Aits.Models
//{
//    public class EmployeeGroup
//    {
//        public int Id { get; set; }
//        public string GroupName { get; set; }
//        public List<EmployeeGroupMember> Members { get; set; } = new List<EmployeeGroupMember>();
//    }

//    public class EmployeeGroupMember
//    {
//        public int Id { get; set; }
//        public int EmployeeGroupId { get; set; }
//        public string Employee { get; set; }
//        public string EmployeeName { get; set; }
//        public string ERPNextUserId { get; set; }
//    }
//}








using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class EmployeeGroup
    {
        public int Id { get; set; }
        public string ErpNextId { get; set; }
        public string GroupName { get; set; }
        public string GroupCode { get; set; }
        public List<EmployeeGroupMember> Members { get; set; } = new List<EmployeeGroupMember>();
    }

    public class EmployeeGroupMember
    {
        public int Id { get; set; }
        public int EmployeeGroupId { get; set; }
        public string Employee { get; set; }
        public string EmployeeName { get; set; }
        public string ERPNextUserId { get; set; }
    }
}