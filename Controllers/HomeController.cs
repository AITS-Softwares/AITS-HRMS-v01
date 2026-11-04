using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Erp_Aits.Controllers
{
    public class HomeController : Controller
    {
        private static readonly object DashboardCacheLock = new();
        private static HrmDashboardViewModel? _lastLiveDashboard;
        private readonly IConfiguration _configuration;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IConfiguration configuration, ILogger<HomeController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "HRMS Dashboard";
            ViewData["Module"] = "HRMS Overview";
            ViewData["Icon"] = "chart-line";
            try
            {
                var dashboard = BuildDashboard();
                dashboard.IsLive = true;
                dashboard.LastUpdatedUtc = DateTime.UtcNow;
                lock (DashboardCacheLock) _lastLiveDashboard = dashboard;
                return View(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to load HRMS dashboard.");
                lock (DashboardCacheLock)
                {
                    if (_lastLiveDashboard != null)
                    {
                        _lastLiveDashboard.IsLive = false;
                        _lastLiveDashboard.DataMessage = "Live SQL data is temporarily unavailable. Showing the last successfully synced HRMS snapshot.";
                        return View(_lastLiveDashboard);
                    }
                }

                return View(new HrmDashboardViewModel
                {
                    IsLive = false,
                    DataMessage = "Live SQL data is temporarily unavailable. Retry after SQL Server responds."
                });
            }
        }

        private HrmDashboardViewModel BuildDashboard()
        {
            var dashboard = new HrmDashboardViewModel();
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();

            dashboard.TotalEmployees = SafeCount(connection, "SELECT COUNT(*) FROM Employees WITH (NOLOCK) WHERE ISNULL(IsDeleted, 0) = 0");
            dashboard.ActiveEmployees = SafeCount(connection, "SELECT COUNT(*) FROM Employees WITH (NOLOCK) WHERE ISNULL(IsDeleted, 0) = 0 AND ISNULL(Status, '') = 'Active'");
            dashboard.ActiveDepartments = SafeCount(connection, "SELECT COUNT(*) FROM Departments WITH (NOLOCK) WHERE ISNULL(IsDeleted, 0) = 0 AND ISNULL(Disabled, 0) = 0");
            dashboard.ActiveShiftTypes = SafeCount(connection, "SELECT COUNT(*) FROM ShiftTypes WITH (NOLOCK) WHERE ISNULL(IsDeleted, 0) = 0");
            dashboard.PresentToday = SafeCount(connection, "SELECT COUNT(*) FROM Attendances WITH (NOLOCK) WHERE ISNULL(IsDeleted, 0) = 0 AND AttendanceDate = CAST(GETDATE() AS date) AND Status = 'Present'");
            dashboard.AbsentToday = SafeCount(connection, "SELECT COUNT(*) FROM Attendances WITH (NOLOCK) WHERE ISNULL(IsDeleted, 0) = 0 AND AttendanceDate = CAST(GETDATE() AS date) AND Status = 'Absent'");
            dashboard.LateToday = SafeCount(connection, "SELECT COUNT(*) FROM Attendances WITH (NOLOCK) WHERE ISNULL(IsDeleted, 0) = 0 AND AttendanceDate = CAST(GETDATE() AS date) AND ISNULL(LateEntry, 0) = 1");
            dashboard.OpenJobOpenings = SafeCount(connection, "SELECT COUNT(*) FROM JobOpenings WITH (NOLOCK) WHERE ISNULL(IsDeleted, 0) = 0 AND ISNULL(Status, '') NOT IN ('Closed', 'Cancelled')");
            dashboard.OpenApplicants = SafeCount(connection, "SELECT COUNT(*) FROM JobApplicants WITH (NOLOCK) WHERE ISNULL(IsDeleted, 0) = 0 AND ISNULL(Status, 'Open') IN ('Open', 'Pending')");
            dashboard.AcceptedOffers = SafeCount(connection, "SELECT COUNT(*) FROM JobOffers WITH (NOLOCK) WHERE ISNULL(IsDeleted, 0) = 0 AND Status = 'Accepted'");
            dashboard.PendingLeaves = SafeCount(connection, "SELECT COUNT(*) FROM LeaveApplications WITH (NOLOCK) WHERE ISNULL(IsDeleted, 0) = 0 AND ISNULL(Status, '') IN ('Open', 'Pending')");
            dashboard.PendingExpenses = SafeCount(connection, "SELECT COUNT(*) FROM ExpenseClaims WITH (NOLOCK) WHERE ISNULL(IsDeleted, 0) = 0 AND ISNULL(ApprovalStatus, '') IN ('Draft', 'Open', 'Pending')");
            dashboard.AttendanceRate = dashboard.ActiveEmployees > 0 ? Math.Min(100, (int)Math.Round(dashboard.PresentToday * 100.0 / dashboard.ActiveEmployees)) : 0;
            try { dashboard.Departments = ReadDepartments(connection); } catch (Exception ex) { _logger.LogWarning(ex, "Department dashboard data could not load."); }
            try { dashboard.UpcomingLeaves = ReadUpcomingLeaves(connection); } catch (Exception ex) { _logger.LogWarning(ex, "Leave dashboard data could not load."); }
            try { dashboard.Activities = ReadRecentActivities(connection); } catch (Exception ex) { _logger.LogWarning(ex, "Activity dashboard data could not load."); }
            return dashboard;
        }

        private int SafeCount(SqlConnection connection, string sql)
        {
            try
            {
                using var command = new SqlCommand(sql, connection) { CommandTimeout = 12 };
                var value = command.ExecuteScalar();
                return value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "A dashboard metric query could not load.");
                return 0;
            }
        }

        private static List<HrmDepartmentSnapshot> ReadDepartments(SqlConnection connection)
        {
            var results = new List<HrmDepartmentSnapshot>();
            const string sql = @"SELECT TOP (6) ISNULL(NULLIF(Department, ''), 'Unassigned') Department, COUNT(*) EmployeeCount
                                 FROM Employees WITH (NOLOCK) WHERE ISNULL(IsDeleted, 0) = 0
                                 GROUP BY ISNULL(NULLIF(Department, ''), 'Unassigned') ORDER BY COUNT(*) DESC, Department";
            using var command = new SqlCommand(sql, connection) { CommandTimeout = 12 };
            using var reader = command.ExecuteReader();
            while (reader.Read()) results.Add(new HrmDepartmentSnapshot { Name = reader["Department"].ToString() ?? "Unassigned", Count = Convert.ToInt32(reader["EmployeeCount"]) });
            return results;
        }

        private static List<HrmUpcomingLeave> ReadUpcomingLeaves(SqlConnection connection)
        {
            var results = new List<HrmUpcomingLeave>();
            const string sql = @"SELECT TOP (5) EmployeeName, LeaveType, FromDate, ToDate, Status FROM LeaveApplications WITH (NOLOCK)
                                 WHERE ISNULL(IsDeleted, 0) = 0 AND FromDate >= CAST(GETDATE() AS date)
                                 AND ISNULL(Status, '') NOT IN ('Cancelled', 'Rejected') ORDER BY FromDate";
            using var command = new SqlCommand(sql, connection) { CommandTimeout = 12 };
            using var reader = command.ExecuteReader();
            while (reader.Read()) results.Add(new HrmUpcomingLeave { EmployeeName = reader["EmployeeName"].ToString() ?? "Employee", LeaveType = reader["LeaveType"].ToString() ?? "Leave", FromDate = reader["FromDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["FromDate"]), ToDate = reader["ToDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["ToDate"]), Status = reader["Status"].ToString() ?? "Open" });
            return results;
        }

        private static List<HrmActivity> ReadRecentActivities(SqlConnection connection)
        {
            var results = new List<HrmActivity>();
            const string sql = @"SELECT TOP (8) ActivityType, ActivityTitle, ActivityDetail, ActivityDate, Icon, Theme FROM (
                                    SELECT 'Employee' ActivityType, 'Employee added' ActivityTitle, FullName ActivityDetail, ISNULL(CreatedOn, GETDATE()) ActivityDate, 'fa-user-plus' Icon, 'teal' Theme FROM Employees WITH (NOLOCK) WHERE ISNULL(IsDeleted, 0) = 0
                                    UNION ALL SELECT 'Recruitment', 'Application received', ApplicantName, ISNULL(CreatedOn, GETDATE()), 'fa-user-check', 'violet' FROM JobApplicants WITH (NOLOCK) WHERE ISNULL(IsDeleted, 0) = 0
                                    UNION ALL SELECT 'Leave', 'Leave request created', ISNULL(EmployeeName, Employee), ISNULL(CreatedOn, GETDATE()), 'fa-calendar-check', 'amber' FROM LeaveApplications WITH (NOLOCK) WHERE ISNULL(IsDeleted, 0) = 0
                                    UNION ALL SELECT 'Expense', 'Expense claim created', ISNULL(EmployeeName, ExpenseClaimCode), ISNULL(CreatedOn, GETDATE()), 'fa-receipt', 'blue' FROM ExpenseClaims WITH (NOLOCK) WHERE ISNULL(IsDeleted, 0) = 0
                                 ) Activities ORDER BY ActivityDate DESC";
            using var command = new SqlCommand(sql, connection) { CommandTimeout = 12 };
            using var reader = command.ExecuteReader();
            while (reader.Read()) results.Add(new HrmActivity { Type = reader["ActivityType"].ToString() ?? "HRMS", Title = reader["ActivityTitle"].ToString() ?? "Updated", Detail = reader["ActivityDetail"].ToString() ?? "HRMS record", Date = reader["ActivityDate"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(reader["ActivityDate"]), Icon = reader["Icon"].ToString() ?? "fa-circle-info", Theme = reader["Theme"].ToString() ?? "teal" });
            return results;
        }
    }

    public class HrmDashboardViewModel
    {
        public bool IsLive { get; set; }
        public DateTime? LastUpdatedUtc { get; set; }
        public string? DataMessage { get; set; }
        public int TotalEmployees { get; set; } public int ActiveEmployees { get; set; } public int ActiveDepartments { get; set; } public int ActiveShiftTypes { get; set; }
        public int PresentToday { get; set; } public int AbsentToday { get; set; } public int LateToday { get; set; } public int OpenJobOpenings { get; set; }
        public int OpenApplicants { get; set; } public int AcceptedOffers { get; set; } public int PendingLeaves { get; set; } public int PendingExpenses { get; set; } public int AttendanceRate { get; set; }
        public List<HrmDepartmentSnapshot> Departments { get; set; } = new(); public List<HrmUpcomingLeave> UpcomingLeaves { get; set; } = new(); public List<HrmActivity> Activities { get; set; } = new();
    }
    public class HrmDepartmentSnapshot { public string Name { get; set; } = ""; public int Count { get; set; } }
    public class HrmUpcomingLeave { public string EmployeeName { get; set; } = ""; public string LeaveType { get; set; } = ""; public DateTime? FromDate { get; set; } public DateTime? ToDate { get; set; } public string Status { get; set; } = ""; }
    public class HrmActivity { public string Type { get; set; } = ""; public string Title { get; set; } = ""; public string Detail { get; set; } = ""; public DateTime Date { get; set; } public string Icon { get; set; } = ""; public string Theme { get; set; } = ""; }
}
