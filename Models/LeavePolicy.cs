using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class LeavePolicy
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ErpNextId { get; set; }
        public int DocStatus { get; set; }
        public string Status { get; set; } = "Draft";

        public List<LeavePolicyDetail> Details { get; set; } = new List<LeavePolicyDetail>();
    }
}
