//using System;
//using System.Collections.Generic;
//using Microsoft.Data.SqlClient;
//using Erp_Aits.Models;

//namespace Erp_Aits.Services
//{
//    public class TimeSheetService
//    {
//        private readonly string _connectionString;

//        public TimeSheetService(string connectionString)
//        {
//            _connectionString = connectionString;
//        }

//        public List<TimeSheet> GetAll(string idFilter, string employee, string status)
//        {
//            var list = new List<TimeSheet>();

//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = "SELECT * FROM TimeSheets WHERE IsDeleted = 0";

//                if (!string.IsNullOrEmpty(idFilter)) query += " AND CAST(Id AS VARCHAR) LIKE @IdFilter";
//                if (!string.IsNullOrEmpty(employee)) query += " AND Employee LIKE @Employee";
//                if (!string.IsNullOrEmpty(status)) query += " AND Status = @Status";

//                query += " ORDER BY Id DESC";

//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    if (!string.IsNullOrEmpty(idFilter)) cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");
//                    if (!string.IsNullOrEmpty(employee)) cmd.Parameters.AddWithValue("@Employee", "%" + employee + "%");
//                    if (!string.IsNullOrEmpty(status)) cmd.Parameters.AddWithValue("@Status", status);

//                    conn.Open();
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            list.Add(MapTimeSheet(reader));
//                        }
//                    }
//                }
//            }

//            return list;
//        }

//        public TimeSheet GetById(int id)
//        {
//            TimeSheet sheet = null;

//            using (var conn = new SqlConnection(_connectionString))
//            {
//                conn.Open();

//                using (var cmd = new SqlCommand("SELECT * FROM TimeSheets WHERE Id = @Id AND IsDeleted = 0", conn))
//                {
//                    cmd.Parameters.AddWithValue("@Id", id);
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        if (reader.Read())
//                        {
//                            sheet = MapTimeSheet(reader);
//                        }
//                    }
//                }

//                if (sheet != null)
//                {
//                    using (var cmd = new SqlCommand("SELECT * FROM TimeSheetDetails WHERE TimeSheetId = @Id", conn))
//                    {
//                        cmd.Parameters.AddWithValue("@Id", id);
//                        using (var reader = cmd.ExecuteReader())
//                        {
//                            while (reader.Read())
//                            {
//                                sheet.TimeLogs.Add(new TimeSheetDetail
//                                {
//                                    Id = Convert.ToInt32(reader["Id"]),
//                                    TimeSheetId = id,
//                                    ActivityType = reader["ActivityType"]?.ToString() ?? string.Empty,
//                                    FromTime = reader["FromTime"] as DateTime?,
//                                    ToTime = reader["ToTime"] as DateTime?,
//                                    Hrs = reader["Hrs"] != DBNull.Value ? Convert.ToDecimal(reader["Hrs"]) : 0,
//                                    Project = reader["Project"]?.ToString() ?? string.Empty,
//                                    Task = reader["Task"]?.ToString() ?? string.Empty,
//                                    IsBillable = reader["IsBillable"] != DBNull.Value && Convert.ToBoolean(reader["IsBillable"])
//                                });
//                            }
//                        }
//                    }
//                }
//            }

//            return sheet;
//        }

//        public int Insert(TimeSheet t)
//        {
//            int newId;

//            using (var conn = new SqlConnection(_connectionString))
//            {
//                conn.Open();

//                using (var cmd = new SqlCommand(@"
//                    INSERT INTO TimeSheets
//                        (Series, Status, Company, Project, Customer, Currency, ExchangeRate, Employee,
//                         TotalWorkingHours, TotalBillableHours, TotalBillableAmount, TotalCostingAmount, Note)
//                    OUTPUT INSERTED.Id
//                    VALUES
//                        (@Series, @Status, @Company, @Project, @Customer, @Currency, @ExchangeRate, @Employee,
//                         @TotalWorkingHours, @TotalBillableHours, @TotalBillableAmount, @TotalCostingAmount, @Note)", conn))
//                {
//                    cmd.Parameters.AddWithValue("@Series", t.Series ?? "TS-.YYYY.-");
//                    cmd.Parameters.AddWithValue("@Status", t.Status ?? "Draft");
//                    cmd.Parameters.AddWithValue("@Company", (object)t.Company ?? DBNull.Value);
//                    cmd.Parameters.AddWithValue("@Project", (object)t.Project ?? DBNull.Value);
//                    cmd.Parameters.AddWithValue("@Customer", (object)t.Customer ?? DBNull.Value);
//                    cmd.Parameters.AddWithValue("@Currency", t.Currency ?? "INR");
//                    cmd.Parameters.AddWithValue("@ExchangeRate", t.ExchangeRate);
//                    cmd.Parameters.AddWithValue("@Employee", (object)t.Employee ?? DBNull.Value);
//                    cmd.Parameters.AddWithValue("@TotalWorkingHours", t.TotalWorkingHours);
//                    cmd.Parameters.AddWithValue("@TotalBillableHours", t.TotalBillableHours);
//                    cmd.Parameters.AddWithValue("@TotalBillableAmount", t.TotalBillableAmount);
//                    cmd.Parameters.AddWithValue("@TotalCostingAmount", t.TotalCostingAmount);
//                    cmd.Parameters.AddWithValue("@Note", (object)t.Note ?? DBNull.Value);

//                    newId = (int)cmd.ExecuteScalar();
//                }

//                foreach (var log in t.TimeLogs)
//                {
//                    if (string.IsNullOrWhiteSpace(log.ActivityType) && log.Hrs == 0) continue;

//                    using (var cmd = new SqlCommand(@"
//                        INSERT INTO TimeSheetDetails
//                            (TimeSheetId, ActivityType, FromTime, ToTime, Hrs, Project, Task, IsBillable)
//                        VALUES
//                            (@TimeSheetId, @ActivityType, @FromTime, @ToTime, @Hrs, @Project, @Task, @IsBillable)", conn))
//                    {
//                        cmd.Parameters.AddWithValue("@TimeSheetId", newId);
//                        cmd.Parameters.AddWithValue("@ActivityType", (object)log.ActivityType ?? DBNull.Value);
//                        cmd.Parameters.AddWithValue("@FromTime", (object)log.FromTime ?? DBNull.Value);
//                        cmd.Parameters.AddWithValue("@ToTime", (object)log.ToTime ?? DBNull.Value);
//                        cmd.Parameters.AddWithValue("@Hrs", log.Hrs);
//                        cmd.Parameters.AddWithValue("@Project", (object)log.Project ?? DBNull.Value);
//                        cmd.Parameters.AddWithValue("@Task", (object)log.Task ?? DBNull.Value);
//                        cmd.Parameters.AddWithValue("@IsBillable", log.IsBillable);

//                        cmd.ExecuteNonQuery();
//                    }
//                }
//            }

//            return newId;
//        }

//        private TimeSheet MapTimeSheet(SqlDataReader reader)
//        {
//            return new TimeSheet
//            {
//                Id = Convert.ToInt32(reader["Id"]),
//                Series = reader["Series"]?.ToString() ?? "TS-.YYYY.-",
//                Status = reader["Status"]?.ToString() ?? "Draft",
//                Company = reader["Company"]?.ToString() ?? string.Empty,
//                Project = reader["Project"]?.ToString() ?? string.Empty,
//                Customer = reader["Customer"]?.ToString() ?? string.Empty,
//                Currency = reader["Currency"]?.ToString() ?? "INR",
//                ExchangeRate = reader["ExchangeRate"] != DBNull.Value ? Convert.ToDecimal(reader["ExchangeRate"]) : 1.000m,
//                Employee = reader["Employee"]?.ToString() ?? string.Empty,
//                TotalWorkingHours = reader["TotalWorkingHours"] != DBNull.Value ? Convert.ToDecimal(reader["TotalWorkingHours"]) : 0,
//                TotalBillableHours = reader["TotalBillableHours"] != DBNull.Value ? Convert.ToDecimal(reader["TotalBillableHours"]) : 0,
//                TotalBillableAmount = reader["TotalBillableAmount"] != DBNull.Value ? Convert.ToDecimal(reader["TotalBillableAmount"]) : 0,
//                TotalCostingAmount = reader["TotalCostingAmount"] != DBNull.Value ? Convert.ToDecimal(reader["TotalCostingAmount"]) : 0,
//                Note = reader["Note"]?.ToString() ?? string.Empty
//            };
//        }
//    }
//}





using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class TimeSheetService
    {
        private readonly string _connectionString;

        public TimeSheetService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<TimeSheet> GetAll(string idFilter, string employee, string status)
        {
            var list = new List<TimeSheet>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM TimeSheets WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(idFilter)) query += " AND CAST(Id AS VARCHAR) LIKE @IdFilter";
                if (!string.IsNullOrEmpty(employee)) query += " AND Employee LIKE @Employee";
                if (!string.IsNullOrEmpty(status)) query += " AND Status = @Status";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(idFilter)) cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");
                    if (!string.IsNullOrEmpty(employee)) cmd.Parameters.AddWithValue("@Employee", "%" + employee + "%");
                    if (!string.IsNullOrEmpty(status)) cmd.Parameters.AddWithValue("@Status", status);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(MapTimeSheet(reader));
                        }
                    }
                }
            }

            return list;
        }

        public TimeSheet GetById(int id)
        {
            TimeSheet sheet = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand("SELECT * FROM TimeSheets WHERE Id = @Id AND IsDeleted = 0", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            sheet = MapTimeSheet(reader);
                        }
                    }
                }

                if (sheet != null)
                {
                    using (var cmd = new SqlCommand("SELECT * FROM TimeSheetDetails WHERE TimeSheetId = @Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                sheet.TimeLogs.Add(new TimeSheetDetail
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    TimeSheetId = id,
                                    ActivityType = reader["ActivityType"]?.ToString() ?? string.Empty,
                                    FromTime = reader["FromTime"] != DBNull.Value ? Convert.ToDateTime(reader["FromTime"]) : (DateTime?)null,
                                    ToTime = reader["ToTime"] != DBNull.Value ? Convert.ToDateTime(reader["ToTime"]) : (DateTime?)null,
                                    Hrs = reader["Hrs"] != DBNull.Value ? Convert.ToDecimal(reader["Hrs"]) : 0,
                                    Project = reader["Project"]?.ToString() ?? string.Empty,
                                    Task = reader["Task"]?.ToString() ?? string.Empty,
                                    IsBillable = reader["IsBillable"] != DBNull.Value && Convert.ToBoolean(reader["IsBillable"])
                                });
                            }
                        }
                    }
                }
            }

            return sheet;
        }

        public int Insert(TimeSheet t)
        {
            int newId;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand(@"
                    INSERT INTO TimeSheets
                        (ErpNextId, Series, Status, Company, Project, Customer, Currency, ExchangeRate, Employee,
                         TotalWorkingHours, TotalBillableHours, TotalBillableAmount, TotalCostingAmount, Note)
                    OUTPUT INSERTED.Id
                    VALUES
                        (@ErpNextId, @Series, @Status, @Company, @Project, @Customer, @Currency, @ExchangeRate, @Employee,
                         @TotalWorkingHours, @TotalBillableHours, @TotalBillableAmount, @TotalCostingAmount, @Note)", conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", (object)t.ErpNextId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Series", t.Series ?? "TS-.YYYY.-");
                    cmd.Parameters.AddWithValue("@Status", t.Status ?? "Draft");
                    cmd.Parameters.AddWithValue("@Company", (object)t.Company ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Project", (object)t.Project ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Customer", (object)t.Customer ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Currency", t.Currency ?? "INR");
                    cmd.Parameters.AddWithValue("@ExchangeRate", t.ExchangeRate);
                    cmd.Parameters.AddWithValue("@Employee", (object)t.Employee ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TotalWorkingHours", t.TotalWorkingHours);
                    cmd.Parameters.AddWithValue("@TotalBillableHours", t.TotalBillableHours);
                    cmd.Parameters.AddWithValue("@TotalBillableAmount", t.TotalBillableAmount);
                    cmd.Parameters.AddWithValue("@TotalCostingAmount", t.TotalCostingAmount);
                    cmd.Parameters.AddWithValue("@Note", (object)t.Note ?? DBNull.Value);

                    newId = (int)cmd.ExecuteScalar();
                }

                foreach (var log in t.TimeLogs)
                {
                    if (string.IsNullOrWhiteSpace(log.ActivityType) && log.Hrs == 0) continue;

                    using (var cmd = new SqlCommand(@"
                        INSERT INTO TimeSheetDetails
                            (TimeSheetId, ActivityType, FromTime, ToTime, Hrs, Project, Task, IsBillable)
                        VALUES
                            (@TimeSheetId, @ActivityType, @FromTime, @ToTime, @Hrs, @Project, @Task, @IsBillable)", conn))
                    {
                        cmd.Parameters.AddWithValue("@TimeSheetId", newId);
                        cmd.Parameters.AddWithValue("@ActivityType", (object)log.ActivityType ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@FromTime", (object)log.FromTime ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ToTime", (object)(log.ToTime ?? (object)DBNull.Value));
                        cmd.Parameters.AddWithValue("@Hrs", log.Hrs);
                        cmd.Parameters.AddWithValue("@Project", (object)log.Project ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Task", (object)log.Task ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IsBillable", log.IsBillable);

                        cmd.ExecuteNonQuery();
                    }
                }
            }

            return newId;
        }

        public int InsertOrUpdate(TimeSheet t)
        {
            var existing = GetByErpNextId(t.ErpNextId) ?? GetBySeries(t.Series);
            if (existing == null) return Insert(t);

            t.Id = existing.Id;
            Update(t);
            return existing.Id;
        }

        public TimeSheet GetByErpNextId(string erpNextId)
        {
            if (string.IsNullOrWhiteSpace(erpNextId)) return null;
            return GetByColumn("ErpNextId", erpNextId);
        }

        public TimeSheet GetBySeries(string series)
        {
            if (string.IsNullOrWhiteSpace(series)) return null;
            return GetByColumn("Series", series);
        }

        public void Update(TimeSheet t)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand(@"
                    UPDATE TimeSheets
                    SET ErpNextId = @ErpNextId,
                        Series = @Series,
                        Status = @Status,
                        Company = @Company,
                        Project = @Project,
                        Customer = @Customer,
                        Currency = @Currency,
                        ExchangeRate = @ExchangeRate,
                        Employee = @Employee,
                        TotalWorkingHours = @TotalWorkingHours,
                        TotalBillableHours = @TotalBillableHours,
                        TotalBillableAmount = @TotalBillableAmount,
                        TotalCostingAmount = @TotalCostingAmount,
                        Note = @Note,
                        IsDeleted = 0
                    WHERE Id = @Id", conn))
                {
                    AddHeaderParameters(cmd, t);
                    cmd.Parameters.AddWithValue("@Id", t.Id);
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new SqlCommand("DELETE FROM TimeSheetDetails WHERE TimeSheetId = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", t.Id);
                    cmd.ExecuteNonQuery();
                }

                foreach (var log in t.TimeLogs)
                {
                    if (string.IsNullOrWhiteSpace(log.ActivityType) && log.Hrs == 0) continue;

                    using (var cmd = new SqlCommand(@"
                        INSERT INTO TimeSheetDetails
                            (TimeSheetId, ActivityType, FromTime, ToTime, Hrs, Project, Task, IsBillable)
                        VALUES
                            (@TimeSheetId, @ActivityType, @FromTime, @ToTime, @Hrs, @Project, @Task, @IsBillable)", conn))
                    {
                        cmd.Parameters.AddWithValue("@TimeSheetId", t.Id);
                        cmd.Parameters.AddWithValue("@ActivityType", (object)log.ActivityType ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@FromTime", (object)log.FromTime ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ToTime", (object)log.ToTime ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Hrs", log.Hrs);
                        cmd.Parameters.AddWithValue("@Project", (object)log.Project ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Task", (object)log.Task ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IsBillable", log.IsBillable);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public void MarkDeletedById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("UPDATE TimeSheets SET IsDeleted = 1 WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private TimeSheet GetByColumn(string column, string value)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = column == "ErpNextId"
                    ? "SELECT TOP 1 * FROM TimeSheets WHERE ErpNextId = @Value"
                    : "SELECT TOP 1 * FROM TimeSheets WHERE Series = @Value";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Value", value);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) return MapTimeSheet(reader);
                    }
                }
            }

            return null;
        }

        private static void AddHeaderParameters(SqlCommand cmd, TimeSheet t)
        {
            cmd.Parameters.AddWithValue("@ErpNextId", (object)t.ErpNextId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Series", t.Series ?? "TS-.YYYY.-");
            cmd.Parameters.AddWithValue("@Status", t.Status ?? "Draft");
            cmd.Parameters.AddWithValue("@Company", (object)t.Company ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Project", (object)t.Project ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Customer", (object)t.Customer ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Currency", t.Currency ?? "INR");
            cmd.Parameters.AddWithValue("@ExchangeRate", t.ExchangeRate);
            cmd.Parameters.AddWithValue("@Employee", (object)t.Employee ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TotalWorkingHours", t.TotalWorkingHours);
            cmd.Parameters.AddWithValue("@TotalBillableHours", t.TotalBillableHours);
            cmd.Parameters.AddWithValue("@TotalBillableAmount", t.TotalBillableAmount);
            cmd.Parameters.AddWithValue("@TotalCostingAmount", t.TotalCostingAmount);
            cmd.Parameters.AddWithValue("@Note", (object)t.Note ?? DBNull.Value);
        }

        private TimeSheet MapTimeSheet(SqlDataReader reader)
        {
            return new TimeSheet
            {
                Id = Convert.ToInt32(reader["Id"]),
                ErpNextId = HasColumn(reader, "ErpNextId") && reader["ErpNextId"] != DBNull.Value ? reader["ErpNextId"].ToString() : string.Empty,
                Series = reader["Series"]?.ToString() ?? "TS-.YYYY.-",
                Status = reader["Status"]?.ToString() ?? "Draft",
                Company = reader["Company"]?.ToString() ?? string.Empty,
                Project = reader["Project"]?.ToString() ?? string.Empty,
                Customer = reader["Customer"]?.ToString() ?? string.Empty,
                Currency = reader["Currency"]?.ToString() ?? "INR",
                ExchangeRate = reader["ExchangeRate"] != DBNull.Value ? Convert.ToDecimal(reader["ExchangeRate"]) : 1.000m,
                Employee = reader["Employee"]?.ToString() ?? string.Empty,
                TotalWorkingHours = reader["TotalWorkingHours"] != DBNull.Value ? Convert.ToDecimal(reader["TotalWorkingHours"]) : 0,
                TotalBillableHours = reader["TotalBillableHours"] != DBNull.Value ? Convert.ToDecimal(reader["TotalBillableHours"]) : 0,
                TotalBillableAmount = reader["TotalBillableAmount"] != DBNull.Value ? Convert.ToDecimal(reader["TotalBillableAmount"]) : 0,
                TotalCostingAmount = reader["TotalCostingAmount"] != DBNull.Value ? Convert.ToDecimal(reader["TotalCostingAmount"]) : 0,
                Note = reader["Note"]?.ToString() ?? string.Empty
            };
        }

        private static bool HasColumn(System.Data.Common.DbDataReader reader, string column)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(column, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }
    }
}
