using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IDEA_Collection.Models;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Http;
using PagedList.Core;
using System.IO;
using IDEA_Collection.Helpper;

namespace IDEA_Collection.Areas.QADepartment.Controllers
{
    [Area("QADepartment")]
    public class QACommentsController : Controller
    {
        private readonly CollectIdeasContext _context;
        public INotyfService _notyfService { get; }
        public QACommentsController(CollectIdeasContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }

        // GET: QADepartment/QAComments
        public async Task<IActionResult> Index(int? page)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 20;
            var departmentID = HttpContext.Session.GetString("DepartmentId");
            var lsCommentsDepartment = _context.Comments
                .Include(i => i.Account)
                .Include(i => i.Post)
                .Where(a => a.Account.DepartmentId == Convert.ToInt32(departmentID))
                .OrderBy(x => x.CreatedDate); ;
            PagedList<Comment> models = new PagedList<Comment>(lsCommentsDepartment, pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }

        // GET: QADepartment/QAComments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
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

        // GET: QADepartment/QAComments/Create
        public IActionResult Create()
        {
            var departmentID = HttpContext.Session.GetString("DepartmentId");
            var accountID = _context.Accounts.Where(a => a.DepartmentId == Convert.ToInt32(departmentID) && a.RoleId != 1002);
            ViewData["AccountId"] = new SelectList(accountID, "AccountId", "FullName");
            ViewData["PostId"] = new SelectList(_context.Ideas, "PostId", "PostId");
            return View();
        }

        // POST: QADepartment/QAComments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CommentId,Contents,Thumb,Published,Alias,CreatedDate,AccountId,Likes,Unlikes,Anonymously,PostId")] Comment comment, Microsoft.AspNetCore.Http.IFormFile fThumb)
        {
            var departmentID = HttpContext.Session.GetString("DepartmentId");
            var accountID = _context.Accounts.Where(a => a.DepartmentId == Convert.ToInt32(departmentID) && a.RoleId != 1002);
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
            ViewData["AccountId"] = new SelectList(accountID, "AccountId", "FullName", comment.AccountId);
            ViewData["PostId"] = new SelectList(_context.Ideas, "PostId", "PostId", comment.PostId);
            return View(comment);
        }

        // GET: QADepartment/QAComments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var departmentID = HttpContext.Session.GetString("DepartmentId");
            var accountID = _context.Accounts
                .AsNoTracking()
                .Where(a => a.DepartmentId == Convert.ToInt32(departmentID) && a.RoleId != 1002);
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            ViewData["AccountId"] = new SelectList(accountID, "AccountId", "FullName", comment.AccountId);
            ViewData["PostId"] = new SelectList(_context.Ideas, "PostId", "PostId", comment.PostId);
            return View(comment);
        }

        // POST: QADepartment/QAComments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CommentId,Contents,Thumb,Published,Alias,CreatedDate,AccountId,Likes,Unlikes,Anonymously,PostId")] Comment comment, Microsoft.AspNetCore.Http.IFormFile fThumb)
        {
            var departmentID = HttpContext.Session.GetString("DepartmentId");
            var accountID = _context.Accounts
                .AsNoTracking()
                .Where(a => a.DepartmentId == Convert.ToInt32(departmentID) && a.RoleId != 1002);
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
            ViewData["AccountId"] = new SelectList(accountID, "AccountId", "FullName", comment.AccountId);
            ViewData["PostId"] = new SelectList(_context.Ideas, "PostId", "PostId", comment.PostId);
            return View(comment);
        }

        // GET: QADepartment/QAComments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
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

        // POST: QADepartment/QAComments/Delete/5
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
