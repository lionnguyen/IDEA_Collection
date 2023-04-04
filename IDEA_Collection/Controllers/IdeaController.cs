using AspNetCoreHero.ToastNotification.Abstractions;
using IDEA_Collection.Email;
using IDEA_Collection.Helpper;
using IDEA_Collection.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IDEA_Collection.Controllers
{
    public class IdeaController : Controller
    {
        private readonly CollectIdeasContext _context;
        public INotyfService _notyfService { get; }
        private readonly ISendMailService _emailSender;

        public IdeaController(CollectIdeasContext context, INotyfService notyfService, ISendMailService emailSender)
        {
            _context = context;
            _notyfService = notyfService;
            _emailSender = emailSender;

        }
        // GET
        [Route("create-idea.html", Name = "CreateIdea")]
        public IActionResult Create()
        {
            var taikhoanID = HttpContext.Session.GetString("AccountId");
            if (taikhoanID == null)
            {
                return RedirectToAction("Login", "Accounts", new { returnUrl = "/create-idea.html" });
            }
            var taikhoan = _context.Accounts.SingleOrDefault(x => x.AccountId == (Convert.ToInt32(taikhoanID)));
            var department = _context.Departments.SingleOrDefault(x => x.DepartmentId == taikhoan.DepartmentId);
            if (department.ClosureDates <= DateTime.Now)
            {
                _notyfService.Success("The department has ended, you cannot submit more ideas!");
                return RedirectToAction("Index", "Home");
            }
            if (department.StartDates > DateTime.Now)
            {
                _notyfService.Success("The department hasn't started you can't submit ideas!");
                return RedirectToAction("Index", "Home");
            }
            ViewBag.avata = taikhoan.Avatar;
            ViewBag.fullname = taikhoan.FullName;
            ViewData["DanhMuc"] = new SelectList(_context.Categories, "CatId", "CatName");
            return View();
        }

        [Route("create-idea.html", Name = "CreateIdea")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PostId,Title,Scontents,Contents,Thumb,Published,Anonymously,Alias,CreatedDate,Author,AccountId,CatId,Likes,Unlikes,CommentId")] Idea idea, Microsoft.AspNetCore.Http.IFormFile fThumb)
        {
            var taikhoanID = HttpContext.Session.GetString("AccountId");
            if (taikhoanID == null)
            {
                return RedirectToAction("Login", "Accounts", new { returnUrl = "/create-idea.html" });
            }
          
            if (ModelState.IsValid)
            {
                var taikhoan = _context.Accounts.SingleOrDefault(x => x.AccountId == (Convert.ToInt32(taikhoanID)));
                if (fThumb != null)
                {
                    string extension = Path.GetExtension(fThumb.FileName);
                    string image = Utilities.SEOUrl($"Post-id:{idea.PostId}") + extension;
                    idea.Thumb = await Utilities.UploadFile(fThumb, @"ideas", image.ToLower());
                }
                idea.Published = false;
                idea.AccountId = taikhoan.AccountId;
                idea.CreatedDate = DateTime.Now;
                _context.Add(idea);
                await _context.SaveChangesAsync();
                _notyfService.Success("Create success.Wait for the QA to approve the post!");
                var accountDepartment = _context.Accounts.AsNoTracking().Where(x => x.DepartmentId == taikhoan.DepartmentId && x.RoleId == 1002).ToList();

                foreach (var item in accountDepartment)
                {
                    await _emailSender.SendEmailAsync(item.Email, "Notification!", "You have a new post that needs to be approved.");
                }
                return RedirectToAction("Index", "Home", new { Areas = "" });
            }
            ViewData["DanhMuc"] = new SelectList(_context.Categories, "CatId", "CatName", idea.CatId);
            return View(idea);
        }
        // GET
        [Route("edit-idea.html", Name = "EditIdea")]
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

            var idea = await _context.Ideas.FindAsync(id);
            if (idea == null || idea.AccountId != Convert.ToInt32(taikhoanID))
            {
                _notyfService.Success("You cannot update this post!");
                return RedirectToAction("Index", "Home");
            }
            ViewBag.avata = taikhoan.Avatar;
            ViewBag.fullname = taikhoan.FullName;
            ViewData["DanhMuc"] = new SelectList(_context.Categories, "CatId", "CatName", idea.CatId);
            return View(idea);
        }

        // POST
        [Route("edit-idea.html", Name = "EditIdea")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PostId,Title,Scontents,Contents,Thumb,Published,Anonymously,Alias,CreatedDate,Author,AccountId,CatId,Likes,Unlikes,CommentId")] Idea idea, Microsoft.AspNetCore.Http.IFormFile fThumb)
        {
            var taikhoanID = HttpContext.Session.GetString("AccountId");
            if (taikhoanID == null)
            {
                return RedirectToAction("Login", "Accounts");
            }
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
                        string image = Utilities.SEOUrl($"Post-id:{idea.PostId}") + extension;
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
                return RedirectToAction("Index", "Home");
            }
            ViewData["DanhMuc"] = new SelectList(_context.Categories, "CatId", "CatName", idea.CatId);
            return View(idea);
        }

        [Route("delete-idea.html", Name = "DeleteIdea")]
        public async Task<IActionResult> Delete(int? id)
        {
            var taikhoanID = HttpContext.Session.GetString("AccountId");
            if (taikhoanID == null)
            {
                return RedirectToAction("Login", "Accounts");
            }

            var idea = await _context.Ideas
                .Include(i => i.Account)
                .Include(i => i.Cat)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (idea == null || idea.AccountId != Convert.ToInt32(taikhoanID))
            {
                _notyfService.Success("You cannot delete this post!");
                return RedirectToAction("Index", "Home");
            }
            var comment = await _context.Comments.AsNoTracking().Where(a => a.PostId == idea.PostId).ToListAsync();
            foreach (var item in comment)
            {
                _context.Comments.Remove(item);
            }
            var like = await _context.Likes.AsNoTracking().Where(a => a.PostId == idea.PostId).ToListAsync();
            foreach (var item in like)
            {
                _context.Likes.Remove(item);
            }
            var unlike = await _context.Unlikes.AsNoTracking().Where(a => a.PostId == idea.PostId).ToListAsync();
            foreach (var item in unlike)
            {
                _context.Unlikes.Remove(item);
            }
            _context.Ideas.Remove(idea);
            await _context.SaveChangesAsync();
            _notyfService.Success("Delete success!");
            return RedirectToAction("Index", "Home");
        }
        private bool IdeaExists(int id)
        {
            return _context.Ideas.Any(e => e.PostId == id);
        }
    }
}
