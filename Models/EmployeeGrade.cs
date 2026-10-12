namespace Erp_Aits.Models
{
    public class EmployeeGrade
    {
        public int Id { get; set; }
        public string GradeName { get; set; }
        public string DefaultSalaryStructure { get; set; }
        public decimal? DefaultBasePay { get; set; }
        public string ErpNextId { get; set; }
    }
}