using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class TrainingResult
    {
        public int Id { get; set; }
        public string TrainingEvent { get; set; }

        public List<TrainingResultEmployee> Employees { get; set; } = new List<TrainingResultEmployee>();
    }

    public class TrainingResultEmployee
    {
        public int Id { get; set; }
        public int TrainingResultId { get; set; }
        public string Employee { get; set; }
        public decimal? Hours { get; set; }
        public string Grade { get; set; }
        public string Comments { get; set; }
    }

    public class TrainingResultListRow
    {
        public int Id { get; set; }
        public string TrainingEvent { get; set; }
    }
}
