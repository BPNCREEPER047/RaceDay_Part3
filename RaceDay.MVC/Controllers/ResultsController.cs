using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    public class ResultsController : BaseController
    {
        private readonly IRaceDayApiService _api;
        public ResultsController(IRaceDayApiService api) => _api = api;

        public async Task<IActionResult> MyResults()
        {
            if (!IsLoggedIn) return RequireLogin();
            if (IsOrganiser) return RequireParticipant();
            return View(await _api.GetMyResultsAsync());
        }

        public async Task<IActionResult> EventResults(int eventId)
        {
            if (!IsLoggedIn)  return RequireLogin();
            if (!IsOrganiser) return RequireOrganiser();

            ViewBag.Event      = await _api.GetEventAsync(eventId);
            ViewBag.Enrolments = await _api.GetEventEnrolmentsAsync(eventId);
            return View(await _api.GetEventResultsAsync(eventId));
        }

        [HttpGet]
        public async Task<IActionResult> Capture(int eventId)
        {
            if (!IsLoggedIn)  return RequireLogin();
            if (!IsOrganiser) return RequireOrganiser();

            var ev         = await _api.GetEventAsync(eventId);
            var enrolments = await _api.GetEventEnrolmentsAsync(eventId);
            var existing   = await _api.GetEventResultsAsync(eventId);
            var enteredIds = existing.Select(r => r.EnrolmentID).ToHashSet();

            ViewBag.Event      = ev;
            ViewBag.Enrolments = enrolments.Where(e => !enteredIds.Contains(e.EnrolmentID)).ToList();
            return View(new CreateResultViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Capture(int eventId, CreateResultViewModel vm)
        {
            if (!IsLoggedIn)  return RequireLogin();
            if (!IsOrganiser) return RequireOrganiser();

            if (!ModelState.IsValid)
            {
                ViewBag.Event      = await _api.GetEventAsync(eventId);
                ViewBag.Enrolments = await _api.GetEventEnrolmentsAsync(eventId);
                return View(vm);
            }

            var result = await _api.CreateResultAsync(vm);
            TempData[result != null ? "Success" : "Error"] = result != null
                ? $"Result captured for {result.ParticipantName}."
                : "Failed to capture result.";

            return RedirectToAction("EventResults", new { eventId });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, int eventId,
            string finishTime, int? position, string? notes)
        {
            if (!IsLoggedIn)  return RequireLogin();
            if (!IsOrganiser) return RequireOrganiser();

            var updated = await _api.UpdateResultAsync(id, new UpdateResultApiDto
            {
                FinishTime = finishTime,
                Position   = position,
                Notes      = notes
            });

            TempData[updated != null ? "Success" : "Error"] =
                updated != null ? "Result updated." : "Failed to update result.";
            return RedirectToAction("EventResults", new { eventId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, int eventId)
        {
            if (!IsLoggedIn)  return RequireLogin();
            if (!IsOrganiser) return RequireOrganiser();

            var success = await _api.DeleteResultAsync(id);
            TempData[success ? "Success" : "Error"] =
                success ? "Result deleted." : "Failed to delete result.";
            return RedirectToAction("EventResults", new { eventId });
        }
    }
}
