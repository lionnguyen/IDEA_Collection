using AspNetCoreHero.ToastNotification.Abstractions;
using IDEA_Collection.Email;
using IDEA_Collection.Helpper;
using IDEA_Collection.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.IO;

namespace IDEA_Collection.Controllers
{
    public class CommentController : Controller
    {

        private readonly CollectIdeasContext _context;
        public INotyfService _notyfService { get; }
        private readonly ISendMailService _emailSender;

        public CommentController(CollectIdeasContext context, INotyfService notyfService, ISendMailService emailSender)
        {
            _context = context;
            _notyfService = notyfService;
            _emailSender = emailSender;
        }
        [HttpPost]
        public async Task<IActionResult> Comment(int? id)
        {
            try
            {
                var idea = _context.Ideas.AsNoTracking().SingleOrDefault(x => x.PostId == id);
                if (idea == null) return NotFound();
                var comments = _context.Comments
                    .Include(x => x.Account)
                    .Include(x => x.Post)
                    .AsNoTracking()
                    .Where(x => x.PostId == id && x.Published == true)
                    .OrderByDescending(x => x.CreatedDate)
                    .ToList();
                ViewBag.CommentsCounts = comments.Count();
                return PartialView("Comment", comments);
            }
            catch
            {
                return NotFound();
            }
        }
        [Route("create-comment.html", Name = "CreateComment")]
        public IActionResult Create(int? idPost)
        {
            var taikhoanID = HttpContext.Session.GetString("AccountId");

            if (taikhoanID == null)
            {
                return RedirectToAction("Login", "Accounts");
            }
            var taikhoan = _context.Accounts.SingleOrDefault(x => x.AccountId == (Convert.ToInt32(taikhoanID)));
            var idea = _context.Ideas
                      .FirstOrDefault(x => x.PostId == idPost);
            if (idea == null) return NotFound();
            ViewBag.Ideas = idea.Contents;
            ViewBag.ideasId = idea.PostId;
            ViewBag.avata = taikhoan.Avatar;
            return View();
        }

        // POST: Admin/AdminComments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Route("create-comment.html", Name = "CreateComment")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int idPost, [Bind("CommentId,Contents,Thumb,Published,Alias,CreatedDate,AccountId,Likes,Unlikes,Anonymously,PostId")] Comment comment, Microsoft.AspNetCore.Http.IFormFile fThumb)
        {
            var taikhoanID = HttpContext.Session.GetString("AccountId");
            if (taikhoanID == null)
            {
                return RedirectToAction("Login", "Accounts");
            }
            var idea = _context.Ideas
                 .FirstOrDefault(x => x.PostId == idPost);
            if (idea == null) return NotFound();
            if (ModelState.IsValid)
            {
                var taikhoan = _context.Accounts.SingleOrDefault(x => x.AccountId == (Convert.ToInt32(taikhoanID)));

                if (fThumb != null)
                {
                    string extension = Path.GetExtension(fThumb.FileName);
                    string image = Utilities.SEOUrl("Post-comment" + comment.CommentId.ToString()) + extension;
                    comment.Thumb = await Utilities.UploadFile(fThumb, @"comments", image.ToLower());
                }
                comment.AccountId = taikhoan.AccountId;
                comment.Published = true;
                comment.PostId = idea.PostId;
                comment.CreatedDate = DateTime.Now;
                _context.Add(comment);
                await _context.SaveChangesAsync();
                _notyfService.Success("Create success!");
                await _emailSender.SendEmailAsync(idea.Account.Email, "Notification!", "Your post has a new comment");
                ViewBag.Ideas = idea.Contents;
                ViewBag.avata = taikhoan.Avatar;
                ViewBag.ideasId = idea.PostId;
                return RedirectToAction("Index", "Home");
            }
            return View(comment);
        }
        [Route("edit-comment.html", Name = "EditComment")]
        public async Task<IActionResult> Edit(int? id)
        {
            var taikhoanID = HttpContext.Session.GetString("AccountId");
            if (taikhoanID == null)
            {
                return RedirectToAction("Login", "Accounts");
            }
            var taikhoan = _context.Accounts.SingleOrDefault(x => x.AccountId == (Convert.ToInt32(taikhoanID)));
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments.FindAsync(id);
            if (comment == null || comment.AccountId != Convert.ToInt32(taikhoanID))
            {
                _notyfService.Success("You cannot update this comment!");
                return RedirectToAction("Index", "Home");
            }
            var idea = _context.Comments
                .Include(x => x.Post)
                .FirstOrDefault(x => x.CommentId == id);
            ViewBag.Ideas = idea.Post.Contents;
            ViewBag.avata = taikhoan.Avatar;
            return View(comment);
        }

        // POST: Admin/AdminComments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Route("edit-comment.html", Name = "EditComment")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CommentId,Contents,Thumb,Published,Alias,CreatedDate,AccountId,Likes,Unlikes,Anonymously,PostId")] Comment comment, Microsoft.AspNetCore.Http.IFormFile fThumb)
        {
            var taikhoanID = HttpContext.Session.GetString("AccountId");
            if (taikhoanID == null)
            {
                return RedirectToAction("Login", "Accounts");
            }
            if (id != comment.CommentId)
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
                return RedirectToAction("Index", "Home");
            }
            return View(comment);
        }


        [Route("delete-comment.html", Name = "DeleteComment")]
        public async Task<IActionResult> Delete(int id)
        {
            var taikhoanID = HttpContext.Session.GetString("AccountId");
            if (taikhoanID == null)
            {
                return RedirectToAction("Login", "Accounts");
            }
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null || comment.AccountId != Convert.ToInt32(taikhoanID))
            {
                _notyfService.Success("You cannot delete this comment!");
                return RedirectToAction("Index", "Home");
            }
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            _notyfService.Success("Delete success!");
            return RedirectToAction("Index", "Home");
        }
        private bool CommentExists(int id)
        {
            return _context.Comments.Any(e => e.CommentId == id);
        }

    }
}
