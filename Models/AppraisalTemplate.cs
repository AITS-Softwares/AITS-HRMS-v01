using System;
using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class AppraisalTemplate
    {
        public int Id { get; set; }
        public string AppraisalTemplateTitle { get; set; }
        public string Description { get; set; }

        public string ErpName { get; set; }

        public List<AppraisalTemplateKRA> KRAs { get; set; } = new List<AppraisalTemplateKRA>();
        public List<AppraisalTemplateRatingCriterion> RatingCriteria { get; set; } = new List<AppraisalTemplateRatingCriterion>();
    }

    public class AppraisalTemplateKRA
    {
        public int Id { get; set; }
        public int AppraisalTemplateId { get; set; }
        public string KRA { get; set; }
        public decimal Weightage { get; set; }
    }

    public class AppraisalTemplateRatingCriterion
    {
        public int Id { get; set; }
        public int AppraisalTemplateId { get; set; }
        public string Criteria { get; set; }
        public decimal Weightage { get; set; }
    }

    public class AppraisalTemplateListRow
    {
        public int Id { get; set; }
        public string AppraisalTemplateTitle { get; set; }
    }
}