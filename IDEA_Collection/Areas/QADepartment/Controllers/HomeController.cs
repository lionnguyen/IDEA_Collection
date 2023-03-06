using Microsoft.AspNetCore.Mvc;
using AspNetCoreHero.ToastNotification.Abstractions;
using IDEA_Collection.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace IDEA_Collection.Areas.QADepartment.Controllers
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

        [Authorize]
        [Area("QADepartment")]
        [Route("qa.html", Name = "QA")]
        public IActionResult Index()
        {
            var taikhoanID = HttpContext.Session.GetString("AccountId");
            var roleID = int.Parse(HttpContext.Session.GetString("RoletId"));
            if (taikhoanID == null || roleID != 1002)
            {
                _notyfService.Success("You cannot access this page!");
                return RedirectToAction("Index", "Home", new { Area = "" });
            }
            if (taikhoanID != null)
            {
                var khachhang = _context.Accounts.AsNoTracking().Include(x => x.Department).SingleOrDefault(x => x.AccountId == Convert.ToInt32(taikhoanID));

                if (khachhang != null)
                {
                    var avata = khachhang.Avatar;
                    var department = khachhang.Department.DepartmentName;
                    ViewBag.avata = avata;
                    ViewBag.department = department;
                }
            }
            return View();
        }
    }
}
