namespace Erp_Aits.Infrastructure.ErpNext
{
    public class ErpNextAuditEntry
    {
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public string CorrelationId { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? StatusCode { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
