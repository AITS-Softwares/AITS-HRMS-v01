using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class ShiftRequestService
    {
        private readonly string _connectionString;

        public ShiftRequestService(string connectionString)
        {
            _connectionString = connectionString;
        }


        public List<ShiftRequest> GetAll(
            string idFilter = null,
            string employeeNameFilter = null)
        {
            var list = new List<ShiftRequest>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT *
                    FROM ShiftRequests
                    WHERE IsDeleted = 0";

                if (!string.IsNullOrWhiteSpace(idFilter))
                {
                    query +=
                        " AND ShiftRequestCode LIKE @IdFilter";
                }

                if (!string.IsNullOrWhiteSpace(employeeNameFilter))
                {
                    query +=
                        " AND EmployeeName LIKE @EmployeeNameFilter";
                }

                query += " ORDER BY Id DESC";


                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrWhiteSpace(idFilter))
                    {
                        cmd.Parameters.AddWithValue(
                            "@IdFilter",
                            "%" + idFilter + "%"
                        );
                    }

                    if (!string.IsNullOrWhiteSpace(employeeNameFilter))
                    {
                        cmd.Parameters.AddWithValue(
                            "@EmployeeNameFilter",
                            "%" + employeeNameFilter + "%"
                        );
                    }

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

        public string GetNextCode()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
            SELECT COUNT(*)
            FROM ShiftRequests";

                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();

                    int count =
                        Convert.ToInt32(
                            cmd.ExecuteScalar()
                        ) + 1;

                    return $"HR-SHR-{DateTime.Now.Year}-{count:D5}";
                }
            }
        }
        public ShiftRequest GetById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT *
                    FROM ShiftRequests
                    WHERE Id = @Id
                    AND IsDeleted = 0";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@Id",
                        id
                    );

                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapRow(reader);
                        }
                    }
                }
            }

            return null;
        }


        public ShiftRequest GetByErpNextId(
            string shiftRequestCode)
        {
            if (string.IsNullOrWhiteSpace(
                shiftRequestCode))
            {
                return null;
            }

            using (var conn =
                new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT TOP 1 *
                    FROM ShiftRequests
                    WHERE ShiftRequestCode = @Code
                    AND IsDeleted = 0";

                using (var cmd =
                    new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@Code",
                        shiftRequestCode
                    );

                    conn.Open();

                    using (var reader =
                        cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapRow(reader);
                        }
                    }
                }
            }

            return null;
        }


        public HashSet<string> GetExistingCodes()
        {
            var codes =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase
                );

            using (var conn =
                new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT ShiftRequestCode
                    FROM ShiftRequests
                    WHERE ShiftRequestCode IS NOT NULL
                      AND ShiftRequestCode != ''
                      AND IsDeleted = 0";

                using (var cmd =
                    new SqlCommand(query, conn))
                {
                    conn.Open();

                    using (var reader =
                        cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            codes.Add(
                                reader["ShiftRequestCode"]
                                    .ToString()
                            );
                        }
                    }
                }
            }

            return codes;
        }


        public int InsertOrUpdate(
            ShiftRequest model)
        {
            using (var conn =
                new SqlConnection(_connectionString))
            {
                conn.Open();

                int existingId = 0;


                if (!string.IsNullOrWhiteSpace(
                    model.ShiftRequestCode))
                {
                    using (var findCmd =
                        new SqlCommand(@"
                            SELECT TOP 1 Id
                            FROM ShiftRequests
                            WHERE ShiftRequestCode = @Code",
                            conn))
                    {
                        findCmd.Parameters.AddWithValue(
                            "@Code",
                            model.ShiftRequestCode
                        );

                        var value =
                            findCmd.ExecuteScalar();

                        if (
                            value != null &&
                            value != DBNull.Value
                        )
                        {
                            existingId =
                                Convert.ToInt32(value);
                        }
                    }
                }


                // ==============================
                // UPDATE EXISTING
                // ==============================

                if (existingId > 0)
                {
                    string update = @"
                        UPDATE ShiftRequests
                        SET
                            ShiftType = @ShiftType,
                            Company = @Company,
                            Employee = @Employee,
                            EmployeeName = @EmployeeName,
                            Department = @Department,
                            Approver = @Approver,
                            Status = @Status,
                            FromDate = @FromDate,
                            ToDate = @ToDate,
                            DocStatus = @DocStatus,
                            AmendedFrom = @AmendedFrom,
                            IsDeleted = 0
                        WHERE Id = @Id";


                    using (var cmd =
                        new SqlCommand(update, conn))
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


                // ==============================
                // INSERT NEW
                // ==============================

                string insert = @"
                    INSERT INTO ShiftRequests
                    (
                        ShiftRequestCode,
                        ShiftType,
                        Company,
                        Employee,
                        EmployeeName,
                        Department,
                        Approver,
                        Status,
                        FromDate,
                        ToDate,
                        DocStatus,
                        AmendedFrom,
                        IsDeleted
                    )
                    OUTPUT INSERTED.Id
                    VALUES
                    (
                        @ShiftRequestCode,
                        @ShiftType,
                        @Company,
                        @Employee,
                        @EmployeeName,
                        @Department,
                        @Approver,
                        @Status,
                        @FromDate,
                        @ToDate,
                        @DocStatus,
                        @AmendedFrom,
                        0
                    )";


                using (var cmd =
                    new SqlCommand(insert, conn))
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
            ShiftRequest model)
        {
            return InsertOrUpdate(model);
        }

        public void MarkDeletedById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("UPDATE ShiftRequests SET IsDeleted = 1 WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }


        private static void AddParameters(
            SqlCommand cmd,
            ShiftRequest model)
        {
            cmd.Parameters.AddWithValue(
                "@ShiftRequestCode",
                DbValue(model.ShiftRequestCode)
            );

            cmd.Parameters.AddWithValue(
                "@ShiftType",
                DbValue(model.ShiftType)
            );

            cmd.Parameters.AddWithValue(
                "@Company",
                DbValue(model.Company)
            );

            cmd.Parameters.AddWithValue(
                "@Employee",
                DbValue(model.Employee)
            );

            cmd.Parameters.AddWithValue(
                "@EmployeeName",
                DbValue(model.EmployeeName)
            );

            cmd.Parameters.AddWithValue(
                "@Department",
                DbValue(model.Department)
            );

            cmd.Parameters.AddWithValue(
                "@Approver",
                DbValue(model.Approver)
            );

            cmd.Parameters.AddWithValue(
                "@Status",
                string.IsNullOrWhiteSpace(
                    model.Status)
                    ? "Draft"
                    : model.Status
            );

            cmd.Parameters.AddWithValue(
                "@FromDate",
                model.FromDate.HasValue
                    ? (object)model.FromDate.Value
                    : DBNull.Value
            );

            cmd.Parameters.AddWithValue(
                "@ToDate",
                model.ToDate.HasValue
                    ? (object)model.ToDate.Value
                    : DBNull.Value
            );

            cmd.Parameters.AddWithValue(
                "@DocStatus",
                model.DocStatus
            );

            cmd.Parameters.AddWithValue(
                "@AmendedFrom",
                DbValue(model.AmendedFrom)
            );
        }


        private static object DbValue(
            string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? DBNull.Value
                : value;
        }


        private ShiftRequest MapRow(
            System.Data.Common.DbDataReader reader)
        {
            bool HasColumn(string column)
            {
                for (
                    int i = 0;
                    i < reader.FieldCount;
                    i++
                )
                {
                    if (
                        reader.GetName(i)
                            .Equals(
                                column,
                                StringComparison.OrdinalIgnoreCase
                            )
                    )
                    {
                        return true;
                    }
                }

                return false;
            }


            return new ShiftRequest
            {
                Id =
                    Convert.ToInt32(
                        reader["Id"]
                    ),

                ShiftRequestCode =
                    reader["ShiftRequestCode"] == DBNull.Value
                        ? ""
                        : reader["ShiftRequestCode"].ToString(),

                ShiftType =
                    reader["ShiftType"] == DBNull.Value
                        ? ""
                        : reader["ShiftType"].ToString(),

                Company =
                    reader["Company"] == DBNull.Value
                        ? ""
                        : reader["Company"].ToString(),

                Employee =
                    reader["Employee"] == DBNull.Value
                        ? ""
                        : reader["Employee"].ToString(),

                EmployeeName =
                    reader["EmployeeName"] == DBNull.Value
                        ? ""
                        : reader["EmployeeName"].ToString(),

                Department =
                    HasColumn("Department") &&
                    reader["Department"] != DBNull.Value
                        ? reader["Department"].ToString()
                        : "",

                Approver =
                    reader["Approver"] == DBNull.Value
                        ? ""
                        : reader["Approver"].ToString(),

                Status =
                    reader["Status"] == DBNull.Value
                        ? "Draft"
                        : reader["Status"].ToString(),

                FromDate =
                    reader["FromDate"] == DBNull.Value
                        ? (DateTime?)null
                        : Convert.ToDateTime(
                            reader["FromDate"]
                        ),

                ToDate =
                    reader["ToDate"] == DBNull.Value
                        ? (DateTime?)null
                        : Convert.ToDateTime(
                            reader["ToDate"]
                        ),

                DocStatus =
                    HasColumn("DocStatus") &&
                    reader["DocStatus"] != DBNull.Value
                        ? Convert.ToInt32(
                            reader["DocStatus"]
                        )
                        : 0,

                AmendedFrom =
                    HasColumn("AmendedFrom") &&
                    reader["AmendedFrom"] != DBNull.Value
                        ? reader["AmendedFrom"].ToString()
                        : ""
            };
        }
    }
}
