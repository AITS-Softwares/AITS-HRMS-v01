using System;

namespace Erp_Aits.Models
{
    public class TrainingEvent
    {
        public int Id { get; set; }
        public string TrainingEventCode { get; set; }

        public string EventName { get; set; }
        public string Type { get; set; } = "Seminar";
        public string TrainingProgram { get; set; }
        public string Level { get; set; }
        public string EventStatus { get; set; } = "Scheduled";
        public string Company { get; set; } = "AITS";
        public bool HasCertificate { get; set; }

        public string TrainerName { get; set; }
        public string Supplier { get; set; }
        public string TrainerEmail { get; set; }
        public string ContactNumber { get; set; }

        public string Course { get; set; }
        public string Location { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public string Introduction { get; set; }
    }
}
