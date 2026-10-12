using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Erp_Aits.Services
{
    public class CampaignSchedulerService : BackgroundService
    {
        private readonly ILogger<CampaignSchedulerService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly string _connectionString;
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30); // Check every 30 seconds

        public CampaignSchedulerService(
            ILogger<CampaignSchedulerService> logger,
            IServiceScopeFactory serviceScopeFactory,
            IConfiguration configuration)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _connectionString = configuration.GetConnectionString("DefaultConnection");

            _logger.LogInformation("Campaign Scheduler Service initialized.");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Campaign Scheduler Service started.");

            // Run immediately on startup
            await ProcessScheduledCampaigns(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessScheduledCampaigns(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in scheduler service: {ex.Message}");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Campaign Scheduler Service stopped.");
        }

        private async Task ProcessScheduledCampaigns(CancellationToken stoppingToken)
        {
            try
            {
                var scheduledCampaigns = await GetDueCampaigns();
                _logger.LogInformation($"Checking for due campaigns. Found: {scheduledCampaigns.Count}");

                if (!scheduledCampaigns.Any())
                    return;

                _logger.LogInformation($"Found {scheduledCampaigns.Count} campaigns to process");

                foreach (var campaignId in scheduledCampaigns)
                {
                    if (stoppingToken.IsCancellationRequested)
                        break;

                    try
                    {
                        _logger.LogInformation($"Processing scheduled campaign: {campaignId}");

                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var controller = scope.ServiceProvider.GetRequiredService<Erp_Aits.Controllers.CampaignsController>();

                            // Send the campaign
                            var success = await controller.SendCampaignAsync(campaignId);

                            if (success)
                            {
                                _logger.LogInformation($"Scheduled campaign {campaignId} sent successfully");
                                await MarkSchedulerProcessed(campaignId, "Completed");
                            }
                            else
                            {
                                _logger.LogError($"Failed to send scheduled campaign {campaignId}");
                                await MarkSchedulerProcessed(campaignId, "Failed");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing scheduled campaign {campaignId}: {ex.Message}");
                        _logger.LogError($"Stack trace: {ex.StackTrace}");
                        await MarkSchedulerProcessed(campaignId, "Error");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ProcessScheduledCampaigns: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
            }
        }

        private async Task<List<int>> GetDueCampaigns()
        {
            var campaigns = new List<int>();
            var now = DateTime.Now;

            try
            {
                if (string.IsNullOrWhiteSpace(_connectionString))
                {
                    _logger.LogWarning("Campaign scheduler skipped because DefaultConnection is not configured.");
                    return campaigns;
                }

                using (var conn = new SqlConnection(_connectionString))
                {
                    var query = @"
                        SELECT CampaignId 
                        FROM CampaignScheduler 
                        WHERE Status = 'Pending' 
                        AND ScheduledTime <= @Now
                        AND IsActive = 1
                        ORDER BY ScheduledTime ASC";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Now", now);
                        conn.Open();

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                campaigns.Add(Convert.ToInt32(reader["CampaignId"]));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting due campaigns: {ex.Message}");
            }

            return campaigns;
        }

        private async Task MarkSchedulerProcessed(int campaignId, string status)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    var query = @"
                        UPDATE CampaignScheduler 
                        SET Status = @Status, 
                            ProcessedDate = GETDATE(),
                            ModifiedDate = GETDATE()
                        WHERE CampaignId = @CampaignId";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CampaignId", campaignId);
                        cmd.Parameters.AddWithValue("@Status", status);

                        conn.Open();
                        await cmd.ExecuteNonQueryAsync();
                        _logger.LogInformation($"Marked scheduler for campaign {campaignId} as {status}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error marking scheduler processed: {ex.Message}");
            }
        }
    }
}
