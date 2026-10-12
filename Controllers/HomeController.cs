using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Erp_Aits.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Erp_Aits.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IConfiguration configuration, ILogger<HomeController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("DefaultConnection");
        }

        private int? GetCompanyId()
        {
            var companyId = HttpContext.Session.GetString("CompanyId");
            if (!string.IsNullOrEmpty(companyId))
            {
                return Convert.ToInt32(companyId);
            }
            return 1;
        }

        public IActionResult Index()
        {
            try
            {
                ViewData["Title"] = "Dashboard";
                ViewData["Module"] = "Overview";
                ViewData["Icon"] = "house";

                var companyId = GetCompanyId();
                var stats = GetDashboardStats(companyId);
                ViewBag.Stats = stats;

                ViewBag.RecentActivities = GetRecentActivities(companyId);
                ViewBag.UpcomingTasks = GetUpcomingTasks(companyId);

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Dashboard: {ex.Message}");
                ViewBag.Stats = new DashboardStats();
                ViewBag.RecentActivities = new List<object>();
                ViewBag.UpcomingTasks = new List<object>();
                return View();
            }
        }

        private DashboardStats GetDashboardStats(int? companyId)
        {
            var stats = new DashboardStats();

            using (var conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                // Total Leads
                using (var cmd = new SqlCommand(@"
                    SELECT COUNT(*) FROM Leads WHERE IsActive = 1 AND IsDeleted = 0 
                    AND (CompanyId = @CompanyId OR CompanyId IS NULL)", conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", companyId ?? 1);
                    stats.TotalLeads = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Total Customers (as Opportunities)
                using (var cmd = new SqlCommand(@"
                    SELECT COUNT(*) FROM Customers WHERE IsActive = 1 
                    AND (CompanyId = @CompanyId OR CompanyId IS NULL)", conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", companyId ?? 1);
                    stats.TotalOpportunities = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Active Campaigns
                using (var cmd = new SqlCommand(@"
                    SELECT COUNT(*) FROM Campaigns WHERE IsActive = 1 AND IsDeleted = 0 
                    AND (Status = 'Scheduled' OR (Status = 'Sent' AND SentAt >= DATEADD(DAY, -30, GETDATE())))
                    AND CompanyId = @CompanyId", conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", companyId ?? 1);
                    stats.ActiveCampaigns = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Total Calls (from EmailLogs)
                using (var cmd = new SqlCommand(@"
                    SELECT COUNT(*) FROM EmailLogs 
                    WHERE CampaignId IS NOT NULL 
                    AND CompanyId = @CompanyId", conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", companyId ?? 1);
                    stats.TotalCalls = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Lead Growth
                using (var cmd = new SqlCommand(@"
                    SELECT 
                        (SELECT COUNT(*) FROM Leads WHERE IsActive = 1 AND IsDeleted = 0 
                         AND (CompanyId = @CompanyId OR CompanyId IS NULL)
                         AND MONTH(CreatedDate) = MONTH(GETDATE()) 
                         AND YEAR(CreatedDate) = YEAR(GETDATE())) as ThisMonth,
                        (SELECT COUNT(*) FROM Leads WHERE IsActive = 1 AND IsDeleted = 0 
                         AND (CompanyId = @CompanyId OR CompanyId IS NULL)
                         AND MONTH(CreatedDate) = MONTH(DATEADD(MONTH, -1, GETDATE())) 
                         AND YEAR(CreatedDate) = YEAR(DATEADD(MONTH, -1, GETDATE()))) as LastMonth", conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", companyId ?? 1);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int thisMonth = reader["ThisMonth"] != DBNull.Value ? Convert.ToInt32(reader["ThisMonth"]) : 0;
                            int lastMonth = reader["LastMonth"] != DBNull.Value ? Convert.ToInt32(reader["LastMonth"]) : 1;
                            stats.LeadGrowth = lastMonth > 0 ? (int)Math.Round((double)(thisMonth - lastMonth) / lastMonth * 100) : 0;
                        }
                    }
                }

                // Opportunity Growth
                using (var cmd = new SqlCommand(@"
                    SELECT 
                        (SELECT COUNT(*) FROM Customers WHERE IsActive = 1 
                         AND (CompanyId = @CompanyId OR CompanyId IS NULL)
                         AND MONTH(CreatedDate) = MONTH(GETDATE()) 
                         AND YEAR(CreatedDate) = YEAR(GETDATE())) as ThisMonth,
                        (SELECT COUNT(*) FROM Customers WHERE IsActive = 1 
                         AND (CompanyId = @CompanyId OR CompanyId IS NULL)
                         AND MONTH(CreatedDate) = MONTH(DATEADD(MONTH, -1, GETDATE())) 
                         AND YEAR(CreatedDate) = YEAR(DATEADD(MONTH, -1, GETDATE()))) as LastMonth", conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", companyId ?? 1);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int thisMonth = reader["ThisMonth"] != DBNull.Value ? Convert.ToInt32(reader["ThisMonth"]) : 0;
                            int lastMonth = reader["LastMonth"] != DBNull.Value ? Convert.ToInt32(reader["LastMonth"]) : 1;
                            stats.OpportunityGrowth = lastMonth > 0 ? (int)Math.Round((double)(thisMonth - lastMonth) / lastMonth * 100) : 0;
                        }
                    }
                }

                // Call Growth
                using (var cmd = new SqlCommand(@"
                    SELECT 
                        (SELECT COUNT(*) FROM EmailLogs 
                         WHERE CampaignId IS NOT NULL AND CompanyId = @CompanyId
                         AND MONTH(CreatedDate) = MONTH(GETDATE()) 
                         AND YEAR(CreatedDate) = YEAR(GETDATE())) as ThisMonth,
                        (SELECT COUNT(*) FROM EmailLogs 
                         WHERE CampaignId IS NOT NULL AND CompanyId = @CompanyId
                         AND MONTH(CreatedDate) = MONTH(DATEADD(MONTH, -1, GETDATE())) 
                         AND YEAR(CreatedDate) = YEAR(DATEADD(MONTH, -1, GETDATE()))) as LastMonth", conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", companyId ?? 1);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int thisMonth = reader["ThisMonth"] != DBNull.Value ? Convert.ToInt32(reader["ThisMonth"]) : 0;
                            int lastMonth = reader["LastMonth"] != DBNull.Value ? Convert.ToInt32(reader["LastMonth"]) : 1;
                            stats.CallGrowth = lastMonth > 0 ? (int)Math.Round((double)(thisMonth - lastMonth) / lastMonth * 100) : 0;
                        }
                    }
                }

                // Campaign Trend
                using (var cmd = new SqlCommand(@"
                    SELECT 
                        (SELECT COUNT(*) FROM Campaigns WHERE IsActive = 1 AND IsDeleted = 0 
                         AND CompanyId = @CompanyId
                         AND MONTH(CreatedDate) = MONTH(GETDATE()) 
                         AND YEAR(CreatedDate) = YEAR(GETDATE())) as ThisMonth,
                        (SELECT COUNT(*) FROM Campaigns WHERE IsActive = 1 AND IsDeleted = 0 
                         AND CompanyId = @CompanyId
                         AND MONTH(CreatedDate) = MONTH(DATEADD(MONTH, -1, GETDATE())) 
                         AND YEAR(CreatedDate) = YEAR(DATEADD(MONTH, -1, GETDATE()))) as LastMonth", conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", companyId ?? 1);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int thisMonth = reader["ThisMonth"] != DBNull.Value ? Convert.ToInt32(reader["ThisMonth"]) : 0;
                            int lastMonth = reader["LastMonth"] != DBNull.Value ? Convert.ToInt32(reader["LastMonth"]) : 1;
                            stats.CampaignTrend = lastMonth > 0 ? (int)Math.Round((double)(thisMonth - lastMonth) / lastMonth * 100) : 0;
                        }
                    }
                }

                // Conversion Rate
                using (var cmd = new SqlCommand(@"
                    SELECT 
                        (SELECT COUNT(*) FROM Customers WHERE IsActive = 1 
                         AND (CompanyId = @CompanyId OR CompanyId IS NULL)) as Customers,
                        (SELECT COUNT(*) FROM Leads WHERE IsActive = 1 AND IsDeleted = 0 
                         AND (CompanyId = @CompanyId OR CompanyId IS NULL)) as Leads", conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", companyId ?? 1);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int customers = reader["Customers"] != DBNull.Value ? Convert.ToInt32(reader["Customers"]) : 0;
                            int leads = reader["Leads"] != DBNull.Value ? Convert.ToInt32(reader["Leads"]) : 1;
                            stats.ConversionRate = leads > 0 ? (int)Math.Round((double)customers / leads * 100) : 0;
                        }
                    }
                }

                // Email Open Rate
                using (var cmd = new SqlCommand(@"
                    SELECT 
                        (SELECT COUNT(*) FROM EmailLogs WHERE Status = 'sent' OR Status = 'opened') as Sent,
                        (SELECT COUNT(*) FROM EmailLogs WHERE IsOpened = 1) as Opened", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int sent = reader["Sent"] != DBNull.Value ? Convert.ToInt32(reader["Sent"]) : 1;
                            int opened = reader["Opened"] != DBNull.Value ? Convert.ToInt32(reader["Opened"]) : 0;
                            stats.EmailOpenRate = sent > 0 ? (int)Math.Round((double)opened / sent * 100) : 0;
                        }
                    }
                }

                // Satisfaction Score
                using (var cmd = new SqlCommand(@"
                    SELECT 
                        (SELECT COUNT(*) FROM EmailLogs WHERE IsOpened = 1) as Opened,
                        (SELECT COUNT(*) FROM EmailLogs WHERE LinkClicked = 1) as Clicked,
                        (SELECT COUNT(*) FROM EmailLogs WHERE Status = 'sent' OR Status = 'opened') as Total", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int opened = reader["Opened"] != DBNull.Value ? Convert.ToInt32(reader["Opened"]) : 0;
                            int clicked = reader["Clicked"] != DBNull.Value ? Convert.ToInt32(reader["Clicked"]) : 0;
                            int total = reader["Total"] != DBNull.Value ? Convert.ToInt32(reader["Total"]) : 1;

                            double openRate = total > 0 ? (double)opened / total : 0;
                            double clickRate = total > 0 ? (double)clicked / total : 0;
                            stats.SatisfactionScore = Math.Round(1 + (openRate * 2) + (clickRate * 2), 1);
                            if (stats.SatisfactionScore > 5) stats.SatisfactionScore = 5;
                        }
                    }
                }

                // Avg Response Time
                using (var cmd = new SqlCommand(@"
                    SELECT 
                        AVG(CAST(DATEDIFF(MINUTE, CreatedDate, SentAt) AS FLOAT)) as AvgMinutes
                    FROM EmailLogs 
                    WHERE SentAt IS NOT NULL AND CreatedDate IS NOT NULL
                    AND DATEDIFF(MINUTE, CreatedDate, SentAt) > 0
                    AND CompanyId = @CompanyId", conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", companyId ?? 1);
                    var result = cmd.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        double avgMinutes = Convert.ToDouble(result);
                        if (avgMinutes < 60)
                            stats.AvgResponseTime = $"{(int)avgMinutes} min";
                        else if (avgMinutes < 1440)
                            stats.AvgResponseTime = $"{(int)(avgMinutes / 60)} hrs";
                        else
                            stats.AvgResponseTime = $"{(int)(avgMinutes / 1440)} days";

                        stats.ResponseTimePercent = avgMinutes < 60 ? 80 : (avgMinutes < 240 ? 50 : 20);
                    }
                }
            }

            return stats;
        }

        private List<dynamic> GetRecentActivities(int? companyId)
        {
            var activities = new List<dynamic>();

            using (var conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                // Get recent Leads
                using (var cmd = new SqlCommand(@"
                    SELECT TOP 3 'Lead' as Type, 
                           ISNULL(FirstName, '') + ' ' + ISNULL(LastName, '') as Name, 
                           CreatedDate, 
                           'New lead added' as Action,
                           'user-plus' as Icon, 
                           'blue' as IconClass,
                           'System' as [User]
                    FROM Leads 
                    WHERE IsActive = 1 AND IsDeleted = 0 
                    AND (CompanyId = @CompanyId OR CompanyId IS NULL)
                    ORDER BY CreatedDate DESC", conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", companyId ?? 1);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            activities.Add(new
                            {
                                Title = "New lead added",
                                Description = reader["Name"]?.ToString() ?? "Unknown",
                                User = reader["User"]?.ToString() ?? "System",
                                TimeAgo = GetTimeAgo(Convert.ToDateTime(reader["CreatedDate"])),
                                Icon = "fa-user-plus",
                                IconClass = "blue"
                            });
                        }
                    }
                }

                // Get recent Campaigns
                using (var cmd = new SqlCommand(@"
                    SELECT TOP 2 CampaignName, CreatedDate, 
                           'Campaign created' as Action,
                           'bullhorn' as Icon, 
                           'orange' as IconClass,
                           'Marketing Team' as [User]
                    FROM Campaigns 
                    WHERE IsDeleted = 0 AND CompanyId = @CompanyId
                    ORDER BY CreatedDate DESC", conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", companyId ?? 1);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            activities.Add(new
                            {
                                Title = "Campaign created",
                                Description = reader["CampaignName"]?.ToString() ?? "Unknown",
                                User = reader["User"]?.ToString() ?? "Marketing Team",
                                TimeAgo = GetTimeAgo(Convert.ToDateTime(reader["CreatedDate"])),
                                Icon = "fa-bullhorn",
                                IconClass = "orange"
                            });
                        }
                    }
                }

                // Get recent Email sends
                using (var cmd = new SqlCommand(@"
                    SELECT TOP 2 [To], SentAt, 
                           CASE WHEN IsOpened = 1 THEN 'Email opened' ELSE 'Email sent' END as Action,
                           'envelope' as Icon, 
                           CASE WHEN IsOpened = 1 THEN 'green' ELSE 'blue' END as IconClass,
                           'System' as [User]
                    FROM EmailLogs 
                    WHERE CompanyId = @CompanyId
                    ORDER BY SentAt DESC", conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", companyId ?? 1);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            activities.Add(new
                            {
                                Title = reader["Action"]?.ToString() ?? "Email activity",
                                Description = reader["To"]?.ToString() ?? "Unknown",
                                User = reader["User"]?.ToString() ?? "System",
                                TimeAgo = GetTimeAgo(Convert.ToDateTime(reader["SentAt"])),
                                Icon = "fa-envelope",
                                IconClass = reader["IconClass"]?.ToString() ?? "blue"
                            });
                        }
                    }
                }
            }

            return activities.OrderByDescending(a => GetTimeAgoValue(a.TimeAgo)).Take(5).ToList();
        }

        private List<dynamic> GetUpcomingTasks(int? companyId)
        {
            var tasks = new List<dynamic>();

            using (var conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                // Get upcoming Campaigns
                using (var cmd = new SqlCommand(@"
                    SELECT TOP 5 CampaignName, ScheduledTime,
                           CASE 
                               WHEN ScheduledTime <= DATEADD(DAY, 1, GETDATE()) THEN 'high'
                               WHEN ScheduledTime <= DATEADD(DAY, 3, GETDATE()) THEN 'medium'
                               ELSE 'low'
                           END as Priority,
                           'fa-bullhorn' as Icon,
                           'primary' as IconColor
                    FROM Campaigns 
                    WHERE ScheduledTime >= GETDATE() 
                    AND Status = 'Scheduled' 
                    AND IsDeleted = 0 
                    AND CompanyId = @CompanyId
                    ORDER BY ScheduledTime ASC", conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", companyId ?? 1);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var scheduledTime = Convert.ToDateTime(reader["ScheduledTime"]);
                            tasks.Add(new
                            {
                                Title = reader["CampaignName"]?.ToString() ?? "Scheduled Campaign",
                                PriorityClass = reader["Priority"]?.ToString() ?? "low",
                                Icon = "fa-bullhorn",
                                IconColor = "primary",
                                DateTime = scheduledTime.ToString("MMM dd 'at' h:mm tt"),
                                DaysLeft = GetDaysLeft(scheduledTime)
                            });
                        }
                    }
                }

                // If no campaigns, show default tasks
                if (!tasks.Any())
                {
                    tasks.Add(new
                    {
                        Title = "No upcoming campaigns",
                        PriorityClass = "low",
                        Icon = "fa-check-circle",
                        IconColor = "success",
                        DateTime = "All caught up!",
                        DaysLeft = ""
                    });
                }
            }

            return tasks;
        }

        private string GetDaysLeft(DateTime date)
        {
            var days = (date - DateTime.Now).Days;
            if (days < 0) return "Overdue";
            if (days == 0) return "Today";
            if (days == 1) return "Tomorrow";
            return $"{days} days left";
        }

        private int GetTimeAgoValue(string timeAgo)
        {
            if (timeAgo.Contains("Just now")) return 0;
            if (timeAgo.Contains("minutes"))
            {
                var parts = timeAgo.Split(' ');
                if (parts.Length > 0 && int.TryParse(parts[0], out int minutes))
                    return minutes;
            }
            if (timeAgo.Contains("hours"))
            {
                var parts = timeAgo.Split(' ');
                if (parts.Length > 0 && int.TryParse(parts[0], out int hours))
                    return hours * 60;
            }
            if (timeAgo.Contains("days"))
            {
                var parts = timeAgo.Split(' ');
                if (parts.Length > 0 && int.TryParse(parts[0], out int days))
                    return days * 1440;
            }
            if (timeAgo.Contains("weeks"))
            {
                var parts = timeAgo.Split(' ');
                if (parts.Length > 0 && int.TryParse(parts[0], out int weeks))
                    return weeks * 10080;
            }
            if (timeAgo.Contains("months"))
            {
                var parts = timeAgo.Split(' ');
                if (parts.Length > 0 && int.TryParse(parts[0], out int months))
                    return months * 43200;
            }
            if (timeAgo.Contains("years"))
            {
                var parts = timeAgo.Split(' ');
                if (parts.Length > 0 && int.TryParse(parts[0], out int years))
                    return years * 525600;
            }
            return 999999;
        }

        private string GetTimeAgo(DateTime date)
        {
            var diff = DateTime.Now - date;

            if (diff.TotalMinutes < 1)
                return "Just now";
            if (diff.TotalMinutes < 60)
                return $"{(int)diff.TotalMinutes} minutes ago";
            if (diff.TotalHours < 24)
                return $"{(int)diff.TotalHours} hours ago";
            if (diff.TotalDays < 7)
                return $"{(int)diff.TotalDays} days ago";
            if (diff.TotalDays < 30)
                return $"{(int)(diff.TotalDays / 7)} weeks ago";
            if (diff.TotalDays < 365)
                return $"{(int)(diff.TotalDays / 30)} months ago";
            return $"{(int)(diff.TotalDays / 365)} years ago";
        }
    }

    public class DashboardStats
    {
        public int TotalLeads { get; set; }
        public int TotalOpportunities { get; set; }
        public int ActiveCampaigns { get; set; }
        public int TotalCalls { get; set; }
        public int LeadGrowth { get; set; }
        public int OpportunityGrowth { get; set; }
        public int CampaignTrend { get; set; }
        public int CallGrowth { get; set; }
        public int ConversionRate { get; set; }
        public int EmailOpenRate { get; set; }
        public double SatisfactionScore { get; set; }
        public string AvgResponseTime { get; set; } = "N/A";
        public int ResponseTimePercent { get; set; }
    }
}