using System;

namespace Erp_Aits.Models
{
    public class EnergyPointLog
    {
        public int Id { get; set; }
        public string LogCode { get; set; }

        public string UserName { get; set; }




        public string Type { get; set; }
        public string Rule { get; set; }
        public string ReferenceDoctype { get; set; }
        public string ReferenceName { get; set; }
        public int Points { get; set; }
        public bool Reverted { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
}
