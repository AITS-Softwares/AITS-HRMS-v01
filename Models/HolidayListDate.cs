using System;

namespace Erp_Aits.Models
{
    public class HolidayListDate
    {
        public int Id { get; set; }
        public int HolidayListId { get; set; }
        public DateTime HolidayDate { get; set; }
        public string Description { get; set; }
    }
}
