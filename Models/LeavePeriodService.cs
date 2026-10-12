using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class LeavePeriodService
    {
        private readonly string _connectionString;

        public LeavePeriodService(string connectionString)
        {
            _connectionString = connectionString;
        }


        public List<LeavePeriod> GetAll(
            string id = null)
        {
            var list =
                new List<LeavePeriod>();


            using (var conn =
                new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT
                        Id,
                        FromDate,
                        ToDate,
                        Company,
                        HolidayListForOptionalLeave,
                        IsActive,
                        ErpNextId
                    FROM LeavePeriods
                    WHERE IsDeleted = 0";


                if (!string.IsNullOrWhiteSpace(id))
                {
                    query +=
                        " AND Company LIKE @Id";
                }


                query +=
                    " ORDER BY Id DESC";


                using (var cmd =
                    new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        cmd.Parameters.AddWithValue(
                            "@Id",
                            "%" + id + "%"
                        );
                    }


                    conn.Open();


                    using (var reader =
                        cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(
                                Map(reader)
                            );
                        }
                    }
                }
            }


            return list;
        }


        public LeavePeriod GetById(
            int id)
        {
            using (var conn =
                new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT
                        Id,
                        FromDate,
                        ToDate,
                        Company,
                        HolidayListForOptionalLeave,
                        IsActive,
                        ErpNextId
                    FROM LeavePeriods
                    WHERE Id = @Id
                      AND IsDeleted = 0";


                using (var cmd =
                    new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@Id",
                        id
                    );


                    conn.Open();


                    using (var reader =
                        cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return Map(reader);
                        }
                    }
                }
            }


            return null;
        }


        public LeavePeriod GetByErpNextId(
            string erpNextId)
        {
            if (string.IsNullOrWhiteSpace(
                erpNextId))
            {
                return null;
            }


            using (var conn =
                new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT
                        Id,
                        FromDate,
                        ToDate,
                        Company,
                        HolidayListForOptionalLeave,
                        IsActive,
                        ErpNextId
                    FROM LeavePeriods
                    WHERE ErpNextId = @ErpNextId
                      AND IsDeleted = 0";


                using (var cmd =
                    new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@ErpNextId",
                        erpNextId
                    );


                    conn.Open();


                    using (var reader =
                        cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return Map(reader);
                        }
                    }
                }
            }


            return null;
        }


        public bool ExistsByErpNextId(
            string erpNextId)
        {
            return GetByErpNextId(
                erpNextId
            ) != null;
        }


        public int InsertOrUpdate(
            LeavePeriod model)
        {
            using (var conn =
                new SqlConnection(_connectionString))
            {
                conn.Open();


                int existingId =
                    0;


                if (!string.IsNullOrWhiteSpace(
                    model.ErpNextId))
                {
                    using (var find =
                        new SqlCommand(@"
                            SELECT TOP 1 Id
                            FROM LeavePeriods
                            WHERE ErpNextId = @ErpNextId",
                            conn))
                    {
                        find.Parameters.AddWithValue(
                            "@ErpNextId",
                            model.ErpNextId
                        );


                        object value =
                            find.ExecuteScalar();


                        if (
                            value != null &&
                            value != DBNull.Value
                        )
                        {
                            existingId =
                                Convert.ToInt32(
                                    value
                                );
                        }
                    }
                }


                if (existingId > 0)
                {
                    using (var cmd =
                        new SqlCommand(@"
                            UPDATE LeavePeriods
                            SET
                                FromDate = @FromDate,
                                ToDate = @ToDate,
                                Company = @Company,
                                HolidayListForOptionalLeave = @HolidayList,
                                IsActive = @IsActive,
                                IsDeleted = 0
                            WHERE Id = @Id",
                            conn))
                    {
                        AddParameters(
                            cmd,
                            model
                        );


                        cmd.Parameters.AddWithValue(
                            "@Id",
                            existingId
                        );


                        cmd.ExecuteNonQuery();
                    }


                    return existingId;
                }


                using (var cmd =
                    new SqlCommand(@"
                        INSERT INTO LeavePeriods
                        (
                            FromDate,
                            ToDate,
                            Company,
                            HolidayListForOptionalLeave,
                            IsActive,
                            ErpNextId,
                            IsDeleted
                        )
                        OUTPUT INSERTED.Id
                        VALUES
                        (
                            @FromDate,
                            @ToDate,
                            @Company,
                            @HolidayList,
                            @IsActive,
                            @ErpNextId,
                            0
                        )",
                        conn))
                {
                    AddParameters(
                        cmd,
                        model
                    );


                    return Convert.ToInt32(
                        cmd.ExecuteScalar()
                    );
                }
            }
        }


        public int Insert(
            LeavePeriod model)
        {
            return InsertOrUpdate(
                model
            );
        }


        public void InsertFromErpNext(
            LeavePeriod model)
        {
            InsertOrUpdate(
                model
            );
        }


        public void UpdateFromErpNext(
            LeavePeriod model)
        {
            InsertOrUpdate(
                model
            );
        }


        public void Update(
            LeavePeriod model)
        {
            using (var conn =
                new SqlConnection(_connectionString))
            {
                string query = @"
                    UPDATE LeavePeriods
                    SET
                        FromDate = @FromDate,
                        ToDate = @ToDate,
                        Company = @Company,
                        HolidayListForOptionalLeave = @HolidayList,
                        IsActive = @IsActive,
                        ErpNextId = @ErpNextId,
                        IsDeleted = 0
                    WHERE Id = @Id";


                using (var cmd =
                    new SqlCommand(query, conn))
                {
                    AddParameters(
                        cmd,
                        model
                    );


                    cmd.Parameters.AddWithValue(
                        "@Id",
                        model.Id
                    );


                    conn.Open();

                    cmd.ExecuteNonQuery();
                }
            }
        }


        public void MarkDeletedById(
            int id)
        {
            using (var conn =
                new SqlConnection(_connectionString))
            {
                string query = @"
                    UPDATE LeavePeriods
                    SET IsDeleted = 1
                    WHERE Id = @Id";


                using (var cmd =
                    new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@Id",
                        id
                    );


                    conn.Open();

                    cmd.ExecuteNonQuery();
                }
            }
        }


        public void UpdateErpNextId(
            int id,
            string erpNextId)
        {
            using (var conn =
                new SqlConnection(_connectionString))
            {
                string query = @"
                    UPDATE LeavePeriods
                    SET ErpNextId = @ErpNextId
                    WHERE Id = @Id";


                using (var cmd =
                    new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@ErpNextId",
                        string.IsNullOrWhiteSpace(
                            erpNextId)
                            ? (object)DBNull.Value
                            : erpNextId
                    );


                    cmd.Parameters.AddWithValue(
                        "@Id",
                        id
                    );


                    conn.Open();

                    cmd.ExecuteNonQuery();
                }
            }
        }


        private static void AddParameters(
            SqlCommand cmd,
            LeavePeriod model)
        {
            cmd.Parameters.AddWithValue(
                "@FromDate",
                model.FromDate
            );


            cmd.Parameters.AddWithValue(
                "@ToDate",
                model.ToDate
            );


            cmd.Parameters.AddWithValue(
                "@Company",
                string.IsNullOrWhiteSpace(
                    model.Company)
                    ? (object)DBNull.Value
                    : model.Company
            );


            cmd.Parameters.AddWithValue(
                "@HolidayList",
                string.IsNullOrWhiteSpace(
                    model.HolidayListForOptionalLeave)
                    ? (object)DBNull.Value
                    : model.HolidayListForOptionalLeave
            );


            cmd.Parameters.AddWithValue(
                "@IsActive",
                model.IsActive
            );


            cmd.Parameters.AddWithValue(
                "@ErpNextId",
                string.IsNullOrWhiteSpace(
                    model.ErpNextId)
                    ? (object)DBNull.Value
                    : model.ErpNextId
            );
        }


        private LeavePeriod Map(
            SqlDataReader reader)
        {
            return new LeavePeriod
            {
                Id =
                    Convert.ToInt32(
                        reader["Id"]
                    ),

                FromDate =
                    Convert.ToDateTime(
                        reader["FromDate"]
                    ),

                ToDate =
                    Convert.ToDateTime(
                        reader["ToDate"]
                    ),

                Company =
                    reader["Company"] ==
                    DBNull.Value
                        ? ""
                        : reader["Company"]
                            .ToString(),

                HolidayListForOptionalLeave =
                    reader[
                        "HolidayListForOptionalLeave"
                    ] == DBNull.Value
                        ? null
                        : reader[
                            "HolidayListForOptionalLeave"
                        ].ToString(),

                IsActive =
                    reader["IsActive"] !=
                    DBNull.Value &&
                    Convert.ToBoolean(
                        reader["IsActive"]
                    ),

                ErpNextId =
                    reader["ErpNextId"] ==
                    DBNull.Value
                        ? null
                        : reader["ErpNextId"]
                            .ToString()
            };
        }
    }
}
