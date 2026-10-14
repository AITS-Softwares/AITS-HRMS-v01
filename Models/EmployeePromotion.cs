using System;
using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class EmployeePromotion
    {
        public int Id { get; set; }
        public string ErpNextId { get; set; }
        public string Employee { get; set; }
        public string EmployeeName { get; set; }
        public DateTime PromotionDate { get; set; } = DateTime.Now;
        public string SalaryCurrency { get; set; } = "INR";
        public string Company { get; set; } = "AITS";
        public decimal? CurrentCTC { get; set; }

        public List<EmployeePromotionDetail> Details { get; set; } = new List<EmployeePromotionDetail>();
    }

    public class EmployeePromotionDetail
    {
        public int Id { get; set; }
        public int EmployeePromotionId { get; set; }
        public string FieldName { get; set; }
        public string Property { get; set; }
        public string CurrentValue { get; set; }
        public string NewValue { get; set; }
    }

    public class EmployeePromotionListRow
    {
        public int Id { get; set; }
        public string ErpNextId { get; set; }
        public string Employee { get; set; }
        public string EmployeeName { get; set; }
        public DateTime PromotionDate { get; set; }
    }
}
