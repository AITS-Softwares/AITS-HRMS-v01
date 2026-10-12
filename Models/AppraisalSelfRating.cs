//namespace Erp_Aits.Models
//{
//    public class AppraisalSelfRating
//    {
//        public int Id { get; set; }
//        public int AppraisalId { get; set; }
//        public string Criteria { get; set; }
//        public decimal Weightage { get; set; }
//        public int Rating { get; set; }
//    }
//}


namespace Erp_Aits.Models
{
    public class AppraisalSelfRating
    {
        public int Id { get; set; }
        public int AppraisalId { get; set; }
        public string Criteria { get; set; }
        public decimal Weightage { get; set; }
        public decimal Rating { get; set; }
        public string Reflections { get; set; }
    }
}