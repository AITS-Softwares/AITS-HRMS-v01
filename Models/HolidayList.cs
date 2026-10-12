//using System;
//using System.Collections.Generic;

//namespace Erp_Aits.Models
//{
//    public class HolidayList
//    {
//        public int Id { get; set; }
//        public string HolidayListName { get; set; }
//        public DateTime FromDate { get; set; }
//        public DateTime ToDate { get; set; }
//        public int TotalHolidays { get; set; }
//        public string Color { get; set; }

//        public List<HolidayListDate> Holidays { get; set; } = new List<HolidayListDate>();
//    }
//}




using System;
using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class HolidayList
    {
        public int Id { get; set; }
        public string HolidayListName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalHolidays { get; set; }
        public string Color { get; set; }

        // Added fields matching ERPNext layout
        public string Country { get; set; }
        public string Subdivision { get; set; }

        public string ErpNextId { get; set; }

        public List<HolidayListDate> Holidays { get; set; } = new List<HolidayListDate>();
    }
}