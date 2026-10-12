namespace Erp_Aits.Infrastructure.ErpNext
{
    public class ErpNextOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 60;
    }
}
