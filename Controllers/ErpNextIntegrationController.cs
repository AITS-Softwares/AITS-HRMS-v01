using Erp_Aits.Infrastructure.ErpNext;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Erp_Aits.Controllers
{
    [Route("Integration/ErpNext")]
    public class ErpNextIntegrationController : Controller
    {
        private readonly IErpNextClient _erpNextClient;

        public ErpNextIntegrationController(IErpNextClient erpNextClient)
        {
            _erpNextClient = erpNextClient;
        }

        [HttpGet("Health")]
        public async Task<IActionResult> Health(CancellationToken cancellationToken)
        {
            try
            {
                string response = await _erpNextClient.CheckConnectionAsync(cancellationToken);
                return Json(new
                {
                    success = true,
                    message = "ERPNext connection is working.",
                    erpNextResponse = JsonConvert.DeserializeObject(response)
                });
            }
            catch (ErpNextApiException ex)
            {
                Response.StatusCode = 502;
                return Json(new
                {
                    success = false,
                    message = "ERPNext connection failed.",
                    details = ex.Message,
                    erpNextStatusCode = ex.StatusCode.HasValue ? (int?)ex.StatusCode.Value : null
                });
            }
        }

        [HttpGet("Departments")]
        public async Task<IActionResult> Departments(CancellationToken cancellationToken)
        {
            try
            {
                string response = await _erpNextClient.GetListAsync(
                    "Department",
                    new[] { "name", "department_name" },
                    20,
                    cancellationToken);

                return Content(response, "application/json");
            }
            catch (ErpNextApiException ex)
            {
                Response.StatusCode = 502;
                return Json(new
                {
                    success = false,
                    message = "Could not fetch departments from ERPNext.",
                    details = ex.Message,
                    erpNextStatusCode = ex.StatusCode.HasValue ? (int?)ex.StatusCode.Value : null
                });
            }
        }

        [HttpPost("TestDepartment")]
        public async Task<IActionResult> TestDepartment(CancellationToken cancellationToken)
        {
            string departmentName = "HRMS Integration Test " + DateTime.Now.ToString("yyyyMMddHHmmss");

            try
            {
                var payload = new
                {
                    department_name = departmentName
                };

                string response = await _erpNextClient.CreateDocumentAsync("Department", payload, cancellationToken);

                return Json(new
                {
                    success = true,
                    message = "Test Department created in ERPNext.",
                    departmentName,
                    erpNextResponse = JsonConvert.DeserializeObject(response)
                });
            }
            catch (ErpNextApiException ex)
            {
                Response.StatusCode = 502;
                return Json(new
                {
                    success = false,
                    message = "Could not create test Department in ERPNext.",
                    departmentName,
                    details = ex.Message,
                    erpNextStatusCode = ex.StatusCode.HasValue ? (int?)ex.StatusCode.Value : null
                });
            }
        }
    }
}
