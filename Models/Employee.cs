//using System;

//namespace Erp_Aits.Models
//{
//    public class Employee
//    {
//        public int Id { get; set; }
//        public string ErpNextId { get; set; }
//        public string EmployeeCode { get; set; }
//        public string FullName { get; set; }

//        // Overview
//        public string Salutation { get; set; }
//        public string FirstName { get; set; }
//        public string MiddleName { get; set; }
//        public string LastName { get; set; }
//        public string Gender { get; set; }
//        public DateTime? DateOfBirth { get; set; }
//        public DateTime? DateOfJoining { get; set; }
//        public string Status { get; set; }

//        // Company Details
//        public string Company { get; set; }
//        public string Department { get; set; }
//        public string Designation { get; set; }
//        public string Grade { get; set; }
//        public string EmploymentType { get; set; }
//        public string ReportsTo { get; set; }
//        public string EmployeeNumber { get; set; }
//        public string Branch { get; set; }
//        public string UserId { get; set; }

//        // Joining
//        public string JobApplicant { get; set; }
//        public DateTime? ConfirmationDate { get; set; }
//        public int? NoticeDays { get; set; }
//        public DateTime? OfferDate { get; set; }
//        public DateTime? ContractEndDate { get; set; }
//        public DateTime? DateOfRetirement { get; set; }

//        // Address & Contacts
//        public string Mobile { get; set; }
//        public string PersonalEmail { get; set; }
//        public string CompanyEmail { get; set; }
//        public string PreferredContactEmail { get; set; }
//        public bool Unsubscribed { get; set; }
//        public string CurrentAddress { get; set; }
//        public string PermanentAddress { get; set; }
//        public string CurrentAddressIs { get; set; }
//        public string PermanentAddressIs { get; set; }
//        public string EmergencyContactName { get; set; }
//        public string EmergencyPhone { get; set; }
//        public string Relation { get; set; }

//        // Attendance & Leaves
//        public string AttendanceDeviceId { get; set; }
//        public string HolidayList { get; set; }
//        public string DefaultShift { get; set; }
//        public string ExpenseApprover { get; set; }
//        public string ShiftRequestApprover { get; set; }
//        public string LeaveApprover { get; set; }

//        // Salary
//        public decimal? CTC { get; set; }
//        public string SalaryCurrency { get; set; }
//        public string PayrollCostCenter { get; set; }
//        public string SalaryMode { get; set; }

//        public string BankName { get; set; }
//        public string BankAccountNo { get; set; }
//        public string IFSCCode { get; set; }

//        // Personal Details
//        public string MaritalStatus { get; set; }
//        public string BloodGroup { get; set; }
//        public string FamilyBackground { get; set; }
//        public string HealthDetails { get; set; }
//        public string HealthInsuranceProvider { get; set; }
//        public string PassportNumber { get; set; }
//        public DateTime? PassportDateOfIssue { get; set; }
//        public DateTime? PassportValidUpto { get; set; }
//        public string PassportPlaceOfIssue { get; set; }

//        // Profile
//        public string Bio { get; set; }

//        // Employee Exit
//        public DateTime? ResignationLetterDate { get; set; }
//        public DateTime? ExitInterviewHeldOn { get; set; }
//        public string LeaveEncashed { get; set; }
//        public DateTime? RelievingDate { get; set; }
//        public string NewWorkplace { get; set; }
//        public string ReasonForLeaving { get; set; }
//        public string Feedback { get; set; }

//        // Used in list page
//        public string Email { get; set; }
//        public string Phone { get; set; }
//    }
//}
















using System;
using System.Collections.Generic;

namespace Erp_Aits.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        public string ErpNextId { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }

        // Overview
        public string Salutation { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? DateOfJoining { get; set; }
        public string Status { get; set; }

        // Company Details
        public string Company { get; set; }
        public string Department { get; set; }
        public string Designation { get; set; }
        public string Grade { get; set; }
        public string EmploymentType { get; set; }
        public string ReportsTo { get; set; }
        public string EmployeeNumber { get; set; }
        public string Branch { get; set; }
        public string UserId { get; set; }

        // Joining
        public string JobApplicant { get; set; }
        public DateTime? ConfirmationDate { get; set; }
        public int? NoticeDays { get; set; }
        public DateTime? OfferDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public DateTime? DateOfRetirement { get; set; }

        // Address & Contacts
        public string Mobile { get; set; }
        public string PersonalEmail { get; set; }
        public string CompanyEmail { get; set; }
        public string PreferredContactEmail { get; set; }
        public bool Unsubscribed { get; set; }
        public string CurrentAddress { get; set; }
        public string PermanentAddress { get; set; }
        public string CurrentAddressIs { get; set; }
        public string PermanentAddressIs { get; set; }
        public string EmergencyContactName { get; set; }
        public string EmergencyPhone { get; set; }
        public string Relation { get; set; }

        // Attendance & Leaves
        public string AttendanceDeviceId { get; set; }
        public string HolidayList { get; set; }
        public string DefaultShift { get; set; }
        public string ExpenseApprover { get; set; }
        public string ShiftRequestApprover { get; set; }
        public string LeaveApprover { get; set; }

        // Salary
        public decimal? CTC { get; set; }
        public string SalaryCurrency { get; set; }
        public string PayrollCostCenter { get; set; }
        public string SalaryMode { get; set; }

        public string BankName { get; set; }
        public string BankAccountNo { get; set; }
        public string IFSCCode { get; set; }
        public string MICRCode { get; set; }
        public string IBAN { get; set; }

        // Personal Details
        public string MaritalStatus { get; set; }
        public string BloodGroup { get; set; }
        public string FamilyBackground { get; set; }
        public string HealthDetails { get; set; }
        public string HealthInsuranceProvider { get; set; }
        public string PassportNumber { get; set; }
        public DateTime? PassportDateOfIssue { get; set; }
        public DateTime? PassportValidUpto { get; set; }
        public string PassportPlaceOfIssue { get; set; }

        // Profile
        public string Bio { get; set; }

        // Employee Exit
        public DateTime? ResignationLetterDate { get; set; }
        public DateTime? ExitInterviewHeldOn { get; set; }
        public string LeaveEncashed { get; set; }
        public DateTime? RelievingDate { get; set; }
        public string NewWorkplace { get; set; }
        public string ReasonForLeaving { get; set; }
        public string Feedback { get; set; }

        // Used in list page
        // Used in list page
        public string Email { get; set; }
        public string Phone { get; set; }




        public string PreferredEmail { get; set; }
        public string PanNumber { get; set; }
        public string ProvidentFundAccount { get; set; }
        //public string MICRCode { get; set; }
        //public string IBAN { get; set; }
        public string HealthInsuranceNo { get; set; }
        public DateTime? EncashmentDate { get; set; }


















        // ERPNext Employee Profile child tables
        public List<EmployeeEducationRow> EducationRows { get; set; } = new List<EmployeeEducationRow>();
        public List<EmployeeExternalWorkHistoryRow> ExternalWorkHistoryRows { get; set; } = new List<EmployeeExternalWorkHistoryRow>();
        public List<EmployeeInternalWorkHistoryRow> InternalWorkHistoryRows { get; set; } = new List<EmployeeInternalWorkHistoryRow>();
        public bool ChildTablesPosted { get; set; }
    }


    public class EmployeeEducationRow
    {
        public string SchoolUniversity { get; set; }
        public string Qualification { get; set; }
        public string Level { get; set; }
        public int? YearOfPassing { get; set; }
        public string ClassPercentage { get; set; }
        public string MajorOptionalSubjects { get; set; }
    }

    public class EmployeeExternalWorkHistoryRow
    {
        public string CompanyName { get; set; }
        public string Designation { get; set; }
        public decimal? Salary { get; set; }
        public string Address { get; set; }
        public string Contact { get; set; }
        public string TotalExperience { get; set; }
    }

    public class EmployeeInternalWorkHistoryRow
    {
        public string Branch { get; set; }
        public string Department { get; set; }
        public string Designation { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }










}
