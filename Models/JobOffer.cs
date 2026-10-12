using System;

namespace Erp_Aits.Models
{
    public class JobOffer
    {
        public int Id { get; set; }
        public string JobApplicant { get; set; }
        public string ApplicantName { get; set; }
        public string Status { get; set; } = "Awaiting Response";
        public DateTime OfferDate { get; set; } = DateTime.Now;
        public string Designation { get; set; }
        public string Company { get; set; }
        public string JobOfferTermTemplate { get; set; }
        public string SelectTermsAndConditions { get; set; }
        public string TermsAndConditions { get; set; }
        public string LetterHead { get; set; }
        public string PrintHeading { get; set; }
    }
}
