////using System;
////using System.Collections.Generic;
////using Microsoft.Data.SqlClient;
////using Erp_Aits.Models;

////namespace Erp_Aits.Services
////{
////    public class EmployeeGrievanceService
////    {
////        private readonly string _connectionString;

////        public EmployeeGrievanceService(string connectionString)
////        {
////            _connectionString = connectionString;
////        }

////        public List<EmployeeGrievance> GetAll(string idFilter, string subjectFilter)
////        {
////            var list = new List<EmployeeGrievance>();

////            using (var conn = new SqlConnection(_connectionString))
////            {
////                string query = "SELECT * FROM EmployeeGrievances WHERE IsDeleted = 0";

////                if (!string.IsNullOrEmpty(idFilter)) query += " AND CAST(Id AS VARCHAR) LIKE @IdFilter";
////                if (!string.IsNullOrEmpty(subjectFilter)) query += " AND Subject LIKE @SubjectFilter";

////                query += " ORDER BY Id DESC";

////                using (var cmd = new SqlCommand(query, conn))
////                {
////                    if (!string.IsNullOrEmpty(idFilter)) cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");
////                    if (!string.IsNullOrEmpty(subjectFilter)) cmd.Parameters.AddWithValue("@SubjectFilter", "%" + subjectFilter + "%");

////                    conn.Open();
////                    using (var reader = cmd.ExecuteReader())
////                    {
////                        while (reader.Read())
////                        {
////                            list.Add(MapGrievance(reader));
////                        }
////                    }
////                }
////            }

////            return list;
////        }

////        public EmployeeGrievance GetById(int id)
////        {
////            EmployeeGrievance model = null;

////            using (var conn = new SqlConnection(_connectionString))
////            {
////                conn.Open();
////                using (var cmd = new SqlCommand("SELECT * FROM EmployeeGrievances WHERE Id = @Id AND IsDeleted = 0", conn))
////                {
////                    cmd.Parameters.AddWithValue("@Id", id);
////                    using (var reader = cmd.ExecuteReader())
////                    {
////                        if (reader.Read())
////                        {
////                            model = MapGrievance(reader);
////                        }
////                    }
////                }
////            }

////            return model;
////        }

////        public int Insert(EmployeeGrievance g)
////        {
////            using (var conn = new SqlConnection(_connectionString))
////            {
////                using (var cmd = new SqlCommand(@"
////                    INSERT INTO EmployeeGrievances
////                        (Subject, GrievanceDate, RaisedBy, Status, GrievanceAgainstParty, GrievanceAgainst,
////                         AssociatedDocumentType, AssociatedDocument, GrievanceType, Description, CauseOfGrievance,
////                         ResolvedBy, ResolutionDate, ResolutionDetails, EmployeeResponsible)
////                    OUTPUT INSERTED.Id
////                    VALUES
////                        (@Subject, @GrievanceDate, @RaisedBy, @Status, @GrievanceAgainstParty, @GrievanceAgainst,
////                         @AssociatedDocumentType, @AssociatedDocument, @GrievanceType, @Description, @CauseOfGrievance,
////                         @ResolvedBy, @ResolutionDate, @ResolutionDetails, @EmployeeResponsible)", conn))
////                {
////                    cmd.Parameters.AddWithValue("@Subject", g.Subject ?? (object)DBNull.Value);
////                    cmd.Parameters.AddWithValue("@GrievanceDate", g.GrievanceDate);
////                    cmd.Parameters.AddWithValue("@RaisedBy", g.RaisedBy ?? (object)DBNull.Value);
////                    cmd.Parameters.AddWithValue("@Status", g.Status ?? "Invalid");
////                    cmd.Parameters.AddWithValue("@GrievanceAgainstParty", (object)g.GrievanceAgainstParty ?? DBNull.Value);
////                    cmd.Parameters.AddWithValue("@GrievanceAgainst", (object)g.GrievanceAgainst ?? DBNull.Value);
////                    cmd.Parameters.AddWithValue("@AssociatedDocumentType", (object)g.AssociatedDocumentType ?? DBNull.Value);
////                    cmd.Parameters.AddWithValue("@AssociatedDocument", (object)g.AssociatedDocument ?? DBNull.Value);
////                    cmd.Parameters.AddWithValue("@GrievanceType", (object)g.GrievanceType ?? DBNull.Value);
////                    cmd.Parameters.AddWithValue("@Description", (object)g.Description ?? DBNull.Value);
////                    cmd.Parameters.AddWithValue("@CauseOfGrievance", (object)g.CauseOfGrievance ?? DBNull.Value);
////                    cmd.Parameters.AddWithValue("@ResolvedBy", (object)g.ResolvedBy ?? DBNull.Value);
////                    cmd.Parameters.AddWithValue("@ResolutionDate", (object)g.ResolutionDate ?? DBNull.Value);
////                    cmd.Parameters.AddWithValue("@ResolutionDetails", (object)g.ResolutionDetails ?? DBNull.Value);
////                    cmd.Parameters.AddWithValue("@EmployeeResponsible", (object)g.EmployeeResponsible ?? DBNull.Value);

////                    conn.Open();
////                    return (int)cmd.ExecuteScalar();
////                }
////            }
////        }

////        private EmployeeGrievance MapGrievance(SqlDataReader reader)
////        {
////            return new EmployeeGrievance
////            {
////                Id = Convert.ToInt32(reader["Id"]),
////                Subject = reader["Subject"]?.ToString() ?? string.Empty,
////                GrievanceDate = Convert.ToDateTime(reader["GrievanceDate"]),
////                RaisedBy = reader["RaisedBy"]?.ToString() ?? string.Empty,
////                Status = reader["Status"]?.ToString() ?? "Invalid",
////                GrievanceAgainstParty = reader["GrievanceAgainstParty"]?.ToString() ?? string.Empty,
////                GrievanceAgainst = reader["GrievanceAgainst"]?.ToString() ?? string.Empty,
////                AssociatedDocumentType = reader["AssociatedDocumentType"]?.ToString() ?? string.Empty,
////                AssociatedDocument = reader["AssociatedDocument"]?.ToString() ?? string.Empty,
////                GrievanceType = reader["GrievanceType"]?.ToString() ?? string.Empty,
////                Description = reader["Description"]?.ToString() ?? string.Empty,
////                CauseOfGrievance = reader["CauseOfGrievance"]?.ToString() ?? string.Empty,
////                ResolvedBy = reader["ResolvedBy"]?.ToString() ?? string.Empty,
////                ResolutionDate = reader["ResolutionDate"] as DateTime?,
////                ResolutionDetails = reader["ResolutionDetails"]?.ToString() ?? string.Empty,
////                EmployeeResponsible = reader["EmployeeResponsible"]?.ToString() ?? string.Empty
////            };
////        }
////    }
////}




//using System;
//using System.Collections.Generic;
//using Microsoft.Data.SqlClient;
//using Erp_Aits.Models;

//namespace Erp_Aits.Services
//{
//    public class EmployeeGrievanceService
//    {
//        private readonly string _connectionString;

//        public EmployeeGrievanceService(string connectionString)
//        {
//            _connectionString = connectionString;
//        }

//        public List<EmployeeGrievance> GetAll(string idFilter = null, string subjectFilter = null)
//        {
//            var list = new List<EmployeeGrievance>();

//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = "SELECT * FROM EmployeeGrievances WHERE IsDeleted = 0";

//                if (!string.IsNullOrEmpty(idFilter)) query += " AND CAST(Id AS VARCHAR) LIKE @IdFilter";
//                if (!string.IsNullOrEmpty(subjectFilter)) query += " AND Subject LIKE @SubjectFilter";

//                query += " ORDER BY Id DESC";

//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    if (!string.IsNullOrEmpty(idFilter)) cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");
//                    if (!string.IsNullOrEmpty(subjectFilter)) cmd.Parameters.AddWithValue("@SubjectFilter", "%" + subjectFilter + "%");

//                    conn.Open();
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            list.Add(MapGrievance(reader));
//                        }
//                    }
//                }
//            }

//            return list;
//        }

//        public HashSet<string> GetExistingErpNextIds()
//        {
//            var ids = new HashSet<string>();
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = "SELECT ErpNextId FROM EmployeeGrievances WHERE ErpNextId IS NOT NULL AND ErpNextId != ''";
//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    conn.Open();
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            ids.Add(reader["ErpNextId"].ToString());
//                        }
//                    }
//                }
//            }
//            return ids;
//        }

//        public EmployeeGrievance GetById(int id)
//        {
//            EmployeeGrievance model = null;

//            using (var conn = new SqlConnection(_connectionString))
//            {
//                conn.Open();
//                using (var cmd = new SqlCommand("SELECT * FROM EmployeeGrievances WHERE Id = @Id AND IsDeleted = 0", conn))
//                {
//                    cmd.Parameters.AddWithValue("@Id", id);
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        if (reader.Read())
//                        {
//                            model = MapGrievance(reader);
//                        }
//                    }
//                }
//            }

//            return model;
//        }

//        public int Insert(EmployeeGrievance g)
//        {
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                using (var cmd = new SqlCommand(@"
//                    INSERT INTO EmployeeGrievances
//                        (ErpNextId, Subject, GrievanceDate, RaisedBy, Status, GrievanceAgainstParty, GrievanceAgainst,
//                         AssociatedDocumentType, AssociatedDocument, GrievanceType, Description, CauseOfGrievance,
//                         ResolvedBy, ResolutionDate, ResolutionDetails, EmployeeResponsible, IsDeleted)
//                    OUTPUT INSERTED.Id
//                    VALUES
//                        (@ErpNextId, @Subject, @GrievanceDate, @RaisedBy, @Status, @GrievanceAgainstParty, @GrievanceAgainst,
//                         @AssociatedDocumentType, @AssociatedDocument, @GrievanceType, @Description, @CauseOfGrievance,
//                         @ResolvedBy, @ResolutionDate, @ResolutionDetails, @EmployeeResponsible, 0)", conn))
//                {
//                    cmd.Parameters.AddWithValue("@ErpNextId", (object)g.ErpNextId ?? DBNull.Value);
//                    cmd.Parameters.AddWithValue("@Subject", g.Subject ?? (object)DBNull.Value);
//                    cmd.Parameters.AddWithValue("@GrievanceDate", g.GrievanceDate);
//                    cmd.Parameters.AddWithValue("@RaisedBy", g.RaisedBy ?? (object)DBNull.Value);
//                    cmd.Parameters.AddWithValue("@Status", g.Status ?? "Invalid");
//                    cmd.Parameters.AddWithValue("@GrievanceAgainstParty", (object)g.GrievanceAgainstParty ?? DBNull.Value);
//                    cmd.Parameters.AddWithValue("@GrievanceAgainst", (object)g.GrievanceAgainst ?? DBNull.Value);
//                    cmd.Parameters.AddWithValue("@AssociatedDocumentType", (object)g.AssociatedDocumentType ?? DBNull.Value);
//                    cmd.Parameters.AddWithValue("@AssociatedDocument", (object)g.AssociatedDocument ?? DBNull.Value);
//                    cmd.Parameters.AddWithValue("@GrievanceType", (object)g.GrievanceType ?? DBNull.Value);
//                    cmd.Parameters.AddWithValue("@Description", (object)g.Description ?? DBNull.Value);
//                    cmd.Parameters.AddWithValue("@CauseOfGrievance", (object)g.CauseOfGrievance ?? DBNull.Value);
//                    cmd.Parameters.AddWithValue("@ResolvedBy", (object)g.ResolvedBy ?? DBNull.Value);
//                    cmd.Parameters.AddWithValue("@ResolutionDate", (object)g.ResolutionDate ?? DBNull.Value);
//                    cmd.Parameters.AddWithValue("@ResolutionDetails", (object)g.ResolutionDetails ?? DBNull.Value);
//                    cmd.Parameters.AddWithValue("@EmployeeResponsible", (object)g.EmployeeResponsible ?? DBNull.Value);

//                    conn.Open();
//                    return (int)cmd.ExecuteScalar();
//                }
//            }
//        }

//        private EmployeeGrievance MapGrievance(SqlDataReader reader)
//        {
//            return new EmployeeGrievance
//            {
//                Id = Convert.ToInt32(reader["Id"]),
//                ErpNextId = reader["ErpNextId"] == DBNull.Value ? null : reader["ErpNextId"].ToString(),
//                Subject = reader["Subject"]?.ToString() ?? string.Empty,
//                GrievanceDate = Convert.ToDateTime(reader["GrievanceDate"]),
//                RaisedBy = reader["RaisedBy"]?.ToString() ?? string.Empty,
//                Status = reader["Status"]?.ToString() ?? "Invalid",
//                GrievanceAgainstParty = reader["GrievanceAgainstParty"]?.ToString() ?? string.Empty,
//                GrievanceAgainst = reader["GrievanceAgainst"]?.ToString() ?? string.Empty,
//                AssociatedDocumentType = reader["AssociatedDocumentType"]?.ToString() ?? string.Empty,
//                AssociatedDocument = reader["AssociatedDocument"]?.ToString() ?? string.Empty,
//                GrievanceType = reader["GrievanceType"]?.ToString() ?? string.Empty,
//                Description = reader["Description"]?.ToString() ?? string.Empty,
//                CauseOfGrievance = reader["CauseOfGrievance"]?.ToString() ?? string.Empty,
//                ResolvedBy = reader["ResolvedBy"]?.ToString() ?? string.Empty,
//                ResolutionDate = reader["ResolutionDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ResolutionDate"]),
//                ResolutionDetails = reader["ResolutionDetails"]?.ToString() ?? string.Empty,
//                EmployeeResponsible = reader["EmployeeResponsible"]?.ToString() ?? string.Empty
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
    public class EmployeeGrievanceService
    {
        private readonly string _connectionString;

        public EmployeeGrievanceService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<EmployeeGrievance> GetAll(string idFilter = null, string subjectFilter = null)
        {
            var list = new List<EmployeeGrievance>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM EmployeeGrievances WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(idFilter)) query += " AND CAST(Id AS VARCHAR) LIKE @IdFilter";
                if (!string.IsNullOrEmpty(subjectFilter)) query += " AND Subject LIKE @SubjectFilter";

                query += " ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(idFilter)) cmd.Parameters.AddWithValue("@IdFilter", "%" + idFilter + "%");
                    if (!string.IsNullOrEmpty(subjectFilter)) cmd.Parameters.AddWithValue("@SubjectFilter", "%" + subjectFilter + "%");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(MapGrievance(reader));
                        }
                    }
                }
            }

            return list;
        }

        public HashSet<string> GetExistingErpNextIds()
        {
            var ids = new HashSet<string>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT ErpNextId FROM EmployeeGrievances WHERE ErpNextId IS NOT NULL AND ErpNextId != ''";
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ids.Add(reader["ErpNextId"].ToString());
                        }
                    }
                }
            }
            return ids;
        }

        public EmployeeGrievance GetById(int id)
        {
            EmployeeGrievance model = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT * FROM EmployeeGrievances WHERE Id = @Id AND IsDeleted = 0", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model = MapGrievance(reader);
                        }
                    }
                }
            }

            return model;
        }

        public int Insert(EmployeeGrievance g)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand(@"
                    INSERT INTO EmployeeGrievances
                        (ErpNextId, Subject, GrievanceDate, RaisedBy, Status, GrievanceAgainstParty, GrievanceAgainst,
                         AssociatedDocumentType, AssociatedDocument, GrievanceType, Description, CauseOfGrievance,
                         ResolvedBy, ResolutionDate, ResolutionDetails, EmployeeResponsible, IsDeleted)
                    OUTPUT INSERTED.Id
                    VALUES
                        (@ErpNextId, @Subject, @GrievanceDate, @RaisedBy, @Status, @GrievanceAgainstParty, @GrievanceAgainst,
                         @AssociatedDocumentType, @AssociatedDocument, @GrievanceType, @Description, @CauseOfGrievance,
                         @ResolvedBy, @ResolutionDate, @ResolutionDetails, @EmployeeResponsible, 0)", conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", (object)g.ErpNextId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Subject", g.Subject ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@GrievanceDate", g.GrievanceDate);
                    cmd.Parameters.AddWithValue("@RaisedBy", g.RaisedBy ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", g.Status ?? "Invalid");
                    cmd.Parameters.AddWithValue("@GrievanceAgainstParty", (object)g.GrievanceAgainstParty ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@GrievanceAgainst", (object)g.GrievanceAgainst ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AssociatedDocumentType", (object)g.AssociatedDocumentType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AssociatedDocument", (object)g.AssociatedDocument ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@GrievanceType", (object)g.GrievanceType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Description", (object)g.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CauseOfGrievance", (object)g.CauseOfGrievance ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ResolvedBy", (object)g.ResolvedBy ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ResolutionDate", (object)g.ResolutionDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ResolutionDetails", (object)g.ResolutionDetails ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmployeeResponsible", (object)g.EmployeeResponsible ?? DBNull.Value);

                    conn.Open();
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        private EmployeeGrievance MapGrievance(SqlDataReader reader)
        {
            // Helper method to safely verify column existence before reading
            bool HasColumn(SqlDataReader r, string columnName)
            {
                for (int i = 0; i < r.FieldCount; i++)
                {
                    if (r.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                return false;
            }

            return new EmployeeGrievance
            {
                Id = Convert.ToInt32(reader["Id"]),
                ErpNextId = (HasColumn(reader, "ErpNextId") && reader["ErpNextId"] != DBNull.Value) ? reader["ErpNextId"].ToString() : null,
                Subject = reader["Subject"]?.ToString() ?? string.Empty,
                GrievanceDate = Convert.ToDateTime(reader["GrievanceDate"]),
                RaisedBy = reader["RaisedBy"]?.ToString() ?? string.Empty,
                Status = reader["Status"]?.ToString() ?? "Invalid",
                GrievanceAgainstParty = reader["GrievanceAgainstParty"]?.ToString() ?? string.Empty,
                GrievanceAgainst = reader["GrievanceAgainst"]?.ToString() ?? string.Empty,
                AssociatedDocumentType = reader["AssociatedDocumentType"]?.ToString() ?? string.Empty,
                AssociatedDocument = reader["AssociatedDocument"]?.ToString() ?? string.Empty,
                GrievanceType = reader["GrievanceType"]?.ToString() ?? string.Empty,
                Description = reader["Description"]?.ToString() ?? string.Empty,
                CauseOfGrievance = reader["CauseOfGrievance"]?.ToString() ?? string.Empty,
                ResolvedBy = reader["ResolvedBy"]?.ToString() ?? string.Empty,
                ResolutionDate = (HasColumn(reader, "ResolutionDate") && reader["ResolutionDate"] != DBNull.Value) ? Convert.ToDateTime(reader["ResolutionDate"]) : (DateTime?)null,
                ResolutionDetails = reader["ResolutionDetails"]?.ToString() ?? string.Empty,
                EmployeeResponsible = reader["EmployeeResponsible"]?.ToString() ?? string.Empty
            };
        }
    }
}