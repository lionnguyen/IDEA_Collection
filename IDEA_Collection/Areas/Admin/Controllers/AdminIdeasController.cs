using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IDEA_Collection.Models;
using PagedList.Core;
using System.IO;
using IDEA_Collection.Helpper;
using AspNetCoreHero.ToastNotification.Abstractions;

namespace IDEA_Collection.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminIdeasController : Controller
    {
        private readonly CollectIdeasContext _context;
        public INotyfService _notyfService { get; }
        public AdminIdeasController(CollectIdeasContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }

        // GET: Admin/AdminIdeas
        public async Task<IActionResult> Index(int? page)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 20;
            var lsIdea = _context.Ideas
                .Include(x => x.Account)
                .Include(x => x.Cat)
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedDate);
            PagedList<Idea> models = new PagedList<Idea>(lsIdea, pageNumber, pageSize);

            ViewData["DanhMuc"] = new SelectList(_context.Categories, "CatId", "CatName");
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }

        // GET: Admin/AdminIdeas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
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

        // GET: Admin/AdminIdeas/Create
        public IActionResult Create()
        {
            ViewData["DanhMuc"] = new SelectList(_context.Categories, "CatId", "CatName");
            ViewData["CommentId"] = new SelectList(_context.Comments, "CommentId", "CommentId");
            ViewData["Account"] = new SelectList(_context.Accounts, "AccountId", "FullName");
            return View();
        }

        // POST: Admin/AdminIdeas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PostId,Title,Scontents,Contents,Thumb,Published,Anonymously,Alias,CreatedDate,Author,AccountId,CatId,Likes,Unlikes,CommentId")] Idea idea, Microsoft.AspNetCore.Http.IFormFile fThumb)
        {
            if (ModelState.IsValid)
            {
                if (fThumb != null)
                {
                    string extension = Path.GetExtension(fThumb.FileName);
                    string image = Utilities.SEOUrl(idea.PostId.ToString()) + extension;
                    idea.Thumb = await Utilities.UploadFile(fThumb, @"ideas", image.ToLower());
                }
                idea.CreatedDate = DateTime.Now;
                _context.Add(idea);
                await _context.SaveChangesAsync();
                _notyfService.Success("Create success!");
                return RedirectToAction(nameof(Index));
            }
            ViewData["DanhMuc"] = new SelectList(_context.Categories, "CatId", "CatName", idea.CatId);
            return View(idea);
        }

        // GET: Admin/AdminIdeas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var idea = await _context.Ideas.FindAsync(id);
            if (idea == null)
            {
                return NotFound();
            }
            ViewData["DanhMuc"] = new SelectList(_context.Categories, "CatId", "CatName", idea.CatId);
            return View(idea);
        }

        // POST: Admin/AdminIdeas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PostId,Title,Scontents,Contents,Thumb,Published,Anonymously,Alias,CreatedDate,Author,AccountId,CatId,Likes,Unlikes,CommentId")] Idea idea, Microsoft.AspNetCore.Http.IFormFile fThumb)
        {
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
                        string image = Utilities.SEOUrl(idea.PostId.ToString()) + extension;
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
            ViewData["DanhMuc"] = new SelectList(_context.Categories, "CatId", "CatName", idea.CatId);
            return View(idea);
        }

        // GET: Admin/AdminIdeas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
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

        // POST: Admin/AdminIdeas/Delete/5
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
