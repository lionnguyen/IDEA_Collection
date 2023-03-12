using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IDEA_Collection.Models;
using Microsoft.AspNetCore.Http;
using AspNetCoreHero.ToastNotification.Abstractions;
using PagedList.Core;
using System.IO;
using IDEA_Collection.Helpper;

namespace IDEA_Collection.Areas.QADepartment.Controllers
{
    [Area("QADepartment")]
    public class QAIdeasController : Controller
    {
        private readonly CollectIdeasContext _context;
        public INotyfService _notyfService { get; }
        public QAIdeasController(CollectIdeasContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }

        // GET: QADepartment/QAIdeas
        public async Task<IActionResult> Index(int? page)
        {
            var accountQAId = HttpContext.Session.GetString("AccountId");
            if (accountQAId != null)
            {
                var QA = _context.Accounts.AsNoTracking().SingleOrDefault(x => x.AccountId == Convert.ToInt32(accountQAId));
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
            var lsIdeasDepartment = _context.Ideas
                .Include(i => i.Account)
                .Include(i => i.Cat)
                .Where(a => a.Account.DepartmentId == Convert.ToInt32(departmentID))
                .OrderBy(x => x.CreatedDate); ;
            PagedList<Idea> models = new PagedList<Idea>(lsIdeasDepartment, pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }

        // GET: QADepartment/QAIdeas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var accountQAId = HttpContext.Session.GetString("AccountId");
            if (accountQAId != null)
            {
                var QA = _context.Accounts.AsNoTracking().SingleOrDefault(x => x.AccountId == Convert.ToInt32(accountQAId));
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

            var idea = await _context.Ideas
                .Include(i => i.Account)
                .Include(i => i.Cat)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (idea == null)
            {
                return NotFound();
            }

            return View(idea);
        }

        // GET: QADepartment/QAIdeas/Create
        public IActionResult Create()
        {
            var accountQAId = HttpContext.Session.GetString("AccountId");
            if (accountQAId != null)
            {
                var QA = _context.Accounts.AsNoTracking().SingleOrDefault(x => x.AccountId == Convert.ToInt32(accountQAId));
                if (QA != null)
                {
                    var avata = QA.Avatar;
                    var fullname = QA.FullName;
                    ViewBag.avata = avata;
                    ViewBag.fullname = fullname;
                }
            }
            var departmentID = HttpContext.Session.GetString("DepartmentId");
            var accountID = _context.Accounts.Where(a => a.DepartmentId == Convert.ToInt32(departmentID) && a.RoleId != 1002);
            ViewData["AccountId"] = new SelectList(accountID, "AccountId", "FullName");
            ViewData["CatId"] = new SelectList(_context.Categories, "CatId", "CatName");
            return View();
        }

        // POST: QADepartment/QAIdeas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PostId,Title,Scontents,Contents,Thumb,Published,Anonymously,Alias,CreatedDate,Author,AccountId,CatId,Likes,Unlikes")] Idea idea, Microsoft.AspNetCore.Http.IFormFile fThumb)
        {
            var departmentID = HttpContext.Session.GetString("DepartmentId");
            var accountID = _context.Accounts
                .AsNoTracking()
                .Where(a => a.DepartmentId == Convert.ToInt32(departmentID) && a.RoleId != 1002);
            if (ModelState.IsValid)
            {
                if (fThumb != null)
                {
                    string extension = Path.GetExtension(fThumb.FileName);
                    string image = Utilities.SEOUrl("Post-thumb-" + idea.PostId.ToString()) + extension;
                    idea.Thumb = await Utilities.UploadFile(fThumb, @"ideas", image.ToLower());
                }
                idea.CreatedDate = DateTime.Now;
                _context.Add(idea);
                await _context.SaveChangesAsync();
                _notyfService.Success("Create success!");
                return RedirectToAction(nameof(Index));
            }

            ViewData["AccountId"] = new SelectList(accountID, "AccountId", "FullName");
            ViewData["CatId"] = new SelectList(_context.Categories, "CatId", "CatName");
            return View(idea);
        }

        // GET: QADepartment/QAIdeas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var accountQAId = HttpContext.Session.GetString("AccountId");
            if (accountQAId != null)
            {
                var QA = _context.Accounts.AsNoTracking().SingleOrDefault(x => x.AccountId == Convert.ToInt32(accountQAId));
                if (QA != null)
                {
                    var avata = QA.Avatar;
                    var fullname = QA.FullName;
                    ViewBag.avata = avata;
                    ViewBag.fullname = fullname;
                }
            }
            var departmentID = HttpContext.Session.GetString("DepartmentId");
            var accountID = _context.Accounts
                .AsNoTracking()
                .Where(a => a.DepartmentId == Convert.ToInt32(departmentID) && a.RoleId != 1002);
            if (id == null)
            {
                return NotFound();
            }

            var idea = await _context.Ideas.FindAsync(id);
            if (idea == null)
            {
                return NotFound();
            }
            ViewData["AccountId"] = new SelectList(accountID, "AccountId", "FullName", idea.AccountId);
            ViewData["CatId"] = new SelectList(_context.Categories, "CatId", "CatName", idea.CatId);
            return View(idea);
        }

        // POST: QADepartment/QAIdeas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PostId,Title,Scontents,Contents,Thumb,Published,Anonymously,Alias,CreatedDate,Author,AccountId,CatId,Likes,Unlikes")] Idea idea, Microsoft.AspNetCore.Http.IFormFile fThumb)
        {
            var departmentID = HttpContext.Session.GetString("DepartmentId");
            var accountID = _context.Accounts
                .AsNoTracking()
                .Where(a => a.DepartmentId == Convert.ToInt32(departmentID) && a.RoleId != 1002);
            if (id != idea.PostId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (fThumb != null)
                    {
                        string extension = Path.GetExtension(fThumb.FileName);
                        string image = Utilities.SEOUrl("Post-thumb-" + idea.PostId.ToString()) + extension;
                        idea.Thumb = await Utilities.UploadFile(fThumb, @"ideas", image.ToLower());
                    }
                    idea.CreatedDate = DateTime.Now;
                    _context.Update(idea);
                    await _context.SaveChangesAsync();
                    _notyfService.Success("Update success!");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IdeaExists(idea.PostId))
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
            ViewData["AccountId"] = new SelectList(accountID, "AccountId", "FullName", idea.AccountId);
            ViewData["CatId"] = new SelectList(_context.Categories, "CatId", "CatName", idea.CatId);
            return View(idea);
        }

        // GET: QADepartment/QAIdeas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var accountQAId = HttpContext.Session.GetString("AccountId");
            if (accountQAId != null)
            {
                var QA = _context.Accounts.AsNoTracking().SingleOrDefault(x => x.AccountId == Convert.ToInt32(accountQAId));
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

            var idea = await _context.Ideas
                .Include(i => i.Account)
                .Include(i => i.Cat)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (idea == null)
            {
                return NotFound();
            }

            return View(idea);
        }

        // POST: QADepartment/QAIdeas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var idea = await _context.Ideas.FindAsync(id);
            _context.Ideas.Remove(idea);
            await _context.SaveChangesAsync();
            _notyfService.Success("Delete success!");
            return RedirectToAction(nameof(Index));
        }

        private bool IdeaExists(int id)
        {
            return _context.Ideas.Any(e => e.PostId == id);
        }
    }
}
