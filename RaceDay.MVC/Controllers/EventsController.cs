using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    public class EventsController : BaseController
    {
        private readonly IRaceDayApiService _api;
        public EventsController(IRaceDayApiService api) => _api = api;

        // ── Organiser Dashboard ──────────────────────────────────────────────
        public async Task<IActionResult> Dashboard()
        {
            if (!IsLoggedIn)  return RequireLogin();
            if (!IsOrganiser) return RequireOrganiser();

            var allEvents = await _api.GetEventsAsync();
            var mine = allEvents.Where(e => e.OrganiserID == CurrentUserId).ToList();

            int totalEnrolments = 0;
            foreach (var ev in mine)
            {
                var enrolments = await _api.GetEventEnrolmentsAsync(ev.EventID);
                totalEnrolments += enrolments.Count;
            }

            return View(new OrganiserDashboardViewModel
            {
                Events          = mine,
                TotalEvents     = mine.Count,
                TotalEnrolments = totalEnrolments,
                UpcomingEvents  = mine
                    .Where(e => e.EventDate > DateTime.UtcNow)
                    .OrderBy(e => e.EventDate)
                    .Take(5).ToList()
            });
        }

        // ── Event Detail (public) ────────────────────────────────────────────
        public async Task<IActionResult> Details(int id)
        {
            var ev = await _api.GetEventAsync(id);
            if (ev == null) { TempData["Error"] = "Event not found."; return RedirectToAction("Index", "Home"); }
            return View(ev);
        }

        // ── Create ───────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!IsLoggedIn)  return RequireLogin();
            if (!IsOrganiser) return RequireOrganiser();
            var categories = await _api.GetCategoriesAsync();
            return View(new CreateEventViewModel { Categories = categories });
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateEventViewModel vm)
        {
            if (!IsLoggedIn)  return RequireLogin();
            if (!IsOrganiser) return RequireOrganiser();

            vm.Categories = await _api.GetCategoriesAsync();
            if (!ModelState.IsValid) return View(vm);

            var created = await _api.CreateEventAsync(new CreateEventApiDto
            {
                EventName       = vm.EventName,
                Description     = vm.Description,
                EventDate       = vm.EventDate,
                Location        = vm.Location,
                Distance        = vm.Distance,
                EventType       = vm.EventType.ToLower(),
                MaxParticipants = vm.MaxParticipants,
                CategoryID      = vm.CategoryID
            });

            if (created == null)
            {
                ModelState.AddModelError("", "Failed to create event. Check all fields.");
                return View(vm);
            }

            // Upload banner after creation
            if (vm.BannerImage != null && vm.BannerImage.Length > 0)
            {
                var url = await _api.UploadEventBannerAsync(created.EventID, vm.BannerImage);
                if (url != null)
                    await _api.UpdateBannerAsync(created.EventID, url);
            }

            TempData["Success"] = $"Event '{created.EventName}' created!";
            return RedirectToAction("Dashboard");
        }

        // ── Edit ─────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsLoggedIn)  return RequireLogin();
            if (!IsOrganiser) return RequireOrganiser();

            var ev = await _api.GetEventAsync(id);
            if (ev == null || ev.OrganiserID != CurrentUserId)
            {
                TempData["Error"] = "Event not found or access denied.";
                return RedirectToAction("Dashboard");
            }

            return View(new EditEventViewModel
            {
                EventID           = ev.EventID,
                EventName         = ev.EventName,
                Description       = ev.Description,
                EventDate         = ev.EventDate,
                Location          = ev.Location,
                Distance          = ev.Distance,
                EventType         = ev.EventType,
                MaxParticipants   = ev.MaxParticipants,
                CategoryID        = ev.CategoryID,
                ExistingBannerUrl = ev.BannerImageUrl,
                Categories        = await _api.GetCategoriesAsync()
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditEventViewModel vm)
        {
            if (!IsLoggedIn)  return RequireLogin();
            if (!IsOrganiser) return RequireOrganiser();

            vm.Categories = await _api.GetCategoriesAsync();
            if (!ModelState.IsValid) return View(vm);

            var updated = await _api.UpdateEventAsync(id, new CreateEventApiDto
            {
                EventName       = vm.EventName,
                Description     = vm.Description,
                EventDate       = vm.EventDate,
                Location        = vm.Location,
                Distance        = vm.Distance,
                EventType       = vm.EventType.ToLower(),
                MaxParticipants = vm.MaxParticipants,
                CategoryID      = vm.CategoryID
            });

            if (updated == null)
            {
                ModelState.AddModelError("", "Failed to update event.");
                return View(vm);
            }

            if (vm.BannerImage != null && vm.BannerImage.Length > 0)
            {
                var url = await _api.UploadEventBannerAsync(id, vm.BannerImage);
                if (url != null) await _api.UpdateBannerAsync(id, url);
            }

            TempData["Success"] = "Event updated!";
            return RedirectToAction("Dashboard");
        }

        // ── Delete ───────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsLoggedIn)  return RequireLogin();
            if (!IsOrganiser) return RequireOrganiser();

            var success = await _api.DeleteEventAsync(id);
            TempData[success ? "Success" : "Error"] =
                success ? "Event deleted." : "Failed to delete event.";
            return RedirectToAction("Dashboard");
        }

        // ── Enrolments (Organiser) ───────────────────────────────────────────
        public async Task<IActionResult> Enrolments(int id)
        {
            if (!IsLoggedIn)  return RequireLogin();
            if (!IsOrganiser) return RequireOrganiser();

            var ev = await _api.GetEventAsync(id);
            if (ev == null) { TempData["Error"] = "Event not found."; return RedirectToAction("Dashboard"); }

            ViewBag.Event = ev;
            return View(await _api.GetEventEnrolmentsAsync(id));
        }
    }
}
