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
                string query = "SELECT Id, JobApplicant, ApplicantName, Status, OfferDate FROM JobOffers WHERE IsDeleted = 0";

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

        public void InsertOffer(JobOffer o)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    INSERT INTO JobOffers
                        (JobApplicant, ApplicantName, Status, OfferDate, Designation, Company,
                         JobOfferTermTemplate, SelectTermsAndConditions, TermsAndConditions,
                         LetterHead, PrintHeading)
                    VALUES
                        (@JobApplicant, @ApplicantName, @Status, @OfferDate, @Designation, @Company,
                         @JobOfferTermTemplate, @SelectTermsAndConditions, @TermsAndConditions,
                         @LetterHead, @PrintHeading)";

                using (var cmd = new SqlCommand(query, conn))
                {
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

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
