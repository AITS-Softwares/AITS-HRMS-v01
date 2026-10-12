//using System;
//using System.Collections.Generic;
//using System.Net.Http;
//using System.Text;
//using System.Threading.Tasks;
//using Newtonsoft.Json;
//using Microsoft.Extensions.Configuration;

//namespace Erp_Aits.Services
//{
//    public class ErpNextService
//    {
//        private readonly HttpClient _httpClient;
//        private readonly string _baseUrl;
//        private readonly IConfiguration _configuration;

//        // Constructor 1: ASP.NET Core Dependency Injection / Typed HttpClient
//        public ErpNextService(HttpClient httpClient, IConfiguration configuration)
//        {
//            _configuration = configuration;
//            _baseUrl = configuration["ErpNext:BaseUrl"] ?? "";
//            string apiKey = configuration["ErpNext:ApiKey"] ?? "";
//            string apiSecret = configuration["ErpNext:ApiSecret"] ?? "";

//            _httpClient = httpClient;
//            if (_httpClient.BaseAddress == null && !string.IsNullOrEmpty(_baseUrl))
//            {
//                _httpClient.BaseAddress = new Uri(_baseUrl);
//            }

//            if (!_httpClient.DefaultRequestHeaders.Contains("Authorization"))
//            {
//                _httpClient.DefaultRequestHeaders.Add("Authorization", $"token {apiKey}:{apiSecret}");
//            }
//        }

//        // Constructor 2: Fallback for manual instantiation
//        public ErpNextService(IConfiguration configuration)
//            : this(new HttpClient(), configuration)
//        {
//        }

//        public async Task<string> GetAsync(string url)
//        {
//            var response = await _httpClient.GetAsync(url);
//            string content = await response.Content.ReadAsStringAsync();

//            if (!response.IsSuccessStatusCode)
//            {
//                throw new Exception($"ERPNext returned {(int)response.StatusCode}: {content}");
//            }

//            return content;
//        }

//        //public async Task<string> PostAsync(string url, object payload)
//        //{
//        //    string json = JsonConvert.SerializeObject(payload);
//        //    var content = new StringContent(json, Encoding.UTF8, "application/json");

//        //    var response = await _httpClient.PostAsync(url, content);
//        //    string responseContent = await response.Content.ReadAsStringAsync();

//        //    if (!response.IsSuccessStatusCode)
//        //    {
//        //        throw new Exception($"ERPNext returned {(int)response.StatusCode}: {responseContent}");
//        //    }

//        //    return responseContent;
//        //}


//        public async Task<string> PostAsync(string url, object payload)
//        {
//            // Wrap the payload in a "data" wrapper as required by ERPNext Resource API
//            var wrappedPayload = new { data = payload };
//            string json = JsonConvert.SerializeObject(wrappedPayload);
//            var content = new StringContent(json, Encoding.UTF8, "application/json");

//            var response = await _httpClient.PostAsync(url, content);
//            string responseContent = await response.Content.ReadAsStringAsync();

//            if (!response.IsSuccessStatusCode)
//            {
//                throw new Exception($"ERPNext returned {(int)response.StatusCode}: {responseContent}");
//            }

//            return responseContent;
//        }

//        public async Task<string> PutAsync(string url, object payload)
//        {
//            string json = JsonConvert.SerializeObject(payload);
//            var content = new StringContent(json, Encoding.UTF8, "application/json");

//            var response = await _httpClient.PutAsync(url, content);
//            string responseContent = await response.Content.ReadAsStringAsync();

//            if (!response.IsSuccessStatusCode)
//            {
//                throw new Exception($"ERPNext returned {(int)response.StatusCode}: {responseContent}");
//            }

//            return responseContent;
//        }

//        public async Task<string> DeleteAsync(string url)
//        {
//            var response = await _httpClient.DeleteAsync(url);
//            string content = await response.Content.ReadAsStringAsync();

//            if (!response.IsSuccessStatusCode)
//            {
//                throw new Exception($"ERPNext returned {(int)response.StatusCode}: {content}");
//            }

//            return content;
//        }

//        // Auto-creates missing Link master records in ERPNext
//        public async Task EnsureLinkExistsAsync(string doctype, string value, string fieldName = "name")
//        {
//            if (string.IsNullOrWhiteSpace(value)) return;

//            try
//            {
//                string checkUrl = $"/api/resource/{Uri.EscapeDataString(doctype)}/{Uri.EscapeDataString(value)}";
//                await GetAsync(checkUrl);
//            }
//            catch
//            {
//                try
//                {
//                    var payload = new Dictionary<string, object>();

//                    if (doctype == "Holiday List")
//                    {
//                        payload["holiday_list_name"] = value;
//                        payload["from_date"] = new DateTime(DateTime.Now.Year, 1, 1).ToString("yyyy-MM-dd");
//                        payload["to_date"] = new DateTime(DateTime.Now.Year, 12, 31).ToString("yyyy-MM-dd");
//                        payload["weekly_off"] = "Sunday";

//                        await PostAsync($"/api/resource/{Uri.EscapeDataString(doctype)}", payload);
//                    }
//                    else if (doctype == "Company")
//                    {
//                        string holidayListName = $"Standard Holiday List {DateTime.Now.Year}";
//                        await EnsureLinkExistsAsync("Holiday List", holidayListName, "holiday_list_name");

//                        payload["company_name"] = value;
//                        payload["company_abbr"] = "VCIPL";
//                        payload["default_currency"] = "INR";
//                        payload["country"] = "India";
//                        payload["default_holiday_list"] = holidayListName;

//                        await PostAsync($"/api/resource/{Uri.EscapeDataString(doctype)}", payload);
//                    }
//                    else if (doctype == "Employee")
//                    {
//                        string companyName = "Vinod Cookware India Private Limited - VCIPL";
//                        await EnsureLinkExistsAsync("Company", companyName, "company_name");

//                        string holidayListName = $"Standard Holiday List {DateTime.Now.Year}";
//                        await EnsureLinkExistsAsync("Holiday List", holidayListName, "holiday_list_name");

//                        payload["first_name"] = value;
//                        payload["company"] = companyName;
//                        payload["date_of_joining"] = DateTime.Now.ToString("yyyy-MM-dd");
//                        payload["gender"] = "Male";
//                        payload["date_of_birth"] = "1995-01-01";
//                        payload["status"] = "Active";
//                        payload["holiday_list"] = holidayListName;

//                        await PostAsync($"/api/resource/{Uri.EscapeDataString(doctype)}", payload);
//                    }
//                    else if (doctype == "User")
//                    {
//                        string userEmail = value.Contains("@") ? value : $"{value.ToLower().Replace(" ", ".")}@vcipl.in";
//                        payload["email"] = userEmail;
//                        payload["first_name"] = value.Contains("@") ? value.Split('@')[0] : value;
//                        payload["send_welcome_email"] = 0;
//                        payload["enabled"] = 1;

//                        await PostAsync($"/api/resource/{Uri.EscapeDataString(doctype)}", payload);
//                    }
//                    //    else if (doctype == "Leave Type")
//                    //    {
//                    //        payload["leave_type_name"] = value;
//                    //        await PostAsync($"/api/resource/{Uri.EscapeDataString(doctype)}", payload);
//                    //    }
//                    //    else
//                    //    {
//                    //        payload[fieldName] = value;
//                    //        await PostAsync($"/api/resource/{Uri.EscapeDataString(doctype)}", payload);
//                    //    }

//                    //    else if (doctype == "Mode of Payment")
//                    //    {
//                    //        payload["mode_of_payment"] = value;
//                    //        payload["type"] = "Cash"; // Default type requirement for Frappe
//                    //        await PostAsync($"/api/resource/{Uri.EscapeDataString(doctype)}", payload);
//                    //    }

//                    //}
//                    else if (doctype == "Leave Type")
//                    {
//                        payload["leave_type_name"] = value;
//                        await PostAsync($"/api/resource/{Uri.EscapeDataString(doctype)}", payload);
//                    }
//                    else if (doctype == "Mode of Payment")
//                    {
//                        payload["mode_of_payment"] = value;
//                        payload["type"] = "Cash"; // Default type requirement for Frappe
//                        await PostAsync($"/api/resource/{Uri.EscapeDataString(doctype)}", payload);
//                    }
//                    else
//                    {
//                        payload[fieldName] = value;
//                        await PostAsync($"/api/resource/{Uri.EscapeDataString(doctype)}", payload);
//                    }
//                }
//                catch
//                {
//                    // Ignore on-the-fly creation failures safely
//                }
//            }
//        }
//        //}

//        public async Task<string> GetOrCreateEmployeeIdAsync(string employeeInput, string employeeName)
//        {
//            if (string.IsNullOrWhiteSpace(employeeInput)) return employeeInput;

//            try
//            {
//                string checkUrl = $"/api/resource/Employee/{Uri.EscapeDataString(employeeInput)}";
//                await GetAsync(checkUrl);
//                return employeeInput;
//            }
//            catch
//            {
//                try
//                {
//                    string searchName = string.IsNullOrWhiteSpace(employeeName) ? employeeInput : employeeName;
//                    string filters = Uri.EscapeDataString($"[[\"first_name\",\"=\",\"{searchName}\"]]");
//                    string searchUrl = $"/api/resource/Employee?fields=[\"name\"]&filters={filters}";

//                    string searchResponse = await GetAsync(searchUrl);
//                    dynamic searchParsed = JsonConvert.DeserializeObject(searchResponse);

//                    if (searchParsed.data != null && searchParsed.data.Count > 0)
//                    {
//                        return (string)searchParsed.data[0].name;
//                    }
//                }
//                catch { }

//                try
//                {
//                    string companyName = "Vinod Cookware India Private Limited - VCIPL";
//                    await EnsureLinkExistsAsync("Company", companyName, "company_name");

//                    var payload = new Dictionary<string, object>
//                    {
//                        ["first_name"] = string.IsNullOrWhiteSpace(employeeName) ? employeeInput : employeeName,
//                        ["company"] = companyName,
//                        ["date_of_joining"] = DateTime.Now.ToString("yyyy-MM-dd"),
//                        ["gender"] = "Male",
//                        ["date_of_birth"] = "1995-01-01",
//                        ["status"] = "Active"
//                    };

//                    string createResponse = await PostAsync("/api/resource/Employee", payload);
//                    dynamic createdParsed = JsonConvert.DeserializeObject(createResponse);
//                    return (string)createdParsed.data.name;
//                }
//                catch
//                {
//                    return employeeInput;
//                }
//            }
//        }

//        public async Task EnsureEmployeeHasHolidayListAsync(string employeeId)
//        {
//            if (string.IsNullOrWhiteSpace(employeeId)) return;

//            try
//            {
//                string holidayListName = $"Standard Holiday List {DateTime.Now.Year}";
//                await EnsureLinkExistsAsync("Holiday List", holidayListName, "holiday_list_name");

//                string empJson = await GetAsync($"/api/resource/Employee/{Uri.EscapeDataString(employeeId)}");
//                dynamic parsed = JsonConvert.DeserializeObject(empJson);

//                if (parsed.data != null && (parsed.data.holiday_list == null || string.IsNullOrWhiteSpace((string)parsed.data.holiday_list)))
//                {
//                    var updatePayload = new
//                    {
//                        holiday_list = holidayListName
//                    };

//                    await PutAsync($"/api/resource/Employee/{Uri.EscapeDataString(employeeId)}", updatePayload);
//                }
//            }
//            catch
//            {
//                // Safe fallback if update fails
//            }
//        }

//        public async Task EnsureLeaveAllocationExistsAsync(string employee, string leaveType, string company)
//        {
//            if (string.IsNullOrWhiteSpace(employee) || string.IsNullOrWhiteSpace(leaveType)) return;

//            try
//            {
//                string fields = Uri.EscapeDataString("[\"name\"]");
//                string filters = Uri.EscapeDataString($"[[\"employee\",\"=\",\"{employee}\"],[\"leave_type\",\"=\",\"{leaveType}\"],[\"docstatus\",\"=\",1]]");
//                string checkUrl = $"/api/resource/Leave%20Allocation?fields={fields}&filters={filters}";

//                string response = await GetAsync(checkUrl);
//                dynamic parsed = JsonConvert.DeserializeObject(response);

//                if (parsed.data != null && parsed.data.Count > 0) return;
//            }
//            catch { }

//            try
//            {
//                var allocationPayload = new
//                {
//                    employee = employee,
//                    leave_type = leaveType,
//                    company = string.IsNullOrWhiteSpace(company) ? "Vinod Cookware India Private Limited - VCIPL" : company,
//                    from_date = new DateTime(DateTime.Now.Year, 1, 1).ToString("yyyy-MM-dd"),
//                    to_date = new DateTime(DateTime.Now.Year, 12, 31).ToString("yyyy-MM-dd"),
//                    new_leaves_allocated = 30,
//                    docstatus = 1
//                };

//                await PostAsync("/api/resource/Leave%20Allocation", allocationPayload);
//            }
//            catch (Exception ex) when (ex.Message.Contains("409") || ex.Message.Contains("DuplicateEntryError") || ex.Message.Contains("already exists"))
//            {
//                // Ignore duplicate allocation errors safely
//            }
//        }
//    }
//}





















using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace Erp_Aits.Services
{
    public class ErpNextService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly IConfiguration _configuration;

        // Constructor 1: ASP.NET Core Dependency Injection / Typed HttpClient
        public ErpNextService(HttpClient httpClient, IConfiguration configuration)
        {
            _configuration = configuration;
            //_baseUrl = configuration["ErpNext:BaseUrl"] ?? "";
            _baseUrl = (configuration["ErpNext:BaseUrl"] ?? "").Trim().TrimEnd('/');
            string apiKey = configuration["ErpNext:ApiKey"] ?? "";
            string apiSecret = configuration["ErpNext:ApiSecret"] ?? "";

            _httpClient = httpClient;
            //if (_httpClient.BaseAddress == null && !string.IsNullOrEmpty(_baseUrl))
            //{
            //    _httpClient.BaseAddress = new Uri(_baseUrl);
            //}
            if (string.IsNullOrWhiteSpace(_baseUrl))
            {
                throw new Exception("ERPNext BaseUrl is missing in appsettings.json.");
            }

            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri(_baseUrl);
            }

            _httpClient.Timeout = TimeSpan.FromSeconds(60);
            if (!_httpClient.DefaultRequestHeaders.Contains("Authorization"))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"token {apiKey}:{apiSecret}");
            }
        }

        // Constructor 2: Fallback for manual instantiation
        public ErpNextService(IConfiguration configuration)
            : this(new HttpClient(), configuration)
        {
        }




        public async Task<string> PostMethodAsync(string url, object payload)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            string responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"ERPNext returned {(int)response.StatusCode}: {responseContent}");
            }

            return responseContent;
        }


        //    public async Task<string> PostFormAsync(
        //string url,
        //Dictionary<string, string> formData)
        //    {
        //        var content =
        //            new FormUrlEncodedContent(formData);

        //        var response =
        //            await _httpClient.PostAsync(url, content);

        //        string responseContent =
        //            await response.Content.ReadAsStringAsync();

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            throw new Exception(
        //                $"ERPNext returned {(int)response.StatusCode}: {responseContent}"
        //            );
        //        }

        //        return responseContent;
        //    }


        public async Task<string> PostFormDataAsync(
    string url,
    Dictionary<string, string> formData)
        {
            var content = new FormUrlEncodedContent(formData);

            var response = await _httpClient.PostAsync(url, content);

            string responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"ERPNext returned {(int)response.StatusCode}: {responseContent}"
                );
            }

            return responseContent;
        }




        public async Task<string> PostFormUrlEncodedAsync(string url, Dictionary<string, string> formData)
        {
            using var content = new FormUrlEncodedContent(formData);

            var response = await _httpClient.PostAsync(url, content);
            string responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"ERPNext returned {(int)response.StatusCode}: {responseContent}");
            }

            return responseContent;
        }












        public async Task<string> PostFormAsync(
    string url,
    Dictionary<string, string> formData)
        {
            var content =
                new FormUrlEncodedContent(formData);

            var response =
                await _httpClient.PostAsync(url, content);

            string responseContent =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"ERPNext returned {(int)response.StatusCode}: {responseContent}"
                );
            }

            return responseContent;
        }













        public async Task<string> GetAsync(string url)
        {
            string fullUrl = BuildDebugUrl(url);

            try
            {
                var response = await _httpClient.GetAsync(url);
                string content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"ERPNext returned {(int)response.StatusCode} for {fullUrl}: {content}");
                }

                return content;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Could not connect to ERPNext. URL: {fullUrl}. Details: {ex.GetBaseException().Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new Exception($"ERPNext request timed out. URL: {fullUrl}. Details: {ex.GetBaseException().Message}", ex);
            }
        }


        private string BuildDebugUrl(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var absoluteUri))
            {
                return absoluteUri.ToString();
            }

            if (_httpClient.BaseAddress == null)
            {
                return url;
            }

            return new Uri(_httpClient.BaseAddress, url).ToString();
        }

        //public async Task<string> PostAsync(string url, object payload)
        //{
        //    string json = JsonConvert.SerializeObject(payload);
        //    var content = new StringContent(json, Encoding.UTF8, "application/json");

        //    var response = await _httpClient.PostAsync(url, content);
        //    string responseContent = await response.Content.ReadAsStringAsync();

        //    if (!response.IsSuccessStatusCode)
        //    {
        //        throw new Exception($"ERPNext returned {(int)response.StatusCode}: {responseContent}");
        //    }

        //    return responseContent;
        //}


        public async Task<string> PostAsync(string url, object payload)
        {
            // Wrap the payload in a "data" wrapper as required by ERPNext Resource API
            var wrappedPayload = new { data = payload };
            string json = JsonConvert.SerializeObject(wrappedPayload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            string responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"ERPNext returned {(int)response.StatusCode}: {responseContent}");
            }

            return responseContent;
        }




       
















        public async Task<string> PostResourceAsync(string doctype, object payload)
        {
            return await SendJsonAsync(
                HttpMethod.Post,
                $"/api/resource/{Uri.EscapeDataString(doctype)}",
                payload);
        }

        public async Task<string> PutResourceAsync(string doctype, string documentName, object payload)
        {
            return await SendJsonAsync(
                HttpMethod.Put,
                $"/api/resource/{Uri.EscapeDataString(doctype)}/{Uri.EscapeDataString(documentName)}",
                payload);
        }

        private async Task<string> SendJsonAsync(HttpMethod method, string url, object payload)
        {
            string json = payload is string rawJson ? rawJson : JsonConvert.SerializeObject(payload);
            using var request = new HttpRequestMessage(method, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"ERPNext returned {(int)response.StatusCode}: {responseContent}");
            }

            return responseContent;
        }

        public async Task<string> PutAsync(string url, object payload)
        {
            string json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(url, content);
            string responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"ERPNext returned {(int)response.StatusCode}: {responseContent}");
            }

            return responseContent;
        }

        public async Task<string> DeleteAsync(string url)
        {
            var response = await _httpClient.DeleteAsync(url);
            string content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"ERPNext returned {(int)response.StatusCode}: {content}");
            }

            return content;
        }

        // Auto-creates missing Link master records in ERPNext
        public async Task EnsureLinkExistsAsync(string doctype, string value, string fieldName = "name")
        {
            if (string.IsNullOrWhiteSpace(value)) return;

            try
            {
                string checkUrl = $"/api/resource/{Uri.EscapeDataString(doctype)}/{Uri.EscapeDataString(value)}";
                await GetAsync(checkUrl);
            }
            catch
            {
                try
                {
                    var payload = new Dictionary<string, object>();

                    if (doctype == "Holiday List")
                    {
                        payload["holiday_list_name"] = value;
                        payload["from_date"] = new DateTime(DateTime.Now.Year, 1, 1).ToString("yyyy-MM-dd");
                        payload["to_date"] = new DateTime(DateTime.Now.Year, 12, 31).ToString("yyyy-MM-dd");
                        payload["weekly_off"] = "Sunday";

                        await PostAsync($"/api/resource/{Uri.EscapeDataString(doctype)}", payload);
                    }
                    else if (doctype == "Company")
                    {
                        string holidayListName = $"Standard Holiday List {DateTime.Now.Year}";
                        await EnsureLinkExistsAsync("Holiday List", holidayListName, "holiday_list_name");

                        payload["company_name"] = value;
                        payload["company_abbr"] = "VCIPL";
                        payload["default_currency"] = "INR";
                        payload["country"] = "India";
                        payload["default_holiday_list"] = holidayListName;

                        await PostAsync($"/api/resource/{Uri.EscapeDataString(doctype)}", payload);
                    }
                    else if (doctype == "Employee")
                    {
                        string companyName = "Vinod Cookware India Private Limited - VCIPL";
                        await EnsureLinkExistsAsync("Company", companyName, "company_name");

                        string holidayListName = $"Standard Holiday List {DateTime.Now.Year}";
                        await EnsureLinkExistsAsync("Holiday List", holidayListName, "holiday_list_name");

                        payload["first_name"] = value;
                        payload["company"] = companyName;
                        payload["date_of_joining"] = DateTime.Now.ToString("yyyy-MM-dd");
                        payload["gender"] = "Male";
                        payload["date_of_birth"] = "1995-01-01";
                        payload["status"] = "Active";
                        payload["holiday_list"] = holidayListName;

                        await PostAsync($"/api/resource/{Uri.EscapeDataString(doctype)}", payload);
                    }
                    else if (doctype == "User")
                    {
                        string userEmail = value.Contains("@") ? value : $"{value.ToLower().Replace(" ", ".")}@vcipl.in";
                        payload["email"] = userEmail;
                        payload["first_name"] = value.Contains("@") ? value.Split('@')[0] : value;
                        payload["send_welcome_email"] = 0;
                        payload["enabled"] = 1;

                        await PostAsync($"/api/resource/{Uri.EscapeDataString(doctype)}", payload);
                    }
                    //    else if (doctype == "Leave Type")
                    //    {
                    //        payload["leave_type_name"] = value;
                    //        await PostAsync($"/api/resource/{Uri.EscapeDataString(doctype)}", payload);
                    //    }
                    //    else
                    //    {
                    //        payload[fieldName] = value;
                    //        await PostAsync($"/api/resource/{Uri.EscapeDataString(doctype)}", payload);
                    //    }

                    //    else if (doctype == "Mode of Payment")
                    //    {
                    //        payload["mode_of_payment"] = value;
                    //        payload["type"] = "Cash"; // Default type requirement for Frappe
                    //        await PostAsync($"/api/resource/{Uri.EscapeDataString(doctype)}", payload);
                    //    }

                    //}
                    else if (doctype == "Leave Type")
                    {
                        payload["leave_type_name"] = value;
                        await PostAsync($"/api/resource/{Uri.EscapeDataString(doctype)}", payload);
                    }
                    else if (doctype == "Mode of Payment")
                    {
                        payload["mode_of_payment"] = value;
                        payload["type"] = "Cash"; // Default type requirement for Frappe
                        await PostAsync($"/api/resource/{Uri.EscapeDataString(doctype)}", payload);
                    }
                    else
                    {
                        payload[fieldName] = value;
                        await PostAsync($"/api/resource/{Uri.EscapeDataString(doctype)}", payload);
                    }
                }
                catch
                {
                    // Ignore on-the-fly creation failures safely
                }
            }
        }
        //}

        public async Task<string> GetOrCreateEmployeeIdAsync(string employeeInput, string employeeName)
        {
            if (string.IsNullOrWhiteSpace(employeeInput)) return employeeInput;

            try
            {
                string checkUrl = $"/api/resource/Employee/{Uri.EscapeDataString(employeeInput)}";
                await GetAsync(checkUrl);
                return employeeInput;
            }
            catch
            {
                try
                {
                    string searchName = string.IsNullOrWhiteSpace(employeeName) ? employeeInput : employeeName;
                    string filters = Uri.EscapeDataString($"[[\"first_name\",\"=\",\"{searchName}\"]]");
                    string searchUrl = $"/api/resource/Employee?fields=[\"name\"]&filters={filters}";

                    string searchResponse = await GetAsync(searchUrl);
                    dynamic searchParsed = JsonConvert.DeserializeObject(searchResponse);

                    if (searchParsed.data != null && searchParsed.data.Count > 0)
                    {
                        return (string)searchParsed.data[0].name;
                    }
                }
                catch { }

                try
                {
                    string companyName = "Vinod Cookware India Private Limited - VCIPL";
                    await EnsureLinkExistsAsync("Company", companyName, "company_name");

                    var payload = new Dictionary<string, object>
                    {
                        ["first_name"] = string.IsNullOrWhiteSpace(employeeName) ? employeeInput : employeeName,
                        ["company"] = companyName,
                        ["date_of_joining"] = DateTime.Now.ToString("yyyy-MM-dd"),
                        ["gender"] = "Male",
                        ["date_of_birth"] = "1995-01-01",
                        ["status"] = "Active"
                    };

                    string createResponse = await PostAsync("/api/resource/Employee", payload);
                    dynamic createdParsed = JsonConvert.DeserializeObject(createResponse);
                    return (string)createdParsed.data.name;
                }
                catch
                {
                    return employeeInput;
                }
            }
        }

        public async Task EnsureEmployeeHasHolidayListAsync(string employeeId)
        {
            if (string.IsNullOrWhiteSpace(employeeId)) return;

            try
            {
                string holidayListName = $"Standard Holiday List {DateTime.Now.Year}";
                await EnsureLinkExistsAsync("Holiday List", holidayListName, "holiday_list_name");

                string empJson = await GetAsync($"/api/resource/Employee/{Uri.EscapeDataString(employeeId)}");
                dynamic parsed = JsonConvert.DeserializeObject(empJson);

                if (parsed.data != null && (parsed.data.holiday_list == null || string.IsNullOrWhiteSpace((string)parsed.data.holiday_list)))
                {
                    var updatePayload = new
                    {
                        holiday_list = holidayListName
                    };

                    await PutAsync($"/api/resource/Employee/{Uri.EscapeDataString(employeeId)}", updatePayload);
                }
            }
            catch
            {
                // Safe fallback if update fails
            }
        }

        public async Task EnsureLeaveAllocationExistsAsync(string employee, string leaveType, string company)
        {
            if (string.IsNullOrWhiteSpace(employee) || string.IsNullOrWhiteSpace(leaveType)) return;

            try
            {
                string fields = Uri.EscapeDataString("[\"name\"]");
                string filters = Uri.EscapeDataString($"[[\"employee\",\"=\",\"{employee}\"],[\"leave_type\",\"=\",\"{leaveType}\"],[\"docstatus\",\"=\",1]]");
                string checkUrl = $"/api/resource/Leave%20Allocation?fields={fields}&filters={filters}";

                string response = await GetAsync(checkUrl);
                dynamic parsed = JsonConvert.DeserializeObject(response);

                if (parsed.data != null && parsed.data.Count > 0) return;
            }
            catch { }

            try
            {
                var allocationPayload = new
                {
                    employee = employee,
                    leave_type = leaveType,
                    company = string.IsNullOrWhiteSpace(company) ? "Vinod Cookware India Private Limited - VCIPL" : company,
                    from_date = new DateTime(DateTime.Now.Year, 1, 1).ToString("yyyy-MM-dd"),
                    to_date = new DateTime(DateTime.Now.Year, 12, 31).ToString("yyyy-MM-dd"),
                    new_leaves_allocated = 30,
                    docstatus = 1
                };

                await PostAsync("/api/resource/Leave%20Allocation", allocationPayload);
            }
            catch (Exception ex) when (ex.Message.Contains("409") || ex.Message.Contains("DuplicateEntryError") || ex.Message.Contains("already exists"))
            {
                // Ignore duplicate allocation errors safely
            }
        }
    }
}
