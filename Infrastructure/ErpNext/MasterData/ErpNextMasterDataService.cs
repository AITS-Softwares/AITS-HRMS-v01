using System.Data;
using Newtonsoft.Json.Linq;

namespace Erp_Aits.Infrastructure.ErpNext.MasterData
{
    public class ErpNextMasterDataService : IErpNextMasterDataService
    {
        private static readonly IReadOnlyDictionary<string, ErpNextMasterDefinition> MasterDefinitions =
            new Dictionary<string, ErpNextMasterDefinition>(StringComparer.OrdinalIgnoreCase)
            {
                ["company"] = new()
                {
                    Key = "company",
                    ModuleName = "Master - Company",
                    DocType = "Company",
                    DisplayField = "company_name",
                    AllowCreateFromHrms = false
                },
                ["branch"] = new()
                {
                    Key = "branch",
                    ModuleName = "Master - Branch",
                    DocType = "Branch",
                    DisplayField = "branch",
                    AllowCreateFromHrms = true
                },
                ["department"] = new()
                {
                    Key = "department",
                    ModuleName = "Master - Department",
                    DocType = "Department",
                    DisplayField = "department_name",
                    AllowCreateFromHrms = true
                },
                ["designation"] = new()
                {
                    Key = "designation",
                    ModuleName = "Master - Designation",
                    DocType = "Designation",
                    DisplayField = "designation_name",
                    AllowCreateFromHrms = true
                },
                ["employee-grade"] = new()
                {
                    Key = "employee-grade",
                    ModuleName = "Master - Employee Grade",
                    DocType = "Employee Grade",
                    DisplayField = "employee_grade",
                    AllowCreateFromHrms = true
                },
                ["employee-group"] = new()
                {
                    Key = "employee-group",
                    ModuleName = "Master - Employee Group",
                    DocType = "Employee Group",
                    DisplayField = "employee_group_name",
                    AllowCreateFromHrms = true
                },
                ["holiday-list"] = new()
                {
                    Key = "holiday-list",
                    ModuleName = "Master - Holiday List",
                    DocType = "Holiday List",
                    DisplayField = "holiday_list_name",
                    AllowCreateFromHrms = false
                },
                ["shift-type"] = new()
                {
                    Key = "shift-type",
                    ModuleName = "Master - Shift Type",
                    DocType = "Shift Type",
                    DisplayField = "shift_type_name",
                    AllowCreateFromHrms = true
                },
                ["leave-type"] = new()
                {
                    Key = "leave-type",
                    ModuleName = "Master - Leave Type",
                    DocType = "Leave Type",
                    DisplayField = "leave_type_name",
                    AllowCreateFromHrms = true
                },
                ["cost-center"] = new()
                {
                    Key = "cost-center",
                    ModuleName = "Master - Cost Center",
                    DocType = "Cost Center",
                    DisplayField = "cost_center_name",
                    AllowCreateFromHrms = false
                }
            };

        private readonly IErpNextClient _erpNextClient;
        private readonly IDbConnection _dbConnection;

        public ErpNextMasterDataService(IErpNextClient erpNextClient, IDbConnection dbConnection)
        {
            _erpNextClient = erpNextClient;
            _dbConnection = dbConnection;
        }

        public IReadOnlyCollection<ErpNextMasterDefinition> GetSupportedMasters()
        {
            return MasterDefinitions.Values.OrderBy(x => x.Key).ToList();
        }

        public async Task<IReadOnlyList<MasterDataRecord>> GetFromErpNextAsync(
            string masterType,
            int limit = 100,
            CancellationToken cancellationToken = default)
        {
            var definition = GetDefinition(masterType);
            string response = await _erpNextClient.GetListAsync(
                definition.DocType,
                new[] { "name", definition.DisplayField },
                limit,
                cancellationToken);

            var parsed = JObject.Parse(response);
            var records = new List<MasterDataRecord>();

            foreach (JToken row in parsed["data"] ?? new JArray())
            {
                string name = row["name"]?.ToString() ?? string.Empty;
                string displayName = row[definition.DisplayField]?.ToString() ?? name;

                if (!string.IsNullOrWhiteSpace(name))
                {
                    records.Add(new MasterDataRecord
                    {
                        Name = name,
                        DisplayName = displayName,
                        DocType = definition.DocType
                    });
                }
            }

            return records;
        }

        public async Task<MasterDataSyncResult> SyncFromErpNextAsync(
            string masterType,
            int limit = 500,
            CancellationToken cancellationToken = default)
        {
            var definition = GetDefinition(masterType);
            IReadOnlyList<MasterDataRecord> records = await GetFromErpNextAsync(masterType, limit, cancellationToken);

            EnsureConnectionOpen();

            int inserted = 0;
            int updated = 0;

            foreach (MasterDataRecord record in records)
            {
                if (UpsertMapping(definition, record))
                {
                    inserted++;
                }
                else
                {
                    updated++;
                }
            }

            InsertAuditLog(
                Guid.NewGuid().ToString("N"),
                definition.ModuleName,
                "SYNC_FROM_ERPNEXT",
                definition.DocType,
                null,
                "Success",
                $"Fetched {records.Count} {definition.DocType} record(s). Inserted mappings: {inserted}. Updated mappings: {updated}.");

            return new MasterDataSyncResult
            {
                MasterType = definition.Key,
                DocType = definition.DocType,
                RecordsFetched = records.Count,
                RecordsInserted = inserted,
                RecordsUpdated = updated
            };
        }

        public async Task<IReadOnlyList<MasterDataSyncResult>> SyncAllFromErpNextAsync(CancellationToken cancellationToken = default)
        {
            var results = new List<MasterDataSyncResult>();

            foreach (ErpNextMasterDefinition definition in MasterDefinitions.Values)
            {
                results.Add(await SyncFromErpNextAsync(definition.Key, 500, cancellationToken));
            }

            return results;
        }

        public async Task<string> CreateInErpNextAsync(
            string masterType,
            CreateMasterDataRequest request,
            CancellationToken cancellationToken = default)
        {
            var definition = GetDefinition(masterType);

            if (!definition.AllowCreateFromHrms)
            {
                throw new InvalidOperationException($"{definition.DocType} creation from HRMS is disabled for Phase 2.");
            }

            if (string.IsNullOrWhiteSpace(request.DisplayName))
            {
                throw new ArgumentException("DisplayName is required.");
            }

            var payload = new Dictionary<string, object>
            {
                [definition.DisplayField] = request.DisplayName.Trim()
            };

            string response = await _erpNextClient.CreateDocumentAsync(definition.DocType, payload, cancellationToken);
            var parsed = JObject.Parse(response);
            string documentName = parsed["data"]?["name"]?.ToString() ?? request.DisplayName.Trim();

            var record = new MasterDataRecord
            {
                Name = documentName,
                DisplayName = request.DisplayName.Trim(),
                DocType = definition.DocType
            };

            EnsureConnectionOpen();
            UpsertMapping(definition, record);

            InsertAuditLog(
                Guid.NewGuid().ToString("N"),
                definition.ModuleName,
                "CREATE_IN_ERPNEXT",
                definition.DocType,
                documentName,
                "Success",
                $"Created {definition.DocType} '{documentName}' from HRMS.");

            return response;
        }

        private static ErpNextMasterDefinition GetDefinition(string masterType)
        {
            if (!MasterDefinitions.TryGetValue(masterType, out ErpNextMasterDefinition? definition))
            {
                string supported = string.Join(", ", MasterDefinitions.Keys.OrderBy(x => x));
                throw new ArgumentException($"Unsupported master type '{masterType}'. Supported values: {supported}.");
            }

            return definition;
        }

        private bool UpsertMapping(ErpNextMasterDefinition definition, MasterDataRecord record)
        {
            using IDbCommand existsCommand = _dbConnection.CreateCommand();
            existsCommand.CommandText = @"
SELECT COUNT(1)
FROM dbo.IntegrationRecordMap
WHERE ErpNextDocType = @ErpNextDocType
  AND ErpNextDocumentName = @ErpNextDocumentName";
            AddParameter(existsCommand, "@ErpNextDocType", definition.DocType);
            AddParameter(existsCommand, "@ErpNextDocumentName", record.Name);

            int existingCount = Convert.ToInt32(existsCommand.ExecuteScalar());

            if (existingCount == 0)
            {
                using IDbCommand insertCommand = _dbConnection.CreateCommand();
                insertCommand.CommandText = @"
INSERT INTO dbo.IntegrationRecordMap
(
    ModuleName,
    LocalTableName,
    LocalRecordId,
    ErpNextDocType,
    ErpNextDocumentName,
    LastSyncDirection,
    LastSyncStatus,
    LastSyncedAtUtc,
    UpdatedAtUtc
)
VALUES
(
    @ModuleName,
    @LocalTableName,
    @LocalRecordId,
    @ErpNextDocType,
    @ErpNextDocumentName,
    'ERPNextToHRMS',
    'Success',
    SYSUTCDATETIME(),
    SYSUTCDATETIME()
)";
                AddParameter(insertCommand, "@ModuleName", definition.ModuleName);
                AddParameter(insertCommand, "@LocalTableName", DBNull.Value);
                AddParameter(insertCommand, "@LocalRecordId", record.DisplayName);
                AddParameter(insertCommand, "@ErpNextDocType", definition.DocType);
                AddParameter(insertCommand, "@ErpNextDocumentName", record.Name);
                insertCommand.ExecuteNonQuery();
                return true;
            }

            using IDbCommand updateCommand = _dbConnection.CreateCommand();
            updateCommand.CommandText = @"
UPDATE dbo.IntegrationRecordMap
SET ModuleName = @ModuleName,
    LocalRecordId = @LocalRecordId,
    LastSyncDirection = 'ERPNextToHRMS',
    LastSyncStatus = 'Success',
    LastSyncedAtUtc = SYSUTCDATETIME(),
    UpdatedAtUtc = SYSUTCDATETIME()
WHERE ErpNextDocType = @ErpNextDocType
  AND ErpNextDocumentName = @ErpNextDocumentName";
            AddParameter(updateCommand, "@ModuleName", definition.ModuleName);
            AddParameter(updateCommand, "@LocalRecordId", record.DisplayName);
            AddParameter(updateCommand, "@ErpNextDocType", definition.DocType);
            AddParameter(updateCommand, "@ErpNextDocumentName", record.Name);
            updateCommand.ExecuteNonQuery();

            return false;
        }

        private void InsertAuditLog(
            string correlationId,
            string moduleName,
            string operation,
            string docType,
            string? documentName,
            string status,
            string message)
        {
            using IDbCommand command = _dbConnection.CreateCommand();
            command.CommandText = @"
INSERT INTO dbo.SyncAuditLog
(
    CorrelationId,
    ModuleName,
    Operation,
    ErpNextDocType,
    ErpNextDocumentName,
    Status,
    Message
)
VALUES
(
    @CorrelationId,
    @ModuleName,
    @Operation,
    @ErpNextDocType,
    @ErpNextDocumentName,
    @Status,
    @Message
)";
            AddParameter(command, "@CorrelationId", correlationId);
            AddParameter(command, "@ModuleName", moduleName);
            AddParameter(command, "@Operation", operation);
            AddParameter(command, "@ErpNextDocType", docType);
            AddParameter(command, "@ErpNextDocumentName", documentName ?? (object)DBNull.Value);
            AddParameter(command, "@Status", status);
            AddParameter(command, "@Message", message);
            command.ExecuteNonQuery();
        }

        private void EnsureConnectionOpen()
        {
            if (_dbConnection.State != ConnectionState.Open)
            {
                _dbConnection.Open();
            }
        }

        private static void AddParameter(IDbCommand command, string name, object value)
        {
            IDbDataParameter parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }
    }
}
