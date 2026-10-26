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

        public int InsertApplicant(JobApplicant a)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var schemaCmd = new SqlCommand("IF COL_LENGTH('dbo.JobApplicants','ErpNextId') IS NULL ALTER TABLE dbo.JobApplicants ADD ErpNextId NVARCHAR(255) NULL; IF COL_LENGTH('dbo.JobApplicants','SourceName') IS NULL ALTER TABLE dbo.JobApplicants ADD SourceName NVARCHAR(255) NULL;", conn)) schemaCmd.ExecuteNonQuery();
                string query = @"
                    INSERT INTO JobApplicants
                        (ErpNextId, ApplicantName, EmailAddress, JobOpening, Designation, PhoneNumber, Status, Country,
                         Source, SourceName, ApplicantRating, CoverLetter, ResumeAttachment, ResumeAttachmentName, ResumeLink,
                         Currency, LowerRange, UpperRange)
                    OUTPUT INSERTED.Id
                    VALUES
                        (@ErpNextId, @ApplicantName, @EmailAddress, @JobOpening, @Designation, @PhoneNumber, @Status, @Country,
                         @Source, @SourceName, @ApplicantRating, @CoverLetter, @ResumeAttachment, @ResumeAttachmentName, @ResumeLink,
                         @Currency, @LowerRange, @UpperRange)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", a.ErpNextId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ApplicantName", a.ApplicantName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmailAddress", a.EmailAddress ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@JobOpening", a.JobOpening ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Designation", a.Designation ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PhoneNumber", a.PhoneNumber ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", a.Status ?? "Open");
                    cmd.Parameters.AddWithValue("@Country", a.Country ?? "India");
                    cmd.Parameters.AddWithValue("@Source", a.Source ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SourceName", a.SourceName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ApplicantRating", a.ApplicantRating);
                    cmd.Parameters.AddWithValue("@CoverLetter", a.CoverLetter ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ResumeAttachment", a.ResumeAttachment ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ResumeAttachmentName", a.ResumeAttachmentName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ResumeLink", a.ResumeLink ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Currency", a.Currency ?? "INR");
                    cmd.Parameters.AddWithValue("@LowerRange", (object)a.LowerRange ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@UpperRange", (object)a.UpperRange ?? DBNull.Value);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public JobApplicant? GetById(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using (var schemaCmd = new SqlCommand("IF COL_LENGTH('dbo.JobApplicants','ErpNextId') IS NULL ALTER TABLE dbo.JobApplicants ADD ErpNextId NVARCHAR(255) NULL; IF COL_LENGTH('dbo.JobApplicants','SourceName') IS NULL ALTER TABLE dbo.JobApplicants ADD SourceName NVARCHAR(255) NULL;", conn)) schemaCmd.ExecuteNonQuery();
            using var cmd = new SqlCommand("SELECT * FROM JobApplicants WHERE Id=@Id AND IsDeleted=0", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;
            return new JobApplicant { Id=id, ErpNextId=reader["ErpNextId"] == DBNull.Value ? null : reader["ErpNextId"]?.ToString(), ApplicantName=reader["ApplicantName"]?.ToString() ?? "", EmailAddress=reader["EmailAddress"]?.ToString() ?? "", JobOpening=reader["JobOpening"]?.ToString() ?? "", Designation=reader["Designation"]?.ToString() ?? "", PhoneNumber=reader["PhoneNumber"]?.ToString() ?? "", Status=reader["Status"]?.ToString() ?? "Open", Country=reader["Country"]?.ToString() ?? "India", Source=reader["Source"]?.ToString() ?? "", SourceName=reader["SourceName"]?.ToString() ?? "", ApplicantRating=reader["ApplicantRating"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ApplicantRating"]), CoverLetter=reader["CoverLetter"]?.ToString() ?? "", ResumeAttachment=reader["ResumeAttachment"]?.ToString() ?? "", ResumeAttachmentName=reader["ResumeAttachmentName"]?.ToString() ?? "", ResumeLink=reader["ResumeLink"]?.ToString() ?? "", Currency=reader["Currency"]?.ToString() ?? "INR", LowerRange=reader["LowerRange"] == DBNull.Value ? null : Convert.ToDecimal(reader["LowerRange"]), UpperRange=reader["UpperRange"] == DBNull.Value ? null : Convert.ToDecimal(reader["UpperRange"]) };
        }

        public void UpdateApplicant(JobApplicant a)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"UPDATE JobApplicants SET ApplicantName=@Name, EmailAddress=@Email, JobOpening=@Job, Designation=@Designation, PhoneNumber=@Phone, Status=@Status, Country=@Country, Source=@Source, SourceName=@SourceName, ApplicantRating=@Rating, CoverLetter=@CoverLetter, ResumeAttachment=@Attachment, ResumeAttachmentName=@AttachmentName, ResumeLink=@ResumeLink, Currency=@Currency, LowerRange=@Lower, UpperRange=@Upper WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id",a.Id); cmd.Parameters.AddWithValue("@Name",a.ApplicantName ?? ""); cmd.Parameters.AddWithValue("@Email",a.EmailAddress ?? ""); cmd.Parameters.AddWithValue("@Job",a.JobOpening ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@Designation",a.Designation ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@Phone",a.PhoneNumber ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@Status",a.Status ?? "Open"); cmd.Parameters.AddWithValue("@Country",a.Country ?? "India"); cmd.Parameters.AddWithValue("@Source",a.Source ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@SourceName",a.SourceName ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@Rating",a.ApplicantRating); cmd.Parameters.AddWithValue("@CoverLetter",a.CoverLetter ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@Attachment",a.ResumeAttachment ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@AttachmentName",a.ResumeAttachmentName ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@ResumeLink",a.ResumeLink ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@Currency",a.Currency ?? "INR"); cmd.Parameters.AddWithValue("@Lower",a.LowerRange ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@Upper",a.UpperRange ?? (object)DBNull.Value); conn.Open(); cmd.ExecuteNonQuery();
        }
    }
}
