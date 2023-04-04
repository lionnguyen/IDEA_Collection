using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IDEA_Collection.Models;
using AspNetCoreHero.ToastNotification.Abstractions;
using PagedList.Core;
using Microsoft.AspNetCore.Http;

namespace CollectIdeas.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminCategoriesController : Controller
    {
        private readonly CollectIdeasContext _context;
        public INotyfService _notyfService { get; }

        public AdminCategoriesController(CollectIdeasContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }

        // GET: Admin/AdminCategories
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
            var lsCategory = _context.Categories
                .AsNoTracking()
                .OrderBy(x => x.CatId);
            PagedList<Category> models = new PagedList<Category>(lsCategory, pageNumber, pageSize);

            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }

        // GET: Admin/AdminCategories/Details/5
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

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.CatId == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Admin/AdminCategories/Create
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
            return View();
        }

        // POST: Admin/AdminCategories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CatId,CatName,Description,Published,Thumb")] Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                _notyfService.Success("Create success!");
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Admin/AdminCategories/Edit/5
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

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Admin/AdminCategories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CatId,CatName,Description,Published,Thumb")] Category category)
        {

            if (id != category.CatId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    _notyfService.Success("Update success!");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.CatId))
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
            return View(category);
        }

        // GET: Admin/AdminCategories/Delete/5
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

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.CatId == id);
            if (category == null)
            {
                return NotFound();
            }
            var idea = await _context.Ideas.FirstOrDefaultAsync(m => m.CatId == id);
            if (idea != null)
            {
                _notyfService.Success("The category in use cannot be deleted!");
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // POST: Admin/AdminCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            _notyfService.Success("Delete success!");
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.CatId == id);
        }
    }
}
