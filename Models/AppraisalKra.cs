//namespace Erp_Aits.Models
//{
//    public class AppraisalKra
//    {
//        public int Id { get; set; }
//        public int AppraisalId { get; set; }
//        public string Kra { get; set; }
//        public decimal Weightage { get; set; }
//        public decimal? GoalCompletion { get; set; }
//        public decimal? GoalScore { get; set; }
//    }
//}


namespace Erp_Aits.Models
{
    public class AppraisalKra
    {
        public int Id { get; set; }
        public int AppraisalId { get; set; }
        public string Kra { get; set; }
        public decimal Weightage { get; set; }
        public decimal GoalCompletion { get; set; }
        public decimal GoalScore { get; set; }
    }
}