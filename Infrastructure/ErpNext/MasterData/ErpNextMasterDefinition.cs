namespace Erp_Aits.Infrastructure.ErpNext.MasterData
{
    public class ErpNextMasterDefinition
    {
        public required string Key { get; init; }
        public required string ModuleName { get; init; }
        public required string DocType { get; init; }
        public required string DisplayField { get; init; }
        public bool AllowCreateFromHrms { get; init; }
    }
}
