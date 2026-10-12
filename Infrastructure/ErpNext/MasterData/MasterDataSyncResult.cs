namespace Erp_Aits.Infrastructure.ErpNext.MasterData
{
    public class MasterDataSyncResult
    {
        public string MasterType { get; set; } = string.Empty;
        public string DocType { get; set; } = string.Empty;
        public int RecordsFetched { get; set; }
        public int RecordsInserted { get; set; }
        public int RecordsUpdated { get; set; }
    }
}
