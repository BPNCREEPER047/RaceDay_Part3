using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    public class CategoriesController : BaseController
    {
        private readonly IRaceDayApiService _api;
        public CategoriesController(IRaceDayApiService api) => _api = api;

        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn)  return RequireLogin();
            if (!IsOrganiser) return RequireOrganiser();
            return View(await _api.GetCategoriesAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryViewModel vm)
        {
            if (!IsLoggedIn)  return RequireLogin();
            if (!IsOrganiser) return RequireOrganiser();

            var created = await _api.CreateCategoryAsync(vm);
            TempData[created != null ? "Success" : "Error"] = created != null
                ? $"Category '{created.CategoryName}' created."
                : "Failed to create category.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsLoggedIn)  return RequireLogin();
            if (!IsOrganiser) return RequireOrganiser();

            var success = await _api.DeleteCategoryAsync(id);
            TempData[success ? "Success" : "Error"] =
                success ? "Category deleted." : "Could not delete category.";
            return RedirectToAction("Index");
        }
    }
}
