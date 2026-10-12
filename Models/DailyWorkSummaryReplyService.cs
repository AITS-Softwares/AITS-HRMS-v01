using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class DailyWorkSummaryReplyService
    {
        private readonly string _connectionString;

        public DailyWorkSummaryReplyService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<DailyWorkSummaryReplyRow> GetReplies(int groupId, DateTime fromDate, DateTime toDate)
        {
            var rows = new List<DailyWorkSummaryReplyRow>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT r.Id, g.Name AS GroupName, r.Employee, r.EmployeeName, r.ReplyDate, r.Summary
                    FROM DailyWorkSummaryReplies r
                    INNER JOIN DailyWorkSummaryGroups g ON g.Id = r.GroupId
                    WHERE r.IsDeleted = 0
                      AND r.GroupId = @GroupId
                      AND r.ReplyDate BETWEEN @FromDate AND @ToDate
                    ORDER BY r.ReplyDate DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@GroupId", groupId);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            rows.Add(new DailyWorkSummaryReplyRow
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                GroupName = reader["GroupName"]?.ToString() ?? string.Empty,
                                Employee = reader["Employee"]?.ToString() ?? string.Empty,
                                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
                                ReplyDate = Convert.ToDateTime(reader["ReplyDate"]),
                                Summary = reader["Summary"]?.ToString() ?? string.Empty
                            });
                        }
                    }
                }
            }

            return rows;
        }

        // For populating the Group filter dropdown — using a small class instead of tuple (safer with ViewBag)
        public List<GroupOption> GetAllGroupsForDropdown()
        {
            var groups = new List<GroupOption>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT Id, Name FROM DailyWorkSummaryGroups WHERE IsDeleted = 0 ORDER BY Name";
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            groups.Add(new GroupOption
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Name = reader["Name"]?.ToString() ?? string.Empty
                            });
                        }
                    }
                }
            }

            return groups;
        }
    }

    public class GroupOption
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
