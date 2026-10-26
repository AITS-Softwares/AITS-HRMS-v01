using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class JobOfferService
    {
        private readonly string _connectionString;

        public JobOfferService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<JobOffer> GetAllOffers(string idFilter, string applicantFilter, string status)
        {
            var list = new List<JobOffer>();

            using (var conn = new SqlConnection(_connectionString))
            {
                EnsureErpNextIdColumn(conn);
                string query = "SELECT Id, ErpNextId, JobApplicant, ApplicantName, Status, OfferDate FROM JobOffers WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(idFilter))
                    query += " AND CAST(Id AS VARCHAR) LIKE @IdFilter";

                if (!string.IsNullOrEmpty(applicantFilter))
                    query += " AND ApplicantName LIKE @ApplicantFilter";

                if (!string.IsNullOrEmpty(status))
                    query += " AND Status = @Status";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(idFilter))
                        cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");

                    if (!string.IsNullOrEmpty(applicantFilter))
                        cmd.Parameters.AddWithValue("@ApplicantFilter", "%" + applicantFilter + "%");

                    if (!string.IsNullOrEmpty(status))
                        cmd.Parameters.AddWithValue("@Status", status);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new JobOffer
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                ErpNextId = reader["ErpNextId"]?.ToString(),
                                JobApplicant = reader["JobApplicant"]?.ToString() ?? string.Empty,
                                ApplicantName = reader["ApplicantName"]?.ToString() ?? string.Empty,
                                Status = reader["Status"]?.ToString() ?? "Awaiting Response",
                                OfferDate = Convert.ToDateTime(reader["OfferDate"])
                            });
                        }
                    }
                }
            }

            return list;
        }

        public int InsertOffer(JobOffer o)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                EnsureErpNextIdColumn(conn);
                string query = @"
                    INSERT INTO JobOffers
                        (ErpNextId, JobApplicant, ApplicantName, Status, OfferDate, Designation, Company,
                         JobOfferTermTemplate, SelectTermsAndConditions, TermsAndConditions,
                         LetterHead, PrintHeading, OfferTermsJson, DocStatus)
                    OUTPUT INSERTED.Id
                    VALUES
                        (@ErpNextId, @JobApplicant, @ApplicantName, @Status, @OfferDate, @Designation, @Company,
                         @JobOfferTermTemplate, @SelectTermsAndConditions, @TermsAndConditions,
                         @LetterHead, @PrintHeading, @OfferTermsJson, @DocStatus)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", o.ErpNextId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@JobApplicant", o.JobApplicant ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ApplicantName", o.ApplicantName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", o.Status ?? "Awaiting Response");
                    cmd.Parameters.AddWithValue("@OfferDate", o.OfferDate);
                    cmd.Parameters.AddWithValue("@Designation", o.Designation ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Company", o.Company ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@JobOfferTermTemplate", o.JobOfferTermTemplate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SelectTermsAndConditions", o.SelectTermsAndConditions ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@TermsAndConditions", o.TermsAndConditions ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@LetterHead", o.LetterHead ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PrintHeading", o.PrintHeading ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@OfferTermsJson", o.OfferTermsJson ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@DocStatus", o.DocStatus);

                    conn.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public JobOffer? GetById(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            EnsureErpNextIdColumn(conn);
            conn.Open();
            using var cmd = new SqlCommand("SELECT * FROM JobOffers WHERE Id=@Id AND IsDeleted=0", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;
            return new JobOffer
            {
                Id = id,
                ErpNextId = reader["ErpNextId"] == DBNull.Value ? null : reader["ErpNextId"]?.ToString(),
                DocStatus = reader["DocStatus"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DocStatus"]),
                JobApplicant = reader["JobApplicant"]?.ToString() ?? string.Empty,
                ApplicantName = reader["ApplicantName"]?.ToString() ?? string.Empty,
                Status = reader["Status"]?.ToString() ?? "Awaiting Response",
                OfferDate = reader["OfferDate"] == DBNull.Value ? DateTime.Today : Convert.ToDateTime(reader["OfferDate"]),
                Designation = reader["Designation"]?.ToString() ?? string.Empty,
                Company = reader["Company"]?.ToString() ?? string.Empty,
                JobOfferTermTemplate = reader["JobOfferTermTemplate"]?.ToString() ?? string.Empty,
                SelectTermsAndConditions = reader["SelectTermsAndConditions"]?.ToString() ?? string.Empty,
                TermsAndConditions = reader["TermsAndConditions"]?.ToString() ?? string.Empty,
                LetterHead = reader["LetterHead"]?.ToString() ?? string.Empty,
                PrintHeading = reader["PrintHeading"]?.ToString() ?? string.Empty,
                OfferTermsJson = reader["OfferTermsJson"]?.ToString() ?? string.Empty
            };
        }

        public void UpdateOffer(JobOffer o)
        {
            using var conn = new SqlConnection(_connectionString);
            EnsureErpNextIdColumn(conn);
            using var cmd = new SqlCommand(@"UPDATE JobOffers SET JobApplicant=@JobApplicant, ApplicantName=@ApplicantName, Status=@Status, OfferDate=@OfferDate, Designation=@Designation, Company=@Company, JobOfferTermTemplate=@JobOfferTermTemplate, SelectTermsAndConditions=@SelectTermsAndConditions, TermsAndConditions=@TermsAndConditions, LetterHead=@LetterHead, PrintHeading=@PrintHeading, OfferTermsJson=@OfferTermsJson, DocStatus=@DocStatus WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", o.Id); cmd.Parameters.AddWithValue("@JobApplicant", o.JobApplicant ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@ApplicantName", o.ApplicantName ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@Status", o.Status ?? "Awaiting Response"); cmd.Parameters.AddWithValue("@OfferDate", o.OfferDate); cmd.Parameters.AddWithValue("@Designation", o.Designation ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@Company", o.Company ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@JobOfferTermTemplate", o.JobOfferTermTemplate ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@SelectTermsAndConditions", o.SelectTermsAndConditions ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@TermsAndConditions", o.TermsAndConditions ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@LetterHead", o.LetterHead ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@PrintHeading", o.PrintHeading ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@OfferTermsJson", o.OfferTermsJson ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@DocStatus", o.DocStatus);
            conn.Open(); cmd.ExecuteNonQuery();
        }

        private static void EnsureErpNextIdColumn(SqlConnection conn)
        {
            var closeWhenDone = conn.State != System.Data.ConnectionState.Open;
            if (closeWhenDone)
                conn.Open();
            using var cmd = new SqlCommand(@"
                IF COL_LENGTH('dbo.JobOffers', 'ErpNextId') IS NULL
                    ALTER TABLE dbo.JobOffers ADD ErpNextId NVARCHAR(255) NULL;
                IF COL_LENGTH('dbo.JobOffers', 'OfferTermsJson') IS NULL
                    ALTER TABLE dbo.JobOffers ADD OfferTermsJson NVARCHAR(MAX) NULL;
                IF COL_LENGTH('dbo.JobOffers', 'DocStatus') IS NULL
                    ALTER TABLE dbo.JobOffers ADD DocStatus INT NOT NULL CONSTRAINT DF_JobOffers_DocStatus DEFAULT(0);", conn);
            try
            {
                cmd.ExecuteNonQuery();
            }
            finally
            {
                if (closeWhenDone)
                    conn.Close();
            }
        }
    }
}
