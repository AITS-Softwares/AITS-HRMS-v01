using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class JobOpeningService


    {
        private readonly string _connectionString;

        public JobOpeningService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<JobOpening> GetAllJobOpenings(string status = null)
        {
            var list = new List<JobOpening>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT Id, JobTitle, Designation, Status, PostedOn, Company, Department
                                  FROM JobOpenings WHERE IsDeleted = 0";

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
                            list.Add(new JobOpening
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                JobTitle = reader["JobTitle"]?.ToString() ?? string.Empty,
                                Designation = reader["Designation"]?.ToString() ?? string.Empty,
                                Status = reader["Status"]?.ToString() ?? string.Empty,
                                PostedOn = Convert.ToDateTime(reader["PostedOn"]),
                                Company = reader["Company"]?.ToString() ?? string.Empty,
                                Department = reader["Department"]?.ToString() ?? string.Empty
                            });
                        }
                    }
                }
            }

            return list;
        }

        public void InsertJobOpening(JobOpening model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    INSERT INTO JobOpenings
                        (JobOpeningTemplate, JobTitle, Designation, Status, PostedOn, ClosesOn,
                         Company, EmploymentType, Department, Location, PublishOnWebsite,
                         Description, Currency, SalaryPaidPer, LowerRange, UpperRange, PublishSalaryRange)
                    VALUES
                        (@JobOpeningTemplate, @JobTitle, @Designation, @Status, @PostedOn, @ClosesOn,
                         @Company, @EmploymentType, @Department, @Location, @PublishOnWebsite,
                         @Description, @Currency, @SalaryPaidPer, @LowerRange, @UpperRange, @PublishSalaryRange)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@JobOpeningTemplate", (object)model.JobOpeningTemplate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@JobTitle", model.JobTitle ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Designation", model.Designation ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", model.Status ?? "Open");
                    cmd.Parameters.AddWithValue("@PostedOn", model.PostedOn);
                    cmd.Parameters.AddWithValue("@ClosesOn", (object)model.ClosesOn ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Company", model.Company ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmploymentType", (object)model.EmploymentType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Department", (object)model.Department ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Location", (object)model.Location ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PublishOnWebsite", model.PublishOnWebsite);
                    cmd.Parameters.AddWithValue("@Description", (object)model.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Currency", model.Currency ?? "INR");
                    cmd.Parameters.AddWithValue("@SalaryPaidPer", model.SalaryPaidPer ?? "Month");
                    cmd.Parameters.AddWithValue("@LowerRange", (object)model.LowerRange ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@UpperRange", (object)model.UpperRange ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PublishSalaryRange", model.PublishSalaryRange);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
