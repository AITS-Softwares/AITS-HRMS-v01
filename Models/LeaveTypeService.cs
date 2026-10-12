//using System;
//using System.Collections.Generic;
//using Microsoft.Data.SqlClient;
//using Erp_Aits.Models;

//namespace Erp_Aits.Services
//{
//    public class LeaveTypeService
//    {
//        private readonly string _connectionString;

//        public LeaveTypeService(string connectionString)
//        {
//            _connectionString = connectionString;
//        }

//        public List<LeaveType> GetAll(string id = null)
//        {
//            var list = new List<LeaveType>();
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = @"SELECT Id, LeaveTypeName, MaxLeavesAllowed, ApplicableAfterWorkingDays, MaxContinuousDaysAllowed,
//                                         IsCarryForward, IsLWP, IsPartiallyPaidLeave, IsOptionalLeave,
//                                         AllowNegativeBalance, AllowOverAllocation, IncludeHolidaysWithinLeaves, IsCompensatory,
//                                         AllowEncashment, IsEarnedLeave
//                                  FROM LeaveTypes WHERE IsDeleted = 0";

//                if (!string.IsNullOrEmpty(id))
//                    query += " AND LeaveTypeName LIKE @Id";

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
//                            list.Add(Map(reader));
//                        }
//                    }
//                }
//            }
//            return list;
//        }

//        public LeaveType GetById(int id)
//        {
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = @"SELECT Id, LeaveTypeName, MaxLeavesAllowed, ApplicableAfterWorkingDays, MaxContinuousDaysAllowed,
//                                         IsCarryForward, IsLWP, IsPartiallyPaidLeave, IsOptionalLeave,
//                                         AllowNegativeBalance, AllowOverAllocation, IncludeHolidaysWithinLeaves, IsCompensatory,
//                                         AllowEncashment, IsEarnedLeave
//                                  FROM LeaveTypes WHERE Id = @Id AND IsDeleted = 0";

//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    cmd.Parameters.AddWithValue("@Id", id);
//                    conn.Open();
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        if (reader.Read())
//                            return Map(reader);
//                    }
//                }
//            }
//            return null;
//        }

//        public int Insert(LeaveType model)
//        {
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                string query = @"INSERT INTO LeaveTypes
//                    (LeaveTypeName, MaxLeavesAllowed, ApplicableAfterWorkingDays, MaxContinuousDaysAllowed,
//                     IsCarryForward, IsLWP, IsPartiallyPaidLeave, IsOptionalLeave,
//                     AllowNegativeBalance, AllowOverAllocation, IncludeHolidaysWithinLeaves, IsCompensatory,
//                     AllowEncashment, IsEarnedLeave)
//                    OUTPUT INSERTED.Id
//                    VALUES
//                    (@LeaveTypeName, @MaxLeavesAllowed, @ApplicableAfterWorkingDays, @MaxContinuousDaysAllowed,
//                     @IsCarryForward, @IsLWP, @IsPartiallyPaidLeave, @IsOptionalLeave,
//                     @AllowNegativeBalance, @AllowOverAllocation, @IncludeHolidaysWithinLeaves, @IsCompensatory,
//                     @AllowEncashment, @IsEarnedLeave)";

//                using (var cmd = new SqlCommand(query, conn))
//                {
//                    cmd.Parameters.AddWithValue("@LeaveTypeName", model.LeaveTypeName ?? string.Empty);
//                    cmd.Parameters.AddWithValue("@MaxLeavesAllowed", (object)model.MaxLeavesAllowed ?? DBNull.Value);
//                    cmd.Parameters.AddWithValue("@ApplicableAfterWorkingDays", (object)model.ApplicableAfterWorkingDays ?? DBNull.Value);
//                    cmd.Parameters.AddWithValue("@MaxContinuousDaysAllowed", (object)model.MaxContinuousDaysAllowed ?? DBNull.Value);
//                    cmd.Parameters.AddWithValue("@IsCarryForward", model.IsCarryForward);
//                    cmd.Parameters.AddWithValue("@IsLWP", model.IsLWP);
//                    cmd.Parameters.AddWithValue("@IsPartiallyPaidLeave", model.IsPartiallyPaidLeave);
//                    cmd.Parameters.AddWithValue("@IsOptionalLeave", model.IsOptionalLeave);
//                    cmd.Parameters.AddWithValue("@AllowNegativeBalance", model.AllowNegativeBalance);
//                    cmd.Parameters.AddWithValue("@AllowOverAllocation", model.AllowOverAllocation);
//                    cmd.Parameters.AddWithValue("@IncludeHolidaysWithinLeaves", model.IncludeHolidaysWithinLeaves);
//                    cmd.Parameters.AddWithValue("@IsCompensatory", model.IsCompensatory);
//                    cmd.Parameters.AddWithValue("@AllowEncashment", model.AllowEncashment);
//                    cmd.Parameters.AddWithValue("@IsEarnedLeave", model.IsEarnedLeave);

//                    conn.Open();
//                    return (int)cmd.ExecuteScalar();
//                }
//            }
//        }

//        private LeaveType Map(SqlDataReader reader)
//        {
//            return new LeaveType
//            {
//                Id = Convert.ToInt32(reader["Id"]),
//                LeaveTypeName = reader["LeaveTypeName"]?.ToString(),
//                MaxLeavesAllowed = reader["MaxLeavesAllowed"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["MaxLeavesAllowed"]),
//                ApplicableAfterWorkingDays = reader["ApplicableAfterWorkingDays"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["ApplicableAfterWorkingDays"]),
//                MaxContinuousDaysAllowed = reader["MaxContinuousDaysAllowed"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["MaxContinuousDaysAllowed"]),
//                IsCarryForward = Convert.ToBoolean(reader["IsCarryForward"]),
//                IsLWP = Convert.ToBoolean(reader["IsLWP"]),
//                IsPartiallyPaidLeave = Convert.ToBoolean(reader["IsPartiallyPaidLeave"]),
//                IsOptionalLeave = Convert.ToBoolean(reader["IsOptionalLeave"]),
//                AllowNegativeBalance = Convert.ToBoolean(reader["AllowNegativeBalance"]),
//                AllowOverAllocation = Convert.ToBoolean(reader["AllowOverAllocation"]),
//                IncludeHolidaysWithinLeaves = Convert.ToBoolean(reader["IncludeHolidaysWithinLeaves"]),
//                IsCompensatory = Convert.ToBoolean(reader["IsCompensatory"]),
//                AllowEncashment = Convert.ToBoolean(reader["AllowEncashment"]),
//                IsEarnedLeave = Convert.ToBoolean(reader["IsEarnedLeave"])
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
    public class LeaveTypeService
    {
        private readonly string _connectionString;
        private static readonly object EnsureColumnsLock = new object();
        private static bool _columnsEnsured;

        public LeaveTypeService(string connectionString)
        {
            _connectionString = connectionString;
            EnsureColumnsOnce();
        }

        public List<LeaveType> GetAll(string id = null)
        {
            var list = new List<LeaveType>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT Id, LeaveTypeName, MaxLeavesAllowed, ApplicableAfterWorkingDays, MaxContinuousDaysAllowed,
                                         IsCarryForward, IsLWP, IsPartiallyPaidLeave, IsOptionalLeave,
                                         AllowNegativeBalance, AllowOverAllocation, IncludeHolidaysWithinLeaves, IsCompensatory,
                                         AllowEncashment, MaxEncashableLeaves, NonEncashableLeaves, EarningComponent,
                                         IsEarnedLeave, EarnedLeaveFrequency, AllocateOnDay, Rounding,
                                         MaxCarryForwardedLeaves, ExpireCarryForwardedLeaves, FractionOfDailySalaryPerLeave, ErpNextId
                                  FROM LeaveTypes WHERE IsDeleted = 0";

                if (!string.IsNullOrEmpty(id))
                    query += " AND LeaveTypeName LIKE @Id";

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
                            list.Add(Map(reader));
                        }
                    }
                }
            }
            return list;
        }

        public LeaveType GetById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT Id, LeaveTypeName, MaxLeavesAllowed, ApplicableAfterWorkingDays, MaxContinuousDaysAllowed,
                                         IsCarryForward, IsLWP, IsPartiallyPaidLeave, IsOptionalLeave,
                                         AllowNegativeBalance, AllowOverAllocation, IncludeHolidaysWithinLeaves, IsCompensatory,
                                         AllowEncashment, MaxEncashableLeaves, NonEncashableLeaves, EarningComponent,
                                         IsEarnedLeave, EarnedLeaveFrequency, AllocateOnDay, Rounding,
                                         MaxCarryForwardedLeaves, ExpireCarryForwardedLeaves, FractionOfDailySalaryPerLeave, ErpNextId
                                  FROM LeaveTypes WHERE Id = @Id AND IsDeleted = 0";

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

        public int Insert(LeaveType model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"INSERT INTO LeaveTypes
                    (LeaveTypeName, MaxLeavesAllowed, ApplicableAfterWorkingDays, MaxContinuousDaysAllowed,
                     IsCarryForward, IsLWP, IsPartiallyPaidLeave, IsOptionalLeave,
                     AllowNegativeBalance, AllowOverAllocation, IncludeHolidaysWithinLeaves, IsCompensatory,
                     AllowEncashment, MaxEncashableLeaves, NonEncashableLeaves, EarningComponent,
                     IsEarnedLeave, EarnedLeaveFrequency, AllocateOnDay, Rounding,
                     MaxCarryForwardedLeaves, ExpireCarryForwardedLeaves, FractionOfDailySalaryPerLeave, ErpNextId)
                    OUTPUT INSERTED.Id
                    VALUES
                    (@LeaveTypeName, @MaxLeavesAllowed, @ApplicableAfterWorkingDays, @MaxContinuousDaysAllowed,
                     @IsCarryForward, @IsLWP, @IsPartiallyPaidLeave, @IsOptionalLeave,
                     @AllowNegativeBalance, @AllowOverAllocation, @IncludeHolidaysWithinLeaves, @IsCompensatory,
                     @AllowEncashment, @MaxEncashableLeaves, @NonEncashableLeaves, @EarningComponent,
                     @IsEarnedLeave, @EarnedLeaveFrequency, @AllocateOnDay, @Rounding,
                     @MaxCarryForwardedLeaves, @ExpireCarryForwardedLeaves, @FractionOfDailySalaryPerLeave, @ErpNextId)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@LeaveTypeName", model.LeaveTypeName ?? string.Empty);
                    cmd.Parameters.AddWithValue("@MaxLeavesAllowed", (object)model.MaxLeavesAllowed ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ApplicableAfterWorkingDays", (object)model.ApplicableAfterWorkingDays ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@MaxContinuousDaysAllowed", (object)model.MaxContinuousDaysAllowed ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsCarryForward", model.IsCarryForward);
                    cmd.Parameters.AddWithValue("@IsLWP", model.IsLWP);
                    cmd.Parameters.AddWithValue("@IsPartiallyPaidLeave", model.IsPartiallyPaidLeave);
                    cmd.Parameters.AddWithValue("@IsOptionalLeave", model.IsOptionalLeave);
                    cmd.Parameters.AddWithValue("@AllowNegativeBalance", model.AllowNegativeBalance);
                    cmd.Parameters.AddWithValue("@AllowOverAllocation", model.AllowOverAllocation);
                    cmd.Parameters.AddWithValue("@IncludeHolidaysWithinLeaves", model.IncludeHolidaysWithinLeaves);
                    cmd.Parameters.AddWithValue("@IsCompensatory", model.IsCompensatory);
                    cmd.Parameters.AddWithValue("@AllowEncashment", model.AllowEncashment);
                    cmd.Parameters.AddWithValue("@MaxEncashableLeaves", (object)model.MaxEncashableLeaves ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NonEncashableLeaves", (object)model.NonEncashableLeaves ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EarningComponent", (object)model.EarningComponent ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsEarnedLeave", model.IsEarnedLeave);
                    cmd.Parameters.AddWithValue("@EarnedLeaveFrequency", (object)model.EarnedLeaveFrequency ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AllocateOnDay", (object)model.AllocateOnDay ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Rounding", (object)model.Rounding ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@MaxCarryForwardedLeaves", (object)model.MaxCarryForwardedLeaves ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ExpireCarryForwardedLeaves", (object)model.ExpireCarryForwardedLeaves ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FractionOfDailySalaryPerLeave", (object)model.FractionOfDailySalaryPerLeave ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ErpNextId", (object)model.ErpNextId ?? DBNull.Value);

                    conn.Open();
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        public void UpdateErpNextId(int id, string erpNextId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE LeaveTypes SET ErpNextId = @ErpNextId WHERE Id = @Id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", (object)erpNextId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public bool ExistsByErpNextId(string erpNextId)
        {
            if (string.IsNullOrWhiteSpace(erpNextId)) return false;

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT COUNT(*) FROM LeaveTypes 
                                WHERE ErpNextId = @ErpNextId OR LeaveTypeName = @ErpNextId";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ErpNextId", erpNextId);
                    conn.Open();
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }

        public void InsertFromErpNext(LeaveType model)
        {
            Insert(model);
        }

        public void UpdateFromErpNext(LeaveType model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"UPDATE LeaveTypes
                    SET LeaveTypeName = @LeaveTypeName,
                        MaxLeavesAllowed = @MaxLeavesAllowed,
                        ApplicableAfterWorkingDays = @ApplicableAfterWorkingDays,
                        MaxContinuousDaysAllowed = @MaxContinuousDaysAllowed,
                        IsCarryForward = @IsCarryForward,
                        IsLWP = @IsLWP,
                        IsPartiallyPaidLeave = @IsPartiallyPaidLeave,
                        IsOptionalLeave = @IsOptionalLeave,
                        AllowNegativeBalance = @AllowNegativeBalance,
                        AllowOverAllocation = @AllowOverAllocation,
                        IncludeHolidaysWithinLeaves = @IncludeHolidaysWithinLeaves,
                        IsCompensatory = @IsCompensatory,
                        AllowEncashment = @AllowEncashment,
                        MaxEncashableLeaves = @MaxEncashableLeaves,
                        NonEncashableLeaves = @NonEncashableLeaves,
                        EarningComponent = @EarningComponent,
                        IsEarnedLeave = @IsEarnedLeave,
                        EarnedLeaveFrequency = @EarnedLeaveFrequency,
                        AllocateOnDay = @AllocateOnDay,
                        Rounding = @Rounding,
                        MaxCarryForwardedLeaves = @MaxCarryForwardedLeaves,
                        ExpireCarryForwardedLeaves = @ExpireCarryForwardedLeaves,
                        FractionOfDailySalaryPerLeave = @FractionOfDailySalaryPerLeave,
                        ErpNextId = @ErpNextId,
                        IsDeleted = 0
                    WHERE ErpNextId = @ErpNextId OR LeaveTypeName = @LeaveTypeName";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@LeaveTypeName", model.LeaveTypeName ?? string.Empty);
                    cmd.Parameters.AddWithValue("@MaxLeavesAllowed", (object)model.MaxLeavesAllowed ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ApplicableAfterWorkingDays", (object)model.ApplicableAfterWorkingDays ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@MaxContinuousDaysAllowed", (object)model.MaxContinuousDaysAllowed ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsCarryForward", model.IsCarryForward);
                    cmd.Parameters.AddWithValue("@IsLWP", model.IsLWP);
                    cmd.Parameters.AddWithValue("@IsPartiallyPaidLeave", model.IsPartiallyPaidLeave);
                    cmd.Parameters.AddWithValue("@IsOptionalLeave", model.IsOptionalLeave);
                    cmd.Parameters.AddWithValue("@AllowNegativeBalance", model.AllowNegativeBalance);
                    cmd.Parameters.AddWithValue("@AllowOverAllocation", model.AllowOverAllocation);
                    cmd.Parameters.AddWithValue("@IncludeHolidaysWithinLeaves", model.IncludeHolidaysWithinLeaves);
                    cmd.Parameters.AddWithValue("@IsCompensatory", model.IsCompensatory);
                    cmd.Parameters.AddWithValue("@AllowEncashment", model.AllowEncashment);
                    cmd.Parameters.AddWithValue("@MaxEncashableLeaves", (object)model.MaxEncashableLeaves ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NonEncashableLeaves", (object)model.NonEncashableLeaves ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EarningComponent", (object)model.EarningComponent ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsEarnedLeave", model.IsEarnedLeave);
                    cmd.Parameters.AddWithValue("@EarnedLeaveFrequency", (object)model.EarnedLeaveFrequency ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AllocateOnDay", (object)model.AllocateOnDay ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Rounding", (object)model.Rounding ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@MaxCarryForwardedLeaves", (object)model.MaxCarryForwardedLeaves ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ExpireCarryForwardedLeaves", (object)model.ExpireCarryForwardedLeaves ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FractionOfDailySalaryPerLeave", (object)model.FractionOfDailySalaryPerLeave ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ErpNextId", (object)model.ErpNextId ?? DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Update(LeaveType model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"UPDATE LeaveTypes
                    SET LeaveTypeName = @LeaveTypeName,
                        MaxLeavesAllowed = @MaxLeavesAllowed,
                        ApplicableAfterWorkingDays = @ApplicableAfterWorkingDays,
                        MaxContinuousDaysAllowed = @MaxContinuousDaysAllowed,
                        IsCarryForward = @IsCarryForward,
                        IsLWP = @IsLWP,
                        IsPartiallyPaidLeave = @IsPartiallyPaidLeave,
                        IsOptionalLeave = @IsOptionalLeave,
                        AllowNegativeBalance = @AllowNegativeBalance,
                        AllowOverAllocation = @AllowOverAllocation,
                        IncludeHolidaysWithinLeaves = @IncludeHolidaysWithinLeaves,
                        IsCompensatory = @IsCompensatory,
                        AllowEncashment = @AllowEncashment,
                        MaxEncashableLeaves = @MaxEncashableLeaves,
                        NonEncashableLeaves = @NonEncashableLeaves,
                        EarningComponent = @EarningComponent,
                        IsEarnedLeave = @IsEarnedLeave,
                        EarnedLeaveFrequency = @EarnedLeaveFrequency,
                        AllocateOnDay = @AllocateOnDay,
                        Rounding = @Rounding,
                        MaxCarryForwardedLeaves = @MaxCarryForwardedLeaves,
                        ExpireCarryForwardedLeaves = @ExpireCarryForwardedLeaves,
                        FractionOfDailySalaryPerLeave = @FractionOfDailySalaryPerLeave,
                        ErpNextId = @ErpNextId,
                        IsDeleted = 0
                    WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    AddParameters(cmd, model);
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void MarkDeletedById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("UPDATE LeaveTypes SET IsDeleted = 1 WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private LeaveType Map(SqlDataReader reader)
        {
            bool hasErpNextId = false;
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals("ErpNextId", StringComparison.OrdinalIgnoreCase))
                {
                    hasErpNextId = true;
                    break;
                }
            }

            return new LeaveType
            {
                Id = Convert.ToInt32(reader["Id"]),
                LeaveTypeName = reader["LeaveTypeName"]?.ToString(),
                MaxLeavesAllowed = reader["MaxLeavesAllowed"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["MaxLeavesAllowed"]),
                ApplicableAfterWorkingDays = reader["ApplicableAfterWorkingDays"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["ApplicableAfterWorkingDays"]),
                MaxContinuousDaysAllowed = reader["MaxContinuousDaysAllowed"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["MaxContinuousDaysAllowed"]),
                IsCarryForward = Convert.ToBoolean(reader["IsCarryForward"]),
                IsLWP = Convert.ToBoolean(reader["IsLWP"]),
                IsPartiallyPaidLeave = Convert.ToBoolean(reader["IsPartiallyPaidLeave"]),
                IsOptionalLeave = Convert.ToBoolean(reader["IsOptionalLeave"]),
                AllowNegativeBalance = Convert.ToBoolean(reader["AllowNegativeBalance"]),
                AllowOverAllocation = Convert.ToBoolean(reader["AllowOverAllocation"]),
                IncludeHolidaysWithinLeaves = Convert.ToBoolean(reader["IncludeHolidaysWithinLeaves"]),
                IsCompensatory = Convert.ToBoolean(reader["IsCompensatory"]),
                AllowEncashment = Convert.ToBoolean(reader["AllowEncashment"]),
                MaxEncashableLeaves = reader["MaxEncashableLeaves"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["MaxEncashableLeaves"]),
                NonEncashableLeaves = reader["NonEncashableLeaves"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["NonEncashableLeaves"]),
                EarningComponent = reader["EarningComponent"] == DBNull.Value ? null : reader["EarningComponent"].ToString(),
                IsEarnedLeave = Convert.ToBoolean(reader["IsEarnedLeave"]),
                EarnedLeaveFrequency = reader["EarnedLeaveFrequency"] == DBNull.Value ? null : reader["EarnedLeaveFrequency"].ToString(),
                AllocateOnDay = reader["AllocateOnDay"] == DBNull.Value ? null : reader["AllocateOnDay"].ToString(),
                Rounding = reader["Rounding"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["Rounding"]),
                MaxCarryForwardedLeaves = reader["MaxCarryForwardedLeaves"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["MaxCarryForwardedLeaves"]),
                ExpireCarryForwardedLeaves = reader["ExpireCarryForwardedLeaves"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["ExpireCarryForwardedLeaves"]),
                FractionOfDailySalaryPerLeave = reader["FractionOfDailySalaryPerLeave"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["FractionOfDailySalaryPerLeave"]),
                ErpNextId = hasErpNextId && reader["ErpNextId"] != DBNull.Value ? reader["ErpNextId"].ToString() : null
            };
        }

        private static void AddParameters(SqlCommand cmd, LeaveType model)
        {
            cmd.Parameters.AddWithValue("@LeaveTypeName", model.LeaveTypeName ?? string.Empty);
            cmd.Parameters.AddWithValue("@MaxLeavesAllowed", (object)model.MaxLeavesAllowed ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ApplicableAfterWorkingDays", (object)model.ApplicableAfterWorkingDays ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MaxContinuousDaysAllowed", (object)model.MaxContinuousDaysAllowed ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsCarryForward", model.IsCarryForward);
            cmd.Parameters.AddWithValue("@IsLWP", model.IsLWP);
            cmd.Parameters.AddWithValue("@IsPartiallyPaidLeave", model.IsPartiallyPaidLeave);
            cmd.Parameters.AddWithValue("@IsOptionalLeave", model.IsOptionalLeave);
            cmd.Parameters.AddWithValue("@AllowNegativeBalance", model.AllowNegativeBalance);
            cmd.Parameters.AddWithValue("@AllowOverAllocation", model.AllowOverAllocation);
            cmd.Parameters.AddWithValue("@IncludeHolidaysWithinLeaves", model.IncludeHolidaysWithinLeaves);
            cmd.Parameters.AddWithValue("@IsCompensatory", model.IsCompensatory);
            cmd.Parameters.AddWithValue("@AllowEncashment", model.AllowEncashment);
            cmd.Parameters.AddWithValue("@MaxEncashableLeaves", (object)model.MaxEncashableLeaves ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NonEncashableLeaves", (object)model.NonEncashableLeaves ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EarningComponent", (object)model.EarningComponent ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsEarnedLeave", model.IsEarnedLeave);
            cmd.Parameters.AddWithValue("@EarnedLeaveFrequency", (object)model.EarnedLeaveFrequency ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AllocateOnDay", (object)model.AllocateOnDay ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Rounding", (object)model.Rounding ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MaxCarryForwardedLeaves", (object)model.MaxCarryForwardedLeaves ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ExpireCarryForwardedLeaves", (object)model.ExpireCarryForwardedLeaves ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FractionOfDailySalaryPerLeave", (object)model.FractionOfDailySalaryPerLeave ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ErpNextId", (object)model.ErpNextId ?? DBNull.Value);
        }

        private void EnsureColumnsOnce()
        {
            if (_columnsEnsured)
                return;

            lock (EnsureColumnsLock)
            {
                if (_columnsEnsured)
                    return;

                try
                {
                    EnsureColumns();
                    _columnsEnsured = true;
                }
                catch
                {
                    // Do not block every HR page when SQL Server is slow.
                    // The next app restart can retry the schema check.
                    _columnsEnsured = true;
                }
            }
        }

        private void EnsureColumns()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
IF COL_LENGTH('LeaveTypes', 'MaxEncashableLeaves') IS NULL ALTER TABLE LeaveTypes ADD MaxEncashableLeaves DECIMAL(18,3) NULL;
IF COL_LENGTH('LeaveTypes', 'NonEncashableLeaves') IS NULL ALTER TABLE LeaveTypes ADD NonEncashableLeaves DECIMAL(18,3) NULL;
IF COL_LENGTH('LeaveTypes', 'EarningComponent') IS NULL ALTER TABLE LeaveTypes ADD EarningComponent NVARCHAR(255) NULL;
IF COL_LENGTH('LeaveTypes', 'EarnedLeaveFrequency') IS NULL ALTER TABLE LeaveTypes ADD EarnedLeaveFrequency NVARCHAR(50) NULL;
IF COL_LENGTH('LeaveTypes', 'AllocateOnDay') IS NULL ALTER TABLE LeaveTypes ADD AllocateOnDay NVARCHAR(50) NULL;
IF COL_LENGTH('LeaveTypes', 'Rounding') IS NULL ALTER TABLE LeaveTypes ADD Rounding DECIMAL(18,3) NULL;
IF COL_LENGTH('LeaveTypes', 'MaxCarryForwardedLeaves') IS NULL ALTER TABLE LeaveTypes ADD MaxCarryForwardedLeaves DECIMAL(18,3) NULL;
IF COL_LENGTH('LeaveTypes', 'ExpireCarryForwardedLeaves') IS NULL ALTER TABLE LeaveTypes ADD ExpireCarryForwardedLeaves INT NULL;
IF COL_LENGTH('LeaveTypes', 'FractionOfDailySalaryPerLeave') IS NULL ALTER TABLE LeaveTypes ADD FractionOfDailySalaryPerLeave DECIMAL(18,3) NULL;";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandTimeout = 5;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
