using System;
using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class StaffingPlan
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Company { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Department { get; set; }
        public decimal TotalEstimatedBudget { get; set; }
        public List<StaffingPlanDetail> Details { get; set; } = new List<StaffingPlanDetail>();
    }
}
