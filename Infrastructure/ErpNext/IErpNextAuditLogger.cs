namespace Erp_Aits.Infrastructure.ErpNext
{
    public interface IErpNextAuditLogger
    {
        Task LogAsync(ErpNextAuditEntry entry, CancellationToken cancellationToken = default);
    }
}
