using System;
using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class LeaveBlockList
    {
        public int Id { get; set; }
        public string LeaveBlockListName { get; set; }
        public string LeaveType { get; set; }
        public string Company { get; set; }
        public bool AppliesToCompany { get; set; }
        public string ErpNextId { get; set; }
        public bool SyncToErpNext { get; set; } = true;

        public List<LeaveBlockListDate> BlockDates { get; set; } = new List<LeaveBlockListDate>();
        public List<LeaveBlockListAllowedUser> AllowedUsers { get; set; } = new List<LeaveBlockListAllowedUser>();
    }
}
