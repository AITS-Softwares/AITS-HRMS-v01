//using System.Collections.Generic;

//namespace Erp_Aits.Models
//{
//    public class EmployeeOnboardingTemplate
//    {
//        public int Id { get; set; }
//        public string Title { get; set; }
//        public string Designation { get; set; }
//        public string Company { get; set; } = "AITS";
//        public string EmployeeGrade { get; set; }
//        public string Department { get; set; }
//        public List<EmployeeOnboardingTemplateActivity> Activities { get; set; } = new List<EmployeeOnboardingTemplateActivity>();
//    }

//    public class EmployeeOnboardingTemplateActivity
//    {
//        public int Id { get; set; }
//        public int TemplateId { get; set; }
//        public string ActivityName { get; set; }
//        public string UserName { get; set; }
//        public int? BeginOnDays { get; set; }
//        public int? DurationDays { get; set; }
//    }
//}





using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class EmployeeOnboardingTemplate
    {
        public int Id { get; set; }
        public string ErpNextId { get; set; }
        public string Title { get; set; }
        public string Designation { get; set; }
        public string Company { get; set; } = "AITS";
        public string EmployeeGrade { get; set; }
        public string Department { get; set; }
        public List<EmployeeOnboardingTemplateActivity> Activities { get; set; } = new List<EmployeeOnboardingTemplateActivity>();
    }

    public class EmployeeOnboardingTemplateActivity
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public string ActivityName { get; set; }
        public string UserName { get; set; }
        public int? BeginOnDays { get; set; }
        public int? DurationDays { get; set; }
    }
}