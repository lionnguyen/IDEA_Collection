using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using System.IO;
using IDEA_Collection.Extension;
using AspNetCoreHero.ToastNotification.Abstractions;
using IDEA_Collection.Helpper;
using IDEA_Collection.Models;
using Microsoft.AspNetCore.Http;
using Org.BouncyCastle.Crypto;

namespace IDEA_Collection.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminAccountsController : Controller
    {
        private readonly CollectIdeasContext _context;
        public INotyfService _notyfService { get; }
        public AdminAccountsController(CollectIdeasContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }

        // GET: Admin/AdminAccounts
        public IActionResult Index(int? page)
        {
            var accountID = HttpContext.Session.GetString("AccountId");
            if (accountID != null)
            {
                var admin = _context.Accounts.AsNoTracking().SingleOrDefault(x => x.AccountId == Convert.ToInt32(accountID));
                if (admin != null)
                {
                    var avata = admin.Avatar;
                    var fullname = admin.FullName;
                    ViewBag.avata = avata;
                    ViewBag.fullname = fullname;
                }
            }
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 20;
            var lsAccount = _context.Accounts
                .Include(x => x.Role)
                .Include(x => x.Department)
                .AsNoTracking()
                .OrderBy(x => x.RoleId);
            PagedList<Account> models = new PagedList<Account>(lsAccount, pageNumber, pageSize);

            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }

        // GET: Admin/AdminAccounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var accountID = HttpContext.Session.GetString("AccountId");
            if (accountID != null)
            {
                var admin = _context.Accounts.AsNoTracking().SingleOrDefault(x => x.AccountId == Convert.ToInt32(accountID));
                if (admin != null)
                {
                    var avata = admin.Avatar;
                    var fullname = admin.FullName;
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

        // GET: Admin/AdminAccounts/Create
        public IActionResult Create()
        {
            var accountID = HttpContext.Session.GetString("AccountId");
            if (accountID != null)
            {
                var admin = _context.Accounts.AsNoTracking().SingleOrDefault(x => x.AccountId == Convert.ToInt32(accountID));
                if (admin != null)
                {
                    var avata = admin.Avatar;
                    var fullname = admin.FullName;
                    ViewBag.avata = avata;
                    ViewBag.fullname = fullname;
                }
            }
            ViewData["Department"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName");
            ViewData["Role"] = new SelectList(_context.Roles, "RoleId", "RoleName");
            return View();
        }

        // POST: Admin/AdminAccounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AccountId,Phone,Birthday,Email,Password,Address,Active,FullName,RoleId,DepartmentId,LastLogin,CreateDate,Avatar")] Account account, Microsoft.AspNetCore.Http.IFormFile fThumb)
        {
            if (ModelState.IsValid)
            {
                if (fThumb != null)
                {
                    string extension = Path.GetExtension(fThumb.FileName);
                    string image = Utilities.SEOUrl(account.FullName) + extension;
                    account.Avatar = await Utilities.UploadFile(fThumb, @"avatas", image.ToLower());
                }
                account.Password = account.Password.Trim().ToMD5();
                account.CreateDate = DateTime.Now;
                _context.Add(account);
                await _context.SaveChangesAsync();
                _notyfService.Success("Create success!");
                return RedirectToAction(nameof(Index));
            }
            ViewData["Department"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName");
            ViewData["Role"] = new SelectList(_context.Roles, "RoleId", "RoleName");
            return View(account);
        }

        // GET: Admin/AdminAccounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var accountID = HttpContext.Session.GetString("AccountId");
            if (accountID != null)
            {
                var admin = _context.Accounts.AsNoTracking().SingleOrDefault(x => x.AccountId == Convert.ToInt32(accountID));
                if (admin != null)
                {
                    var avata = admin.Avatar;
                    var fullname = admin.FullName;
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
            ViewData["Department"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName", account.DepartmentId);
            ViewData["Role"] = new SelectList(_context.Roles, "RoleId", "RoleName", account.RoleId);
            return View(account);
        }

        // POST: Admin/AdminAccounts/Edit/5
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
            ViewData["Department"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName", account.DepartmentId);
            ViewData["Role"] = new SelectList(_context.Roles, "RoleId", "RoleName", account.RoleId);
            return View(account);
        }

        // GET: Admin/AdminAccounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var accountID = HttpContext.Session.GetString("AccountId");
            if (accountID != null)
            {
                var admin = _context.Accounts.AsNoTracking().SingleOrDefault(x => x.AccountId == Convert.ToInt32(accountID));
                if (admin != null)
                {
                    var avata = admin.Avatar;
                    var fullname = admin.FullName;
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

        // POST: Admin/AdminAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            var comment = await _context.Comments.AsNoTracking().Where(a => a.AccountId == account.AccountId).ToListAsync();
            foreach (var item in comment)
            {
                _context.Comments.Remove(item);
            }
            var like = await _context.Likes.AsNoTracking().Where(a => a.AccountId == account.AccountId).ToListAsync();
            foreach (var item in like)
            {
                _context.Likes.Remove(item);
            }
            var unlike = await _context.Unlikes.AsNoTracking().Where(a => a.AccountId == account.AccountId).ToListAsync();
            foreach (var item in unlike)
            {
                _context.Unlikes.Remove(item);
            }
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
