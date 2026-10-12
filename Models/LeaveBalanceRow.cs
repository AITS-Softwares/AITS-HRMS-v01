namespace Erp_Aits.Models
{
    public class LeaveBalanceRow
    {
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string Department { get; set; }
        public int LeavesTaken { get; set; }
        public int TotalAllocated { get; set; }
        public int Balance { get; set; }
    }
}
