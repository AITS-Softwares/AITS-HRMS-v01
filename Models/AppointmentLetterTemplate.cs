using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class AppointmentLetterTemplate
    {
        public int Id { get; set; }
        public string TemplateCode { get; set; }

        public string TemplateName { get; set; }
        public string Introduction { get; set; }

        public List<AppointmentLetterTemplateTerm> Terms { get; set; } = new List<AppointmentLetterTemplateTerm>();

        public string ClosingNotes { get; set; }
    }

    public class AppointmentLetterTemplateTerm
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
