using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class DesignationSkill
    {
        public int Id { get; set; }
        public int DesignationId { get; set; }
        public string Skill { get; set; }
    }

    public class Designation
    {
        public int Id { get; set; }
        public string DesignationCode { get; set; }
        public string DesignationName { get; set; }
        public string Description { get; set; }
        public string AppraisalTemplate { get; set; }

        // Skills Child Table
        public List<DesignationSkill> Skills { get; set; } = new List<DesignationSkill>();
    }
}