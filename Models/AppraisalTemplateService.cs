using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Erp_Aits.Services
{
    public class AppraisalTemplateService
    {
        private readonly string _connectionString;

        public AppraisalTemplateService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<AppraisalTemplateListRow> GetAll(string idFilter)
        {
            var list = new List<AppraisalTemplateListRow>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT Id, AppraisalTemplateTitle FROM AppraisalTemplates WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(idFilter))
                    query += " AND CAST(Id AS VARCHAR) LIKE @IdFilter";

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
                            list.Add(new AppraisalTemplateListRow
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                AppraisalTemplateTitle = reader["AppraisalTemplateTitle"]?.ToString() ?? string.Empty
                            });
                        }
                    }
                }
            }

            return list;
        }

        public AppraisalTemplate GetById(int id)
        {
            AppraisalTemplate model = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string query = "SELECT * FROM AppraisalTemplates WHERE Id = @Id AND IsDeleted = 0";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model = new AppraisalTemplate
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                ErpName = reader["ErpName"]?.ToString() ?? string.Empty,
                                AppraisalTemplateTitle = reader["AppraisalTemplateTitle"]?.ToString() ?? string.Empty,
                                Description = reader["Description"]?.ToString() ?? string.Empty
                            };
                        }
                    }
                }

                if (model == null) return null;

                string q2 = "SELECT Id, AppraisalTemplateId, KRA, Weightage FROM AppraisalTemplateKRAs WHERE AppraisalTemplateId = @Id";
                using (var cmd = new SqlCommand(q2, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.KRAs.Add(new AppraisalTemplateKRA
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                AppraisalTemplateId = Convert.ToInt32(reader["AppraisalTemplateId"]),
                                KRA = reader["KRA"]?.ToString() ?? string.Empty,
                                Weightage = reader["Weightage"] == DBNull.Value
    ? 0m
    : Convert.ToDecimal(reader["Weightage"])
                            });
                        }
                    }
                }

                string q3 = "SELECT Id, AppraisalTemplateId, Criteria, Weightage FROM AppraisalTemplateRatingCriteria WHERE AppraisalTemplateId = @Id";
                using (var cmd = new SqlCommand(q3, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.RatingCriteria.Add(new AppraisalTemplateRatingCriterion
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                AppraisalTemplateId = Convert.ToInt32(reader["AppraisalTemplateId"]),
                                Criteria = reader["Criteria"]?.ToString() ?? string.Empty,
                                Weightage = reader["Weightage"] == DBNull.Value
    ? 0m
    : Convert.ToDecimal(reader["Weightage"])
                            });
                        }
                    }
                }
            }

            return model;
        }


        public int GetLocalIdByErpName(string erpName)
        {
            if (string.IsNullOrWhiteSpace(erpName))
                return 0;

            using (var conn = new SqlConnection(_connectionString))
            {
                const string query = @"
            SELECT TOP 1 Id
            FROM AppraisalTemplates
            WHERE ErpName = @ErpName
              AND IsDeleted = 0";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ErpName", erpName);

                    conn.Open();

                    object result = cmd.ExecuteScalar();

                    return result == null || result == DBNull.Value
                        ? 0
                        : Convert.ToInt32(result);
                }
            }
        }

        public void MarkDeletedById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                const string query = @"
                    UPDATE AppraisalTemplates
                    SET IsDeleted = 1
                    WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }


        private AppraisalTemplate MapErpAppraisalTemplate(
            Newtonsoft.Json.Linq.JToken doc)
        {
            if (doc == null)
                throw new Exception("ERPNext returned an empty Appraisal Template.");

            var model = new AppraisalTemplate
            {
                ErpName = doc["name"]?.ToString() ?? "",
                AppraisalTemplateTitle =
                    doc["template_title"]?.ToString() ?? "",
                Description = doc["description"]?.ToString() ?? ""
            };

            var goals = doc["goals"] as Newtonsoft.Json.Linq.JArray;

            if (goals != null)
            {
                foreach (var row in goals)
                {
                    model.KRAs.Add(new AppraisalTemplateKRA
                    {
                        KRA = row["key_result_area"]?.ToString() ?? "",
                        Weightage = ParseErpWeightage(row["per_weightage"])
                    });
                }
            }

            var criteria = doc["rating_criteria"]
                as Newtonsoft.Json.Linq.JArray;

            if (criteria != null)
            {
                foreach (var row in criteria)
                {
                    model.RatingCriteria.Add(
                        new AppraisalTemplateRatingCriterion
                        {
                            Criteria = row["criteria"]?.ToString() ?? "",
                            Weightage = ParseErpWeightage(
                                row["per_weightage"])
                        });
                }
            }

            return model;
        }


        private decimal ParseErpWeightage(
            Newtonsoft.Json.Linq.JToken value)
        {
            if (value == null ||
                value.Type == Newtonsoft.Json.Linq.JTokenType.Null ||
                string.IsNullOrWhiteSpace(value.ToString()))
            {
                throw new Exception(
                    "ERPNext weightage (per_weightage) is missing.");
            }

            if (decimal.TryParse(
                value.ToString(),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out decimal weightage))
            {
                return weightage;
            }

            throw new Exception(
                "Invalid ERPNext weightage: " + value);
        }
















        public void UpdateFromErpNext(AppraisalTemplate model)
        {
            int localId = GetLocalIdByErpName(model.ErpName);

            if (localId == 0)
                throw new Exception(
                    "Appraisal Template not found in HRMS: " + model.ErpName);

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string updateQuery = @"
                    UPDATE AppraisalTemplates
                    SET AppraisalTemplateTitle = @Title,
                        Description = @Description
                    WHERE Id = @Id";

                        using (var cmd = new SqlCommand(
                            updateQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Id", localId);

                            cmd.Parameters.AddWithValue(
                                "@Title",
                                (object)model.AppraisalTemplateTitle ?? DBNull.Value);

                            cmd.Parameters.AddWithValue(
                                "@Description",
                                (object)model.Description ?? DBNull.Value);

                            cmd.ExecuteNonQuery();
                        }

                        // Refresh existing KRA and Rating Criteria rows.

                        using (var cmd = new SqlCommand(@"
                    DELETE FROM AppraisalTemplateKRAs
                    WHERE AppraisalTemplateId = @Id;

                    DELETE FROM AppraisalTemplateRatingCriteria
                    WHERE AppraisalTemplateId = @Id;",
                            conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Id", localId);
                            cmd.ExecuteNonQuery();
                        }

                        foreach (var kra in model.KRAs
                            ?? new List<AppraisalTemplateKRA>())
                        {
                            using (var cmd = new SqlCommand(@"
                        INSERT INTO AppraisalTemplateKRAs
                        (AppraisalTemplateId, KRA, Weightage)
                        VALUES (@Id, @KRA, @Weightage)",
                                conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Id", localId);
                                cmd.Parameters.AddWithValue("@KRA", kra.KRA);
                                cmd.Parameters.AddWithValue(
                                    "@Weightage", kra.Weightage);

                                cmd.ExecuteNonQuery();
                            }
                        }

                        foreach (var criterion in model.RatingCriteria
                            ?? new List<AppraisalTemplateRatingCriterion>())
                        {
                            using (var cmd = new SqlCommand(@"
                        INSERT INTO AppraisalTemplateRatingCriteria
                        (AppraisalTemplateId, Criteria, Weightage)
                        VALUES (@Id, @Criteria, @Weightage)",
                                conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Id", localId);
                                cmd.Parameters.AddWithValue(
                                    "@Criteria", criterion.Criteria);
                                cmd.Parameters.AddWithValue(
                                    "@Weightage", criterion.Weightage);

                                cmd.ExecuteNonQuery();
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




        public async Task SyncOneFromErpNextAsync(
            ErpNextService erpNextService,
            string erpName)
        {
            if (string.IsNullOrWhiteSpace(erpName))
            {
                throw new Exception(
                    "ERPNext Appraisal Template name is required.");
            }

            // Fetch the complete Appraisal Template from ERPNext.
            string url =
                "/api/resource/Appraisal%20Template/" +
                Uri.EscapeDataString(erpName);

            string response = await erpNextService.GetAsync(url);

            var json = Newtonsoft.Json.Linq.JObject.Parse(response);

            var doc = json["data"];

            if (doc == null)
            {
                throw new Exception(
                    "Appraisal Template not found in ERPNext.");
            }

            // Map ERP fields to the existing HRMS model.
            AppraisalTemplate model = MapErpAppraisalTemplate(doc);

            if (string.IsNullOrWhiteSpace(model.ErpName) ||
                string.IsNullOrWhiteSpace(model.AppraisalTemplateTitle))
            {
                throw new Exception(
                    "ERPNext returned incomplete Appraisal Template details.");
            }

            // Update an existing HRMS record or insert a new record.
            int existingId = GetLocalIdByErpName(model.ErpName);

            if (existingId > 0)
            {
                UpdateFromErpNext(model);
            }
            else
            {
                Insert(
                    model,
                    model.KRAs.Select(x => x.KRA).ToList(),
                    model.KRAs.Select(x => x.Weightage).ToList(),
                    model.RatingCriteria.Select(x => x.Criteria).ToList(),
                    model.RatingCriteria.Select(x => x.Weightage).ToList()
                );
            }
        }




        public async Task<int> SyncAllFromErpNextAsync(
            ErpNextService erpNextService)
        {
            int start = 0;
            const int pageSize = 100;
            int syncedCount = 0;

            while (true)
            {
                string url =
                    "/api/resource/Appraisal%20Template" +
                    "?fields=%5B%22name%22%5D" +
                    "&limit_start=" + start +
                    "&limit_page_length=" + pageSize;

                string response = await erpNextService.GetAsync(url);

                var json = Newtonsoft.Json.Linq.JObject.Parse(response);

                var records = json["data"] as Newtonsoft.Json.Linq.JArray;

                if (records == null)
                    throw new Exception("Invalid ERPNext Appraisal Template response.");

                foreach (var item in records)
                {
                    string erpName = item["name"]?.ToString();

                    if (string.IsNullOrWhiteSpace(erpName))
                        continue;

                    try
                    {
                        await SyncOneFromErpNextAsync(
                            erpNextService,
                            erpName
                        );

                        syncedCount++;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            "Sync failed for Appraisal Template '" +
                            erpName + "': " + ex.Message,
                            ex
                        );
                    }
                }

                if (records.Count < pageSize)
                    break;

                start += records.Count;
            }

            return syncedCount;
        }







        public int Insert(AppraisalTemplate model,
            List<string> kraNames, List<decimal> kraWeightages,
            List<string> criteriaNames, List<decimal> criteriaWeightages)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int newId;

                        string query = @"
    INSERT INTO AppraisalTemplates
    (ErpName, AppraisalTemplateTitle, Description, IsDeleted, CreatedDate)
    OUTPUT INSERTED.Id
    VALUES (@ErpName, @Title, @Description, 0, GETDATE())";

                        using (var cmd = new SqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue(
                                "@ErpName",
                                (object)model.ErpName ?? DBNull.Value
                            );

                            cmd.Parameters.AddWithValue(
                                "@Title",
                                (object)model.AppraisalTemplateTitle ?? DBNull.Value
                            );

                            cmd.Parameters.AddWithValue(
                                "@Description",
                                (object)model.Description ?? DBNull.Value
                            );

                            newId = (int)cmd.ExecuteScalar();
                        }

                        if (kraNames != null)
                        {
                            for (int i = 0; i < kraNames.Count; i++)
                            {
                                if (string.IsNullOrWhiteSpace(kraNames[i])) continue;
                                decimal weightage = (kraWeightages != null && i < kraWeightages.Count) ? kraWeightages[i] : 0;

                                string q = "INSERT INTO AppraisalTemplateKRAs (AppraisalTemplateId, KRA, Weightage) VALUES (@Tid, @KRA, @Weightage)";
                                using (var cmd = new SqlCommand(q, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@Tid", newId);
                                    cmd.Parameters.AddWithValue("@KRA", kraNames[i]);
                                    cmd.Parameters.AddWithValue("@Weightage", weightage);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        if (criteriaNames != null)
                        {
                            for (int i = 0; i < criteriaNames.Count; i++)
                            {
                                if (string.IsNullOrWhiteSpace(criteriaNames[i])) continue;
                                decimal weightage = (criteriaWeightages != null && i < criteriaWeightages.Count) ? criteriaWeightages[i] : 0;

                                string q = "INSERT INTO AppraisalTemplateRatingCriteria (AppraisalTemplateId, Criteria, Weightage) VALUES (@Tid, @Criteria, @Weightage)";
                                using (var cmd = new SqlCommand(q, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@Tid", newId);
                                    cmd.Parameters.AddWithValue("@Criteria", criteriaNames[i]);
                                    cmd.Parameters.AddWithValue("@Weightage", weightage);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                        return newId;
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
