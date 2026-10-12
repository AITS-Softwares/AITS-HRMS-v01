using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Erp_Aits.Services
{
    public class WhatsAppService : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WhatsAppService> _logger;
        private readonly string _accessToken;
        private readonly string _phoneNumberId;
        private readonly string _businessAccountId;
        private readonly string _baseUrl = "https://graph.facebook.com/v18.0";
        private bool _disposed;

        public WhatsAppService(
            HttpClient httpClient,
            ILogger<WhatsAppService> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _accessToken = configuration["WhatsApp:AccessToken"] ??
                "EABASRmCStBcBRxZCZCwA5iuG9xZCHon0lGM7rvif0Y6FgFKROfLZATihP1BZAgMYS6ULYrA0zx4WSlZAA0dIEQYCPMUibNSIV2Afo0eOCUeZAwKq3LDmUazbZConTFogq3Wb1XgFsHJ1DN7eEjHe0XQ3wGZCtjZBjlLkD6a2zdl5DR9tBs6mC3zVgxaUwR1bcK09d5LUmkSVyRk93UZCilVeJWU3vz4KbSYfUpOQcje118ms2cbGhZBqbmDD3xO4tPOVQ4dAWOvI4p8JPc2ZClITPt7dDbYwJHAZDZD";

            _phoneNumberId = configuration["WhatsApp:PhoneNumberId"] ?? "1226944670495536";
            _businessAccountId = configuration["WhatsApp:BusinessAccountId"] ?? "1357248546366828";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        // ============================================================
        // SEND TEXT MESSAGE (NO TEMPLATE REQUIRED)
        // ============================================================
        public async Task<bool> SendTextMessageAsync(string to, string message)
        {
            if (_disposed)
            {
                _logger.LogError("WhatsAppService has been disposed.");
                return false;
            }

            try
            {
                _logger.LogInformation($"Sending WhatsApp message to: {to}");

                var cleanedNumber = CleanPhoneNumber(to);
                if (string.IsNullOrEmpty(cleanedNumber) || cleanedNumber.Length < 10)
                {
                    _logger.LogError($"Invalid phone number: {to}");
                    return false;
                }

                var formattedNumber = FormatPhoneNumber(cleanedNumber);
                _logger.LogInformation($"Formatted phone number: {formattedNumber}");

                var url = $"{_baseUrl}/{_phoneNumberId}/messages";

                // For test numbers, we can send without 24-hour restriction
                var payload = new
                {
                    messaging_product = "whatsapp",
                    to = formattedNumber,
                    type = "text",
                    text = new { body = message }
                };

                var json = JsonSerializer.Serialize(payload);
                _logger.LogInformation($"Payload: {json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"WhatsApp response status: {response.StatusCode}");
                _logger.LogInformation($"WhatsApp response content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"WhatsApp message sent successfully to {to}");
                    return true;
                }
                else
                {
                    // Check if it's the 24-hour error
                    if (responseContent.Contains("131047") || responseContent.Contains("24 hours"))
                    {
                        _logger.LogWarning($"24-hour window expired for {to}. Consider using test numbers or templates.");
                    }
                    _logger.LogError($"WhatsApp message failed: {responseContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending WhatsApp message: {ex.Message}");
                return false;
            }
        }
        // ============================================================
        // SEND WHATSAPP MESSAGE - SIMPLIFIED (NO TEMPLATE)
        // ============================================================
        public async Task<bool> SendWhatsAppMessageAsync(string to, string message)
        {
            try
            {
                _logger.LogInformation($"Sending WhatsApp message to: {to}");
                return await SendTextMessageAsync(to, message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending WhatsApp message: {ex.Message}");
                return false;
            }
        }

        // ============================================================
        // HELPER METHODS
        // ============================================================
        private string CleanPhoneNumber(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return string.Empty;

            var cleaned = new string(phone.Where(c => char.IsDigit(c)).ToArray());

            if (cleaned.StartsWith("0"))
                cleaned = cleaned.Substring(1);

            return cleaned;
        }

        private string FormatPhoneNumber(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return string.Empty;

            // For India: add 91 if missing
            if (phone.Length == 10 && !phone.StartsWith("91"))
            {
                return $"91{phone}";
            }

            // For US: add 1 if missing
            if (phone.Length == 10 && !phone.StartsWith("1"))
            {
                return $"1{phone}";
            }

            return phone;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _httpClient?.Dispose();
                _disposed = true;
            }
        }
    }
}