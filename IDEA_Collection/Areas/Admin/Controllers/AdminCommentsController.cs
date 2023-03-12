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
using Microsoft.AspNetCore.Http;

namespace IDEA_Collection.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminCommentsController : Controller
    {
        private readonly CollectIdeasContext _context;
        public INotyfService _notyfService { get; }
        public AdminCommentsController(CollectIdeasContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }

        // GET: Admin/AdminComments
        public async Task<IActionResult> Index(int? page)
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
            var lsComment = _context.Comments
                .Include(c => c.Account)
                .Include(c => c.Post)
                .AsNoTracking()
                .OrderBy(x => x.AccountId);
            PagedList<Comment> models = new PagedList<Comment>(lsComment, pageNumber, pageSize);

            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }

        // GET: Admin/AdminComments/Details/5
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

            var comment = await _context.Comments
                .Include(c => c.Account)
                .Include(c => c.Post)
                .FirstOrDefaultAsync(m => m.CommentId == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // GET: Admin/AdminComments/Create
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
            ViewData["Account"] = new SelectList(_context.Accounts, "AccountId", "FullName");
            ViewData["PostId"] = new SelectList(_context.Ideas, "PostId", "PostId");
            return View();
        }

        // POST: Admin/AdminComments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CommentId,Contents,Thumb,Published,Alias,CreatedDate,AccountId,Likes,Unlikes,Anonymously,PostId")] Comment comment, Microsoft.AspNetCore.Http.IFormFile fThumb)
        {
            if (ModelState.IsValid)
            {
                var account = _context.Accounts.AsNoTracking().FirstOrDefault(c => c.AccountId == comment.AccountId);
                comment.Alias = account.FullName;
                if (fThumb != null)
                {
                    string extension = Path.GetExtension(fThumb.FileName);
                    string image = Utilities.SEOUrl(comment.CommentId.ToString()) + extension;
                    comment.Thumb = await Utilities.UploadFile(fThumb, @"comments", image.ToLower());
                }
                comment.CreatedDate = DateTime.Now;
                _context.Add(comment);
                await _context.SaveChangesAsync();
                _notyfService.Success("Create success!");
                return RedirectToAction(nameof(Index));
            }
            ViewData["Account"] = new SelectList(_context.Accounts, "AccountId", "FullName");
            ViewData["PostId"] = new SelectList(_context.Ideas, "PostId", "PostId");
            return View(comment);
        }

        // GET: Admin/AdminComments/Edit/5
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

            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            ViewData["AccountId"] = new SelectList(_context.Accounts, "AccountId", "AccountId", comment.AccountId);
            ViewData["PostId"] = new SelectList(_context.Ideas, "PostId", "PostId", comment.PostId);
            return View(comment);
        }

        // POST: Admin/AdminComments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CommentId,Contents,Thumb,Published,Alias,CreatedDate,AccountId,Likes,Unlikes,Anonymously,PostId")] Comment comment, Microsoft.AspNetCore.Http.IFormFile fThumb)
        {
            if (id != comment.CommentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var account = _context.Accounts.AsNoTracking().FirstOrDefault(c => c.AccountId == comment.AccountId);
                    comment.Alias = account.FullName;
                    if (fThumb != null)
                    {
                        string extension = Path.GetExtension(fThumb.FileName);
                        string image = Utilities.SEOUrl(comment.CommentId.ToString()) + extension;
                        comment.Thumb = await Utilities.UploadFile(fThumb, @"comments", image.ToLower());
                    }
                    comment.CreatedDate = DateTime.Now;
                    _context.Update(comment);
                    await _context.SaveChangesAsync();
                    _notyfService.Success("Update success!");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentExists(comment.CommentId))
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
            ViewData["AccountId"] = new SelectList(_context.Accounts, "AccountId", "AccountId", comment.AccountId);
            ViewData["PostId"] = new SelectList(_context.Ideas, "PostId", "PostId", comment.PostId);
            return View(comment);
        }

        // GET: Admin/AdminComments/Delete/5
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

            var comment = await _context.Comments
                .Include(c => c.Account)
                .Include(c => c.Post)
                .FirstOrDefaultAsync(m => m.CommentId == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // POST: Admin/AdminComments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            _notyfService.Success("Delete success!");
            return RedirectToAction(nameof(Index));
        }

        private bool CommentExists(int id)
        {
            return _context.Comments.Any(e => e.CommentId == id);
        }
    }
}
