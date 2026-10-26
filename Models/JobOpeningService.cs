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

        public int InsertJobOpening(JobOpening model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var schemaCmd = new SqlCommand("IF COL_LENGTH('dbo.JobOpenings','ErpNextId') IS NULL ALTER TABLE dbo.JobOpenings ADD ErpNextId NVARCHAR(255) NULL;", conn))
                    schemaCmd.ExecuteNonQuery();
                string query = @"
                    INSERT INTO JobOpenings
                        (ErpNextId, JobOpeningTemplate, JobTitle, Designation, Status, PostedOn, ClosesOn,
                         Company, EmploymentType, Department, Location, PublishOnWebsite,
                         Description, Currency, SalaryPaidPer, LowerRange, UpperRange, PublishSalaryRange)
                    OUTPUT INSERTED.Id
                    VALUES
                        (@ErpNextId, @JobOpeningTemplate, @JobTitle, @Designation, @Status, @PostedOn, @ClosesOn,
                         @Company, @EmploymentType, @Department, @Location, @PublishOnWebsite,
                         @Description, @Currency, @SalaryPaidPer, @LowerRange, @UpperRange, @PublishSalaryRange)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", model.ErpNextId ?? (object)DBNull.Value);
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

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public JobOpening? GetById(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using (var schemaCmd = new SqlCommand("IF COL_LENGTH('dbo.JobOpenings','ErpNextId') IS NULL ALTER TABLE dbo.JobOpenings ADD ErpNextId NVARCHAR(255) NULL;", conn))
                schemaCmd.ExecuteNonQuery();
            using var cmd = new SqlCommand("SELECT * FROM JobOpenings WHERE Id=@Id AND IsDeleted=0", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;
            return new JobOpening
            {
                Id = id, ErpNextId = reader.GetOrdinal("ErpNextId") >= 0 && reader["ErpNextId"] != DBNull.Value ? reader["ErpNextId"]?.ToString() : null, JobOpeningTemplate = reader["JobOpeningTemplate"]?.ToString() ?? "", JobTitle = reader["JobTitle"]?.ToString() ?? "",
                Designation = reader["Designation"]?.ToString() ?? "", Status = reader["Status"]?.ToString() ?? "Open",
                PostedOn = Convert.ToDateTime(reader["PostedOn"]), ClosesOn = reader["ClosesOn"] == DBNull.Value ? null : Convert.ToDateTime(reader["ClosesOn"]),
                Company = reader["Company"]?.ToString() ?? "", EmploymentType = reader["EmploymentType"]?.ToString() ?? "",
                Department = reader["Department"]?.ToString() ?? "", Location = reader["Location"]?.ToString() ?? "",
                PublishOnWebsite = reader["PublishOnWebsite"] != DBNull.Value && Convert.ToBoolean(reader["PublishOnWebsite"]),
                Description = reader["Description"]?.ToString() ?? "", Currency = reader["Currency"]?.ToString() ?? "INR",
                SalaryPaidPer = reader["SalaryPaidPer"]?.ToString() ?? "Month", LowerRange = reader["LowerRange"] == DBNull.Value ? null : Convert.ToDecimal(reader["LowerRange"]),
                UpperRange = reader["UpperRange"] == DBNull.Value ? null : Convert.ToDecimal(reader["UpperRange"]),
                PublishSalaryRange = reader["PublishSalaryRange"] != DBNull.Value && Convert.ToBoolean(reader["PublishSalaryRange"])
            };
        }

        public void UpdateJobOpening(JobOpening model)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"UPDATE JobOpenings SET JobOpeningTemplate=@Template, JobTitle=@Title, Designation=@Designation, Status=@Status, PostedOn=@PostedOn, ClosesOn=@ClosesOn, Company=@Company, EmploymentType=@EmploymentType, Department=@Department, Location=@Location, PublishOnWebsite=@PublishOnWebsite, Description=@Description, Currency=@Currency, SalaryPaidPer=@SalaryPaidPer, LowerRange=@LowerRange, UpperRange=@UpperRange, PublishSalaryRange=@PublishSalaryRange WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", model.Id); cmd.Parameters.AddWithValue("@Template", model.JobOpeningTemplate ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@Title", model.JobTitle ?? ""); cmd.Parameters.AddWithValue("@Designation", model.Designation ?? ""); cmd.Parameters.AddWithValue("@Status", model.Status ?? "Open"); cmd.Parameters.AddWithValue("@PostedOn", model.PostedOn); cmd.Parameters.AddWithValue("@ClosesOn", model.ClosesOn ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@Company", model.Company ?? ""); cmd.Parameters.AddWithValue("@EmploymentType", model.EmploymentType ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@Department", model.Department ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@Location", model.Location ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@PublishOnWebsite", model.PublishOnWebsite); cmd.Parameters.AddWithValue("@Description", model.Description ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@Currency", model.Currency ?? "INR"); cmd.Parameters.AddWithValue("@SalaryPaidPer", model.SalaryPaidPer ?? "Month"); cmd.Parameters.AddWithValue("@LowerRange", model.LowerRange ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@UpperRange", model.UpperRange ?? (object)DBNull.Value); cmd.Parameters.AddWithValue("@PublishSalaryRange", model.PublishSalaryRange);
            conn.Open(); cmd.ExecuteNonQuery();
        }
    }
}
