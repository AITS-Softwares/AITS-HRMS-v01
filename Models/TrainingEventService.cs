using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class TrainingEventService
    {
        private readonly string _connectionString;

        public TrainingEventService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<TrainingEvent> GetAll(string idFilter = null, string eventNameFilter = null, string eventStatusFilter = null,
            string typeFilter = null, string courseFilter = null, string locationFilter = null)
        {
            var list = new List<TrainingEvent>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT Id, TrainingEventCode, EventName, EventStatus, Type, Course, Location
                                  FROM TrainingEvents WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(idFilter)) query += " AND TrainingEventCode LIKE @IdFilter";
                if (!string.IsNullOrEmpty(eventNameFilter)) query += " AND EventName LIKE @EventNameFilter";
                if (!string.IsNullOrEmpty(eventStatusFilter)) query += " AND EventStatus = @EventStatusFilter";
                if (!string.IsNullOrEmpty(typeFilter)) query += " AND Type = @TypeFilter";
                if (!string.IsNullOrEmpty(courseFilter)) query += " AND Course LIKE @CourseFilter";
                if (!string.IsNullOrEmpty(locationFilter)) query += " AND Location LIKE @LocationFilter";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(idFilter)) cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");
                    if (!string.IsNullOrEmpty(eventNameFilter)) cmd.Parameters.AddWithValue("@EventNameFilter", "%" + eventNameFilter + "%");
                    if (!string.IsNullOrEmpty(eventStatusFilter)) cmd.Parameters.AddWithValue("@EventStatusFilter", eventStatusFilter);
                    if (!string.IsNullOrEmpty(typeFilter)) cmd.Parameters.AddWithValue("@TypeFilter", typeFilter);
                    if (!string.IsNullOrEmpty(courseFilter)) cmd.Parameters.AddWithValue("@CourseFilter", "%" + courseFilter + "%");
                    if (!string.IsNullOrEmpty(locationFilter)) cmd.Parameters.AddWithValue("@LocationFilter", "%" + locationFilter + "%");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new TrainingEvent
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                TrainingEventCode = reader["TrainingEventCode"]?.ToString() ?? string.Empty,
                                EventName = reader["EventName"]?.ToString() ?? string.Empty,
                                EventStatus = reader["EventStatus"]?.ToString() ?? string.Empty,
                                Type = reader["Type"]?.ToString() ?? string.Empty,
                                Course = reader["Course"]?.ToString() ?? string.Empty,
                                Location = reader["Location"]?.ToString() ?? string.Empty
                            });
                        }
                    }
                }
            }

            return list;
        }

        public TrainingEvent GetById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM TrainingEvents WHERE Id = @Id AND IsDeleted = 0";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new TrainingEvent
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                TrainingEventCode = reader["TrainingEventCode"]?.ToString() ?? string.Empty,
                                EventName = reader["EventName"]?.ToString() ?? string.Empty,
                                Type = reader["Type"]?.ToString() ?? "Seminar",
                                TrainingProgram = reader["TrainingProgram"]?.ToString() ?? string.Empty,
                                Level = reader["Level"]?.ToString() ?? string.Empty,
                                EventStatus = reader["EventStatus"]?.ToString() ?? "Scheduled",
                                Company = reader["Company"]?.ToString() ?? string.Empty,
                                HasCertificate = Convert.ToBoolean(reader["HasCertificate"]),
                                TrainerName = reader["TrainerName"]?.ToString() ?? string.Empty,
                                Supplier = reader["Supplier"]?.ToString() ?? string.Empty,
                                TrainerEmail = reader["TrainerEmail"]?.ToString() ?? string.Empty,
                                ContactNumber = reader["ContactNumber"]?.ToString() ?? string.Empty,
                                Course = reader["Course"]?.ToString() ?? string.Empty,
                                Location = reader["Location"]?.ToString() ?? string.Empty,
                                StartTime = reader["StartTime"] as DateTime?,
                                EndTime = reader["EndTime"] as DateTime?,
                                Introduction = reader["Introduction"]?.ToString() ?? string.Empty
                            };
                        }
                    }
                }
            }

            return null;
        }

        public string GetNextCode()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT COUNT(*) FROM TrainingEvents";
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    int count = (int)cmd.ExecuteScalar() + 1;
                    return $"TRN-EVT-{count:D5}";
                }
            }
        }

        public int Insert(TrainingEvent model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    INSERT INTO TrainingEvents
                        (TrainingEventCode, EventName, Type, TrainingProgram, Level, EventStatus, Company, HasCertificate,
                         TrainerName, Supplier, TrainerEmail, ContactNumber, Course, Location, StartTime, EndTime, Introduction)
                    OUTPUT INSERTED.Id
                    VALUES
                        (@TrainingEventCode, @EventName, @Type, @TrainingProgram, @Level, @EventStatus, @Company, @HasCertificate,
                         @TrainerName, @Supplier, @TrainerEmail, @ContactNumber, @Course, @Location, @StartTime, @EndTime, @Introduction)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TrainingEventCode", model.TrainingEventCode ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EventName", model.EventName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Type", model.Type ?? "Seminar");
                    cmd.Parameters.AddWithValue("@TrainingProgram", model.TrainingProgram ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Level", model.Level ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EventStatus", model.EventStatus ?? "Scheduled");
                    cmd.Parameters.AddWithValue("@Company", model.Company ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@HasCertificate", model.HasCertificate);
                    cmd.Parameters.AddWithValue("@TrainerName", model.TrainerName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Supplier", model.Supplier ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@TrainerEmail", model.TrainerEmail ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ContactNumber", model.ContactNumber ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Course", model.Course ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Location", model.Location ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@StartTime", (object)model.StartTime ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EndTime", (object)model.EndTime ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Introduction", model.Introduction ?? (object)DBNull.Value);

                    conn.Open();
                    return (int)cmd.ExecuteScalar();
                }
            }
        }
    }
}
