using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{


    public class AppointmentLetterTemplateService
    {
        private readonly string _connectionString;

        public AppointmentLetterTemplateService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<AppointmentLetterTemplate> GetAllTemplates(string idFilter = null, string nameFilter = null)
        {
            var templates = new List<AppointmentLetterTemplate>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT Id, TemplateCode, TemplateName
                                  FROM AppointmentLetterTemplates WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(idFilter))
                    query += " AND TemplateCode LIKE @IdFilter";
                if (!string.IsNullOrEmpty(nameFilter))
                    query += " AND TemplateName LIKE @NameFilter";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(idFilter))
                        cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");
                    if (!string.IsNullOrEmpty(nameFilter))
                        cmd.Parameters.AddWithValue("@NameFilter", "%" + nameFilter + "%");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            templates.Add(new AppointmentLetterTemplate
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                TemplateCode = reader["TemplateCode"]?.ToString() ?? string.Empty,
                                TemplateName = reader["TemplateName"]?.ToString() ?? string.Empty
                            });
                        }
                    }
                }
            }

            return templates;
        }

        public string GetNextTemplateCode()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT COUNT(*) FROM AppointmentLetterTemplates";
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    int count = (int)cmd.ExecuteScalar() + 1;
                    return $"HR-ALT-{DateTime.Now.Year}-{count:D5}";
                }
            }
        }

        public AppointmentLetterTemplate GetById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM AppointmentLetterTemplates WHERE Id = @Id AND IsDeleted = 0";

                AppointmentLetterTemplate template = null;

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            template = new AppointmentLetterTemplate
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                TemplateCode = reader["TemplateCode"]?.ToString() ?? string.Empty,
                                TemplateName = reader["TemplateName"]?.ToString() ?? string.Empty,
                                Introduction = reader["Introduction"]?.ToString() ?? string.Empty,
                                ClosingNotes = reader["ClosingNotes"]?.ToString() ?? string.Empty
                            };
                        }
                    }

                    if (template != null)
                    {
                        template.Terms = GetTerms(conn, template.Id);
                    }
                }

                return template;
            }
        }

        private List<AppointmentLetterTemplateTerm> GetTerms(SqlConnection conn, int templateId)
        {
            var list = new List<AppointmentLetterTemplateTerm>();
            string query = "SELECT * FROM AppointmentLetterTemplateTerms WHERE TemplateId = @TemplateId";

            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@TemplateId", templateId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new AppointmentLetterTemplateTerm
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            TemplateId = templateId,
                            Title = reader["Title"]?.ToString() ?? string.Empty,
                            Description = reader["Description"]?.ToString() ?? string.Empty
                        });
                    }
                }
            }

            return list;
        }

        public void InsertTemplate(AppointmentLetterTemplate template)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int newId;

                        string headerQuery = @"
                            INSERT INTO AppointmentLetterTemplates
                                (TemplateCode, TemplateName, Introduction, ClosingNotes)
                            OUTPUT INSERTED.Id
                            VALUES
                                (@TemplateCode, @TemplateName, @Introduction, @ClosingNotes)";

                        using (var cmd = new SqlCommand(headerQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@TemplateCode", template.TemplateCode ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@TemplateName", template.TemplateName ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Introduction", template.Introduction ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@ClosingNotes", template.ClosingNotes ?? (object)DBNull.Value);

                            newId = (int)cmd.ExecuteScalar();
                        }

                        if (template.Terms != null)
                        {
                            foreach (var t in template.Terms)
                            {
                                if (string.IsNullOrEmpty(t.Title)) continue;

                                string q = @"INSERT INTO AppointmentLetterTemplateTerms
                                             (TemplateId, Title, Description)
                                             VALUES (@TemplateId, @Title, @Description)";
                                using (var cmd = new SqlCommand(q, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@TemplateId", newId);
                                    cmd.Parameters.AddWithValue("@Title", t.Title ?? (object)DBNull.Value);
                                    cmd.Parameters.AddWithValue("@Description", t.Description ?? (object)DBNull.Value);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
