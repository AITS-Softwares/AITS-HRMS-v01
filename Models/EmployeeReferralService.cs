using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class EmployeeReferralService
    {
        private readonly string _connectionString;

        public EmployeeReferralService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<EmployeeReferral> GetAll(string idFilter, string fullName, DateTime? date, string status, string forDesignation, string contactNo, string referrer)
        {
            var list = new List<EmployeeReferral>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM EmployeeReferrals WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(idFilter))
                    query += " AND CAST(Id AS VARCHAR) LIKE @IdFilter";
                if (!string.IsNullOrEmpty(fullName))
                    query += " AND (FirstName LIKE @FullName OR LastName LIKE @FullName)";
                if (date != null)
                    query += " AND ReferralDate = @Date";
                if (!string.IsNullOrEmpty(status))
                    query += " AND Status = @Status";
                if (!string.IsNullOrEmpty(forDesignation))
                    query += " AND ForDesignation LIKE @ForDesignation";
                if (!string.IsNullOrEmpty(contactNo))
                    query += " AND ContactNo LIKE @ContactNo";
                if (!string.IsNullOrEmpty(referrer))
                    query += " AND Referrer LIKE @Referrer";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(idFilter)) cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");
                    if (!string.IsNullOrEmpty(fullName)) cmd.Parameters.AddWithValue("@FullName", "%" + fullName + "%");
                    if (date != null) cmd.Parameters.AddWithValue("@Date", date.Value);
                    if (!string.IsNullOrEmpty(status)) cmd.Parameters.AddWithValue("@Status", status);
                    if (!string.IsNullOrEmpty(forDesignation)) cmd.Parameters.AddWithValue("@ForDesignation", "%" + forDesignation + "%");
                    if (!string.IsNullOrEmpty(contactNo)) cmd.Parameters.AddWithValue("@ContactNo", "%" + contactNo + "%");
                    if (!string.IsNullOrEmpty(referrer)) cmd.Parameters.AddWithValue("@Referrer", "%" + referrer + "%");

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

        public EmployeeReferral GetById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM EmployeeReferrals WHERE Id = @Id AND IsDeleted = 0";
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

        private EmployeeReferral MapRow(System.Data.Common.DbDataReader reader)
        {
            return new EmployeeReferral
            {
                Id = Convert.ToInt32(reader["Id"]),
                FirstName = reader["FirstName"]?.ToString() ?? string.Empty,
                LastName = reader["LastName"]?.ToString() ?? string.Empty,
                ReferralDate = Convert.ToDateTime(reader["ReferralDate"]),
                Status = reader["Status"]?.ToString() ?? "Pending",
                ForDesignation = reader["ForDesignation"]?.ToString() ?? string.Empty,
                Email = reader["Email"]?.ToString() ?? string.Empty,
                CurrentEmployer = reader["CurrentEmployer"]?.ToString() ?? string.Empty,
                ContactNo = reader["ContactNo"]?.ToString() ?? string.Empty,
                CurrentJobTitle = reader["CurrentJobTitle"]?.ToString() ?? string.Empty,
                ResumeLink = reader["ResumeLink"]?.ToString() ?? string.Empty,
                ResumeAttachment = reader["ResumeAttachment"]?.ToString() ?? string.Empty,
                ResumeAttachmentName = reader["ResumeAttachmentName"]?.ToString() ?? string.Empty,
                Referrer = reader["Referrer"]?.ToString() ?? string.Empty,
                IsApplicableForReferralBonus = Convert.ToBoolean(reader["IsApplicableForReferralBonus"]),
                QualifiedReason = reader["QualifiedReason"]?.ToString() ?? string.Empty,
                WorkReferences = reader["WorkReferences"]?.ToString() ?? string.Empty
            };
        }

        public int Insert(EmployeeReferral r)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    INSERT INTO EmployeeReferrals
                        (FirstName, LastName, ReferralDate, Status, ForDesignation, Email,
                         CurrentEmployer, ContactNo, CurrentJobTitle, ResumeLink, ResumeAttachment, ResumeAttachmentName,
                         Referrer, IsApplicableForReferralBonus, QualifiedReason, WorkReferences)
                    OUTPUT INSERTED.Id
                    VALUES
                        (@FirstName, @LastName, @ReferralDate, @Status, @ForDesignation, @Email,
                         @CurrentEmployer, @ContactNo, @CurrentJobTitle, @ResumeLink, @ResumeAttachment, @ResumeAttachmentName,
                         @Referrer, @IsApplicableForReferralBonus, @QualifiedReason, @WorkReferences)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FirstName", r.FirstName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@LastName", r.LastName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ReferralDate", r.ReferralDate);
                    cmd.Parameters.AddWithValue("@Status", r.Status ?? "Pending");
                    cmd.Parameters.AddWithValue("@ForDesignation", r.ForDesignation ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", r.Email ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@CurrentEmployer", r.CurrentEmployer ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ContactNo", r.ContactNo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@CurrentJobTitle", r.CurrentJobTitle ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ResumeLink", r.ResumeLink ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ResumeAttachment", r.ResumeAttachment ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ResumeAttachmentName", r.ResumeAttachmentName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Referrer", r.Referrer ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsApplicableForReferralBonus", r.IsApplicableForReferralBonus);
                    cmd.Parameters.AddWithValue("@QualifiedReason", r.QualifiedReason ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@WorkReferences", r.WorkReferences ?? (object)DBNull.Value);

                    conn.Open();
                    int newId = (int)cmd.ExecuteScalar();
                    return newId;
                }
            }
        }
    }
}
