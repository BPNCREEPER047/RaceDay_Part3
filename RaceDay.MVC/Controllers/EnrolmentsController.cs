using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    public class EnrolmentsController : BaseController
    {
        private readonly IRaceDayApiService _api;
        public EnrolmentsController(IRaceDayApiService api) => _api = api;

        public async Task<IActionResult> MyEnrolments()
        {
            if (!IsLoggedIn)  return RequireLogin();
            if (IsOrganiser)  return RequireParticipant();
            return View(await _api.GetMyEnrolmentsAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Enrol(int eventId)
        {
            if (!IsLoggedIn) return RequireLogin();
            if (IsOrganiser) return RequireParticipant();

            var result = await _api.EnrolAsync(eventId);
            TempData[result != null ? "Success" : "Error"] = result != null
                ? $"Successfully enrolled in {result.EventName}!"
                : "Could not enrol. You may already be enrolled or the event is full.";

            return RedirectToAction("Details", "Events", new { id = eventId });
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            if (!IsLoggedIn) return RequireLogin();
            if (IsOrganiser) return RequireParticipant();

            var success = await _api.CancelEnrolmentAsync(id);
            TempData[success ? "Success" : "Error"] =
                success ? "Enrolment cancelled." : "Could not cancel enrolment.";
            return RedirectToAction("MyEnrolments");
        }
    }
}
