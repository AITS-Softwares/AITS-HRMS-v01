//using System;

//namespace Erp_Aits.Models
//{
//    public class EmployeeGrievance
//    {
//        public int Id { get; set; }
//        public string Subject { get; set; }
//        public DateTime GrievanceDate { get; set; } = DateTime.Now;
//        public string RaisedBy { get; set; }
//        public string Status { get; set; } = "Invalid";
//        public string GrievanceAgainstParty { get; set; }
//        public string GrievanceAgainst { get; set; }
//        public string AssociatedDocumentType { get; set; }
//        public string AssociatedDocument { get; set; }
//        public string GrievanceType { get; set; }
//        public string Description { get; set; }
//        public string CauseOfGrievance { get; set; }
//        public string ResolvedBy { get; set; }
//        public DateTime? ResolutionDate { get; set; }
//        public string ResolutionDetails { get; set; }
//        public string EmployeeResponsible { get; set; }
//    }
//}



using System;

namespace Erp_Aits.Models
{
    public class EmployeeGrievance
    {
        public int Id { get; set; }
        public string ErpNextId { get; set; }
        public string Subject { get; set; }
        public DateTime GrievanceDate { get; set; } = DateTime.Now;
        public string RaisedBy { get; set; }
        public string Status { get; set; } = "Invalid";
        public string GrievanceAgainstParty { get; set; }
        public string GrievanceAgainst { get; set; }
        public string AssociatedDocumentType { get; set; }
        public string AssociatedDocument { get; set; }
        public string GrievanceType { get; set; }
        public string Description { get; set; }
        public string CauseOfGrievance { get; set; }
        public string ResolvedBy { get; set; }
        public DateTime? ResolutionDate { get; set; }
        public string ResolutionDetails { get; set; }
        public string EmployeeResponsible { get; set; }
    }
}