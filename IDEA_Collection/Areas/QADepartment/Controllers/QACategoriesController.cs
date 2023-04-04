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

namespace IDEA_Collection.Areas.QADepartment.Controllers
{
    [Area("QADepartment")]
    public class QACategoriesController : Controller
    {
        private readonly CollectIdeasContext _context;
        public INotyfService _notyfService { get; }
        public QACategoriesController(CollectIdeasContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }

        // GET: QADepartment/QACategories
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
            var lsCategory = _context.Categories
                .AsNoTracking()
                .OrderBy(x => x.CatId);
            PagedList<Category> models = new PagedList<Category>(lsCategory, pageNumber, pageSize);

            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }

        // GET: QADepartment/QACategories/Details/5
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

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.CatId == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: QADepartment/QACategories/Create
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
            return View();
        }

        // POST: QADepartment/QACategories/Create
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

        // GET: QADepartment/QACategories/Edit/5
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

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: QADepartment/QACategories/Edit/5
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

        // GET: QADepartment/QACategories/Delete/5
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

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.CatId == id);
            if (category == null)
            {
                return NotFound();
            }
            var idea = await _context.Ideas.FirstOrDefaultAsync(m => m.CatId == id);
            if(idea != null)
            {
                _notyfService.Success("The category in use cannot be deleted!");
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // POST: QADepartment/QACategories/Delete/5
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
