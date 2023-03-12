using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IDEA_Collection.Models;
using Microsoft.AspNetCore.Http;
using PagedList.Core;
using System.IO;
using IDEA_Collection.Helpper;
using IDEA_Collection.Extension;
using AspNetCoreHero.ToastNotification.Abstractions;

namespace IDEA_Collection.Areas.QADepartment.Controllers
{
    [Area("QADepartment")]
    public class QAAccountsController : Controller
    {
        private readonly CollectIdeasContext _context;
        public INotyfService _notyfService { get; }

        public QAAccountsController(CollectIdeasContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }

        // GET: QADepartment/QAAccounts
        public async Task<IActionResult> Index(int? page)
        {
            var accountID = HttpContext.Session.GetString("AccountId");
            if (accountID != null)
            {
                var QA = _context.Accounts.AsNoTracking().SingleOrDefault(x => x.AccountId == Convert.ToInt32(accountID));
                if (QA != null)
                {
                    var avata = QA.Avatar;
                    var fullname = QA.FullName;
                    ViewBag.avata = avata;
                    ViewBag.fullname = fullname;
                }
            }
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 20;
            var departmentID = HttpContext.Session.GetString("DepartmentId");
            var lsQAAccount = _context.Accounts
                .Include(a => a.Department)
                .Include(a => a.Role)
                .Where(a => a.DepartmentId == Convert.ToInt32(departmentID) && a.RoleId == 2)
                .OrderBy(x => x.CreateDate);

            PagedList<Account> models = new PagedList<Account>(lsQAAccount, pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }

        // GET: QADepartment/QAAccounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var accountID = HttpContext.Session.GetString("AccountId");
            if (accountID != null)
            {
                var QA = _context.Accounts.AsNoTracking().SingleOrDefault(x => x.AccountId == Convert.ToInt32(accountID));
                if (QA != null)
                {
                    var avata = QA.Avatar;
                    var fullname = QA.FullName;
                    ViewBag.avata = avata;
                    ViewBag.fullname = fullname;
                }
            }
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
                .Include(a => a.Department)
                .Include(a => a.Role)
                .FirstOrDefaultAsync(m => m.AccountId == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // GET: QADepartment/QAAccounts/Create
        public IActionResult Create()
        {
            var accountID = HttpContext.Session.GetString("AccountId");
            if (accountID != null)
            {
                var QA = _context.Accounts.AsNoTracking().SingleOrDefault(x => x.AccountId == Convert.ToInt32(accountID));
                if (QA != null)
                {
                    var avata = QA.Avatar;
                    var fullname = QA.FullName;
                    ViewBag.avata = avata;
                    ViewBag.fullname = fullname;
                }
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentId");
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleId");
            return View();
        }

        // POST: QADepartment/QAAccounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AccountId,Phone,Birthday,Email,Password,Address,Active,FullName,RoleId,DepartmentId,LastLogin,CreateDate,Avatar")] Account account, Microsoft.AspNetCore.Http.IFormFile fThumb)
        {
            if (ModelState.IsValid)
            {
                var departmentID = HttpContext.Session.GetString("DepartmentId");
                if (fThumb != null)
                {
                    string extension = Path.GetExtension(fThumb.FileName);
                    string image = Utilities.SEOUrl(account.FullName) + extension;
                    account.Avatar = await Utilities.UploadFile(fThumb, @"avatas", image.ToLower());
                }
                account.DepartmentId = Convert.ToInt32(departmentID);
                account.RoleId = 2;
                account.Password = account.Password.Trim().ToMD5();
                account.CreateDate = DateTime.Now;
                _context.Add(account);
                await _context.SaveChangesAsync();
                _notyfService.Success("Create success!");
                return RedirectToAction(nameof(Index));
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentId", account.DepartmentId);
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleId", account.RoleId);
            return View(account);
        }

        // GET: QADepartment/QAAccounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var accountID = HttpContext.Session.GetString("AccountId");
            if (accountID != null)
            {
                var QA = _context.Accounts.AsNoTracking().SingleOrDefault(x => x.AccountId == Convert.ToInt32(accountID));
                if (QA != null)
                {
                    var avata = QA.Avatar;
                    var fullname = QA.FullName;
                    ViewBag.avata = avata;
                    ViewBag.fullname = fullname;
                }
            }
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentId", account.DepartmentId);
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleId", account.RoleId);
            return View(account);
        }

        // POST: QADepartment/QAAccounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AccountId,Phone,Birthday,Email,Password,Address,Active,FullName,RoleId,DepartmentId,LastLogin,CreateDate,Avatar")] Account account, Microsoft.AspNetCore.Http.IFormFile fThumb)
        {
            if (id != account.AccountId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var accountPost = _context.Accounts.AsNoTracking().FirstOrDefault(x => x.AccountId == account.AccountId);
                    if (fThumb != null)
                    {
                        string extension = Path.GetExtension(fThumb.FileName);
                        string image = Utilities.SEOUrl(account.FullName) + extension;
                        account.Avatar = await Utilities.UploadFile(fThumb, @"avatas", image.ToLower());
                    }
                    if (account.Password != accountPost.Password)
                    {
                        account.Password = account.Password.Trim().ToMD5();
                    }
                    _context.Update(account);
                    await _context.SaveChangesAsync();
                    _notyfService.Success("Update success!");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccountExists(account.AccountId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentId", account.DepartmentId);
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleId", account.RoleId);
            return View(account);
        }

        // GET: QADepartment/QAAccounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var accountID = HttpContext.Session.GetString("AccountId");
            if (accountID != null)
            {
                var QA = _context.Accounts.AsNoTracking().SingleOrDefault(x => x.AccountId == Convert.ToInt32(accountID));
                if (QA != null)
                {
                    var avata = QA.Avatar;
                    var fullname = QA.FullName;
                    ViewBag.avata = avata;
                    ViewBag.fullname = fullname;
                }
            }
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
                .Include(a => a.Department)
                .Include(a => a.Role)
                .FirstOrDefaultAsync(m => m.AccountId == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // POST: QADepartment/QAAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
            _notyfService.Success("Delete success!");
            return RedirectToAction(nameof(Index));
        }

        private bool AccountExists(int id)
        {
            return _context.Accounts.Any(e => e.AccountId == id);
        }
    }
}
