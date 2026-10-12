namespace Erp_Aits.Models
{
    public class TrainingProgram
    {
        public int Id { get; set; }
        public string TrainingProgramCode { get; set; }

        public string TrainingProgramName { get; set; }
        public string Company { get; set; } = "AITS";
        public string Status { get; set; } = "Scheduled";

        public string TrainerName { get; set; }
        public string Supplier { get; set; }
        public string TrainerEmail { get; set; }
        public string ContactNumber { get; set; }

        public string Description { get; set; }
    }
}