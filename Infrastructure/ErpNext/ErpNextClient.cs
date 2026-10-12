using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Erp_Aits.Infrastructure.ErpNext
{
    public class ErpNextClient : IErpNextClient
    {
        private readonly HttpClient _httpClient;
        private readonly IErpNextAuditLogger _auditLogger;
        private readonly ILogger<ErpNextClient> _logger;

        public ErpNextClient(
            HttpClient httpClient,
            IOptions<ErpNextOptions> options,
            IErpNextAuditLogger auditLogger,
            ILogger<ErpNextClient> logger)
        {
            var erpNextOptions = options.Value;

            if (string.IsNullOrWhiteSpace(erpNextOptions.BaseUrl))
            {
                throw new InvalidOperationException("ERPNext BaseUrl is missing.");
            }

            if (string.IsNullOrWhiteSpace(erpNextOptions.ApiKey) ||
                string.IsNullOrWhiteSpace(erpNextOptions.ApiSecret))
            {
                throw new InvalidOperationException("ERPNext API key or secret is missing.");
            }

            _httpClient = httpClient;
            _auditLogger = auditLogger;
            _logger = logger;

            _httpClient.BaseAddress ??= new Uri(erpNextOptions.BaseUrl.Trim().TrimEnd('/') + "/");
            _httpClient.Timeout = TimeSpan.FromSeconds(erpNextOptions.TimeoutSeconds <= 0 ? 60 : erpNextOptions.TimeoutSeconds);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (!_httpClient.DefaultRequestHeaders.Contains("Authorization"))
            {
                _httpClient.DefaultRequestHeaders.Add(
                    "Authorization",
                    $"token {erpNextOptions.ApiKey}:{erpNextOptions.ApiSecret}");
            }
        }

        public Task<string> GetAsync(string url, CancellationToken cancellationToken = default)
        {
            return SendAsync(HttpMethod.Get, url, null, cancellationToken);
        }

        public Task<string> PostAsync(string url, object payload, CancellationToken cancellationToken = default)
        {
            return SendAsync(HttpMethod.Post, url, payload, cancellationToken);
        }

        public Task<string> PutAsync(string url, object payload, CancellationToken cancellationToken = default)
        {
            return SendAsync(HttpMethod.Put, url, payload, cancellationToken);
        }

        public Task<string> DeleteAsync(string url, CancellationToken cancellationToken = default)
        {
            return SendAsync(HttpMethod.Delete, url, null, cancellationToken);
        }

        public Task<string> GetDocumentAsync(string doctype, string documentName, CancellationToken cancellationToken = default)
        {
            string url = $"/api/resource/{Uri.EscapeDataString(doctype)}/{Uri.EscapeDataString(documentName)}";
            return GetAsync(url, cancellationToken);
        }

        public Task<string> GetListAsync(string doctype, string[] fields, int limit = 20, CancellationToken cancellationToken = default)
        {
            string fieldsJson = JsonConvert.SerializeObject(fields);
            string url = $"/api/resource/{Uri.EscapeDataString(doctype)}" +
                         $"?fields={Uri.EscapeDataString(fieldsJson)}" +
                         $"&limit_page_length={limit}";

            return GetAsync(url, cancellationToken);
        }

        public Task<string> CreateDocumentAsync(string doctype, object payload, CancellationToken cancellationToken = default)
        {
            string url = $"/api/resource/{Uri.EscapeDataString(doctype)}";
            return PostAsync(url, payload, cancellationToken);
        }

        public Task<string> UpdateDocumentAsync(string doctype, string documentName, object payload, CancellationToken cancellationToken = default)
        {
            string url = $"/api/resource/{Uri.EscapeDataString(doctype)}/{Uri.EscapeDataString(documentName)}";
            return PutAsync(url, payload, cancellationToken);
        }

        public Task<string> DeleteDocumentAsync(string doctype, string documentName, CancellationToken cancellationToken = default)
        {
            string url = $"/api/resource/{Uri.EscapeDataString(doctype)}/{Uri.EscapeDataString(documentName)}";
            return DeleteAsync(url, cancellationToken);
        }

        public Task<string> CheckConnectionAsync(CancellationToken cancellationToken = default)
        {
            return GetAsync("/api/method/frappe.auth.get_logged_user", cancellationToken);
        }

        private async Task<string> SendAsync(
            HttpMethod method,
            string url,
            object? payload,
            CancellationToken cancellationToken)
        {
            string correlationId = Guid.NewGuid().ToString("N");
            var stopwatch = Stopwatch.StartNew();
            string debugUrl = BuildDebugUrl(url);

            using var request = new HttpRequestMessage(method, url);
            request.Headers.Add("X-Correlation-Id", correlationId);

            if (payload != null)
            {
                string json = payload is string rawJson ? rawJson : JsonConvert.SerializeObject(payload);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            try
            {
                using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
                string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                stopwatch.Stop();

                await _auditLogger.LogAsync(new ErpNextAuditEntry
                {
                    CorrelationId = correlationId,
                    Method = method.Method,
                    Url = debugUrl,
                    Status = response.IsSuccessStatusCode ? "Success" : "Failed",
                    StatusCode = (int)response.StatusCode,
                    ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                    ErrorMessage = response.IsSuccessStatusCode ? null : Shorten(responseContent)
                }, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new ErpNextApiException(
                        $"ERPNext returned {(int)response.StatusCode}. CorrelationId: {correlationId}",
                        response.StatusCode,
                        responseContent);
                }

                return responseContent;
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                stopwatch.Stop();
                await LogTransportFailureAsync(correlationId, method.Method, debugUrl, stopwatch.ElapsedMilliseconds, "ERPNext request timed out.", cancellationToken);
                throw new ErpNextApiException($"ERPNext request timed out. CorrelationId: {correlationId}", null, null, ex);
            }
            catch (HttpRequestException ex)
            {
                stopwatch.Stop();
                await LogTransportFailureAsync(correlationId, method.Method, debugUrl, stopwatch.ElapsedMilliseconds, ex.GetBaseException().Message, cancellationToken);
                throw new ErpNextApiException($"Could not connect to ERPNext. CorrelationId: {correlationId}", null, null, ex);
            }
            catch (ErpNextApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Unexpected ERPNext client error. CorrelationId: {CorrelationId}", correlationId);
                await LogTransportFailureAsync(correlationId, method.Method, debugUrl, stopwatch.ElapsedMilliseconds, ex.Message, cancellationToken);
                throw;
            }
        }

        private string BuildDebugUrl(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var absoluteUri))
            {
                return absoluteUri.ToString();
            }

            return new Uri(_httpClient.BaseAddress!, url).ToString();
        }

        private async Task LogTransportFailureAsync(
            string correlationId,
            string method,
            string url,
            long elapsedMilliseconds,
            string errorMessage,
            CancellationToken cancellationToken)
        {
            await _auditLogger.LogAsync(new ErpNextAuditEntry
            {
                CorrelationId = correlationId,
                Method = method,
                Url = url,
                Status = "Failed",
                ElapsedMilliseconds = elapsedMilliseconds,
                ErrorMessage = Shorten(errorMessage)
            }, cancellationToken);
        }

        private static string Shorten(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return value.Length <= 1000 ? value : value.Substring(0, 1000);
        }
    }
}
