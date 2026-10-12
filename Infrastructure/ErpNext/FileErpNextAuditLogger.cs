using Newtonsoft.Json;

namespace Erp_Aits.Infrastructure.ErpNext
{
    public class FileErpNextAuditLogger : IErpNextAuditLogger
    {
        private static readonly SemaphoreSlim LogFileLock = new(1, 1);
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileErpNextAuditLogger> _logger;

        public FileErpNextAuditLogger(
            IWebHostEnvironment environment,
            ILogger<FileErpNextAuditLogger> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task LogAsync(ErpNextAuditEntry entry, CancellationToken cancellationToken = default)
        {
            try
            {
                string logDirectory = Path.Combine(_environment.ContentRootPath, "App_Data", "IntegrationLogs");
                Directory.CreateDirectory(logDirectory);

                string logPath = Path.Combine(logDirectory, $"erpnext-audit-{DateTime.UtcNow:yyyyMMdd}.log");
                string line = JsonConvert.SerializeObject(entry) + Environment.NewLine;

                await LogFileLock.WaitAsync(cancellationToken);
                try
                {
                    await File.AppendAllTextAsync(logPath, line, cancellationToken);
                }
                finally
                {
                    LogFileLock.Release();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not write ERPNext audit log.");
            }
        }
    }
}
