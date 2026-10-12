namespace Erp_Aits.Infrastructure.ErpNext.MasterData
{
    public interface IErpNextMasterDataService
    {
        IReadOnlyCollection<ErpNextMasterDefinition> GetSupportedMasters();
        Task<IReadOnlyList<MasterDataRecord>> GetFromErpNextAsync(string masterType, int limit = 100, CancellationToken cancellationToken = default);
        Task<MasterDataSyncResult> SyncFromErpNextAsync(string masterType, int limit = 500, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<MasterDataSyncResult>> SyncAllFromErpNextAsync(CancellationToken cancellationToken = default);
        Task<string> CreateInErpNextAsync(string masterType, CreateMasterDataRequest request, CancellationToken cancellationToken = default);
    }
}
