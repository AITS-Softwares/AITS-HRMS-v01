using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class JobRequisitionService
    {
        private readonly string _connectionString;

        public JobRequisitionService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<JobRequisition> GetAll(string idFilter)
        {
            var list = new List<JobRequisition>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM JobRequisitions WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(idFilter))
                    query += " AND Code LIKE @IdFilter";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(idFilter))
                        cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");

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

        public JobRequisition GetById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM JobRequisitions WHERE Id = @Id AND IsDeleted = 0";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            return MapRow(reader);
                    }
                }
            }
            return null;
        }

        public List<dynamic> Search(string name, string designation, string requestedBy)
        {
            var results = new List<dynamic>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT Id, Code, Designation, RequestedBy FROM JobRequisitions WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(name))
                    query += " AND Code LIKE @Name";
                if (!string.IsNullOrEmpty(designation))
                    query += " AND Designation LIKE @Designation";
                if (!string.IsNullOrEmpty(requestedBy))
                    query += " AND RequestedBy LIKE @RequestedBy";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(name))
                        cmd.Parameters.AddWithValue("@Name", "%" + name + "%");
                    if (!string.IsNullOrEmpty(designation))
                        cmd.Parameters.AddWithValue("@Designation", "%" + designation + "%");
                    if (!string.IsNullOrEmpty(requestedBy))
                        cmd.Parameters.AddWithValue("@RequestedBy", "%" + requestedBy + "%");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(new
                            {
                                id = Convert.ToInt32(reader["Id"]),
                                name = reader["Code"]?.ToString(),
                                designation = reader["Designation"]?.ToString(),
                                requestedBy = reader["RequestedBy"]?.ToString()
                            });
                        }
                    }
                }
            }

            return results;
        }

        public string GetNextCode()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT COUNT(*) FROM JobRequisitions";
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    int count = (int)cmd.ExecuteScalar() + 1;
                    return $"HR-HIREQ-{count:D5}";
                }
            }
        }

        public int Insert(JobRequisition r)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    INSERT INTO JobRequisitions
                        (Code, NoOfPositions, Company, Designation, ExpectedCompensation, Status,
                         Department, RequestedBy, PostingDate, ExpectedBy, JobDescription, ReasonForRequesting)
                    OUTPUT INSERTED.Id
                    VALUES
                        (@Code, @NoOfPositions, @Company, @Designation, @ExpectedCompensation, @Status,
                         @Department, @RequestedBy, @PostingDate, @ExpectedBy, @JobDescription, @ReasonForRequesting)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Code", r.Code ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@NoOfPositions", (object)r.NoOfPositions ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Company", r.Company ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Designation", r.Designation ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ExpectedCompensation", (object)r.ExpectedCompensation ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", r.Status ?? "Pending");
                    cmd.Parameters.AddWithValue("@Department", r.Department ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@RequestedBy", r.RequestedBy ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PostingDate", r.PostingDate);
                    cmd.Parameters.AddWithValue("@ExpectedBy", (object)r.ExpectedBy ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@JobDescription", r.JobDescription ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ReasonForRequesting", r.ReasonForRequesting ?? (object)DBNull.Value);

                    conn.Open();
                    int newId = (int)cmd.ExecuteScalar();
                    return newId;
                }
            }
        }

        private JobRequisition MapRow(System.Data.Common.DbDataReader reader)
        {
            return new JobRequisition
            {
                Id = Convert.ToInt32(reader["Id"]),
                Code = reader["Code"]?.ToString() ?? string.Empty,
                NoOfPositions = reader["NoOfPositions"] != DBNull.Value ? Convert.ToInt32(reader["NoOfPositions"]) : (int?)null,
                Company = reader["Company"]?.ToString() ?? string.Empty,
                Designation = reader["Designation"]?.ToString() ?? string.Empty,
                ExpectedCompensation = reader["ExpectedCompensation"] != DBNull.Value ? Convert.ToDecimal(reader["ExpectedCompensation"]) : (decimal?)null,
                Status = reader["Status"]?.ToString() ?? "Pending",
                Department = reader["Department"]?.ToString() ?? string.Empty,
                RequestedBy = reader["RequestedBy"]?.ToString() ?? string.Empty,
                PostingDate = Convert.ToDateTime(reader["PostingDate"]),
                ExpectedBy = reader["ExpectedBy"] != DBNull.Value ? Convert.ToDateTime(reader["ExpectedBy"]) : (DateTime?)null,
                JobDescription = reader["JobDescription"]?.ToString() ?? string.Empty,
                ReasonForRequesting = reader["ReasonForRequesting"]?.ToString() ?? string.Empty
            };
        }
    }
}