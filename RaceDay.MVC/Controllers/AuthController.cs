using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Helpers;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    public class AuthController : BaseController
    {
        private readonly IRaceDayApiService _api;
        public AuthController(IRaceDayApiService api) => _api = api;

        [HttpGet]
        public IActionResult Register()
        {
            if (IsLoggedIn) return RedirectToAction("Index", "Home");
            return View(new RegisterViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _api.RegisterAsync(vm);
            if (result?.User == null)
            {
                ModelState.AddModelError("", "Registration failed. Email may already be in use.");
                return View(vm);
            }

            SessionHelper.SetUser(HttpContext.Session,
                result.User.UserID, result.User.Role,
                result.User.FullName, result.User.Email);

            TempData["Success"] = $"Welcome to RaceDay, {result.User.FullName}!";
            return result.User.Role == "Organiser"
                ? RedirectToAction("Dashboard", "Events")
                : RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (IsLoggedIn) return RedirectToAction("Index", "Home");
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _api.LoginAsync(vm);
            if (result?.User == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(vm);
            }

            SessionHelper.SetUser(HttpContext.Session,
                result.User.UserID, result.User.Role,
                result.User.FullName, result.User.Email);

            TempData["Success"] = $"Welcome back, {result.User.FullName}!";
            return result.User.Role == "Organiser"
                ? RedirectToAction("Dashboard", "Events")
                : RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _api.LogoutAsync();           // Clear API session
            SessionHelper.ClearUser(HttpContext.Session); // Clear MVC session
            TempData["Success"] = "You have been logged out.";
            return RedirectToAction("Index", "Home");
        }
    }
}
