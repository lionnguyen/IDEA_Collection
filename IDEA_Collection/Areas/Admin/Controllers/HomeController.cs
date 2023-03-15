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
using System.Collections.Generic;

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
            var account = _context.Accounts.AsNoTracking().Include(x => x.Department).SingleOrDefault(x => x.AccountId == Convert.ToInt32(taikhoanID));
            List<Idea> lsIdeas = _context.Ideas.Include(x => x.Account).ToList();
            List<Account> lsAccount = _context.Accounts.Include(x => x.Ideas).Where(x => x.RoleId != account.RoleId).OrderByDescending(x => x.Ideas.Count(x => x.Account.RoleId != account.RoleId)).ToList();
            var lsDepartment = _context.Departments.Include(x => x.Accounts).ThenInclude(x => x.Ideas).Where(x => x.DepartmentId != account.DepartmentId).ToList();
            ViewBag.avata = account.Avatar;
            ViewBag.lsIdeas = lsIdeas;
            ViewBag.lsAccount = lsAccount;
            ViewBag.lsDepartment = lsDepartment;
            return View();
        }
    }
}
