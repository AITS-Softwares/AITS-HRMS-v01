namespace Erp_Aits.Models
{
    public class EmployeeOnboardingActivity
    {
        public int Id { get; set; }
        public int EmployeeOnboardingId { get; set; }
        public string ActivityName { get; set; }
        public string User { get; set; }
        public int? BeginOnDays { get; set; }
        public int? DurationDays { get; set; }
    }
}
