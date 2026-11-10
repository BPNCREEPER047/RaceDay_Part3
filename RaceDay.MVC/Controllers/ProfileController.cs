using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Helpers;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    public class ProfileController : BaseController
    {
        private readonly IRaceDayApiService _api;
        public ProfileController(IRaceDayApiService api) => _api = api;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn) return RequireLogin();

            var profile = await _api.GetProfileAsync();
            if (profile == null) { TempData["Error"] = "Could not load profile."; return RedirectToAction("Index", "Home"); }

            ViewBag.ProfileData = profile;
            return View(new UpdateProfileViewModel
            {
                FullName = profile.FullName,
                Email    = profile.Email
            });
        }

        [HttpPost]
        public async Task<IActionResult> Index(UpdateProfileViewModel vm)
        {
            if (!IsLoggedIn) return RequireLogin();

            if (vm.ProfilePicture?.Length > 0)
                await _api.UploadProfilePictureAsync(vm.ProfilePicture);

            var success = await _api.UpdateProfileAsync(new UpdateProfileDto
            {
                FullName = vm.FullName,
                Email    = vm.Email,
                Password = string.IsNullOrWhiteSpace(vm.Password) ? null : vm.Password
            });

            if (!success)
            {
                ModelState.AddModelError("", "Failed to update profile.");
                ViewBag.ProfileData = await _api.GetProfileAsync();
                return View(vm);
            }

            if (!string.IsNullOrWhiteSpace(vm.FullName))
                HttpContext.Session.SetString(SessionHelper.NameKey, vm.FullName);
            if (!string.IsNullOrWhiteSpace(vm.Email))
                HttpContext.Session.SetString(SessionHelper.EmailKey, vm.Email);

            TempData["Success"] = "Profile updated.";
            return RedirectToAction("Index");
        }
    }
}
