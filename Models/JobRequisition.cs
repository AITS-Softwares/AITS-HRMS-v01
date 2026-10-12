using System;

namespace Erp_Aits.Models
{
    public class JobRequisition
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public int? NoOfPositions { get; set; }
        public string Company { get; set; }
        public string Designation { get; set; }
        public decimal? ExpectedCompensation { get; set; }
        public string Status { get; set; } = "Pending";
        public string Department { get; set; }
        public string RequestedBy { get; set; }
        public DateTime PostingDate { get; set; } = DateTime.Now;
        public DateTime? ExpectedBy { get; set; }
        public string JobDescription { get; set; }
        public string ReasonForRequesting { get; set; }
    }
}
