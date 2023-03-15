using Microsoft.AspNetCore.Mvc;
using AspNetCoreHero.ToastNotification.Abstractions;
using IDEA_Collection.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            var qaAccount = _context.Accounts.AsNoTracking().Include(x => x.Department).SingleOrDefault(x => x.AccountId == Convert.ToInt32(taikhoanID));
            List<Idea> ideasLs = _context.Ideas.Include(x=>x.Account).Where(x => x.Account.DepartmentId == qaAccount.DepartmentId).ToList();
            List<Account> accountLs =  _context.Accounts.Include(x=>x.Ideas).Where(x => x.DepartmentId == qaAccount.DepartmentId).OrderByDescending(x=>x.Ideas.Count(x=>x.Account.DepartmentId == qaAccount.DepartmentId)).ToList();
            ViewBag.lsIeas = ideasLs;
            ViewBag.lsAccount = accountLs;
            var avata = qaAccount.Avatar;
            var department = qaAccount.Department.DepartmentName;
            ViewBag.avata = avata;
            ViewBag.department = department;
            return View();
        }
    }
}
