//using System;

//namespace Erp_Aits.Models
//{
//    public class Goal
//    {
//        public int Id { get; set; }
//        public string GoalName { get; set; }
//        public decimal? Progress { get; set; }
//        public string Status { get; set; } = "Pending";
//        public bool IsGroup { get; set; }
//        public string Employee { get; set; }
//        public string Company { get; set; }
//        public DateTime? StartDate { get; set; }
//        public DateTime? EndDate { get; set; }
//        public string AppraisalCycle { get; set; }
//        public string Description { get; set; }
//    }
//}


using System;

namespace Erp_Aits.Models
{
    public class Goal
    {
        public int Id { get; set; }
        public string ErpNextId { get; set; }
        public string GoalName { get; set; }
        public decimal? Progress { get; set; }
        public string Status { get; set; } = "Pending";
        public bool IsGroup { get; set; }
        public string ParentGoal { get; set; }
        public string Employee { get; set; }
        public string EmployeeName { get; set; }
        public string Company { get; set; } = "AITS";
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string AppraisalCycle { get; set; }
        public string Kra { get; set; }
        public string Description { get; set; }
    }
}