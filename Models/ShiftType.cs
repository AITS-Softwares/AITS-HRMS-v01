namespace Erp_Aits.Models
{
    public class ShiftType
    {
        public int Id { get; set; }
        public string ShiftTypeCode { get; set; }

        public string Name { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }

        public string HolidayList { get; set; }
        public string RosterColor { get; set; } = "Blue";
        public bool EnableAutoAttendance { get; set; }
    }
}
