using Microsoft.AspNetCore.Mvc;
using Erp_Aits.Models;

namespace Erp_Aits.Controllers
{
    public class CallsController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Calls";
            ViewData["Module"] = "CRM";
            ViewData["Icon"] = "phone";

            var calls = GetSampleCalls();
            return View(calls);
        }

        public IActionResult Details(int id)
        {
            ViewData["Title"] = "Call Details";
            ViewData["Module"] = "CRM";
            ViewData["Icon"] = "phone";

            var call = GetSampleCalls().FirstOrDefault(c => c.Id == id);
            if (call == null)
                return NotFound();

            return View(call);
        }

        public IActionResult Create()
        {
            ViewData["Title"] = "Log Call";
            ViewData["Module"] = "CRM";
            ViewData["Icon"] = "plus-circle";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Call call)
        {
            if (ModelState.IsValid)
            {
                call.Id = new Random().Next(100, 999);
                call.CreatedDate = DateTime.Now;
                TempData["Success"] = "Call logged successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(call);
        }

        public IActionResult Edit(int id)
        {
            ViewData["Title"] = "Edit Call";
            ViewData["Module"] = "CRM";
            ViewData["Icon"] = "edit";

            var call = GetSampleCalls().FirstOrDefault(c => c.Id == id);
            if (call == null)
                return NotFound();

            return View(call);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Call call)
        {
            if (id != call.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                TempData["Success"] = "Call updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(call);
        }

        public IActionResult Delete(int id)
        {
            ViewData["Title"] = "Delete Call";
            ViewData["Module"] = "CRM";
            ViewData["Icon"] = "trash";

            var call = GetSampleCalls().FirstOrDefault(c => c.Id == id);
            if (call == null)
                return NotFound();

            return View(call);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            TempData["Success"] = "Call deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private List<Call> GetSampleCalls()
        {
            return new List<Call>
            {
                new Call { Id = 1, CallType = "Incoming", ContactPerson = "John Doe",
                    Phone = "+91 98765 43210", CompanyName = "Tech Solutions",
                    CallDate = DateTime.Now.AddHours(-2), Duration = "15 min",
                    Purpose = "Project Discussion", Notes = "Discussed website redesign requirements",
                    Status = "Completed", CreatedDate = DateTime.Now.AddHours(-2), AssignedTo = "Admin User" },
                new Call { Id = 2, CallType = "Outgoing", ContactPerson = "Jane Smith",
                    Phone = "+91 87654 32109", CompanyName = "Startup Innovations",
                    CallDate = DateTime.Now.AddHours(-5), Duration = "10 min",
                    Purpose = "Follow-up", Notes = "Follow-up on mobile app proposal",
                    Status = "Completed", CreatedDate = DateTime.Now.AddHours(-5), AssignedTo = "Admin User" },
                new Call { Id = 3, CallType = "Scheduled", ContactPerson = "Mike Johnson",
                    Phone = "+91 76543 21098", CompanyName = "Cloud Corp",
                    CallDate = DateTime.Now.AddDays(1), Duration = "30 min",
                    Purpose = "Demo Meeting", Notes = "Cloud migration demo scheduled",
                    Status = "Scheduled", CreatedDate = DateTime.Now.AddHours(-1), AssignedTo = "Admin User" }
            };
        }
    }
}