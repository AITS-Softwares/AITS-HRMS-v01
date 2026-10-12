using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Erp_Aits.Models;
using Erp_Aits.Services;

namespace Erp_Aits.Controllers
{
    public class PermissionsController : Controller
    {
        private readonly IPageAccessService _pageAccessService;

        public PermissionsController(IPageAccessService pageAccessService)
        {
            _pageAccessService = pageAccessService;
        }

        // GET: Permissions
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Page Access Management";
            ViewData["Module"] = "Administration";
            ViewData["Icon"] = "lock";

            var model = await _pageAccessService.GetUserPageAccessViewModel();
            return View(model);
        }

        // GET: Permissions/GetUserPages
        [HttpGet]
        public async Task<IActionResult> GetUserPages(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return Json(new { success = false, message = "Invalid user ID." });
                }

                var model = await _pageAccessService.GetUserPageAccessViewModel(userId);
                var user = model.Users.FirstOrDefault(u => u.Id == userId);

                Console.WriteLine($"GetUserPages: UserId={userId}, SelectedPages={string.Join(",", model.SelectedPageIds)}");

                return Json(new
                {
                    success = true,
                    user = user,
                    selectedPages = model.SelectedPageIds,
                    allPages = model.AllPages.Select(p => new
                    {
                        p.Id,
                        p.DisplayName,
                        p.ControllerName,
                        p.ActionName,
                        p.Icon,
                        p.ParentMenu,
                        IsSelected = model.SelectedPageIds.Contains(p.Id)
                    })
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUserPages: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Permissions/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(int userId, string selectedPages)
        {
            try
            {
                Console.WriteLine($"Save called with UserId: {userId}, SelectedPages: {selectedPages}");

                if (userId <= 0)
                {
                    return Json(new { success = false, message = "Invalid user selected." });
                }

                // Parse the selected pages from string
                var selectedPageList = new List<int>();
                if (!string.IsNullOrEmpty(selectedPages))
                {
                    selectedPageList = selectedPages.Split(',')
                        .Where(s => !string.IsNullOrEmpty(s))
                        .Select(int.Parse)
                        .ToList();
                }

                Console.WriteLine($"Parsed {selectedPageList.Count} pages: {string.Join(",", selectedPageList)}");

                var success = await _pageAccessService.SaveUserPageAccess(userId, selectedPageList);

                if (success)
                {
                    Console.WriteLine($"Successfully saved permissions for user {userId}");
                    return Json(new { success = true, message = "Page access updated successfully!" });
                }
                else
                {
                    Console.WriteLine($"Failed to save permissions for user {userId}");
                    return Json(new { success = false, message = "Failed to update page access." });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Save: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
    }
}