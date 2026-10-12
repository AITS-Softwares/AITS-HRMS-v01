using System;
using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class AppointmentLetter
    {
        public int Id { get; set; }
        public string AppointmentLetterCode { get; set; }

        public string JobApplicant { get; set; }
        public string Company { get; set; } = "AITS";
        public DateTime? AppointmentDate { get; set; }
        public string AppointmentLetterTemplate { get; set; }

        public string Introduction { get; set; }
        public List<AppointmentLetterTerm> Terms { get; set; } = new List<AppointmentLetterTerm>();
        public string ClosingNotes { get; set; }
    }

    public class AppointmentLetterTerm
    {
        public int Id { get; set; }
        public int AppointmentLetterId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
