
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class KRAService
    {
        private readonly string _connectionString;

        public KRAService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<KRA> GetAll(string idFilter = null)
        {
            var list = new List<KRA>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT Id, KRACode, ErpName, Title, Description
                    FROM KRAs
                    WHERE IsDeleted = 0";

                if (!string.IsNullOrWhiteSpace(idFilter))
                    query += " AND (KRACode LIKE @Filter OR ErpName LIKE @Filter)";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrWhiteSpace(idFilter))
                        cmd.Parameters.AddWithValue("@Filter", "%" + idFilter + "%");

                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(MapReader(reader));
                        }
                    }
                }
            }

            return list;
        }

        public KRA GetById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                const string query = @"
                    SELECT Id, KRACode, ErpName, Title, Description
                    FROM KRAs
                    WHERE Id = @Id AND IsDeleted = 0";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        return reader.Read() ? MapReader(reader) : null;
                    }
                }
            }
        }

        private KRA MapReader(SqlDataReader reader)
        {
            return new KRA
            {
                Id = Convert.ToInt32(reader["Id"]),
                KRACode = reader["KRACode"]?.ToString() ?? "",
                ErpName = reader["ErpName"]?.ToString() ?? "",
                Title = reader["Title"]?.ToString() ?? "",
                Description = reader["Description"]?.ToString() ?? ""
            };
        }

        public string GetNextCode()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                const string query = @"
                    SELECT ISNULL(MAX(Id), 0) + 1 FROM KRAs";

                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    int number = Convert.ToInt32(cmd.ExecuteScalar());

                    return $"KRA-{number:D5}";
                }
            }
        }

        // Retained for compatibility with your existing code.
        public int Insert(KRA kra)
        {
            if (kra == null || string.IsNullOrWhiteSpace(kra.Title))
                throw new Exception("KRA Title is required.");

            using (var conn = new SqlConnection(_connectionString))
            {
                const string query = @"
                    INSERT INTO KRAs
                    (KRACode, ErpName, Title, Description, IsDeleted)
                    OUTPUT INSERTED.Id
                    VALUES
                    (@KRACode, @ErpName, @Title, @Description, 0)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@KRACode", (object)kra.KRACode ?? DBNull.Value);

                    cmd.Parameters.AddWithValue(
                        "@ErpName", (object)kra.ErpName ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@Title", kra.Title);

                    cmd.Parameters.AddWithValue(
                        "@Description", (object)kra.Description ?? DBNull.Value);

                    conn.Open();

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public int GetLocalIdByErpName(string erpName)
        {
            if (string.IsNullOrWhiteSpace(erpName))
                return 0;

            using (var conn = new SqlConnection(_connectionString))
            {
                const string query = @"
                    SELECT TOP 1 Id
                    FROM KRAs
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
                    UPDATE KRAs
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

        private void MarkMissingErpRecordsDeleted(HashSet<string> erpNames)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                if (erpNames == null || erpNames.Count == 0)
                {
                    using (var cmd = new SqlCommand(@"
                        UPDATE KRAs
                        SET IsDeleted = 1
                        WHERE IsDeleted = 0
                          AND NULLIF(ErpName, '') IS NOT NULL",
                        conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    return;
                }

                var parameterNames = new List<string>();
                int index = 0;

                foreach (string erpName in erpNames)
                {
                    string parameterName = "@ErpName" + index++;
                    parameterNames.Add(parameterName);
                }

                string query = @"
                    UPDATE KRAs
                    SET IsDeleted = 1
                    WHERE IsDeleted = 0
                      AND NULLIF(ErpName, '') IS NOT NULL
                      AND ErpName NOT IN (" +
                    string.Join(",", parameterNames) + ")";

                using (var cmd = new SqlCommand(query, conn))
                {
                    index = 0;

                    foreach (string erpName in erpNames)
                    {
                        cmd.Parameters.AddWithValue(
                            "@ErpName" + index++,
                            erpName);
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Insert or update local HRMS record using ERPNext identity.
        public int InsertOrUpdateFromErpNext(KRA model)
        {
            if (model == null ||
                string.IsNullOrWhiteSpace(model.ErpName) ||
                string.IsNullOrWhiteSpace(model.Title))
            {
                throw new Exception("ERPNext returned incomplete KRA details.");
            }

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int existingId;

                        using (var cmd = new SqlCommand(@"
                            SELECT TOP 1 Id
                            FROM KRAs WITH (UPDLOCK, HOLDLOCK)
                            WHERE ErpName = @ErpName
                            AND IsDeleted = 0",
                            conn, transaction))
                        {
                            cmd.Parameters.AddWithValue(
                                "@ErpName", model.ErpName);

                            object result = cmd.ExecuteScalar();

                            existingId = result == null ||
                                         result == DBNull.Value
                                ? 0
                                : Convert.ToInt32(result);
                        }

                        if (existingId > 0)
                        {
                            using (var cmd = new SqlCommand(@"
                                UPDATE KRAs
                                SET Title = @Title,
                                    Description = @Description
                                WHERE Id = @Id",
                                conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Id", existingId);
                                cmd.Parameters.AddWithValue("@Title", model.Title);
                                cmd.Parameters.AddWithValue(
                                    "@Description",
                                    (object)model.Description ?? DBNull.Value);

                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            return existingId;
                        }

                        int nextNumber;

                        using (var cmd = new SqlCommand(@"
                            SELECT ISNULL(MAX(Id), 0) + 1
                            FROM KRAs WITH (TABLOCKX, HOLDLOCK)",
                            conn, transaction))
                        {
                            nextNumber = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        string localCode = $"KRA-{nextNumber:D5}";

                        int newId;

                        using (var cmd = new SqlCommand(@"
                            INSERT INTO KRAs
                            (KRACode, ErpName, Title, Description, IsDeleted)
                            OUTPUT INSERTED.Id
                            VALUES
                            (@Code, @ErpName, @Title, @Description, 0)",
                            conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Code", localCode);
                            cmd.Parameters.AddWithValue("@ErpName", model.ErpName);
                            cmd.Parameters.AddWithValue("@Title", model.Title);
                            cmd.Parameters.AddWithValue(
                                "@Description",
                                (object)model.Description ?? DBNull.Value);

                            newId = Convert.ToInt32(cmd.ExecuteScalar());
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

        public async Task<int> SyncOneFromErpNextAsync(
            ErpNextService erpNextService,
            string erpName)
        {
            if (string.IsNullOrWhiteSpace(erpName))
                throw new Exception("ERPNext KRA name is required.");

            string response = await erpNextService.GetAsync(
                "/api/resource/KRA/" + Uri.EscapeDataString(erpName));

            JObject parsed = JObject.Parse(response);
            JToken doc = parsed["data"];

            if (doc == null)
                throw new Exception("KRA not found in ERPNext: " + erpName);

            var model = new KRA
            {
                ErpName = doc["name"]?.ToString() ?? "",
                Title = doc["title"]?.ToString() ?? "",
                Description = doc["description"]?.ToString() ?? ""
            };

            return InsertOrUpdateFromErpNext(model);
        }

        public async Task<int> SyncAllFromErpNextAsync(
            ErpNextService erpNextService)
        {
            const int pageSize = 100;

            int start = 0;
            int synced = 0;
            var erpNames = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            while (true)
            {
                string url =
                    "/api/resource/KRA" +
                    "?fields=%5B%22name%22%5D" +
                    "&limit_start=" + start +
                    "&limit_page_length=" + pageSize;

                string response = await erpNextService.GetAsync(url);

                JObject parsed = JObject.Parse(response);

                JArray records = parsed["data"] as JArray;

                if (records == null)
                    throw new Exception("Invalid ERPNext KRA list response.");

                foreach (JToken row in records)
                {
                    string erpName = row["name"]?.ToString();

                    if (string.IsNullOrWhiteSpace(erpName))
                        throw new Exception(
                            "ERPNext returned a KRA without its document name.");

                    erpNames.Add(erpName);

                    try
                    {
                        await SyncOneFromErpNextAsync(
                            erpNextService, erpName);

                        synced++;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            "KRA sync failed for '" + erpName +
                            "': " + ex.Message, ex);
                    }
                }

                if (records.Count < pageSize)
                    break;

                start += records.Count;
            }

            MarkMissingErpRecordsDeleted(erpNames);

            return synced;
        }
    }
}
