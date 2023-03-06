using Microsoft.AspNetCore.Mvc;
using AspNetCoreHero.ToastNotification.Abstractions;
using IDEA_Collection.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text;
namespace IDEA_Collection.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        public INotyfService _notyfService { get; }
        private readonly CollectIdeasContext _context;

        public HomeController(INotyfService notyfService, CollectIdeasContext context)
        {
            _context = context;
            _notyfService = notyfService;
        }
        [Area("Admin")]
        [Route("admin", Name = "AdminIndex")]
        [Authorize]
        public IActionResult Index()
        {

            var taikhoanID = HttpContext.Session.GetString("AccountId");
            var roleID = int.Parse(HttpContext.Session.GetString("RoletId"));
            if (taikhoanID == null || roleID != 1)
            {
                _notyfService.Success("You cannot access this page!");
                return RedirectToAction("Index", "Home", new { Area = "" });
            }
            if (taikhoanID != null)
            {
                var account = _context.Accounts.AsNoTracking().Include(x => x.Department).SingleOrDefault(x => x.AccountId == Convert.ToInt32(taikhoanID));

                if (account != null)
                {
                    var avata = account.Avatar;
                    ViewBag.avata = avata;
                }
            }

            return View();
        }
    }
}
