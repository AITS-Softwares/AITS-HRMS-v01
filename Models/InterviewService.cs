using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class InterviewService
    {
        private readonly string _connectionString;

        public InterviewService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Interview> GetAllInterviews(string idFilter, string round, string applicant, string designation, string status, DateTime? scheduledOn)
        {
            var list = new List<Interview>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Interviews WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(idFilter))
                    query += " AND CAST(Id AS VARCHAR) LIKE @IdFilter";
                if (!string.IsNullOrEmpty(round))
                    query += " AND InterviewRound LIKE @Round";
                if (!string.IsNullOrEmpty(applicant))
                    query += " AND JobApplicant LIKE @Applicant";
                if (!string.IsNullOrEmpty(designation))
                    query += " AND Designation LIKE @Designation";
                if (!string.IsNullOrEmpty(status))
                    query += " AND Status = @Status";
                if (scheduledOn != null)
                    query += " AND ScheduledOn = @ScheduledOn";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(idFilter)) cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");
                    if (!string.IsNullOrEmpty(round)) cmd.Parameters.AddWithValue("@Round", "%" + round + "%");
                    if (!string.IsNullOrEmpty(applicant)) cmd.Parameters.AddWithValue("@Applicant", "%" + applicant + "%");
                    if (!string.IsNullOrEmpty(designation)) cmd.Parameters.AddWithValue("@Designation", "%" + designation + "%");
                    if (!string.IsNullOrEmpty(status)) cmd.Parameters.AddWithValue("@Status", status);
                    if (scheduledOn != null) cmd.Parameters.AddWithValue("@ScheduledOn", scheduledOn.Value);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(MapRow(reader));
                        }
                    }
                }
            }

            return list;
        }

        public Interview GetById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Interviews WHERE Id = @Id AND IsDeleted = 0";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapRow(reader);
                        }
                    }
                }
            }
            return null;
        }

        private Interview MapRow(System.Data.Common.DbDataReader reader)
        {
            return new Interview
            {
                Id = Convert.ToInt32(reader["Id"]),
                InterviewRound = reader["InterviewRound"]?.ToString() ?? string.Empty,
                JobApplicant = reader["JobApplicant"]?.ToString() ?? string.Empty,
                Designation = reader["Designation"]?.ToString() ?? string.Empty,
                Status = reader["Status"]?.ToString() ?? "Pending",
                ScheduledOn = Convert.ToDateTime(reader["ScheduledOn"]),
                ResumeLink = reader["ResumeLink"]?.ToString() ?? string.Empty,
                FromTime = (TimeSpan)reader["FromTime"],
                ToTime = (TimeSpan)reader["ToTime"],
                InterviewSummary = reader["InterviewSummary"]?.ToString() ?? string.Empty,
                Feedback = reader["Feedback"]?.ToString() ?? string.Empty
            };
        }

        public void InsertInterview(Interview i)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    INSERT INTO Interviews
                        (InterviewRound, JobApplicant, Designation, Status, ScheduledOn, ResumeLink,
                         FromTime, ToTime, InterviewSummary, Feedback)
                    VALUES
                        (@InterviewRound, @JobApplicant, @Designation, @Status, @ScheduledOn, @ResumeLink,
                         @FromTime, @ToTime, @InterviewSummary, @Feedback)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@InterviewRound", i.InterviewRound ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@JobApplicant", i.JobApplicant ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Designation", i.Designation ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", i.Status ?? "Pending");
                    cmd.Parameters.AddWithValue("@ScheduledOn", i.ScheduledOn);
                    cmd.Parameters.AddWithValue("@ResumeLink", i.ResumeLink ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FromTime", i.FromTime);
                    cmd.Parameters.AddWithValue("@ToTime", i.ToTime);
                    cmd.Parameters.AddWithValue("@InterviewSummary", i.InterviewSummary ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Feedback", i.Feedback ?? (object)DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
