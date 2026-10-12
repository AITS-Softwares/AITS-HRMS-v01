using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class JobApplicantService
    {
        private readonly string _connectionString;

        public JobApplicantService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<JobApplicant> GetAllApplicants(string status = null)
        {
            var list = new List<JobApplicant>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT Id, ApplicantName, EmailAddress, Status FROM JobApplicants WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(status))
                    query += " AND Status = @Status";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(status))
                        cmd.Parameters.AddWithValue("@Status", status);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new JobApplicant
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                ApplicantName = reader["ApplicantName"]?.ToString() ?? string.Empty,
                                EmailAddress = reader["EmailAddress"]?.ToString() ?? string.Empty,
                                Status = reader["Status"]?.ToString() ?? "Open"
                            });
                        }
                    }
                }
            }

            return list;
        }

        public void InsertApplicant(JobApplicant a)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    INSERT INTO JobApplicants
                        (ApplicantName, EmailAddress, JobOpening, Designation, PhoneNumber, Status, Country,
                         Source, ApplicantRating, CoverLetter, ResumeAttachment, ResumeAttachmentName, ResumeLink,
                         Currency, LowerRange, UpperRange)
                    VALUES
                        (@ApplicantName, @EmailAddress, @JobOpening, @Designation, @PhoneNumber, @Status, @Country,
                         @Source, @ApplicantRating, @CoverLetter, @ResumeAttachment, @ResumeAttachmentName, @ResumeLink,
                         @Currency, @LowerRange, @UpperRange)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ApplicantName", a.ApplicantName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmailAddress", a.EmailAddress ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@JobOpening", a.JobOpening ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Designation", a.Designation ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PhoneNumber", a.PhoneNumber ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", a.Status ?? "Open");
                    cmd.Parameters.AddWithValue("@Country", a.Country ?? "India");
                    cmd.Parameters.AddWithValue("@Source", a.Source ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ApplicantRating", a.ApplicantRating);
                    cmd.Parameters.AddWithValue("@CoverLetter", a.CoverLetter ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ResumeAttachment", a.ResumeAttachment ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ResumeAttachmentName", a.ResumeAttachmentName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ResumeLink", a.ResumeLink ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Currency", a.Currency ?? "INR");
                    cmd.Parameters.AddWithValue("@LowerRange", (object)a.LowerRange ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@UpperRange", (object)a.UpperRange ?? DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
