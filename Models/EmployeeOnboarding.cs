using System;
using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class EmployeeOnboarding
    {
        public int Id { get; set; }

        public string ErpNextId { get; set; }

        public string JobApplicant { get; set; }
        public string JobOffer { get; set; }

        public string EmployeeOnboardingTemplate { get; set; }

        public string Company { get; set; }

        public string BoardingStatus { get; set; }

        public string Employee { get; set; }

        public string EmployeeName { get; set; }

        public DateTime DateOfJoining { get; set; }

        public string Department { get; set; }

        public DateTime OnboardingBeginsOn { get; set; }

        public string Designation { get; set; }

        public string EmployeeGrade { get; set; }

        public string HolidayList { get; set; }

        public string Project { get; set; }

        public bool NotifyUsersByEmail { get; set; }

        public int DocStatus { get; set; }

        public string AmendedFrom { get; set; }

        public List<EmployeeOnboardingActivity> Activities { get; set; }
            = new List<EmployeeOnboardingActivity>();
    }
}