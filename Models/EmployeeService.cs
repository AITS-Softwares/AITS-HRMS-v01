//using System;
//using System.Collections.Generic;
//using Microsoft.Data.SqlClient;
//using Erp_Aits.Models;

//namespace Erp_Aits.Services
//{
//    public class EmployeeService
//    {
//        private readonly string _connectionString;

//        public EmployeeService(string connectionString)
//        {
//            _connectionString = connectionString;
//        }

//        // 1. Get All Employees (Search & Status Filter)
//        //public List<Employee> GetAllEmployees(string company = null)
//        //{
//        //    var list = new List<Employee>();
//        //    using (var conn = new SqlConnection(_connectionString))
//        //    {
//        //        string query = "SELECT * FROM Employees";
//        //        using (var cmd = new SqlCommand(query, conn))
//        //        {
//        //            conn.Open();
//        //            using (var reader = cmd.ExecuteReader())
//        //            {
//        //                while (reader.Read())
//        //                {
//        //                    list.Add(new Employee
//        //                    {
//        //                        Id = reader["Id"] != DBNull.Value ? Convert.ToInt32(reader["Id"]) : 0,
//        //                        ErpNextId = reader["ErpNextId"]?.ToString(),
//        //                        EmployeeCode = reader["EmployeeCode"]?.ToString(),
//        //                        FirstName = reader["FirstName"]?.ToString(),
//        //                        MiddleName = reader["MiddleName"]?.ToString(),
//        //                        LastName = reader["LastName"]?.ToString(),
//        //                        FullName = reader["FullName"]?.ToString(),
//        //                        Gender = reader["Gender"]?.ToString(),
//        //                        Status = reader["Status"]?.ToString() ?? "Active",
//        //                        Company = reader["Company"]?.ToString(),
//        //                        Department = reader["Department"]?.ToString(),
//        //                        Designation = reader["Designation"]?.ToString(),
//        //                        Grade = reader["Grade"]?.ToString(),
//        //                        EmploymentType = reader["EmploymentType"]?.ToString(),
//        //                        ReportsTo = reader["ReportsTo"]?.ToString(),
//        //                        EmployeeNumber = reader["EmployeeNumber"]?.ToString(),
//        //                        Branch = reader["Branch"]?.ToString(),
//        //                        UserId = reader["UserId"]?.ToString(),
//        //                        Mobile = reader["Mobile"]?.ToString() ?? reader["Phone"]?.ToString(),
//        //                        PersonalEmail = reader["PersonalEmail"]?.ToString(),
//        //                        CompanyEmail = reader["CompanyEmail"]?.ToString(),
//        //                        Email = reader["Email"]?.ToString(),
//        //                        CurrentAddress = reader["CurrentAddress"]?.ToString(),
//        //                        PermanentAddress = reader["PermanentAddress"]?.ToString(),
//        //                        DateOfBirth = reader["DateOfBirth"] != DBNull.Value ? Convert.ToDateTime(reader["DateOfBirth"]) : (DateTime?)null,
//        //                        DateOfJoining = reader["DateOfJoining"] != DBNull.Value ? Convert.ToDateTime(reader["DateOfJoining"]) : (DateTime?)null,
//        //                        CTC = reader["CTC"] != DBNull.Value ? Convert.ToDecimal(reader["CTC"]) : (decimal?)null
//        //                    });
//        //                }
//        //            }
//        //        }
//        //    }
//        //    if (!string.IsNullOrEmpty(company))
//        //    {
//        //        list = list.Where(e => e.Company == company).ToList();
//        //    }
//        //    return list;
//        //}




//        public List<Employee> GetAllEmployees(string company = null)
//        {
//            var list = new List<Employee>();
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = "SELECT * FROM Employees WHERE IsDeleted = 0";
//                if (!string.IsNullOrEmpty(company))
//                {
//                    query += " AND Company = @Company";
//                }

//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    if (!string.IsNullOrEmpty(company))
//                    {
//                        cmd.Parameters.AddWithValue("@Company", company);
//                    }

//                    conn.Open();
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            list.Add(MapEmployee(reader));
//                        }
//                    }
//                }
//            }
//            return list;
//        }






//        // 2. Get Employees for Leave Balance Summary Report (3 Arguments)
//        public List<Employee> GetEmployeesForReport(string department, string employee, string status)
//        {
//            var list = new List<Employee>();
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = "SELECT * FROM Employees WHERE IsDeleted = 0";

//                if (!string.IsNullOrEmpty(department))
//                    query += " AND Department = @Department";

//                if (!string.IsNullOrEmpty(employee))
//                    query += " AND (EmployeeCode = @Employee OR FullName LIKE @EmployeeName)";

//                if (!string.IsNullOrEmpty(status))
//                    query += " AND Status = @Status";

//                query += " ORDER BY FullName ASC";

//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    if (!string.IsNullOrEmpty(department)) cmd.Parameters.AddWithValue("@Department", department);
//                    if (!string.IsNullOrEmpty(employee))
//                    {
//                        cmd.Parameters.AddWithValue("@Employee", employee);
//                        cmd.Parameters.AddWithValue("@EmployeeName", $"%{employee}%");
//                    }
//                    if (!string.IsNullOrEmpty(status)) cmd.Parameters.AddWithValue("@Status", status);

//                    conn.Open();
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            list.Add(MapEmployee(reader));
//                        }
//                    }
//                }
//            }
//            return list;
//        }

//        // Overload for 2 arguments if called elsewhere
//        public List<Employee> GetEmployeesForReport(string department = null, string status = null)
//        {
//            return GetEmployeesForReport(department, null, status);
//        }

//        // 3. Search Employees for Shift Tool (4 Arguments)
//        public List<Employee> SearchForShiftTool(string department, string designation, string employmentType, string grade)
//        {
//            var list = new List<Employee>();
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = "SELECT * FROM Employees WHERE IsDeleted = 0";

//                if (!string.IsNullOrEmpty(department))
//                    query += " AND Department = @Department";

//                if (!string.IsNullOrEmpty(designation))
//                    query += " AND Designation = @Designation";

//                if (!string.IsNullOrEmpty(employmentType))
//                    query += " AND EmploymentType = @EmploymentType";

//                if (!string.IsNullOrEmpty(grade))
//                    query += " AND Grade = @Grade";

//                query += " ORDER BY FullName ASC";

//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    if (!string.IsNullOrEmpty(department)) cmd.Parameters.AddWithValue("@Department", department);
//                    if (!string.IsNullOrEmpty(designation)) cmd.Parameters.AddWithValue("@Designation", designation);
//                    if (!string.IsNullOrEmpty(employmentType)) cmd.Parameters.AddWithValue("@EmploymentType", employmentType);
//                    if (!string.IsNullOrEmpty(grade)) cmd.Parameters.AddWithValue("@Grade", grade);

//                    conn.Open();
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            list.Add(MapEmployee(reader));
//                        }
//                    }
//                }
//            }
//            return list;
//        }

//        // 4. Get Existing ERPNext IDs to avoid sync duplicates
//        public HashSet<string> GetExistingErpNextIds()
//        {
//            var ids = new HashSet<string>();
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = "SELECT ErpNextId FROM Employees WHERE ErpNextId IS NOT NULL AND ErpNextId != '' UNION SELECT EmployeeCode FROM Employees WHERE EmployeeCode IS NOT NULL AND EmployeeCode != ''";
//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    conn.Open();
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            ids.Add(reader[0].ToString());
//                        }
//                    }
//                }
//            }
//            return ids;
//        }




//        public System.Data.DataTable GetAllEmployeesTable()
//        {
//            var dt = new System.Data.DataTable();
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = "SELECT * FROM Employees";
//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    using (var adapter = new SqlDataAdapter(cmd))
//                    {
//                        conn.Open();
//                        adapter.Fill(dt);
//                    }
//                }
//            }
//            return dt;
//        }










//        // 5. Get Employee by Local ID
//        public Employee GetById(int id)
//        {
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = "SELECT * FROM Employees WHERE Id = @Id AND IsDeleted = 0";
//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    cmd.Parameters.AddWithValue("@Id", id);
//                    conn.Open();
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        if (reader.Read())
//                        {
//                            return MapEmployee(reader);
//                        }
//                    }
//                }
//            }
//            return null;
//        }

//        // 6. Generate Next Employee Code
//        public string GetNextEmployeeCode()
//        {
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = "SELECT COUNT(*) FROM Employees";
//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    conn.Open();
//                    int count = (int)cmd.ExecuteScalar() + 1;
//                    return $"HR-EMP-{count:D5}";
//                }
//            }
//        }

//        // 7. Insert Employee (Returns New Primary Key Int ID)
//        public int InsertEmployee(Employee emp)
//        {
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = @"
//                    INSERT INTO Employees (
//                        ErpNextId, EmployeeCode, FullName, Salutation, FirstName, MiddleName, LastName, Gender, DateOfBirth, DateOfJoining, Status,
//                        Company, Department, Designation, Grade, EmploymentType, ReportsTo, EmployeeNumber, Branch, UserId,
//                        JobApplicant, ConfirmationDate, NoticeDays, OfferDate, ContractEndDate, DateOfRetirement,
//                        Mobile, PersonalEmail, CompanyEmail, PreferredContactEmail, Unsubscribed, CurrentAddress, PermanentAddress, CurrentAddressIs, PermanentAddressIs, EmergencyContactName, EmergencyPhone, Relation,
//                        AttendanceDeviceId, HolidayList, DefaultShift, ExpenseApprover, ShiftRequestApprover, LeaveApprover,
//                        CTC, SalaryCurrency, PayrollCostCenter, SalaryMode, BankName, BankAccountNo, IFSCCode,
//                        MaritalStatus, BloodGroup, FamilyBackground, HealthDetails, HealthInsuranceProvider, PassportNumber, PassportDateOfIssue, PassportValidUpto, PassportPlaceOfIssue,
//                        Bio, ResignationLetterDate, ExitInterviewHeldOn, LeaveEncashed, RelievingDate, NewWorkplace, ReasonForLeaving, Feedback, Email, Phone, IsDeleted
//                    ) 
//                    OUTPUT INSERTED.Id
//                    VALUES (
//                        @ErpNextId, @EmployeeCode, @FullName, @Salutation, @FirstName, @MiddleName, @LastName, @Gender, @DateOfBirth, @DateOfJoining, @Status,
//                        @Company, @Department, @Designation, @Grade, @EmploymentType, @ReportsTo, @EmployeeNumber, @Branch, @UserId,
//                        @JobApplicant, @ConfirmationDate, @NoticeDays, @OfferDate, @ContractEndDate, @DateOfRetirement,
//                        @Mobile, @PersonalEmail, @CompanyEmail, @PreferredContactEmail, @Unsubscribed, @CurrentAddress, @PermanentAddress, @CurrentAddressIs, @PermanentAddressIs, @EmergencyContactName, @EmergencyPhone, @Relation,
//                        @AttendanceDeviceId, @HolidayList, @DefaultShift, @ExpenseApprover, @ShiftRequestApprover, @LeaveApprover,
//                        @CTC, @SalaryCurrency, @PayrollCostCenter, @SalaryMode, @BankName, @BankAccountNo, @IFSCCode,
//                        @MaritalStatus, @BloodGroup, @FamilyBackground, @HealthDetails, @HealthInsuranceProvider, @PassportNumber, @PassportDateOfIssue, @PassportValidUpto, @PassportPlaceOfIssue,
//                        @Bio, @ResignationLetterDate, @ExitInterviewHeldOn, @LeaveEncashed, @RelievingDate, @NewWorkplace, @ReasonForLeaving, @Feedback, @Email, @Phone, 0
//                    )";

//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    AddEmployeeParameters(cmd, emp);
//                    conn.Open();
//                    return Convert.ToInt32(cmd.ExecuteScalar());
//                }
//            }
//        }

//        // 8. Update Employee
//        public void UpdateEmployee(Employee emp)
//        {
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = @"
//                    UPDATE Employees SET
//                        FullName = @FullName, Salutation = @Salutation, FirstName = @FirstName, MiddleName = @MiddleName, LastName = @LastName,
//                        Gender = @Gender, DateOfBirth = @DateOfBirth, DateOfJoining = @DateOfJoining, Status = @Status, Company = @Company,
//                        Department = @Department, Designation = @Designation, Grade = @Grade, EmploymentType = @EmploymentType, ReportsTo = @ReportsTo,
//                        Mobile = @Mobile, PersonalEmail = @PersonalEmail, CompanyEmail = @CompanyEmail, HolidayList = @HolidayList, LeaveApprover = @LeaveApprover
//                    WHERE Id = @Id";

//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    cmd.Parameters.AddWithValue("@Id", emp.Id);
//                    AddEmployeeParameters(cmd, emp);
//                    conn.Open();
//                    cmd.ExecuteNonQuery();
//                }
//            }
//        }

//        // 9. Update Synced ERPNext ID
//        public void UpdateErpNextId(int id, string erpNextId)
//        {
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = "UPDATE Employees SET ErpNextId = @ErpNextId WHERE Id = @Id";
//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    cmd.Parameters.AddWithValue("@Id", id);
//                    cmd.Parameters.AddWithValue("@ErpNextId", erpNextId ?? (object)DBNull.Value);
//                    conn.Open();
//                    cmd.ExecuteNonQuery();
//                }
//            }
//        }

//        // 10. Soft Delete Employee
//        public void DeleteEmployee(int id)
//        {
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = "UPDATE Employees SET IsDeleted = 1 WHERE Id = @Id";
//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    cmd.Parameters.AddWithValue("@Id", id);
//                    conn.Open();
//                    cmd.ExecuteNonQuery();
//                }
//            }
//        }

//        // Helper Method to Bind SQL Parameters
//        //private void AddEmployeeParameters(SqlCommand cmd, Employee emp)
//        //{
//        //    cmd.Parameters.AddWithValue("@ErpNextId", (object)emp.ErpNextId ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@EmployeeCode", (object)emp.EmployeeCode ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@FullName", (object)emp.FullName ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@Salutation", (object)emp.Salutation ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@FirstName", (object)emp.FirstName ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@MiddleName", (object)emp.MiddleName ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@LastName", (object)emp.LastName ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@Gender", (object)emp.Gender ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@DateOfBirth", (object)emp.DateOfBirth ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@DateOfJoining", (object)emp.DateOfJoining ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@Status", (object)emp.Status ?? "Active");
//        //    cmd.Parameters.AddWithValue("@Company", (object)emp.Company ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@Department", (object)emp.Department ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@Designation", (object)emp.Designation ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@Grade", (object)emp.Grade ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@EmploymentType", (object)emp.EmploymentType ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@ReportsTo", (object)emp.ReportsTo ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@EmployeeNumber", (object)emp.EmployeeNumber ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@Branch", (object)emp.Branch ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@UserId", (object)emp.UserId ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@JobApplicant", (object)emp.JobApplicant ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@ConfirmationDate", (object)emp.ConfirmationDate ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@NoticeDays", (object)emp.NoticeDays ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@OfferDate", (object)emp.OfferDate ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@ContractEndDate", (object)emp.ContractEndDate ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@DateOfRetirement", (object)emp.DateOfRetirement ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@Mobile", (object)emp.Mobile ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@PersonalEmail", (object)emp.PersonalEmail ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@CompanyEmail", (object)emp.CompanyEmail ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@PreferredContactEmail", (object)emp.PreferredContactEmail ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@Unsubscribed", emp.Unsubscribed);
//        //    cmd.Parameters.AddWithValue("@CurrentAddress", (object)emp.CurrentAddress ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@PermanentAddress", (object)emp.PermanentAddress ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@CurrentAddressIs", (object)emp.CurrentAddressIs ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@PermanentAddressIs", (object)emp.PermanentAddressIs ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@EmergencyContactName", (object)emp.EmergencyContactName ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@EmergencyPhone", (object)emp.EmergencyPhone ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@Relation", (object)emp.Relation ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@AttendanceDeviceId", (object)emp.AttendanceDeviceId ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@HolidayList", (object)emp.HolidayList ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@DefaultShift", (object)emp.DefaultShift ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@ExpenseApprover", (object)emp.ExpenseApprover ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@ShiftRequestApprover", (object)emp.ShiftRequestApprover ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@LeaveApprover", (object)emp.LeaveApprover ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@CTC", (object)emp.CTC ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@SalaryCurrency", (object)emp.SalaryCurrency ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@PayrollCostCenter", (object)emp.PayrollCostCenter ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@SalaryMode", (object)emp.SalaryMode ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@BankName", (object)emp.BankName ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@BankAccountNo", (object)emp.BankAccountNo ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@IFSCCode", (object)emp.IFSCCode ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@MaritalStatus", (object)emp.MaritalStatus ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@BloodGroup", (object)emp.BloodGroup ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@FamilyBackground", (object)emp.FamilyBackground ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@HealthDetails", (object)emp.HealthDetails ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@HealthInsuranceProvider", (object)emp.HealthInsuranceProvider ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@PassportNumber", (object)emp.PassportNumber ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@PassportDateOfIssue", (object)emp.PassportDateOfIssue ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@PassportValidUpto", (object)emp.PassportValidUpto ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@PassportPlaceOfIssue", (object)emp.PassportPlaceOfIssue ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@Bio", (object)emp.Bio ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@ResignationLetterDate", (object)emp.ResignationLetterDate ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@ExitInterviewHeldOn", (object)emp.ExitInterviewHeldOn ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@LeaveEncashed", (object)emp.LeaveEncashed ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@RelievingDate", (object)emp.RelievingDate ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@NewWorkplace", (object)emp.NewWorkplace ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@ReasonForLeaving", (object)emp.ReasonForLeaving ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@Feedback", (object)emp.Feedback ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@Email", (object)emp.CompanyEmail ?? (object)emp.PersonalEmail ?? DBNull.Value);
//        //    cmd.Parameters.AddWithValue("@Phone", (object)emp.Mobile ?? DBNull.Value);
//        //}



//        private void AddEmployeeParameters(SqlCommand cmd, Employee emp)
//        {
//            // Auto-calculate FullName if it was not provided explicitly
//            string constructedFullName = !string.IsNullOrWhiteSpace(emp.FullName)
//                ? emp.FullName
//                : $"{emp.FirstName} {emp.MiddleName} {emp.LastName}".Replace("  ", " ").Trim();

//            if (string.IsNullOrWhiteSpace(constructedFullName))
//            {
//                constructedFullName = emp.FirstName ?? "Unknown";
//            }

//            cmd.Parameters.AddWithValue("@ErpNextId", (object)emp.ErpNextId ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@EmployeeCode", (object)emp.EmployeeCode ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@FullName", constructedFullName); // Never NULL!
//            cmd.Parameters.AddWithValue("@Salutation", (object)emp.Salutation ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@FirstName", (object)emp.FirstName ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@MiddleName", (object)emp.MiddleName ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@LastName", (object)emp.LastName ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@Gender", (object)emp.Gender ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@DateOfBirth", (object)emp.DateOfBirth ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@DateOfJoining", (object)emp.DateOfJoining ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@Status", (object)emp.Status ?? "Active");
//            cmd.Parameters.AddWithValue("@Company", (object)emp.Company ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@Department", (object)emp.Department ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@Designation", (object)emp.Designation ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@Grade", (object)emp.Grade ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@EmploymentType", (object)emp.EmploymentType ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@ReportsTo", (object)emp.ReportsTo ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@EmployeeNumber", (object)emp.EmployeeNumber ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@Branch", (object)emp.Branch ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@UserId", (object)emp.UserId ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@JobApplicant", (object)emp.JobApplicant ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@ConfirmationDate", (object)emp.ConfirmationDate ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@NoticeDays", (object)emp.NoticeDays ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@OfferDate", (object)emp.OfferDate ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@ContractEndDate", (object)emp.ContractEndDate ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@DateOfRetirement", (object)emp.DateOfRetirement ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@Mobile", (object)emp.Mobile ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@PersonalEmail", (object)emp.PersonalEmail ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@CompanyEmail", (object)emp.CompanyEmail ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@PreferredContactEmail", (object)emp.PreferredContactEmail ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@Unsubscribed", emp.Unsubscribed);
//            cmd.Parameters.AddWithValue("@CurrentAddress", (object)emp.CurrentAddress ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@PermanentAddress", (object)emp.PermanentAddress ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@CurrentAddressIs", (object)emp.CurrentAddressIs ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@PermanentAddressIs", (object)emp.PermanentAddressIs ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@EmergencyContactName", (object)emp.EmergencyContactName ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@EmergencyPhone", (object)emp.EmergencyPhone ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@Relation", (object)emp.Relation ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@AttendanceDeviceId", (object)emp.AttendanceDeviceId ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@HolidayList", (object)emp.HolidayList ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@DefaultShift", (object)emp.DefaultShift ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@ExpenseApprover", (object)emp.ExpenseApprover ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@ShiftRequestApprover", (object)emp.ShiftRequestApprover ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@LeaveApprover", (object)emp.LeaveApprover ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@CTC", (object)emp.CTC ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@SalaryCurrency", (object)emp.SalaryCurrency ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@PayrollCostCenter", (object)emp.PayrollCostCenter ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@SalaryMode", (object)emp.SalaryMode ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@BankName", (object)emp.BankName ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@BankAccountNo", (object)emp.BankAccountNo ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@IFSCCode", (object)emp.IFSCCode ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@MaritalStatus", (object)emp.MaritalStatus ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@BloodGroup", (object)emp.BloodGroup ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@FamilyBackground", (object)emp.FamilyBackground ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@HealthDetails", (object)emp.HealthDetails ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@HealthInsuranceProvider", (object)emp.HealthInsuranceProvider ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@PassportNumber", (object)emp.PassportNumber ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@PassportDateOfIssue", (object)emp.PassportDateOfIssue ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@PassportValidUpto", (object)emp.PassportValidUpto ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@PassportPlaceOfIssue", (object)emp.PassportPlaceOfIssue ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@Bio", (object)emp.Bio ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@ResignationLetterDate", (object)emp.ResignationLetterDate ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@ExitInterviewHeldOn", (object)emp.ExitInterviewHeldOn ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@LeaveEncashed", (object)emp.LeaveEncashed ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@RelievingDate", (object)emp.RelievingDate ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@NewWorkplace", (object)emp.NewWorkplace ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@ReasonForLeaving", (object)emp.ReasonForLeaving ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@Feedback", (object)emp.Feedback ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@Email", (object)emp.CompanyEmail ?? (object)emp.PersonalEmail ?? DBNull.Value);
//            cmd.Parameters.AddWithValue("@Phone", (object)emp.Mobile ?? DBNull.Value);
//        }


//        public HashSet<string> GetExistingEmployeeCodes()
//        {
//            var codes = new HashSet<string>();
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = "SELECT EmployeeCode FROM Employees WHERE EmployeeCode IS NOT NULL AND EmployeeCode != ''";
//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    conn.Open();
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            codes.Add(reader["EmployeeCode"].ToString());
//                        }
//                    }
//                }
//            }
//            return codes;
//        }

//        // Helper Method to Map SqlDataReader to Employee Model
//        private Employee MapEmployee(SqlDataReader r)
//        {
//            return new Employee
//            {
//                Id = Convert.ToInt32(r["Id"]),
//                ErpNextId = r["ErpNextId"] == DBNull.Value ? null : r["ErpNextId"].ToString(),
//                EmployeeCode = r["EmployeeCode"] == DBNull.Value ? null : r["EmployeeCode"].ToString(),
//                FullName = r["FullName"] == DBNull.Value ? null : r["FullName"].ToString(),
//                Salutation = r["Salutation"] == DBNull.Value ? null : r["Salutation"].ToString(),
//                FirstName = r["FirstName"] == DBNull.Value ? null : r["FirstName"].ToString(),
//                MiddleName = r["MiddleName"] == DBNull.Value ? null : r["MiddleName"].ToString(),
//                LastName = r["LastName"] == DBNull.Value ? null : r["LastName"].ToString(),
//                Gender = r["Gender"] == DBNull.Value ? null : r["Gender"].ToString(),
//                DateOfBirth = r["DateOfBirth"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["DateOfBirth"]),
//                DateOfJoining = r["DateOfJoining"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["DateOfJoining"]),
//                Status = r["Status"] == DBNull.Value ? null : r["Status"].ToString(),
//                Company = r["Company"] == DBNull.Value ? null : r["Company"].ToString(),
//                Department = r["Department"] == DBNull.Value ? null : r["Department"].ToString(),
//                Designation = r["Designation"] == DBNull.Value ? null : r["Designation"].ToString(),
//                Grade = r["Grade"] == DBNull.Value ? null : r["Grade"].ToString(),
//                EmploymentType = r["EmploymentType"] == DBNull.Value ? null : r["EmploymentType"].ToString(),
//                ReportsTo = r["ReportsTo"] == DBNull.Value ? null : r["ReportsTo"].ToString(),
//                EmployeeNumber = r["EmployeeNumber"] == DBNull.Value ? null : r["EmployeeNumber"].ToString(),
//                Branch = r["Branch"] == DBNull.Value ? null : r["Branch"].ToString(),
//                UserId = r["UserId"] == DBNull.Value ? null : r["UserId"].ToString(),
//                JobApplicant = r["JobApplicant"] == DBNull.Value ? null : r["JobApplicant"].ToString(),
//                Mobile = r["Mobile"] == DBNull.Value ? null : r["Mobile"].ToString(),
//                PersonalEmail = r["PersonalEmail"] == DBNull.Value ? null : r["PersonalEmail"].ToString(),
//                CompanyEmail = r["CompanyEmail"] == DBNull.Value ? null : r["CompanyEmail"].ToString(),
//                PreferredContactEmail = r["PreferredContactEmail"] == DBNull.Value ? null : r["PreferredContactEmail"].ToString(),
//                AttendanceDeviceId = r["AttendanceDeviceId"] == DBNull.Value ? null : r["AttendanceDeviceId"].ToString(),
//                HolidayList = r["HolidayList"] == DBNull.Value ? null : r["HolidayList"].ToString(),
//                DefaultShift = r["DefaultShift"] == DBNull.Value ? null : r["DefaultShift"].ToString(),
//                LeaveApprover = r["LeaveApprover"] == DBNull.Value ? null : r["LeaveApprover"].ToString(),
//                ShiftRequestApprover = r["ShiftRequestApprover"] == DBNull.Value ? null : r["ShiftRequestApprover"].ToString(),
//                SalaryCurrency = r["SalaryCurrency"] == DBNull.Value ? null : r["SalaryCurrency"].ToString(),
//                PayrollCostCenter = r["PayrollCostCenter"] == DBNull.Value ? null : r["PayrollCostCenter"].ToString(),
//                SalaryMode = r["SalaryMode"] == DBNull.Value ? null : r["SalaryMode"].ToString(),
//                MaritalStatus = r["MaritalStatus"] == DBNull.Value ? null : r["MaritalStatus"].ToString(),
//                BloodGroup = r["BloodGroup"] == DBNull.Value ? null : r["BloodGroup"].ToString(),
//                FamilyBackground = r["FamilyBackground"] == DBNull.Value ? null : r["FamilyBackground"].ToString(),
//                LeaveEncashed = r["LeaveEncashed"] == DBNull.Value ? null : r["LeaveEncashed"].ToString(),
//                Email = r["Email"] == DBNull.Value ? null : r["Email"].ToString(),
//                Phone = r["Phone"] == DBNull.Value ? null : r["Phone"].ToString()
//            };
//        }
//    }
//}














































using DocumentFormat.OpenXml.Spreadsheet;
using Erp_Aits.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Erp_Aits.Services
{
    public class EmployeeService
    {
        private readonly string _connectionString;

        public EmployeeService(string connectionString)
        {
            _connectionString = connectionString;
        }

        // 1. Get All Employees (Search & Status Filter)
        //public List<Employee> GetAllEmployees(string company = null)
        //{
        //    var list = new List<Employee>();
        //    using (var conn = new SqlConnection(_connectionString))
        //    {
        //        string query = "SELECT * FROM Employees";
        //        using (var cmd = new SqlCommand(query, conn))
        //        {
        //            conn.Open();
        //            using (var reader = cmd.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    list.Add(new Employee
        //                    {
        //                        Id = reader["Id"] != DBNull.Value ? Convert.ToInt32(reader["Id"]) : 0,
        //                        ErpNextId = reader["ErpNextId"]?.ToString(),
        //                        EmployeeCode = reader["EmployeeCode"]?.ToString(),
        //                        FirstName = reader["FirstName"]?.ToString(),
        //                        MiddleName = reader["MiddleName"]?.ToString(),
        //                        LastName = reader["LastName"]?.ToString(),
        //                        FullName = reader["FullName"]?.ToString(),
        //                        Gender = reader["Gender"]?.ToString(),
        //                        Status = reader["Status"]?.ToString() ?? "Active",
        //                        Company = reader["Company"]?.ToString(),
        //                        Department = reader["Department"]?.ToString(),
        //                        Designation = reader["Designation"]?.ToString(),
        //                        Grade = reader["Grade"]?.ToString(),
        //                        EmploymentType = reader["EmploymentType"]?.ToString(),
        //                        ReportsTo = reader["ReportsTo"]?.ToString(),
        //                        EmployeeNumber = reader["EmployeeNumber"]?.ToString(),
        //                        Branch = reader["Branch"]?.ToString(),
        //                        UserId = reader["UserId"]?.ToString(),
        //                        Mobile = reader["Mobile"]?.ToString() ?? reader["Phone"]?.ToString(),
        //                        PersonalEmail = reader["PersonalEmail"]?.ToString(),
        //                        CompanyEmail = reader["CompanyEmail"]?.ToString(),
        //                        Email = reader["Email"]?.ToString(),
        //                        CurrentAddress = reader["CurrentAddress"]?.ToString(),
        //                        PermanentAddress = reader["PermanentAddress"]?.ToString(),
        //                        DateOfBirth = reader["DateOfBirth"] != DBNull.Value ? Convert.ToDateTime(reader["DateOfBirth"]) : (DateTime?)null,
        //                        DateOfJoining = reader["DateOfJoining"] != DBNull.Value ? Convert.ToDateTime(reader["DateOfJoining"]) : (DateTime?)null,
        //                        CTC = reader["CTC"] != DBNull.Value ? Convert.ToDecimal(reader["CTC"]) : (decimal?)null
        //                    });
        //                }
        //            }
        //        }
        //    }
        //    if (!string.IsNullOrEmpty(company))
        //    {
        //        list = list.Where(e => e.Company == company).ToList();
        //    }
        //    return list;
        //}




        public List<Employee> GetAllEmployees(string company = null)
        {

            EnsureEmployeeExtraColumns();
            var list = new List<Employee>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Employees WHERE IsDeleted = 0";
                if (!string.IsNullOrEmpty(company))
                {
                    query += " AND Company = @Company";
                }

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(company))
                    {
                        cmd.Parameters.AddWithValue("@Company", company);
                    }

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(MapEmployee(reader));
                        }
                    }
                }
            }
            return list;
        }






        // 2. Get Employees for Leave Balance Summary Report (3 Arguments)
        public List<Employee> GetEmployeesForReport(string department, string employee, string status)
        {
            EnsureEmployeeExtraColumns();

            var list = new List<Employee>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Employees WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(department))
                    query += " AND Department = @Department";

                if (!string.IsNullOrEmpty(employee))
                    query += " AND (EmployeeCode = @Employee OR FullName LIKE @EmployeeName)";

                if (!string.IsNullOrEmpty(status))
                    query += " AND Status = @Status";

                query += " ORDER BY FullName ASC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(department)) cmd.Parameters.AddWithValue("@Department", department);
                    if (!string.IsNullOrEmpty(employee))
                    {
                        cmd.Parameters.AddWithValue("@Employee", employee);
                        cmd.Parameters.AddWithValue("@EmployeeName", $"%{employee}%");
                    }
                    if (!string.IsNullOrEmpty(status)) cmd.Parameters.AddWithValue("@Status", status);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(MapEmployee(reader));
                        }
                    }
                }
            }
            return list;
        }

        // Overload for 2 arguments if called elsewhere
        public List<Employee> GetEmployeesForReport(string department = null, string status = null)
        {
            return GetEmployeesForReport(department, null, status);
        }

        // 3. Search Employees for Shift Tool (4 Arguments)
        public List<Employee> SearchForShiftTool(string department, string designation, string employmentType, string grade)
        {

            EnsureEmployeeExtraColumns();

            var list = new List<Employee>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Employees WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(department))
                    query += " AND Department = @Department";

                if (!string.IsNullOrEmpty(designation))
                    query += " AND Designation = @Designation";

                if (!string.IsNullOrEmpty(employmentType))
                    query += " AND EmploymentType = @EmploymentType";

                if (!string.IsNullOrEmpty(grade))
                    query += " AND Grade = @Grade";

                query += " ORDER BY FullName ASC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(department)) cmd.Parameters.AddWithValue("@Department", department);
                    if (!string.IsNullOrEmpty(designation)) cmd.Parameters.AddWithValue("@Designation", designation);
                    if (!string.IsNullOrEmpty(employmentType)) cmd.Parameters.AddWithValue("@EmploymentType", employmentType);
                    if (!string.IsNullOrEmpty(grade)) cmd.Parameters.AddWithValue("@Grade", grade);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(MapEmployee(reader));
                        }
                    }
                }
            }
            return list;
        }

        // 4. Get Existing ERPNext IDs to avoid sync duplicates
        public HashSet<string> GetExistingErpNextIds()
        {
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT ErpNextId FROM Employees WHERE ErpNextId IS NOT NULL AND ErpNextId != '' UNION SELECT EmployeeCode FROM Employees WHERE EmployeeCode IS NOT NULL AND EmployeeCode != ''";
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ids.Add(reader[0].ToString());
                        }
                    }
                }
            }
            return ids;
        }

        public bool ExistsByErpNextIdOrCode(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;

            using (var conn = new SqlConnection(_connectionString))
            {
                const string query = @"
                    SELECT COUNT(1)
                    FROM Employees
                    WHERE ErpNextId = @Value OR EmployeeCode = @Value";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Value", value);
                    conn.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        public Employee GetByErpNextIdOrCode(string value)
        {
            EnsureEmployeeExtraColumns();
            if (string.IsNullOrWhiteSpace(value)) return null;

            using (var conn = new SqlConnection(_connectionString))
            {
                const string query = @"
                    SELECT *
                    FROM Employees
                    WHERE ErpNextId = @Value OR EmployeeCode = @Value";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Value", value);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapEmployee(reader);
                        }
                    }
                }
            }

            return null;
        }

        public int UpsertFromErpNext(Employee emp)
        {
            var existing = GetByErpNextIdOrCode(emp.ErpNextId ?? emp.EmployeeCode);

            if (existing == null)
            {
                emp.IsDeleted = false;
                return InsertEmployee(emp);
            }

            emp.Id = existing.Id;
            emp.ErpNextId = string.IsNullOrWhiteSpace(emp.ErpNextId) ? existing.ErpNextId : emp.ErpNextId;
            emp.EmployeeCode = string.IsNullOrWhiteSpace(emp.EmployeeCode) ? existing.EmployeeCode : emp.EmployeeCode;
            emp.IsDeleted = false;

            UpdateEmployeeFull(emp);

            return emp.Id;
        }

        public int MarkMissingErpNextEmployeesAsDeleted(HashSet<string> activeErpNextIds)
        {
            if (activeErpNextIds == null || activeErpNextIds.Count == 0) return 0;

            var idsToDelete = new List<int>();

            using (var conn = new SqlConnection(_connectionString))
            {
                const string selectQuery = @"
                    SELECT Id, ErpNextId
                    FROM Employees
                    WHERE IsDeleted = 0
                      AND ErpNextId IS NOT NULL
                      AND ErpNextId != ''";

                using (var cmd = new SqlCommand(selectQuery, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var erpNextId = reader["ErpNextId"]?.ToString();
                            if (!string.IsNullOrWhiteSpace(erpNextId) && !activeErpNextIds.Contains(erpNextId))
                            {
                                idsToDelete.Add(Convert.ToInt32(reader["Id"]));
                            }
                        }
                    }
                }
            }

            foreach (var id in idsToDelete)
            {
                DeleteEmployee(id);
            }

            return idsToDelete.Count;
        }

        public void DeleteEmployeeByErpNextId(string erpNextId)
        {
            if (string.IsNullOrWhiteSpace(erpNextId)) return;

            using (var conn = new SqlConnection(_connectionString))
            {
                const string query = @"
                    UPDATE Employees
                    SET IsDeleted = 1, Status = 'Inactive'
                    WHERE ErpNextId = @ErpNextId OR EmployeeCode = @ErpNextId";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", erpNextId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }




        public System.Data.DataTable GetAllEmployeesTable()
        {
            var dt = new System.Data.DataTable();
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Employees";
                using (var cmd = new SqlCommand(query, conn))
                {
                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        conn.Open();
                        adapter.Fill(dt);
                    }
                }
            }
            return dt;
        }










        public Employee GetById(int id)
        {
            EnsureEmployeeExtraColumns();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Employees WHERE Id = @Id AND IsDeleted = 0";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var employee = MapEmployee(reader);
                            LoadEmployeeChildRows(employee);
                            return employee;
                        }
                    }
                }
            }
            return null;
        }
        // 6. Generate Next Employee Code
        public string GetNextEmployeeCode()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
            SELECT ISNULL(MAX(TRY_CAST(RIGHT(EmployeeCode, 5) AS INT)), 0)
            FROM Employees
            WHERE EmployeeCode LIKE 'HR-EMP-[0-9][0-9][0-9][0-9][0-9]'";

                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();

                    int maxNo = Convert.ToInt32(cmd.ExecuteScalar());
                    int nextNo = maxNo + 1;

                    return $"HR-EMP-{nextNo:D5}";
                }
            }
        }

        // 7. Insert Employee (Returns New Primary Key Int ID)
        public int InsertEmployee(Employee emp)
        {
            try
            {
                EnsureEmployeeExtraColumns();
            }
            catch (Exception ex)
            {
                throw new Exception("EnsureEmployeeExtraColumns failed: " + ex.Message, ex);
            }

            int employeeId;

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    string query = @"
                INSERT INTO Employees (
                    ErpNextId, EmployeeCode, FullName, Salutation, FirstName, MiddleName, LastName, Gender, DateOfBirth, DateOfJoining, Status,
                    Company, Department, Designation, Grade, EmploymentType, ReportsTo, EmployeeNumber, Branch, UserId,
                    JobApplicant, ConfirmationDate, NoticeDays, OfferDate, ContractEndDate, DateOfRetirement,
                    Mobile, PersonalEmail, CompanyEmail, PreferredContactEmail, PreferredEmail, Unsubscribed, CurrentAddress, PermanentAddress, CurrentAddressIs, PermanentAddressIs, EmergencyContactName, EmergencyPhone, Relation,
                    AttendanceDeviceId, HolidayList, DefaultShift, ExpenseApprover, ShiftRequestApprover, LeaveApprover,
                    CTC, SalaryCurrency, PayrollCostCenter, SalaryMode, PanNumber, ProvidentFundAccount, BankName, BankAccountNo, IFSCCode, MICRCode, IBAN,
                    MaritalStatus, BloodGroup, FamilyBackground, HealthDetails, HealthInsuranceProvider, HealthInsuranceNo, PassportNumber, PassportDateOfIssue, PassportValidUpto, PassportPlaceOfIssue,
                    Bio, ResignationLetterDate, ExitInterviewHeldOn, LeaveEncashed, EncashmentDate, RelievingDate, NewWorkplace, ReasonForLeaving, Feedback, Email, Phone, IsDeleted
                )
                OUTPUT INSERTED.Id
                VALUES (
                    @ErpNextId, @EmployeeCode, @FullName, @Salutation, @FirstName, @MiddleName, @LastName, @Gender, @DateOfBirth, @DateOfJoining, @Status,
                    @Company, @Department, @Designation, @Grade, @EmploymentType, @ReportsTo, @EmployeeNumber, @Branch, @UserId,
                    @JobApplicant, @ConfirmationDate, @NoticeDays, @OfferDate, @ContractEndDate, @DateOfRetirement,
                    @Mobile, @PersonalEmail, @CompanyEmail, @PreferredContactEmail, @PreferredEmail, @Unsubscribed, @CurrentAddress, @PermanentAddress, @CurrentAddressIs, @PermanentAddressIs, @EmergencyContactName, @EmergencyPhone, @Relation,
                    @AttendanceDeviceId, @HolidayList, @DefaultShift, @ExpenseApprover, @ShiftRequestApprover, @LeaveApprover,
                    @CTC, @SalaryCurrency, @PayrollCostCenter, @SalaryMode, @PanNumber, @ProvidentFundAccount, @BankName, @BankAccountNo, @IFSCCode, @MICRCode, @IBAN,
                    @MaritalStatus, @BloodGroup, @FamilyBackground, @HealthDetails, @HealthInsuranceProvider, @HealthInsuranceNo, @PassportNumber, @PassportDateOfIssue, @PassportValidUpto, @PassportPlaceOfIssue,
                    @Bio, @ResignationLetterDate, @ExitInterviewHeldOn, @LeaveEncashed, @EncashmentDate, @RelievingDate, @NewWorkplace, @ReasonForLeaving, @Feedback, @Email, @Phone, 0
                )";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        AddEmployeeParameters(cmd, emp);
                        conn.Open();
                        employeeId = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Main Employees INSERT failed: " + ex.Message, ex);
            }

            try
            {
                ReplaceEmployeeChildRows(employeeId, emp);
            }
            catch (Exception ex)
            {
                throw new Exception("Employee child tables save failed: " + ex.Message, ex);
            }

            return employeeId;
        }

        // 8. Update Employee
        public void UpdateEmployee(Employee emp)
        {
            UpdateEmployeeFull(emp);
        }

        public void UpdateEmployeeFull(Employee emp)
        {

            EnsureEmployeeExtraColumns();

            using (var conn = new SqlConnection(_connectionString))
            {
                //string query = @"
                    //UPDATE Employees SET
                    //    ErpNextId = @ErpNextId,
                    //    EmployeeCode = @EmployeeCode,
                    //    FullName = @FullName, Salutation = @Salutation, FirstName = @FirstName, MiddleName = @MiddleName, LastName = @LastName,
                    //    Gender = @Gender, DateOfBirth = @DateOfBirth, DateOfJoining = @DateOfJoining, Status = @Status, Company = @Company,
                    //    Department = @Department, Designation = @Designation, Grade = @Grade, EmploymentType = @EmploymentType, ReportsTo = @ReportsTo,
                    //    EmployeeNumber = @EmployeeNumber, Branch = @Branch, UserId = @UserId,
                    //    JobApplicant = @JobApplicant, ConfirmationDate = @ConfirmationDate, NoticeDays = @NoticeDays, OfferDate = @OfferDate,
                    //    ContractEndDate = @ContractEndDate, DateOfRetirement = @DateOfRetirement,
                    //    Mobile = @Mobile, PersonalEmail = @PersonalEmail, CompanyEmail = @CompanyEmail, PreferredContactEmail = @PreferredContactEmail,
                    //    PreferredEmail = @PreferredEmail, Unsubscribed = @Unsubscribed, CurrentAddress = @CurrentAddress, PermanentAddress = @PermanentAddress,
                    //    CurrentAddressIs = @CurrentAddressIs, PermanentAddressIs = @PermanentAddressIs,
                    //    EmergencyContactName = @EmergencyContactName, EmergencyPhone = @EmergencyPhone, Relation = @Relation,
                    //    AttendanceDeviceId = @AttendanceDeviceId, HolidayList = @HolidayList, DefaultShift = @DefaultShift,
                    //    ExpenseApprover = @ExpenseApprover, ShiftRequestApprover = @ShiftRequestApprover, LeaveApprover = @LeaveApprover,
                    //    CTC = @CTC, SalaryCurrency = @SalaryCurrency, PayrollCostCenter = @PayrollCostCenter, SalaryMode = @SalaryMode,
                    //    BankName = @BankName, BankAccountNo = @BankAccountNo, IFSCCode = @IFSCCode,
                    //    MaritalStatus = @MaritalStatus, BloodGroup = @BloodGroup, FamilyBackground = @FamilyBackground,
                    //    HealthDetails = @HealthDetails, HealthInsuranceProvider = @HealthInsuranceProvider,
                    //    PassportNumber = @PassportNumber, PassportDateOfIssue = @PassportDateOfIssue,
                    //    PassportValidUpto = @PassportValidUpto, PassportPlaceOfIssue = @PassportPlaceOfIssue,
                    //    Bio = @Bio, ResignationLetterDate = @ResignationLetterDate, ExitInterviewHeldOn = @ExitInterviewHeldOn,
                    //    LeaveEncashed = @LeaveEncashed, RelievingDate = @RelievingDate, NewWorkplace = @NewWorkplace,
                    //    ReasonForLeaving = @ReasonForLeaving, Feedback = @Feedback, Email = @Email, Phone = @Phone, IsDeleted = 0
                    //WHERE Id = @Id";
                string query = @"
UPDATE Employees SET
    ErpNextId = @ErpNextId,
    EmployeeCode = @EmployeeCode,
    FullName = @FullName, Salutation = @Salutation, FirstName = @FirstName, MiddleName = @MiddleName, LastName = @LastName,
    Gender = @Gender, DateOfBirth = @DateOfBirth, DateOfJoining = @DateOfJoining, Status = @Status, Company = @Company,
    Department = @Department, Designation = @Designation, Grade = @Grade, EmploymentType = @EmploymentType, ReportsTo = @ReportsTo,
    EmployeeNumber = @EmployeeNumber, Branch = @Branch, UserId = @UserId,
    JobApplicant = @JobApplicant, ConfirmationDate = @ConfirmationDate, NoticeDays = @NoticeDays, OfferDate = @OfferDate,
    ContractEndDate = @ContractEndDate, DateOfRetirement = @DateOfRetirement,
    Mobile = @Mobile, PersonalEmail = @PersonalEmail, CompanyEmail = @CompanyEmail, PreferredContactEmail = @PreferredContactEmail,
    PreferredEmail = @PreferredEmail, Unsubscribed = @Unsubscribed, CurrentAddress = @CurrentAddress, PermanentAddress = @PermanentAddress,
    CurrentAddressIs = @CurrentAddressIs, PermanentAddressIs = @PermanentAddressIs,
    EmergencyContactName = @EmergencyContactName, EmergencyPhone = @EmergencyPhone, Relation = @Relation,
    AttendanceDeviceId = @AttendanceDeviceId, HolidayList = @HolidayList, DefaultShift = @DefaultShift,
    ExpenseApprover = @ExpenseApprover, ShiftRequestApprover = @ShiftRequestApprover, LeaveApprover = @LeaveApprover,
    CTC = @CTC, SalaryCurrency = @SalaryCurrency, PayrollCostCenter = @PayrollCostCenter, SalaryMode = @SalaryMode,
    PanNumber = @PanNumber, ProvidentFundAccount = @ProvidentFundAccount,
    BankName = @BankName, BankAccountNo = @BankAccountNo, IFSCCode = @IFSCCode, MICRCode = @MICRCode, IBAN = @IBAN,
    MaritalStatus = @MaritalStatus, BloodGroup = @BloodGroup, FamilyBackground = @FamilyBackground,
    HealthDetails = @HealthDetails, HealthInsuranceProvider = @HealthInsuranceProvider, HealthInsuranceNo = @HealthInsuranceNo,
    PassportNumber = @PassportNumber, PassportDateOfIssue = @PassportDateOfIssue,
    PassportValidUpto = @PassportValidUpto, PassportPlaceOfIssue = @PassportPlaceOfIssue,
    Bio = @Bio, ResignationLetterDate = @ResignationLetterDate, ExitInterviewHeldOn = @ExitInterviewHeldOn,
    LeaveEncashed = @LeaveEncashed, EncashmentDate = @EncashmentDate, RelievingDate = @RelievingDate, NewWorkplace = @NewWorkplace,
    ReasonForLeaving = @ReasonForLeaving, Feedback = @Feedback, Email = @Email, Phone = @Phone, IsDeleted = 0
WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", emp.Id);
                    AddEmployeeParameters(cmd, emp);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            ReplaceEmployeeChildRows(emp.Id, emp);
        }

        // 9. Update Synced ERPNext ID
        public void UpdateErpNextId(int id, string erpNextId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE Employees SET ErpNextId = @ErpNextId, EmployeeCode = @ErpNextId WHERE Id = @Id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@ErpNextId", erpNextId ?? (object)DBNull.Value);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // 10. Soft Delete Employee
        public void DeleteEmployee(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE Employees SET IsDeleted = 1, Status = 'Inactive' WHERE Id = @Id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }



        public void LoadEmployeeChildRows(Employee emp)
        {
            if (emp == null || emp.Id <= 0) return;

            EnsureEmployeeChildTables();

            emp.EducationRows = new List<EmployeeEducationRow>();
            emp.ExternalWorkHistoryRows = new List<EmployeeExternalWorkHistoryRow>();
            emp.InternalWorkHistoryRows = new List<EmployeeInternalWorkHistoryRow>();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand(@"
            SELECT SchoolUniversity, Qualification, Level, YearOfPassing, ClassPercentage, MajorOptionalSubjects
            FROM EmployeeEducations
            WHERE EmployeeId = @EmployeeId
            ORDER BY Id", conn))
                {
                    cmd.Parameters.AddWithValue("@EmployeeId", emp.Id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            emp.EducationRows.Add(new EmployeeEducationRow
                            {
                                SchoolUniversity = ReadChildString(reader, "SchoolUniversity"),
                                Qualification = ReadChildString(reader, "Qualification"),
                                Level = ReadChildString(reader, "Level"),
                                YearOfPassing = ReadChildInt(reader, "YearOfPassing"),
                                ClassPercentage = ReadChildString(reader, "ClassPercentage"),
                                MajorOptionalSubjects = ReadChildString(reader, "MajorOptionalSubjects")
                            });
                        }
                    }
                }

                using (var cmd = new SqlCommand(@"
            SELECT CompanyName, Designation, Salary, Address, Contact, TotalExperience
            FROM EmployeeExternalWorkHistories
            WHERE EmployeeId = @EmployeeId
            ORDER BY Id", conn))
                {
                    cmd.Parameters.AddWithValue("@EmployeeId", emp.Id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            emp.ExternalWorkHistoryRows.Add(new EmployeeExternalWorkHistoryRow
                            {
                                CompanyName = ReadChildString(reader, "CompanyName"),
                                Designation = ReadChildString(reader, "Designation"),
                                Salary = ReadChildDecimal(reader, "Salary"),
                                Address = ReadChildString(reader, "Address"),
                                Contact = ReadChildString(reader, "Contact"),
                                TotalExperience = ReadChildString(reader, "TotalExperience")
                            });
                        }
                    }
                }

                using (var cmd = new SqlCommand(@"
            SELECT Branch, Department, Designation, FromDate, ToDate
            FROM EmployeeInternalWorkHistories
            WHERE EmployeeId = @EmployeeId
            ORDER BY Id", conn))
                {
                    cmd.Parameters.AddWithValue("@EmployeeId", emp.Id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            emp.InternalWorkHistoryRows.Add(new EmployeeInternalWorkHistoryRow
                            {
                                Branch = ReadChildString(reader, "Branch"),
                                Department = ReadChildString(reader, "Department"),
                                Designation = ReadChildString(reader, "Designation"),
                                FromDate = ReadChildDate(reader, "FromDate"),
                                ToDate = ReadChildDate(reader, "ToDate")
                            });
                        }
                    }
                }
            }
        }

        private void EnsureEmployeeChildTables()
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(@"
        IF OBJECT_ID('dbo.EmployeeEducations', 'U') IS NULL
        BEGIN
            CREATE TABLE dbo.EmployeeEducations (
                Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                EmployeeId INT NOT NULL,
                SchoolUniversity NVARCHAR(500) NULL,
                Qualification NVARCHAR(200) NULL,
                Level NVARCHAR(100) NULL,
                YearOfPassing INT NULL,
                ClassPercentage NVARCHAR(100) NULL,
                MajorOptionalSubjects NVARCHAR(MAX) NULL
            );
        END;

        IF OBJECT_ID('dbo.EmployeeExternalWorkHistories', 'U') IS NULL
        BEGIN
            CREATE TABLE dbo.EmployeeExternalWorkHistories (
                Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                EmployeeId INT NOT NULL,
                CompanyName NVARCHAR(300) NULL,
                Designation NVARCHAR(200) NULL,
                Salary DECIMAL(18,2) NULL,
                Address NVARCHAR(MAX) NULL,
                Contact NVARCHAR(100) NULL,
                TotalExperience NVARCHAR(100) NULL
            );
        END;

        IF OBJECT_ID('dbo.EmployeeInternalWorkHistories', 'U') IS NULL
        BEGIN
            CREATE TABLE dbo.EmployeeInternalWorkHistories (
                Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                EmployeeId INT NOT NULL,
                Branch NVARCHAR(200) NULL,
                Department NVARCHAR(200) NULL,
                Designation NVARCHAR(200) NULL,
                FromDate DATE NULL,
                ToDate DATE NULL
            );
        END;", conn))
            {
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void EnsureEmployeeExtraColumns()
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(@"
        IF COL_LENGTH('dbo.Employees', 'PreferredEmail') IS NULL
            ALTER TABLE dbo.Employees ADD PreferredEmail NVARCHAR(255) NULL;
        IF COL_LENGTH('dbo.Employees', 'PanNumber') IS NULL
            ALTER TABLE dbo.Employees ADD PanNumber NVARCHAR(50) NULL;
        IF COL_LENGTH('dbo.Employees', 'ProvidentFundAccount') IS NULL
            ALTER TABLE dbo.Employees ADD ProvidentFundAccount NVARCHAR(100) NULL;
        IF COL_LENGTH('dbo.Employees', 'MICRCode') IS NULL
            ALTER TABLE dbo.Employees ADD MICRCode NVARCHAR(50) NULL;
        IF COL_LENGTH('dbo.Employees', 'IBAN') IS NULL
            ALTER TABLE dbo.Employees ADD IBAN NVARCHAR(100) NULL;
        IF COL_LENGTH('dbo.Employees', 'HealthInsuranceNo') IS NULL
            ALTER TABLE dbo.Employees ADD HealthInsuranceNo NVARCHAR(100) NULL;
        IF COL_LENGTH('dbo.Employees', 'EncashmentDate') IS NULL
            ALTER TABLE dbo.Employees ADD EncashmentDate DATE NULL;", conn))
            {
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private static string ReadChildString(SqlDataReader reader, string field)
        {
            return reader[field] == DBNull.Value ? null : reader[field].ToString();
        }

        private static int? ReadChildInt(SqlDataReader reader, string field)
        {
            return reader[field] == DBNull.Value ? (int?)null : Convert.ToInt32(reader[field]);
        }
        private static decimal? ReadChildDecimal(SqlDataReader reader, string field)
        {
            return reader[field] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader[field]);
        }

        private static DateTime? ReadChildDate(SqlDataReader reader, string field)
        {
            return reader[field] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader[field]);
        }







        private void ReplaceEmployeeChildRows(int employeeId, Employee emp)
        {
            if (employeeId <= 0 || emp == null) return;

            EnsureEmployeeChildTables();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand(@"
            DELETE FROM EmployeeEducations WHERE EmployeeId = @EmployeeId;
            DELETE FROM EmployeeExternalWorkHistories WHERE EmployeeId = @EmployeeId;
            DELETE FROM EmployeeInternalWorkHistories WHERE EmployeeId = @EmployeeId;", conn))
                {
                    cmd.Parameters.AddWithValue("@EmployeeId", employeeId);
                    cmd.ExecuteNonQuery();
                }

                foreach (var row in emp.EducationRows ?? new List<EmployeeEducationRow>())
                {
                    if (!HasEducationData(row)) continue;

                    using (var cmd = new SqlCommand(@"
                INSERT INTO EmployeeEducations
                    (EmployeeId, SchoolUniversity, Qualification, Level, YearOfPassing, ClassPercentage, MajorOptionalSubjects)
                VALUES
                    (@EmployeeId, @SchoolUniversity, @Qualification, @Level, @YearOfPassing, @ClassPercentage, @MajorOptionalSubjects)", conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeeId", employeeId);
                        cmd.Parameters.AddWithValue("@SchoolUniversity", (object)row.SchoolUniversity ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Qualification", (object)row.Qualification ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Level", (object)row.Level ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@YearOfPassing", (object)row.YearOfPassing ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ClassPercentage", (object)row.ClassPercentage ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@MajorOptionalSubjects", (object)row.MajorOptionalSubjects ?? DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
                }

                foreach (var row in emp.ExternalWorkHistoryRows ?? new List<EmployeeExternalWorkHistoryRow>())
                {
                    if (!HasExternalWorkHistoryData(row)) continue;

                    using (var cmd = new SqlCommand(@"
                INSERT INTO EmployeeExternalWorkHistories
                    (EmployeeId, CompanyName, Designation, Salary, Address, Contact, TotalExperience)
                VALUES
                    (@EmployeeId, @CompanyName, @Designation, @Salary, @Address, @Contact, @TotalExperience)", conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeeId", employeeId);
                        cmd.Parameters.AddWithValue("@CompanyName", (object)row.CompanyName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Designation", (object)row.Designation ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Salary", (object)row.Salary ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Address", (object)row.Address ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Contact", (object)row.Contact ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@TotalExperience", (object)row.TotalExperience ?? DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
                }

                foreach (var row in emp.InternalWorkHistoryRows ?? new List<EmployeeInternalWorkHistoryRow>())
                {
                    if (!HasInternalWorkHistoryData(row)) continue;

                    using (var cmd = new SqlCommand(@"
                INSERT INTO EmployeeInternalWorkHistories
                    (EmployeeId, Branch, Department, Designation, FromDate, ToDate)
                VALUES
                    (@EmployeeId, @Branch, @Department, @Designation, @FromDate, @ToDate)", conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeeId", employeeId);
                        cmd.Parameters.AddWithValue("@Branch", (object)row.Branch ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Department", (object)row.Department ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Designation", (object)row.Designation ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@FromDate", (object)row.FromDate ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ToDate", (object)row.ToDate ?? DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private static bool HasEducationData(EmployeeEducationRow row)
        {
            return row != null
                && (!string.IsNullOrWhiteSpace(row.SchoolUniversity)
                    || !string.IsNullOrWhiteSpace(row.Qualification)
                    || !string.IsNullOrWhiteSpace(row.Level)
                    || row.YearOfPassing.HasValue
                    || !string.IsNullOrWhiteSpace(row.ClassPercentage)
                    || !string.IsNullOrWhiteSpace(row.MajorOptionalSubjects));
        }

        private static bool HasExternalWorkHistoryData(EmployeeExternalWorkHistoryRow row)
        {
            return row != null
                && (!string.IsNullOrWhiteSpace(row.CompanyName)
                    || !string.IsNullOrWhiteSpace(row.Designation)
                    || row.Salary.HasValue
                    || !string.IsNullOrWhiteSpace(row.Address)
                    || !string.IsNullOrWhiteSpace(row.Contact)
                    || !string.IsNullOrWhiteSpace(row.TotalExperience));
        }

        private static bool HasInternalWorkHistoryData(EmployeeInternalWorkHistoryRow row)
        {
            return row != null
                && (!string.IsNullOrWhiteSpace(row.Branch)
                    || !string.IsNullOrWhiteSpace(row.Department)
                    || !string.IsNullOrWhiteSpace(row.Designation)
                    || row.FromDate.HasValue
                    || row.ToDate.HasValue);
        }




















        // Helper Method to Bind SQL Parameters
        //private void AddEmployeeParameters(SqlCommand cmd, Employee emp)
        //{
        //    cmd.Parameters.AddWithValue("@ErpNextId", (object)emp.ErpNextId ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@EmployeeCode", (object)emp.EmployeeCode ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@FullName", (object)emp.FullName ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@Salutation", (object)emp.Salutation ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@FirstName", (object)emp.FirstName ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@MiddleName", (object)emp.MiddleName ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@LastName", (object)emp.LastName ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@Gender", (object)emp.Gender ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@DateOfBirth", (object)emp.DateOfBirth ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@DateOfJoining", (object)emp.DateOfJoining ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@Status", (object)emp.Status ?? "Active");
        //    cmd.Parameters.AddWithValue("@Company", (object)emp.Company ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@Department", (object)emp.Department ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@Designation", (object)emp.Designation ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@Grade", (object)emp.Grade ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@EmploymentType", (object)emp.EmploymentType ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@ReportsTo", (object)emp.ReportsTo ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@EmployeeNumber", (object)emp.EmployeeNumber ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@Branch", (object)emp.Branch ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@UserId", (object)emp.UserId ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@JobApplicant", (object)emp.JobApplicant ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@ConfirmationDate", (object)emp.ConfirmationDate ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@NoticeDays", (object)emp.NoticeDays ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@OfferDate", (object)emp.OfferDate ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@ContractEndDate", (object)emp.ContractEndDate ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@DateOfRetirement", (object)emp.DateOfRetirement ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@Mobile", (object)emp.Mobile ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@PersonalEmail", (object)emp.PersonalEmail ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@CompanyEmail", (object)emp.CompanyEmail ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@PreferredContactEmail", (object)emp.PreferredContactEmail ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@Unsubscribed", emp.Unsubscribed);
        //    cmd.Parameters.AddWithValue("@CurrentAddress", (object)emp.CurrentAddress ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@PermanentAddress", (object)emp.PermanentAddress ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@CurrentAddressIs", (object)emp.CurrentAddressIs ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@PermanentAddressIs", (object)emp.PermanentAddressIs ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@EmergencyContactName", (object)emp.EmergencyContactName ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@EmergencyPhone", (object)emp.EmergencyPhone ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@Relation", (object)emp.Relation ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@AttendanceDeviceId", (object)emp.AttendanceDeviceId ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@HolidayList", (object)emp.HolidayList ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@DefaultShift", (object)emp.DefaultShift ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@ExpenseApprover", (object)emp.ExpenseApprover ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@ShiftRequestApprover", (object)emp.ShiftRequestApprover ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@LeaveApprover", (object)emp.LeaveApprover ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@CTC", (object)emp.CTC ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@SalaryCurrency", (object)emp.SalaryCurrency ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@PayrollCostCenter", (object)emp.PayrollCostCenter ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@SalaryMode", (object)emp.SalaryMode ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@BankName", (object)emp.BankName ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@BankAccountNo", (object)emp.BankAccountNo ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@IFSCCode", (object)emp.IFSCCode ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@MaritalStatus", (object)emp.MaritalStatus ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@BloodGroup", (object)emp.BloodGroup ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@FamilyBackground", (object)emp.FamilyBackground ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@HealthDetails", (object)emp.HealthDetails ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@HealthInsuranceProvider", (object)emp.HealthInsuranceProvider ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@PassportNumber", (object)emp.PassportNumber ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@PassportDateOfIssue", (object)emp.PassportDateOfIssue ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@PassportValidUpto", (object)emp.PassportValidUpto ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@PassportPlaceOfIssue", (object)emp.PassportPlaceOfIssue ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@Bio", (object)emp.Bio ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@ResignationLetterDate", (object)emp.ResignationLetterDate ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@ExitInterviewHeldOn", (object)emp.ExitInterviewHeldOn ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@LeaveEncashed", (object)emp.LeaveEncashed ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@RelievingDate", (object)emp.RelievingDate ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@NewWorkplace", (object)emp.NewWorkplace ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@ReasonForLeaving", (object)emp.ReasonForLeaving ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@Feedback", (object)emp.Feedback ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@Email", (object)emp.CompanyEmail ?? (object)emp.PersonalEmail ?? DBNull.Value);
        //    cmd.Parameters.AddWithValue("@Phone", (object)emp.Mobile ?? DBNull.Value);
        //}



        private void AddEmployeeParameters(SqlCommand cmd, Employee emp)
        {
            // Auto-calculate FullName if it was not provided explicitly
            string constructedFullName = !string.IsNullOrWhiteSpace(emp.FullName)
                ? emp.FullName
                : $"{emp.FirstName} {emp.MiddleName} {emp.LastName}".Replace("  ", " ").Trim();

            if (string.IsNullOrWhiteSpace(constructedFullName))
            {
                constructedFullName = emp.FirstName ?? "Unknown";
            }

            cmd.Parameters.AddWithValue("@ErpNextId", (object)emp.ErpNextId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EmployeeCode", (object)emp.EmployeeCode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FullName", constructedFullName); // Never NULL!
            cmd.Parameters.AddWithValue("@Salutation", (object)emp.Salutation ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FirstName", (object)emp.FirstName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MiddleName", (object)emp.MiddleName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LastName", (object)emp.LastName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Gender", (object)emp.Gender ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DateOfBirth", (object)emp.DateOfBirth ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DateOfJoining", (object)emp.DateOfJoining ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", (object)emp.Status ?? "Active");
            cmd.Parameters.AddWithValue("@Company", (object)emp.Company ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Department", (object)emp.Department ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Designation", (object)emp.Designation ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Grade", (object)emp.Grade ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EmploymentType", (object)emp.EmploymentType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ReportsTo", (object)emp.ReportsTo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EmployeeNumber", (object)emp.EmployeeNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Branch", (object)emp.Branch ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UserId", (object)emp.UserId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@JobApplicant", (object)emp.JobApplicant ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ConfirmationDate", (object)emp.ConfirmationDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NoticeDays", (object)emp.NoticeDays ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@OfferDate", (object)emp.OfferDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ContractEndDate", (object)emp.ContractEndDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DateOfRetirement", (object)emp.DateOfRetirement ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Mobile", (object)emp.Mobile ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PersonalEmail", (object)emp.PersonalEmail ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CompanyEmail", (object)emp.CompanyEmail ?? DBNull.Value);
            //cmd.Parameters.AddWithValue("@PreferredContactEmail", (object)emp.PreferredContactEmail ?? DBNull.Value);
            //cmd.Parameters.AddWithValue("@Unsubscribed", emp.Unsubscribed);
            cmd.Parameters.AddWithValue("@PreferredContactEmail", (object)emp.PreferredContactEmail ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PreferredEmail", (object)emp.PreferredEmail ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Unsubscribed", emp.Unsubscribed);
            cmd.Parameters.AddWithValue("@CurrentAddress", (object)emp.CurrentAddress ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PermanentAddress", (object)emp.PermanentAddress ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CurrentAddressIs", (object)emp.CurrentAddressIs ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PermanentAddressIs", (object)emp.PermanentAddressIs ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EmergencyContactName", (object)emp.EmergencyContactName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EmergencyPhone", (object)emp.EmergencyPhone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Relation", (object)emp.Relation ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AttendanceDeviceId", (object)emp.AttendanceDeviceId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@HolidayList", (object)emp.HolidayList ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DefaultShift", (object)emp.DefaultShift ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ExpenseApprover", (object)emp.ExpenseApprover ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ShiftRequestApprover", (object)emp.ShiftRequestApprover ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LeaveApprover", (object)emp.LeaveApprover ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CTC", (object)emp.CTC ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SalaryCurrency", (object)emp.SalaryCurrency ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PayrollCostCenter", (object)emp.PayrollCostCenter ?? DBNull.Value);
            //cmd.Parameters.AddWithValue("@SalaryMode", (object)emp.SalaryMode ?? DBNull.Value);
            //cmd.Parameters.AddWithValue("@BankName", (object)emp.BankName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SalaryMode", (object)emp.SalaryMode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PanNumber", (object)emp.PanNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ProvidentFundAccount", (object)emp.ProvidentFundAccount ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@BankName", (object)emp.BankName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@BankAccountNo", (object)emp.BankAccountNo ?? DBNull.Value);
            //cmd.Parameters.AddWithValue("@IFSCCode", (object)emp.IFSCCode ?? DBNull.Value);
            //cmd.Parameters.AddWithValue("@MaritalStatus", (object)emp.MaritalStatus ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IFSCCode", (object)emp.IFSCCode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MICRCode", (object)emp.MICRCode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IBAN", (object)emp.IBAN ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MaritalStatus", (object)emp.MaritalStatus ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@BloodGroup", (object)emp.BloodGroup ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FamilyBackground", (object)emp.FamilyBackground ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@HealthDetails", (object)emp.HealthDetails ?? DBNull.Value);
            //cmd.Parameters.AddWithValue("@HealthInsuranceProvider", (object)emp.HealthInsuranceProvider ?? DBNull.Value);
            //cmd.Parameters.AddWithValue("@PassportNumber", (object)emp.PassportNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@HealthInsuranceProvider", (object)emp.HealthInsuranceProvider ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@HealthInsuranceNo", (object)emp.HealthInsuranceNo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PassportNumber", (object)emp.PassportNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PassportDateOfIssue", (object)emp.PassportDateOfIssue ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PassportValidUpto", (object)emp.PassportValidUpto ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PassportPlaceOfIssue", (object)emp.PassportPlaceOfIssue ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Bio", (object)emp.Bio ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ResignationLetterDate", (object)emp.ResignationLetterDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ExitInterviewHeldOn", (object)emp.ExitInterviewHeldOn ?? DBNull.Value);
            //cmd.Parameters.AddWithValue("@LeaveEncashed", (object)emp.LeaveEncashed ?? DBNull.Value);
            //cmd.Parameters.AddWithValue("@RelievingDate", (object)emp.RelievingDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LeaveEncashed", (object)emp.LeaveEncashed ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EncashmentDate", (object)emp.EncashmentDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RelievingDate", (object)emp.RelievingDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NewWorkplace", (object)emp.NewWorkplace ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ReasonForLeaving", (object)emp.ReasonForLeaving ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Feedback", (object)emp.Feedback ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", (object)emp.CompanyEmail ?? (object)emp.PersonalEmail ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Phone", (object)emp.Mobile ?? DBNull.Value);

        }


        public HashSet<string> GetExistingEmployeeCodes()
        {
            var codes = new HashSet<string>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT EmployeeCode FROM Employees WHERE EmployeeCode IS NOT NULL AND EmployeeCode != ''";
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            codes.Add(reader["EmployeeCode"].ToString());
                        }
                    }
                }
            }
            return codes;
        }

        // Helper Method to Map SqlDataReader to Employee Model
        private Employee MapEmployee(SqlDataReader r)
        {
            string? ReadString(string name) => r[name] == DBNull.Value ? null : r[name].ToString();
            DateTime? ReadDate(string name) => r[name] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r[name]);
            int? ReadInt(string name) => r[name] == DBNull.Value ? (int?)null : Convert.ToInt32(r[name]);
            decimal? ReadDecimal(string name) => r[name] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r[name]);
            bool ReadBool(string name) => r[name] != DBNull.Value && Convert.ToBoolean(r[name]);

            return new Employee
            {
                Id = Convert.ToInt32(r["Id"]),
                IsDeleted = ReadBool("IsDeleted"),
                ErpNextId = ReadString("ErpNextId"),
                EmployeeCode = ReadString("EmployeeCode"),
                FullName = ReadString("FullName"),
                Salutation = ReadString("Salutation"),
                FirstName = ReadString("FirstName"),
                MiddleName = ReadString("MiddleName"),
                LastName = ReadString("LastName"),
                Gender = ReadString("Gender"),
                DateOfBirth = ReadDate("DateOfBirth"),
                DateOfJoining = ReadDate("DateOfJoining"),
                Status = ReadString("Status"),
                Company = ReadString("Company"),
                Department = ReadString("Department"),
                Designation = ReadString("Designation"),
                Grade = ReadString("Grade"),
                EmploymentType = ReadString("EmploymentType"),
                ReportsTo = ReadString("ReportsTo"),
                EmployeeNumber = ReadString("EmployeeNumber"),
                Branch = ReadString("Branch"),
                UserId = ReadString("UserId"),
                JobApplicant = ReadString("JobApplicant"),
                ConfirmationDate = ReadDate("ConfirmationDate"),
                NoticeDays = ReadInt("NoticeDays"),
                OfferDate = ReadDate("OfferDate"),
                ContractEndDate = ReadDate("ContractEndDate"),
                DateOfRetirement = ReadDate("DateOfRetirement"),
                Mobile = ReadString("Mobile"),
                PersonalEmail = ReadString("PersonalEmail"),
                CompanyEmail = ReadString("CompanyEmail"),
                PreferredContactEmail = ReadString("PreferredContactEmail"),
                PreferredEmail = ReadString("PreferredEmail"),
                Unsubscribed = ReadBool("Unsubscribed"),
                CurrentAddress = ReadString("CurrentAddress"),
                PermanentAddress = ReadString("PermanentAddress"),
                CurrentAddressIs = ReadString("CurrentAddressIs"),
                PermanentAddressIs = ReadString("PermanentAddressIs"),
                EmergencyContactName = ReadString("EmergencyContactName"),
                EmergencyPhone = ReadString("EmergencyPhone"),
                Relation = ReadString("Relation"),
                AttendanceDeviceId = ReadString("AttendanceDeviceId"),
                HolidayList = ReadString("HolidayList"),
                DefaultShift = ReadString("DefaultShift"),
                ExpenseApprover = ReadString("ExpenseApprover"),
                ShiftRequestApprover = ReadString("ShiftRequestApprover"),
                LeaveApprover = ReadString("LeaveApprover"),
                CTC = ReadDecimal("CTC"),
                SalaryCurrency = ReadString("SalaryCurrency"),
                PayrollCostCenter = ReadString("PayrollCostCenter"),
                SalaryMode = ReadString("SalaryMode"),
                PanNumber = ReadString("PanNumber"),
                ProvidentFundAccount = ReadString("ProvidentFundAccount"),
                BankName = ReadString("BankName"),
                BankAccountNo = ReadString("BankAccountNo"),
                IFSCCode = ReadString("IFSCCode"),
                MICRCode = ReadString("MICRCode"),
                IBAN = ReadString("IBAN"),
                MaritalStatus = ReadString("MaritalStatus"),
                BloodGroup = ReadString("BloodGroup"),
                FamilyBackground = ReadString("FamilyBackground"),
                HealthDetails = ReadString("HealthDetails"),
                HealthInsuranceProvider = ReadString("HealthInsuranceProvider"),
                HealthInsuranceNo = ReadString("HealthInsuranceNo"),
                PassportNumber = ReadString("PassportNumber"),
                PassportDateOfIssue = ReadDate("PassportDateOfIssue"),
                PassportValidUpto = ReadDate("PassportValidUpto"),
                PassportPlaceOfIssue = ReadString("PassportPlaceOfIssue"),
                Bio = ReadString("Bio"),
                ResignationLetterDate = ReadDate("ResignationLetterDate"),
                ExitInterviewHeldOn = ReadDate("ExitInterviewHeldOn"),
                LeaveEncashed = ReadString("LeaveEncashed"),
                EncashmentDate = ReadDate("EncashmentDate"),
                RelievingDate = ReadDate("RelievingDate"),
                NewWorkplace = ReadString("NewWorkplace"),
                ReasonForLeaving = ReadString("ReasonForLeaving"),
                Feedback = ReadString("Feedback"),
                Email = ReadString("Email"),
                Phone = ReadString("Phone")
            };
        }
    }
}