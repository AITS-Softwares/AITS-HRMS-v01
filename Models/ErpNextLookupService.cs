using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Erp_Aits.Services
{
    public class ErpNextLookupService
    {
        private readonly ErpNextService _erpNextService;

        public ErpNextLookupService(ErpNextService erpNextService)
        {
            _erpNextService = erpNextService;
        }

        private async Task<List<string>> GetNameListAsync(string doctype)
        {
            var names = new List<string>();

            try
            {
                string url = $"/api/resource/{doctype}?fields=[\"name\"]&limit_page_length=0";
                string json = await _erpNextService.GetAsync(url);
                dynamic parsed = JsonConvert.DeserializeObject(json);

                foreach (var row in parsed.data)
                {
                    names.Add((string)row.name);
                }
            }
            catch
            {
                // If the fetch fails (e.g. permission issue), just return an empty list
                // so the dropdown shows nothing rather than crashing the page.
            }

            return names;
        }

        public async Task<List<string>> GetEmployeeGroupsAsync()
        {
            var names = new List<string>();

            try
            {
                string url = "/api/resource/Employee Group?fields=[\"name\"]&limit_page_length=0";
                string json = await _erpNextService.GetAsync(url);
                dynamic parsed = JsonConvert.DeserializeObject(json);

                foreach (var row in parsed.data)
                {
                    names.Add((string)row.name);
                }
            }
            catch
            {
                // silently return empty list on failure
            }

            return names;
        }

        public Task<List<string>> GetDepartmentsAsync() => GetNameListAsync("Department");
        public Task<List<string>> GetDesignationsAsync() => GetNameListAsync("Designation");
        public Task<List<string>> GetBranchesAsync() => GetNameListAsync("Branch");
        public Task<List<string>> GetEmployeesAsync() => GetNameListAsync("Employee"); // for Reports To / Approvers
        public Task<List<string>> GetHolidayListsAsync() => GetNameListAsync("Holiday List");
        public Task<List<string>> GetShiftTypesAsync() => GetNameListAsync("Shift Type"); // for Default Shift
        public Task<List<string>> GetCostCentersAsync() => GetNameListAsync("Cost Center"); // for Payroll Cost Center
    }
}
