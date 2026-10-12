using Erp_Aits.Infrastructure.ErpNext;
using Erp_Aits.Infrastructure.ErpNext.MasterData;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Erp_Aits.Controllers
{
    [Route("Integration/MasterData")]
    public class MasterDataIntegrationController : Controller
    {
        private readonly IErpNextMasterDataService _masterDataService;

        public MasterDataIntegrationController(IErpNextMasterDataService masterDataService)
        {
            _masterDataService = masterDataService;
        }

        [HttpGet("Supported")]
        public IActionResult Supported()
        {
            return Json(new
            {
                success = true,
                data = _masterDataService.GetSupportedMasters()
            });
        }

        [HttpGet("{masterType}")]
        public async Task<IActionResult> GetFromErpNext(
            string masterType,
            int limit = 100,
            CancellationToken cancellationToken = default)
        {
            try
            {
                IReadOnlyList<MasterDataRecord> records =
                    await _masterDataService.GetFromErpNextAsync(masterType, limit, cancellationToken);

                return Json(new
                {
                    success = true,
                    masterType,
                    count = records.Count,
                    data = records
                });
            }
            catch (Exception ex) when (ex is ArgumentException || ex is ErpNextApiException)
            {
                Response.StatusCode = 502;
                return Json(new
                {
                    success = false,
                    message = "Could not fetch master data from ERPNext.",
                    details = ex.Message
                });
            }
        }

        [HttpPost("{masterType}/SyncFromErpNext")]
        public async Task<IActionResult> SyncFromErpNext(
            string masterType,
            int limit = 500,
            CancellationToken cancellationToken = default)
        {
            try
            {
                MasterDataSyncResult result =
                    await _masterDataService.SyncFromErpNextAsync(masterType, limit, cancellationToken);

                return Json(new
                {
                    success = true,
                    message = "Master data synced from ERPNext.",
                    result
                });
            }
            catch (Exception ex) when (ex is ArgumentException || ex is ErpNextApiException)
            {
                Response.StatusCode = 502;
                return Json(new
                {
                    success = false,
                    message = "Could not sync master data from ERPNext.",
                    details = ex.Message
                });
            }
        }

        [HttpPost("SyncAllFromErpNext")]
        public async Task<IActionResult> SyncAllFromErpNext(CancellationToken cancellationToken)
        {
            try
            {
                IReadOnlyList<MasterDataSyncResult> results =
                    await _masterDataService.SyncAllFromErpNextAsync(cancellationToken);

                return Json(new
                {
                    success = true,
                    message = "All supported master data synced from ERPNext.",
                    results
                });
            }
            catch (ErpNextApiException ex)
            {
                Response.StatusCode = 502;
                return Json(new
                {
                    success = false,
                    message = "Could not sync all master data from ERPNext.",
                    details = ex.Message
                });
            }
        }

        [HttpPost("{masterType}/Create")]
        public async Task<IActionResult> Create(
            string masterType,
            [FromBody] CreateMasterDataRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                string response = await _masterDataService.CreateInErpNextAsync(masterType, request, cancellationToken);

                return Json(new
                {
                    success = true,
                    message = "Master data created in ERPNext.",
                    erpNextResponse = JsonConvert.DeserializeObject(response)
                });
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is ErpNextApiException)
            {
                Response.StatusCode = 400;
                return Json(new
                {
                    success = false,
                    message = "Could not create master data in ERPNext.",
                    details = ex.Message
                });
            }
        }
    }
}
