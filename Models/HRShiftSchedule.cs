
using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class HRShiftSchedule
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string ShiftType { get; set; }
        public string Frequency { get; set; } = "Every Week";
        public string Status { get; set; } = "Draft";




        public List<HRShiftScheduleDay> Days { get; set; } = new List<HRShiftScheduleDay>();
    }
}
