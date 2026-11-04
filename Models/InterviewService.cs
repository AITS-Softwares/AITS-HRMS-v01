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
            EnsureInterviewColumns();
        }

        // Existing installations can have the original Interviews table. Keep the
        // schema compatible before any list/detail query reads the new ERP fields.
        private void EnsureInterviewColumns()
        {
            const string sql = @"
IF COL_LENGTH('dbo.Interviews', 'JobOpening') IS NULL ALTER TABLE dbo.Interviews ADD JobOpening NVARCHAR(255) NULL;
IF COL_LENGTH('dbo.Interviews', 'ExpectedAverageRating') IS NULL ALTER TABLE dbo.Interviews ADD ExpectedAverageRating DECIMAL(18,2) NULL;
IF COL_LENGTH('dbo.Interviews', 'AverageRating') IS NULL ALTER TABLE dbo.Interviews ADD AverageRating DECIMAL(18,2) NULL;
IF COL_LENGTH('dbo.Interviews', 'Reminded') IS NULL ALTER TABLE dbo.Interviews ADD Reminded BIT NULL;
IF COL_LENGTH('dbo.Interviews', 'DocStatus') IS NULL ALTER TABLE dbo.Interviews ADD DocStatus INT NULL;";

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 30 };
            conn.Open();
            cmd.ExecuteNonQuery();
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
                ErpNextId = reader["ErpNextId"]?.ToString() ?? string.Empty,
                InterviewRound = reader["InterviewRound"]?.ToString() ?? string.Empty,
                JobApplicant = reader["JobApplicant"]?.ToString() ?? string.Empty,
                JobOpening = GetString(reader, "JobOpening"),
                Designation = reader["Designation"]?.ToString() ?? string.Empty,
                Status = reader["Status"]?.ToString() ?? "Pending",
                ScheduledOn = Convert.ToDateTime(reader["ScheduledOn"]),
                ResumeLink = reader["ResumeLink"]?.ToString() ?? string.Empty,
                FromTime = (TimeSpan)reader["FromTime"],
                ToTime = (TimeSpan)reader["ToTime"],
                InterviewSummary = reader["InterviewSummary"]?.ToString() ?? string.Empty,
                Feedback = reader["Feedback"]?.ToString() ?? string.Empty,
                Interviewers = reader["Interviewers"]?.ToString() ?? string.Empty,
                ExpectedAverageRating = GetDecimal(reader, "ExpectedAverageRating"),
                AverageRating = GetDecimal(reader, "AverageRating"),
                Reminded = GetBoolean(reader, "Reminded"),
                DocStatus = GetInt32(reader, "DocStatus")
            };
        }

        private static bool HasColumn(System.Data.Common.DbDataReader reader, string column)
        {
            try { return reader.GetOrdinal(column) >= 0; }
            catch (IndexOutOfRangeException) { return false; }
        }

        private static string GetString(System.Data.Common.DbDataReader reader, string column) =>
            HasColumn(reader, column) && reader[column] != DBNull.Value ? reader[column]?.ToString() ?? string.Empty : string.Empty;

        private static decimal GetDecimal(System.Data.Common.DbDataReader reader, string column) =>
            HasColumn(reader, column) && reader[column] != DBNull.Value ? Convert.ToDecimal(reader[column]) : 0;

        private static bool GetBoolean(System.Data.Common.DbDataReader reader, string column) =>
            HasColumn(reader, column) && reader[column] != DBNull.Value && Convert.ToBoolean(reader[column]);

        private static int GetInt32(System.Data.Common.DbDataReader reader, string column) =>
            HasColumn(reader, column) && reader[column] != DBNull.Value ? Convert.ToInt32(reader[column]) : 0;

        public int InsertInterview(Interview i)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    INSERT INTO Interviews
                        (ErpNextId, InterviewRound, JobApplicant, JobOpening, Designation, Status, ScheduledOn, ResumeLink,
                         FromTime, ToTime, InterviewSummary, Feedback, Interviewers, ExpectedAverageRating, AverageRating, Reminded, DocStatus)
                    VALUES
                        (@ErpNextId, @InterviewRound, @JobApplicant, @JobOpening, @Designation, @Status, @ScheduledOn, @ResumeLink,
                         @FromTime, @ToTime, @InterviewSummary, @Feedback, @Interviewers, @ExpectedAverageRating, @AverageRating, @Reminded, @DocStatus)";

                using (var cmd = new SqlCommand(query + "; SELECT CAST(SCOPE_IDENTITY() AS int);", conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", i.ErpNextId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@InterviewRound", i.InterviewRound ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@JobApplicant", i.JobApplicant ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@JobOpening", i.JobOpening ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Designation", i.Designation ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", i.Status ?? "Pending");
                    cmd.Parameters.AddWithValue("@ScheduledOn", i.ScheduledOn);
                    cmd.Parameters.AddWithValue("@ResumeLink", i.ResumeLink ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FromTime", i.FromTime);
                    cmd.Parameters.AddWithValue("@ToTime", i.ToTime);
                    cmd.Parameters.AddWithValue("@InterviewSummary", i.InterviewSummary ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Feedback", i.Feedback ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Interviewers", i.Interviewers ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ExpectedAverageRating", i.ExpectedAverageRating);
                    cmd.Parameters.AddWithValue("@AverageRating", i.AverageRating);
                    cmd.Parameters.AddWithValue("@Reminded", i.Reminded);
                    cmd.Parameters.AddWithValue("@DocStatus", i.DocStatus);

                    conn.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void UpdateInterview(Interview i)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"UPDATE Interviews SET ErpNextId=@ErpNextId, InterviewRound=@InterviewRound, JobApplicant=@JobApplicant, JobOpening=@JobOpening, Designation=@Designation, Status=@Status, ScheduledOn=@ScheduledOn, ResumeLink=@ResumeLink, FromTime=@FromTime, ToTime=@ToTime, InterviewSummary=@InterviewSummary, Feedback=@Feedback, Interviewers=@Interviewers, ExpectedAverageRating=@ExpectedAverageRating, AverageRating=@AverageRating, Reminded=@Reminded, DocStatus=@DocStatus WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", i.Id); cmd.Parameters.AddWithValue("@ErpNextId", i.ErpNextId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@InterviewRound", i.InterviewRound ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@JobApplicant", i.JobApplicant ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@JobOpening", i.JobOpening ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@Designation", i.Designation ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@Status", i.Status ?? "Pending"); cmd.Parameters.AddWithValue("@ScheduledOn", i.ScheduledOn); cmd.Parameters.AddWithValue("@ResumeLink", i.ResumeLink ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@FromTime", i.FromTime); cmd.Parameters.AddWithValue("@ToTime", i.ToTime); cmd.Parameters.AddWithValue("@InterviewSummary", i.InterviewSummary ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@Feedback", i.Feedback ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@Interviewers", i.Interviewers ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@ExpectedAverageRating", i.ExpectedAverageRating); cmd.Parameters.AddWithValue("@AverageRating", i.AverageRating); cmd.Parameters.AddWithValue("@Reminded", i.Reminded); cmd.Parameters.AddWithValue("@DocStatus", i.DocStatus);
            conn.Open(); cmd.ExecuteNonQuery();
        }
    }
}
