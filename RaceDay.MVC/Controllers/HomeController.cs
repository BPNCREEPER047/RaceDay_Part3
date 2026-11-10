using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IRaceDayApiService _api;
        public HomeController(IRaceDayApiService api) => _api = api;

        public async Task<IActionResult> Index(string? type, string? search)
        {
            var events = await _api.GetEventsAsync();

            if (!string.IsNullOrWhiteSpace(type))
                events = events.Where(e => e.EventType.ToLower() == type.ToLower()).ToList();

            if (!string.IsNullOrWhiteSpace(search))
                events = events.Where(e =>
                    e.EventName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (e.Location?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();

            ViewBag.FilterType = type;
            ViewBag.Search     = search;
            return View(events);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View();
    }
}
