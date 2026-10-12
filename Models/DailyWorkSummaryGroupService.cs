using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class DailyWorkSummaryGroupService
    {
        private readonly string _connectionString;

        public DailyWorkSummaryGroupService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<DailyWorkSummaryGroup> GetAllGroups()
        {
            var groups = new List<DailyWorkSummaryGroup>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT Id, Name, Enabled FROM DailyWorkSummaryGroups WHERE IsDeleted = 0 ORDER BY Id DESC";
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            groups.Add(new DailyWorkSummaryGroup
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Name = reader["Name"]?.ToString() ?? string.Empty,
                                Enabled = Convert.ToBoolean(reader["Enabled"])
                            });
                        }
                    }
                }
            }

            return groups;
        }

        public void InsertGroup(DailyWorkSummaryGroup group)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int newId;

                        string headerQuery = @"
                            INSERT INTO DailyWorkSummaryGroups
                                (Name, Enabled, SendEmailsAt, HolidayList, Subject, Message)
                            OUTPUT INSERTED.Id
                            VALUES
                                (@Name, @Enabled, @SendEmailsAt, @HolidayList, @Subject, @Message)";

                        using (var cmd = new SqlCommand(headerQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Name", group.Name ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Enabled", group.Enabled);
                            cmd.Parameters.AddWithValue("@SendEmailsAt", group.SendEmailsAt ?? "00:00");
                            cmd.Parameters.AddWithValue("@HolidayList", group.HolidayList ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Subject", group.Subject ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Message", group.Message ?? (object)DBNull.Value);

                            newId = (int)cmd.ExecuteScalar();
                        }

                        if (group.Users != null)
                        {
                            foreach (var u in group.Users)
                            {
                                if (string.IsNullOrEmpty(u.UserName)) continue;

                                string q = @"INSERT INTO DailyWorkSummaryGroupUsers (GroupId, UserName)
                                             VALUES (@GroupId, @UserName)";
                                using (var cmd = new SqlCommand(q, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@GroupId", newId);
                                    cmd.Parameters.AddWithValue("@UserName", u.UserName);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
