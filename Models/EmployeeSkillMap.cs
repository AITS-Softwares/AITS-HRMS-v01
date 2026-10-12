using System;
using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class EmployeeSkillMap
    {
        public int Id { get; set; }

        // ERPNext Employee Skill Map document name.
        // ERP autoname is employee, but keep this explicitly for sync.
        public string ErpNextId { get; set; }

        public string Employee { get; set; }

        public string EmployeeName { get; set; }

        // ERP fetches this from Employee
        public string Designation { get; set; }

        public List<EmployeeSkillMapSkill> Skills { get; set; }
            = new List<EmployeeSkillMapSkill>();

        public List<EmployeeSkillMapTraining> Trainings { get; set; }
            = new List<EmployeeSkillMapTraining>();
    }


    public class EmployeeSkillMapSkill
    {
        public int Id { get; set; }

        public int EmployeeSkillMapId { get; set; }

        // ERP Link -> Skill
        public string Skill { get; set; }

        // ERP field type is Rating
        public decimal Proficiency { get; set; }

        public DateTime? EvaluationDate { get; set; }
    }


    public class EmployeeSkillMapTraining
    {
        public int Id { get; set; }

        public int EmployeeSkillMapId { get; set; }

        // ERP Link -> Training Event
        public string Training { get; set; }

        // ERP fetches this from Training Event
        public DateTime? TrainingDate { get; set; }
    }


    public class EmployeeSkillMapListRow
    {
        public int Id { get; set; }

        public string ErpNextId { get; set; }

        public string Employee { get; set; }

        public string EmployeeName { get; set; }

        public string Designation { get; set; }
    }
}