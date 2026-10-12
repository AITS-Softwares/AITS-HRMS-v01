namespace Erp_Aits.Models
{
    public class StaffingPlanDetail
    {
        public int Id { get; set; }
        public int StaffingPlanId { get; set; }
        public string Designation { get; set; }
        public int Vacancies { get; set; }
        public decimal EstimatedCostPerPosition { get; set; }
        public decimal TotalEstimatedCost { get; set; }
        public int NumberOfPositions { get; set; }
    }
}
