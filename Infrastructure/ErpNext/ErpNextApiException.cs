using System.Net;

namespace Erp_Aits.Infrastructure.ErpNext
{
    public class ErpNextApiException : Exception
    {
        public ErpNextApiException(
            string message,
            HttpStatusCode? statusCode = null,
            string? responseBody = null,
            Exception? innerException = null)
            : base(message, innerException)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }

        public HttpStatusCode? StatusCode { get; }
        public string? ResponseBody { get; }
    }
}
