//using System;
//using System.Collections.Generic;
//using Microsoft.Data.SqlClient;
//using Erp_Aits.Models;

//namespace Erp_Aits.Services
//{
//    public class HolidayListService
//    {
//        private readonly string _connectionString;

//        public HolidayListService(string connectionString)
//        {
//            _connectionString = connectionString;
//        }

//        public List<HolidayList> GetAll(string id = null)
//        {
//            var list = new List<HolidayList>();
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = @"SELECT Id, HolidayListName, FromDate, ToDate, TotalHolidays, Color 
//                                  FROM HolidayLists WHERE IsDeleted = 0";

//                if (!string.IsNullOrEmpty(id))
//                    query += " AND HolidayListName LIKE @Id";

//                query += " ORDER BY Id DESC";

//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    if (!string.IsNullOrEmpty(id))
//                        cmd.Parameters.AddWithValue("@Id", "%" + id + "%");

//                    conn.Open();
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            list.Add(new HolidayList
//                            {
//                                Id = Convert.ToInt32(reader["Id"]),
//                                HolidayListName = reader["HolidayListName"]?.ToString(),
//                                FromDate = Convert.ToDateTime(reader["FromDate"]),
//                                ToDate = Convert.ToDateTime(reader["ToDate"]),
//                                TotalHolidays = Convert.ToInt32(reader["TotalHolidays"]),
//                                Color = reader["Color"]?.ToString()
//                            });
//                        }
//                    }
//                }
//            }
//            return list;
//        }

//        public HolidayList GetById(int id)
//        {
//            HolidayList model = null;
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                conn.Open();

//                string query = @"SELECT Id, HolidayListName, FromDate, ToDate, TotalHolidays, Color 
//                                  FROM HolidayLists WHERE Id = @Id AND IsDeleted = 0";
//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    cmd.Parameters.AddWithValue("@Id", id);
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        if (reader.Read())
//                        {
//                            model = new HolidayList
//                            {
//                                Id = Convert.ToInt32(reader["Id"]),
//                                HolidayListName = reader["HolidayListName"]?.ToString(),
//                                FromDate = Convert.ToDateTime(reader["FromDate"]),
//                                ToDate = Convert.ToDateTime(reader["ToDate"]),
//                                TotalHolidays = Convert.ToInt32(reader["TotalHolidays"]),
//                                Color = reader["Color"]?.ToString()
//                            };
//                        }
//                    }
//                }

//                if (model == null) return null;

//                string datesQuery = @"SELECT Id, HolidayListId, HolidayDate, Description 
//                                       FROM HolidayListDates WHERE HolidayListId = @Id ORDER BY HolidayDate";
//                using (var cmd = new SqlCommand(datesQuery, conn))
//                {
//                    cmd.Parameters.AddWithValue("@Id", id);
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            model.Holidays.Add(new HolidayListDate
//                            {
//                                Id = Convert.ToInt32(reader["Id"]),
//                                HolidayListId = Convert.ToInt32(reader["HolidayListId"]),
//                                HolidayDate = Convert.ToDateTime(reader["HolidayDate"]),
//                                Description = reader["Description"]?.ToString()
//                            });
//                        }
//                    }
//                }
//            }
//            return model;
//        }

//        public int Insert(HolidayList model)
//        {
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                conn.Open();
//                using (var transaction = conn.BeginTransaction())
//                {
//                    try
//                    {
//                        int newId;
//                        string query = @"INSERT INTO HolidayLists 
//                            (HolidayListName, FromDate, ToDate, TotalHolidays, Color)
//                            OUTPUT INSERTED.Id
//                            VALUES (@HolidayListName, @FromDate, @ToDate, @TotalHolidays, @Color)";

//                        using (var cmd = new SqlCommand(query, conn, transaction))
//                        {
//                            cmd.Parameters.AddWithValue("@HolidayListName", model.HolidayListName ?? string.Empty);
//                            cmd.Parameters.AddWithValue("@FromDate", model.FromDate);
//                            cmd.Parameters.AddWithValue("@ToDate", model.ToDate);
//                            cmd.Parameters.AddWithValue("@TotalHolidays", model.Holidays.Count);
//                            cmd.Parameters.AddWithValue("@Color", (object)model.Color ?? DBNull.Value);

//                            newId = (int)cmd.ExecuteScalar();
//                        }

//                        foreach (var h in model.Holidays)
//                        {
//                            string dateQuery = @"INSERT INTO HolidayListDates (HolidayListId, HolidayDate, Description, RowNo)
//                                                  VALUES (@HolidayListId, @HolidayDate, @Description, @RowNo)";
//                            using (var cmd = new SqlCommand(dateQuery, conn, transaction))
//                            {
//                                cmd.Parameters.AddWithValue("@HolidayListId", newId);
//                                cmd.Parameters.AddWithValue("@HolidayDate", h.HolidayDate);
//                                cmd.Parameters.AddWithValue("@Description", h.Description ?? string.Empty);
//                                cmd.Parameters.AddWithValue("@RowNo", model.Holidays.IndexOf(h) + 1);
//                                cmd.ExecuteNonQuery();
//                            }
//                        }

//                        transaction.Commit();
//                        return newId;
//                    }
//                    catch
//                    {
//                        transaction.Rollback();
//                        throw;
//                    }
//                }
//            }
//        }
//    }
//}



using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class HolidayListService
    {
        private readonly string _connectionString;

        public HolidayListService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public HolidayList GetById(int id)
        {
            HolidayList model = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string query = @"
            SELECT Id, HolidayListName, FromDate, ToDate, TotalHolidays, Color, Country, Subdivision
            FROM HolidayLists
            WHERE Id = @Id AND IsDeleted = 0";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model = new HolidayList
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                HolidayListName = reader["HolidayListName"] == DBNull.Value ? "" : reader["HolidayListName"].ToString(),
                                FromDate = reader["FromDate"] == DBNull.Value ? DateTime.Today : Convert.ToDateTime(reader["FromDate"]),
                                ToDate = reader["ToDate"] == DBNull.Value ? DateTime.Today : Convert.ToDateTime(reader["ToDate"]),
                                TotalHolidays = reader["TotalHolidays"] == DBNull.Value ? 0 : Convert.ToInt32(reader["TotalHolidays"]),
                                Color = reader["Color"] == DBNull.Value ? "#3788d8" : reader["Color"].ToString(),
                                Country = reader["Country"] == DBNull.Value ? "" : reader["Country"].ToString(),
                                Subdivision = reader["Subdivision"] == DBNull.Value ? "" : reader["Subdivision"].ToString(),
                                Holidays = new List<HolidayListDate>()
                            };
                        }
                    }
                }

                if (model == null)
                    return null;

                string datesQuery = @"
            SELECT Id, HolidayListId, HolidayDate, Description
            FROM HolidayListDates
            WHERE HolidayListId = @Id
            ORDER BY HolidayDate";

                using (var cmd = new SqlCommand(datesQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.Holidays.Add(new HolidayListDate
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                HolidayListId = Convert.ToInt32(reader["HolidayListId"]),
                                HolidayDate = reader["HolidayDate"] == DBNull.Value
                                    ? DateTime.Today
                                    : Convert.ToDateTime(reader["HolidayDate"]),
                                Description = CleanHolidayDescription(
                                    reader["Description"] == DBNull.Value ? "" : reader["Description"].ToString()
                                )
                            });
                        }
                    }
                }

                model.TotalHolidays = model.Holidays.Count;
            }

            return model;
        }


        private static string CleanHolidayDescription(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            string text = System.Text.RegularExpressions.Regex.Replace(value, "<.*?>", string.Empty);
            return System.Net.WebUtility.HtmlDecode(text).Trim();
        }








        public List<HolidayList> GetAll(string id = null)
        {
            var list = new List<HolidayList>();
            using (var conn = new SqlConnection(_connectionString))
            {
                //string query = @"SELECT Id, HolidayListName, FromDate, ToDate, TotalHolidays, Color, Country, Subdivision 
                //                 FROM HolidayLists WHERE 1=1";
                string query = @"SELECT Id, HolidayListName, FromDate, ToDate, TotalHolidays, Color, Country, Subdivision 
                 FROM HolidayLists WHERE IsDeleted = 0";
                if (!string.IsNullOrEmpty(id))
                    query += " AND HolidayListName LIKE @Id";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(id))
                        cmd.Parameters.AddWithValue("@Id", "%" + id + "%");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new HolidayList
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                HolidayListName = reader["HolidayListName"]?.ToString(),
                                FromDate = Convert.ToDateTime(reader["FromDate"]),
                                ToDate = Convert.ToDateTime(reader["ToDate"]),
                                TotalHolidays = Convert.ToInt32(reader["TotalHolidays"]),
                                Color = reader["Color"]?.ToString(),
                                Country = reader["Country"]?.ToString(),
                                Subdivision = reader["Subdivision"]?.ToString()
                            });
                        }
                    }
                }
            }
            return list;
        }


        public void Update(HolidayList model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new SqlCommand(@"
                    UPDATE HolidayLists
                    SET HolidayListName = @HolidayListName,
                        FromDate = @FromDate,
                        ToDate = @ToDate,
                        TotalHolidays = @TotalHolidays,
                        Color = @Color,
                        Country = @Country,
                        Subdivision = @Subdivision
                    WHERE Id = @Id", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@Id", model.Id);
                            cmd.Parameters.AddWithValue("@HolidayListName", model.HolidayListName ?? "");
                            cmd.Parameters.AddWithValue("@FromDate", model.FromDate);
                            cmd.Parameters.AddWithValue("@ToDate", model.ToDate);
                            cmd.Parameters.AddWithValue("@TotalHolidays", model.Holidays?.Count ?? model.TotalHolidays);
                            cmd.Parameters.AddWithValue("@Color", (object)model.Color ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Country", (object)model.Country ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Subdivision", (object)model.Subdivision ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }

                        using (var cmd = new SqlCommand("DELETE FROM HolidayListDates WHERE HolidayListId = @Id", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@Id", model.Id);
                            cmd.ExecuteNonQuery();
                        }

                        if (model.Holidays != null)
                        {
                            foreach (var h in model.Holidays)
                            {
                                using (var cmd = new SqlCommand(@"
                            INSERT INTO HolidayListDates (HolidayListId, HolidayDate, Description)
                            VALUES (@HolidayListId, @HolidayDate, @Description)", conn, tx))
                                {
                                    cmd.Parameters.AddWithValue("@HolidayListId", model.Id);
                                    cmd.Parameters.AddWithValue("@HolidayDate", h.HolidayDate);
                                    cmd.Parameters.AddWithValue("@Description", h.Description ?? "");
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public void MarkDeletedById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("UPDATE HolidayLists SET IsDeleted = 1 WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }


















        public void InsertOrUpdateFromErpNext(HolidayList model)
        {
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();

                var existing = GetByName(model.HolidayListName);

                if (existing == null)
                {
                    Insert(model);
                    return;
                }

                using (var cmd = new SqlCommand(@"
            UPDATE HolidayLists
            SET 
                FromDate = @FromDate,
                ToDate = @ToDate,
                TotalHolidays = @TotalHolidays,
                Color = @Color,
                Country = @Country,
                Subdivision = @Subdivision
            WHERE Id = @Id", con))
                {
                    cmd.Parameters.AddWithValue("@Id", existing.Id);
                    cmd.Parameters.AddWithValue("@FromDate", model.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", model.ToDate);
                    cmd.Parameters.AddWithValue("@TotalHolidays", model.TotalHolidays);
                    cmd.Parameters.AddWithValue("@Color", (object?)model.Color ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Country", (object?)model.Country ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Subdivision", (object?)model.Subdivision ?? DBNull.Value);

                    cmd.ExecuteNonQuery();
                }
            }
        }


        public HolidayList? GetByName(string holidayListName)
        {
            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(@"
        SELECT TOP 1 Id, HolidayListName, FromDate, ToDate, TotalHolidays, Color, Country, Subdivision
        FROM HolidayLists
        WHERE HolidayListName = @HolidayListName", con))
            {
                cmd.Parameters.AddWithValue("@HolidayListName", holidayListName);

                con.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    return new HolidayList
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        HolidayListName = reader["HolidayListName"]?.ToString() ?? "",
                        FromDate = Convert.ToDateTime(reader["FromDate"]),
                        ToDate = Convert.ToDateTime(reader["ToDate"]),
                        TotalHolidays = Convert.ToInt32(reader["TotalHolidays"]),
                        Color = reader["Color"]?.ToString() ?? "",
                        Country = reader["Country"]?.ToString() ?? "",
                        Subdivision = reader["Subdivision"]?.ToString() ?? ""
                    };
                }
            }
        }













        public int Insert(HolidayList model)
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
                    INSERT INTO HolidayLists
                    (HolidayListName, FromDate, ToDate, TotalHolidays, Color, Country, Subdivision)
                    OUTPUT INSERTED.Id
                    VALUES
                    (@HolidayListName, @FromDate, @ToDate, @TotalHolidays, @Color, @Country, @Subdivision)";

                        using (var cmd = new SqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@HolidayListName", model.HolidayListName ?? string.Empty);
                            cmd.Parameters.AddWithValue("@FromDate", model.FromDate);
                            cmd.Parameters.AddWithValue("@ToDate", model.ToDate);
                            cmd.Parameters.AddWithValue("@TotalHolidays", model.Holidays?.Count ?? 0);
                            cmd.Parameters.AddWithValue("@Color", (object)model.Color ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Country", (object)model.Country ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Subdivision", (object)model.Subdivision ?? DBNull.Value);

                            newId = (int)cmd.ExecuteScalar();
                        }

                        if (model.Holidays != null)
                        {
                            int rowNo = 1;

                            foreach (var h in model.Holidays)
                            {
                                string dateQuery = @"
                            INSERT INTO HolidayListDates
                            (HolidayListId, RowNo, HolidayDate, Description)
                            VALUES
                            (@HolidayListId, @RowNo, @HolidayDate, @Description)";

                                using (var cmd = new SqlCommand(dateQuery, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@HolidayListId", newId);
                                    cmd.Parameters.AddWithValue("@RowNo", rowNo++);
                                    cmd.Parameters.AddWithValue("@HolidayDate", h.HolidayDate);
                                    cmd.Parameters.AddWithValue("@Description", h.Description ?? string.Empty);
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
