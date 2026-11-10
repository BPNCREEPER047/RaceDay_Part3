using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RaceDay.MVC.Helpers;

namespace RaceDay.MVC.Controllers
{
    public abstract class BaseController : Controller
    {
        protected string? CurrentRole   => SessionHelper.GetRole(HttpContext.Session);
        protected string? CurrentName   => SessionHelper.GetName(HttpContext.Session);
        protected int?    CurrentUserId => SessionHelper.GetUserId(HttpContext.Session);
        protected bool    IsLoggedIn    => SessionHelper.IsLoggedIn(HttpContext.Session);
        protected bool    IsOrganiser   => SessionHelper.IsOrganiser(HttpContext.Session);

        protected IActionResult RequireLogin()
        {
            TempData["Error"] = "Please log in to access this page.";
            return RedirectToAction("Login", "Auth");
        }

        protected IActionResult RequireOrganiser()
        {
            TempData["Error"] = "Only Organisers can access this page.";
            return RedirectToAction("Index", "Home");
        }

        protected IActionResult RequireParticipant()
        {
            TempData["Error"] = "Only Participants can access this page.";
            return RedirectToAction("Index", "Home");
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ViewBag.IsLoggedIn    = IsLoggedIn;
            ViewBag.CurrentRole   = CurrentRole;
            ViewBag.CurrentName   = CurrentName;
            ViewBag.CurrentUserId = CurrentUserId;
            base.OnActionExecuting(context);
        }
    }
}
