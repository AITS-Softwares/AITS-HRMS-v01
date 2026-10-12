using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class TrainingFeedbackService
    {
        private readonly string _connectionString;

        public TrainingFeedbackService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<TrainingFeedback> GetAll(string id = null, string employee = null, string employeeName = null, string trainingEvent = null)
        {
            var list = new List<TrainingFeedback>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT Id, Employee, EmployeeName, TrainingEvent, Feedback
                                  FROM TrainingFeedbacks WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(id)) query += " AND CAST(Id AS VARCHAR) LIKE @Id";
                if (!string.IsNullOrEmpty(employee)) query += " AND Employee LIKE @Employee";
                if (!string.IsNullOrEmpty(employeeName)) query += " AND EmployeeName LIKE @EmployeeName";
                if (!string.IsNullOrEmpty(trainingEvent)) query += " AND TrainingEvent LIKE @TrainingEvent";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(id)) cmd.Parameters.AddWithValue("@Id", "%" + id + "%");
                    if (!string.IsNullOrEmpty(employee)) cmd.Parameters.AddWithValue("@Employee", "%" + employee + "%");
                    if (!string.IsNullOrEmpty(employeeName)) cmd.Parameters.AddWithValue("@EmployeeName", "%" + employeeName + "%");
                    if (!string.IsNullOrEmpty(trainingEvent)) cmd.Parameters.AddWithValue("@TrainingEvent", "%" + trainingEvent + "%");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(Map(reader));
                        }
                    }
                }
            }
            return list;
        }

        public TrainingFeedback GetById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT Id, Employee, EmployeeName, TrainingEvent, Feedback
                                  FROM TrainingFeedbacks WHERE Id = @Id AND IsDeleted = 0";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            return Map(reader);
                    }
                }
            }
            return null;
        }

        public int Insert(TrainingFeedback model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"INSERT INTO TrainingFeedbacks (Employee, EmployeeName, TrainingEvent, Feedback)
                                  OUTPUT INSERTED.Id
                                  VALUES (@Employee, @EmployeeName, @TrainingEvent, @Feedback)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Employee", model.Employee ?? string.Empty);
                    cmd.Parameters.AddWithValue("@EmployeeName", (object)model.EmployeeName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TrainingEvent", model.TrainingEvent ?? string.Empty);
                    cmd.Parameters.AddWithValue("@Feedback", model.Feedback ?? string.Empty);

                    conn.Open();
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        private TrainingFeedback Map(SqlDataReader reader)
        {
            return new TrainingFeedback
            {
                Id = Convert.ToInt32(reader["Id"]),
                Employee = reader["Employee"]?.ToString(),
                EmployeeName = reader["EmployeeName"] == DBNull.Value ? null : reader["EmployeeName"].ToString(),
                TrainingEvent = reader["TrainingEvent"]?.ToString(),
                Feedback = reader["Feedback"]?.ToString()
            };
        }
    }
}
