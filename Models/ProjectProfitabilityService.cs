using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Erp_Aits.Models;

namespace Erp_Aits.Services
{
    public class ProjectProfitabilityService
    {
        private readonly string _connectionString;

        public ProjectProfitabilityService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<ProjectProfitabilityRow> GetProfitability(
            DateTime fromDate, DateTime toDate, string customer, string employee, string project, string company)
        {
            var list = new List<ProjectProfitabilityRow>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT
                        t.Customer,
                        d.Project,
                        SUM(d.Hrs) AS TotalHours,
                        SUM(d.Hrs * ISNULL(ac.BillingRate, ISNULL(at.DefaultBillingRate, 0))) AS BillingAmount,
                        SUM(d.Hrs * ISNULL(ac.CostingRate, ISNULL(at.DefaultCostingRate, 0))) AS CostingAmount
                    FROM TimeSheetDetails d
                    INNER JOIN TimeSheets t ON t.Id = d.TimeSheetId
                    LEFT JOIN ActivityCosts ac ON ac.ActivityType = d.ActivityType AND ac.Employee = t.Employee
                    LEFT JOIN ActivityTypes at ON at.Name = d.ActivityType
                    WHERE ISNULL(d.FromTime, t.CreatedOn) BETWEEN @FromDate AND @ToDate
                      AND t.IsDeleted = 0";

                if (!string.IsNullOrEmpty(company)) query += " AND t.Company = @Company";
                if (!string.IsNullOrEmpty(customer)) query += " AND t.Customer LIKE @Customer";
                if (!string.IsNullOrEmpty(employee)) query += " AND t.Employee LIKE @Employee";
                if (!string.IsNullOrEmpty(project)) query += " AND d.Project LIKE @Project";

                query += " GROUP BY t.Customer, d.Project ORDER BY d.Project";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);
                    if (!string.IsNullOrEmpty(company)) cmd.Parameters.AddWithValue("@Company", company);
                    if (!string.IsNullOrEmpty(customer)) cmd.Parameters.AddWithValue("@Customer", "%" + customer + "%");
                    if (!string.IsNullOrEmpty(employee)) cmd.Parameters.AddWithValue("@Employee", "%" + employee + "%");
                    if (!string.IsNullOrEmpty(project)) cmd.Parameters.AddWithValue("@Project", "%" + project + "%");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var billing = reader["BillingAmount"] != DBNull.Value ? Convert.ToDecimal(reader["BillingAmount"]) : 0;
                            var costing = reader["CostingAmount"] != DBNull.Value ? Convert.ToDecimal(reader["CostingAmount"]) : 0;

                            list.Add(new ProjectProfitabilityRow
                            {
                                Customer = reader["Customer"]?.ToString() ?? string.Empty,
                                Project = reader["Project"]?.ToString() ?? string.Empty,
                                TotalHours = reader["TotalHours"] != DBNull.Value ? Convert.ToDecimal(reader["TotalHours"]) : 0,
                                BillingAmount = billing,
                                CostingAmount = costing,
                                Profit = billing - costing
                            });
                        }
                    }
                }
            }

            return list;
        }
    }
}
