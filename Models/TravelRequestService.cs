using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class TravelRequestService
    {
        private readonly string _connectionString;

        public TravelRequestService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<TravelRequest> GetAllTravelRequests(string idFilter = null, string employeeNameFilter = null)
        {
            var requests = new List<TravelRequest>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT Id, TravelRequestCode, Employee, EmployeeName
                                  FROM TravelRequests WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(idFilter))
                    query += " AND TravelRequestCode LIKE @IdFilter";
                if (!string.IsNullOrEmpty(employeeNameFilter))
                    query += " AND EmployeeName LIKE @EmployeeNameFilter";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(idFilter))
                        cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");
                    if (!string.IsNullOrEmpty(employeeNameFilter))
                        cmd.Parameters.AddWithValue("@EmployeeNameFilter", "%" + employeeNameFilter + "%");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            requests.Add(new TravelRequest
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                TravelRequestCode = reader["TravelRequestCode"]?.ToString() ?? string.Empty,
                                Employee = reader["Employee"]?.ToString() ?? string.Empty,
                                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty
                            });
                        }
                    }
                }
            }

            return requests;
        }

        public TravelRequest GetByCode(string code)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM TravelRequests WHERE TravelRequestCode = @Code AND IsDeleted = 0";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Code", code);
                    conn.Open();

                    TravelRequest request = null;

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            request = new TravelRequest
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                TravelRequestCode = reader["TravelRequestCode"]?.ToString() ?? string.Empty,
                                TravelType = reader["TravelType"]?.ToString() ?? string.Empty,
                                PurposeOfTravel = reader["PurposeOfTravel"]?.ToString() ?? string.Empty,
                                TravelFunding = reader["TravelFunding"]?.ToString() ?? string.Empty,
                                DetailsOfSponsor = reader["DetailsOfSponsor"]?.ToString() ?? string.Empty,
                                CopyOfInvitation = reader["CopyOfInvitation"]?.ToString() ?? string.Empty,
                                Employee = reader["Employee"]?.ToString() ?? string.Empty,
                                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
                                DateOfBirth = reader["DateOfBirth"] == DBNull.Value
                                    ? null
                                    : Convert.ToDateTime(reader["DateOfBirth"]),
                                IdentificationDocumentType = reader["IdentificationDocumentType"]?.ToString() ?? string.Empty,
                                ContactNumber = reader["ContactNumber"]?.ToString() ?? string.Empty,
                                IdentificationDocumentNumber = reader["IdentificationDocumentNumber"]?.ToString() ?? string.Empty,
                                ContactEmail = reader["ContactEmail"]?.ToString() ?? string.Empty,
                                PassportNumber = reader["PassportNumber"]?.ToString() ?? string.Empty,
                                Company = reader["Company"]?.ToString() ?? string.Empty,
                                OtherDetailsDescription = reader["OtherDetailsDescription"]?.ToString() ?? string.Empty,
                                CostCenter = reader["CostCenter"]?.ToString() ?? string.Empty,
                                NameOfOrganizer = reader["NameOfOrganizer"]?.ToString() ?? string.Empty,
                                AddressOfOrganizer = reader["AddressOfOrganizer"]?.ToString() ?? string.Empty,
                                EventOtherDetails = reader["EventOtherDetails"]?.ToString() ?? string.Empty
                            };
                        }
                    }

                    if (request != null)
                    {
                        request.Itineraries = GetItineraries(conn, request.Id);
                        request.Costings = GetCostings(conn, request.Id);
                    }

                    return request;
                }
            }
        }

        private List<TravelItinerary> GetItineraries(SqlConnection conn, int travelRequestId)
        {
            var list = new List<TravelItinerary>();
            string query = "SELECT * FROM TravelItineraries WHERE TravelRequestId = @Id";

            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", travelRequestId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new TravelItinerary
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            TravelRequestId = travelRequestId,
                            TravelFrom = reader["TravelFrom"]?.ToString() ?? string.Empty,
                            TravelTo = reader["TravelTo"]?.ToString() ?? string.Empty,
                            ModeOfTravel = reader["ModeOfTravel"]?.ToString() ?? string.Empty,
                            DepartureDatetime = reader["DepartureDatetime"] as DateTime?
                        });
                    }
                }
            }

            return list;
        }

        private List<TravelCosting> GetCostings(SqlConnection conn, int travelRequestId)
        {
            var list = new List<TravelCosting>();
            string query = "SELECT * FROM TravelCostings WHERE TravelRequestId = @Id";

            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", travelRequestId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new TravelCosting
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            TravelRequestId = travelRequestId,
                            ExpenseType = reader["ExpenseType"]?.ToString() ?? string.Empty,
                            SponsoredAmount = reader["SponsoredAmount"] != DBNull.Value ? Convert.ToDecimal(reader["SponsoredAmount"]) : 0,
                            FundedAmount = reader["FundedAmount"] != DBNull.Value ? Convert.ToDecimal(reader["FundedAmount"]) : 0,
                            TotalAmount = reader["TotalAmount"] != DBNull.Value ? Convert.ToDecimal(reader["TotalAmount"]) : 0
                        });
                    }
                }
            }

            return list;
        }

        public string GetNextTravelRequestCode()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT COUNT(*) FROM TravelRequests";
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    int count = (int)cmd.ExecuteScalar() + 1;
                    return $"HR-TRV-{DateTime.Now.Year}-{count:D5}";
                }
            }
        }



        public async Task SyncTravelRequestsFromErpNextAsync(
            ErpNextService erpNextService)
        {
            int start = 0;
            const int pageSize = 100;

            int inserted = 0;
            int updated = 0;
            var erpCodes = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            while (true)
            {
                string listUrl =
                    "/api/resource/Travel%20Request" +
                    "?fields=%5B%22name%22%5D" +
                    "&limit_start=" + start +
                    "&limit_page_length=" + pageSize;

                string json = await erpNextService.GetAsync(listUrl);

                var list = Newtonsoft.Json.Linq.JObject.Parse(json);

                var records = list["data"] as Newtonsoft.Json.Linq.JArray
                    ?? throw new Exception(
                        "ERPNext returned an invalid Travel Request list response.");

                foreach (var item in records)
                {
                    string code = item["name"]?.ToString();

                    if (string.IsNullOrWhiteSpace(code))
                        continue;

                    erpCodes.Add(code);

                    try
                    {
                        string detailUrl =
                            "/api/resource/Travel%20Request/" +
                            Uri.EscapeDataString(code);

                        string detailJson =
                            await erpNextService.GetAsync(detailUrl);

                        var document = Newtonsoft.Json.Linq.JObject
                            .Parse(detailJson)["data"];

                        if (document == null)
                            throw new Exception("ERP document data is missing.");

                        var request = MapErpTravelRequest(document, code);

                        bool exists = GetByCode(code) != null;

                        if (exists)
                        {
                            UpdateTravelRequestFromErp(request);
                            updated++;
                        }
                        else
                        {
                            InsertTravelRequest(request);
                            inserted++;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            "ERP → HRMS sync failed for Travel Request " +
                            code + ": " + ex.Message,
                            ex);
                    }
                }

                if (records.Count < pageSize)
                    break;

                start += records.Count;
            }

            System.Diagnostics.Debug.WriteLine(
                $"Travel Request sync complete. Inserted: {inserted}, Updated: {updated}");

            MarkDeletedIfMissingFromErpNext(erpCodes);
        }


        // MAP ERP DOCUMENT TO EXISTING HRMS MODEL

        public TravelRequest MapErpTravelRequest(
            Newtonsoft.Json.Linq.JToken doc,
            string code)
        {
            string Read(params string[] names)
            {
                foreach (string name in names)
                {
                    var token = doc[name];

                    if (token != null &&
                        token.Type != Newtonsoft.Json.Linq.JTokenType.Null)
                    {
                        return token.ToString();
                    }
                }

                return string.Empty;
            }

            DateTime? ReadDate(params string[] names)
            {
                string value = Read(names);

                return DateTime.TryParse(value, out var date)
                    ? date
                    : (DateTime?)null;
            }

            decimal ReadDecimal(
                Newtonsoft.Json.Linq.JToken row,
                string name)
            {
                string value = row[name]?.ToString();

                return decimal.TryParse(
                    value,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal amount)
                    ? amount
                    : 0m;
            }

            var request = new TravelRequest
            {
                TravelRequestCode = code,

                TravelType = Read("travel_type"),
                PurposeOfTravel = Read("purpose_of_travel"),
                TravelFunding = Read("travel_funding"),

                DetailsOfSponsor = Read("details_of_sponsor"),
                CopyOfInvitation = Read("travel_proof", "copy_of_invitation"),

                Employee = Read("employee"),
                EmployeeName = Read("employee_name"),

                DateOfBirth = ReadDate("date_of_birth"),

                IdentificationDocumentType =
                    Read("personal_id_type", "identification_document_type"),

                IdentificationDocumentNumber =
                    Read("personal_id_number", "identification_document_number"),

                ContactNumber = Read("cell_number", "contact_number"),

                ContactEmail = Read("prefered_email", "contact_email"),

                PassportNumber = Read("passport_number"),

                Company = Read("company"),

                OtherDetailsDescription =
                    Read("description", "other_details_description"),

                CostCenter = Read("cost_center"),

                NameOfOrganizer = Read("name_of_organizer"),

                AddressOfOrganizer = Read("address_of_organizer"),

                EventOtherDetails = Read("event_other_details"),

                Itineraries = new List<TravelItinerary>(),

                Costings = new List<TravelCosting>()
            };

            var itineraryRows =
                doc["itinerary"] as Newtonsoft.Json.Linq.JArray
                ?? doc["itineraries"] as Newtonsoft.Json.Linq.JArray;

            if (itineraryRows != null)
            {
                foreach (var row in itineraryRows)
                {
                    string departure =
                        row["departure_date"]?.ToString()
                        ?? row["departure_datetime"]?.ToString();

                    DateTime? departureDate =
                        DateTime.TryParse(departure, out var date)
                            ? date
                            : (DateTime?)null;

                    request.Itineraries.Add(new TravelItinerary
                    {
                        TravelFrom = row["travel_from"]?.ToString() ?? "",
                        TravelTo = row["travel_to"]?.ToString() ?? "",
                        ModeOfTravel = row["mode_of_travel"]?.ToString() ?? "",
                        DepartureDatetime = departureDate
                    });
                }
            }

            var costingRows =
                doc["costings"] as Newtonsoft.Json.Linq.JArray
                ?? doc["costing"] as Newtonsoft.Json.Linq.JArray;

            if (costingRows != null)
            {
                foreach (var row in costingRows)
                {
                    request.Costings.Add(new TravelCosting
                    {
                        ExpenseType = row["expense_type"]?.ToString() ?? "",

                        SponsoredAmount = ReadDecimal(
                            row, "sponsored_amount"),

                        FundedAmount = ReadDecimal(
                            row, "funded_amount"),

                        TotalAmount = ReadDecimal(
                            row, "total_amount")
                    });
                }
            }

            return request;
        }


        // UPDATE AN EXISTING HRMS RECORD AND ITS CHILD TABLES

        public void UpdateTravelRequestFromErp(TravelRequest request)
        {
            using (var conn = new Microsoft.Data.SqlClient.SqlConnection(
                _connectionString))
            {
                conn.Open();

                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int localId;

                        const string findSql = @"
                    SELECT Id
                    FROM TravelRequests
                    WHERE TravelRequestCode = @Code
                      AND IsDeleted = 0";

                        using (var find = new Microsoft.Data.SqlClient.SqlCommand(
                            findSql, conn, transaction))
                        {
                            find.Parameters.AddWithValue(
                                "@Code", request.TravelRequestCode);

                            object result = find.ExecuteScalar();

                            if (result == null || result == DBNull.Value)
                            {
                                throw new Exception(
                                    "Local record was not found for update: " +
                                    request.TravelRequestCode);
                            }

                            localId = Convert.ToInt32(result);
                        }

                        const string updateSql = @"
                    UPDATE TravelRequests SET
                        TravelType = @TravelType,
                        PurposeOfTravel = @PurposeOfTravel,
                        TravelFunding = @TravelFunding,
                        DetailsOfSponsor = @DetailsOfSponsor,
                        CopyOfInvitation = @CopyOfInvitation,
                        Employee = @Employee,
                        EmployeeName = @EmployeeName,
                        DateOfBirth = @DateOfBirth,
                        IdentificationDocumentType = @IdentificationDocumentType,
                        IdentificationDocumentNumber = @IdentificationDocumentNumber,
                        ContactNumber = @ContactNumber,
                        ContactEmail = @ContactEmail,
                        PassportNumber = @PassportNumber,
                        Company = @Company,
                        OtherDetailsDescription = @OtherDetailsDescription,
                        CostCenter = @CostCenter,
                        NameOfOrganizer = @NameOfOrganizer,
                        AddressOfOrganizer = @AddressOfOrganizer,
                        EventOtherDetails = @EventOtherDetails
                    WHERE Id = @Id";

                        using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(
                            updateSql, conn, transaction))
                        {
                            void Add(string key, object value)
                            {
                                cmd.Parameters.AddWithValue(
                                    key, value ?? DBNull.Value);
                            }

                            Add("@Id", localId);
                            Add("@TravelType", request.TravelType);
                            Add("@PurposeOfTravel", request.PurposeOfTravel);
                            Add("@TravelFunding", request.TravelFunding);
                            Add("@DetailsOfSponsor", request.DetailsOfSponsor);
                            Add("@CopyOfInvitation", request.CopyOfInvitation);

                            Add("@Employee", request.Employee);
                            Add("@EmployeeName", request.EmployeeName);
                            Add("@DateOfBirth", request.DateOfBirth);

                            Add("@IdentificationDocumentType",
                                request.IdentificationDocumentType);

                            Add("@IdentificationDocumentNumber",
                                request.IdentificationDocumentNumber);

                            Add("@ContactNumber", request.ContactNumber);
                            Add("@ContactEmail", request.ContactEmail);
                            Add("@PassportNumber", request.PassportNumber);
                            Add("@Company", request.Company);

                            Add("@OtherDetailsDescription",
                                request.OtherDetailsDescription);

                            Add("@CostCenter", request.CostCenter);
                            Add("@NameOfOrganizer", request.NameOfOrganizer);
                            Add("@AddressOfOrganizer", request.AddressOfOrganizer);
                            Add("@EventOtherDetails", request.EventOtherDetails);

                            cmd.ExecuteNonQuery();
                        }

                        // Replace local child rows using the latest ERP document.
                        // Both deletes and inserts are inside the same transaction.

                        using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(
                            @"DELETE FROM TravelItineraries
                      WHERE TravelRequestId = @Id;

                      DELETE FROM TravelCostings
                      WHERE TravelRequestId = @Id;",
                            conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Id", localId);
                            cmd.ExecuteNonQuery();
                        }

                        foreach (var row in request.Itineraries
                            ?? new List<TravelItinerary>())
                        {
                            using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(
                                @"INSERT INTO TravelItineraries
                          (TravelRequestId, TravelFrom, TravelTo,
                           ModeOfTravel, DepartureDatetime)
                          VALUES
                          (@Id, @From, @To, @Mode, @Departure)",
                                conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Id", localId);

                                cmd.Parameters.AddWithValue(
                                    "@From", (object)row.TravelFrom ?? DBNull.Value);

                                cmd.Parameters.AddWithValue(
                                    "@To", (object)row.TravelTo ?? DBNull.Value);

                                cmd.Parameters.AddWithValue(
                                    "@Mode", (object)row.ModeOfTravel ?? DBNull.Value);

                                cmd.Parameters.AddWithValue(
                                    "@Departure",
                                    (object)row.DepartureDatetime ?? DBNull.Value);

                                cmd.ExecuteNonQuery();
                            }
                        }

                        foreach (var row in request.Costings
                            ?? new List<TravelCosting>())
                        {
                            using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(
                                @"INSERT INTO TravelCostings
                          (TravelRequestId, ExpenseType,
                           SponsoredAmount, FundedAmount, TotalAmount)
                          VALUES
                          (@Id, @Expense, @Sponsored, @Funded, @Total)",
                                conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Id", localId);

                                cmd.Parameters.AddWithValue(
                                    "@Expense",
                                    (object)row.ExpenseType ?? DBNull.Value);

                                cmd.Parameters.AddWithValue(
                                    "@Sponsored", row.SponsoredAmount);

                                cmd.Parameters.AddWithValue(
                                    "@Funded", row.FundedAmount);

                                cmd.Parameters.AddWithValue(
                                    "@Total", row.TotalAmount);

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







        public void InsertTravelRequest(TravelRequest request)
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
                            INSERT INTO TravelRequests
                                (TravelRequestCode, TravelType, PurposeOfTravel, TravelFunding, DetailsOfSponsor, CopyOfInvitation,
                                 Employee, EmployeeName, DateOfBirth, IdentificationDocumentType, ContactNumber, IdentificationDocumentNumber,
                                 ContactEmail, PassportNumber, Company, OtherDetailsDescription, CostCenter,
                                 NameOfOrganizer, AddressOfOrganizer, EventOtherDetails)
                            OUTPUT INSERTED.Id
                            VALUES
                                (@TravelRequestCode, @TravelType, @PurposeOfTravel, @TravelFunding, @DetailsOfSponsor, @CopyOfInvitation,
                                 @Employee, @EmployeeName, @DateOfBirth, @IdentificationDocumentType, @ContactNumber, @IdentificationDocumentNumber,
                                 @ContactEmail, @PassportNumber, @Company, @OtherDetailsDescription, @CostCenter,
                                 @NameOfOrganizer, @AddressOfOrganizer, @EventOtherDetails)";

                        using (var cmd = new SqlCommand(headerQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@TravelRequestCode", request.TravelRequestCode ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@TravelType", request.TravelType ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@PurposeOfTravel", request.PurposeOfTravel ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@TravelFunding", request.TravelFunding ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@DetailsOfSponsor", request.DetailsOfSponsor ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@CopyOfInvitation", request.CopyOfInvitation ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Employee", request.Employee ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@EmployeeName", request.EmployeeName ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@DateOfBirth", (object)request.DateOfBirth ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@IdentificationDocumentType", request.IdentificationDocumentType ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@ContactNumber", request.ContactNumber ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@IdentificationDocumentNumber", request.IdentificationDocumentNumber ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@ContactEmail", request.ContactEmail ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@PassportNumber", request.PassportNumber ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Company", request.Company ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@OtherDetailsDescription", request.OtherDetailsDescription ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@CostCenter", request.CostCenter ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@NameOfOrganizer", request.NameOfOrganizer ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@AddressOfOrganizer", request.AddressOfOrganizer ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@EventOtherDetails", request.EventOtherDetails ?? (object)DBNull.Value);

                            newId = (int)cmd.ExecuteScalar();
                        }

                        if (request.Itineraries != null)
                        {
                            foreach (var it in request.Itineraries)
                            {
                                if (string.IsNullOrEmpty(it.TravelFrom) && string.IsNullOrEmpty(it.TravelTo)) continue;

                                string q = @"INSERT INTO TravelItineraries
                                             (TravelRequestId, TravelFrom, TravelTo, ModeOfTravel, DepartureDatetime)
                                             VALUES (@TravelRequestId, @TravelFrom, @TravelTo, @ModeOfTravel, @DepartureDatetime)";
                                using (var cmd = new SqlCommand(q, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@TravelRequestId", newId);
                                    cmd.Parameters.AddWithValue("@TravelFrom", it.TravelFrom ?? (object)DBNull.Value);
                                    cmd.Parameters.AddWithValue("@TravelTo", it.TravelTo ?? (object)DBNull.Value);
                                    cmd.Parameters.AddWithValue("@ModeOfTravel", it.ModeOfTravel ?? (object)DBNull.Value);
                                    cmd.Parameters.AddWithValue("@DepartureDatetime", (object)it.DepartureDatetime ?? DBNull.Value);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        if (request.Costings != null)
                        {
                            foreach (var c in request.Costings)
                            {
                                if (string.IsNullOrEmpty(c.ExpenseType)) continue;

                                string q = @"INSERT INTO TravelCostings
                                             (TravelRequestId, ExpenseType, SponsoredAmount, FundedAmount, TotalAmount)
                                             VALUES (@TravelRequestId, @ExpenseType, @SponsoredAmount, @FundedAmount, @TotalAmount)";
                                using (var cmd = new SqlCommand(q, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@TravelRequestId", newId);
                                    cmd.Parameters.AddWithValue("@ExpenseType", c.ExpenseType ?? (object)DBNull.Value);
                                    cmd.Parameters.AddWithValue("@SponsoredAmount", c.SponsoredAmount);
                                    cmd.Parameters.AddWithValue("@FundedAmount", c.FundedAmount);
                                    cmd.Parameters.AddWithValue("@TotalAmount", c.TotalAmount);
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

        public int InsertOrUpdateTravelRequest(TravelRequest request)
        {
            var existing = GetByCode(request.TravelRequestCode);

            if (existing != null)
            {
                UpdateTravelRequestFromErp(request);
                return existing.Id;
            }

            InsertTravelRequest(request);
            return GetByCode(request.TravelRequestCode)?.Id ?? 0;
        }

        public void SoftDeleteByCode(string code)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand(@"
                    UPDATE TravelRequests
                    SET IsDeleted = 1
                    WHERE TravelRequestCode = @Code", conn))
                {
                    cmd.Parameters.AddWithValue("@Code", code ?? (object)DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void MarkDeletedIfMissingFromErpNext(HashSet<string> erpCodes)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                if (erpCodes == null || erpCodes.Count == 0)
                {
                    using (var cmd = new SqlCommand(@"
                        UPDATE TravelRequests
                        SET IsDeleted = 1
                        WHERE IsDeleted = 0
                          AND TravelRequestCode IS NOT NULL
                          AND TravelRequestCode <> ''", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    return;
                }

                var parameterNames = erpCodes.Select((_, index) => "@p" + index).ToList();

                string sql = @"
                    UPDATE TravelRequests
                    SET IsDeleted = 1
                    WHERE IsDeleted = 0
                      AND TravelRequestCode IS NOT NULL
                      AND TravelRequestCode <> ''
                      AND TravelRequestCode NOT IN (" + string.Join(",", parameterNames) + ")";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    int i = 0;

                    foreach (string code in erpCodes)
                    {
                        cmd.Parameters.AddWithValue("@p" + i, code);
                        i++;
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
