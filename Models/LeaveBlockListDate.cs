using System;

namespace Erp_Aits.Models
{
    public class LeaveBlockListDate
    {
        public int Id { get; set; }
        public int LeaveBlockListId { get; set; }
        public DateTime BlockDate { get; set; }
        public string Reason { get; set; }
    }
}