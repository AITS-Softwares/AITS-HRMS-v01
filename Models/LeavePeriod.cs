//using System;

//namespace Erp_Aits.Models
//{
//    public class LeavePeriod
//    {
//        public int Id { get; set; }
//        public DateTime FromDate { get; set; }
//        public DateTime ToDate { get; set; }
//        public string Company { get; set; }
//        public string HolidayListForOptionalLeave { get; set; }
//        public bool IsActive { get; set; }
//    }
//}


using System;

namespace Erp_Aits.Models
{
    public class LeavePeriod
    {
        public int Id { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Company { get; set; }
        public string HolidayListForOptionalLeave { get; set; }
        public bool IsActive { get; set; }
        public string ErpNextId { get; set; }
    }
}