namespace Erp_Aits.Infrastructure.ErpNext
{
    public interface IErpNextClient
    {
        Task<string> GetAsync(string url, CancellationToken cancellationToken = default);
        Task<string> PostAsync(string url, object payload, CancellationToken cancellationToken = default);
        Task<string> PutAsync(string url, object payload, CancellationToken cancellationToken = default);
        Task<string> DeleteAsync(string url, CancellationToken cancellationToken = default);
        Task<string> GetDocumentAsync(string doctype, string documentName, CancellationToken cancellationToken = default);
        Task<string> GetListAsync(string doctype, string[] fields, int limit = 20, CancellationToken cancellationToken = default);
        Task<string> CreateDocumentAsync(string doctype, object payload, CancellationToken cancellationToken = default);
        Task<string> UpdateDocumentAsync(string doctype, string documentName, object payload, CancellationToken cancellationToken = default);
        Task<string> DeleteDocumentAsync(string doctype, string documentName, CancellationToken cancellationToken = default);
        Task<string> CheckConnectionAsync(CancellationToken cancellationToken = default);
    }
}
