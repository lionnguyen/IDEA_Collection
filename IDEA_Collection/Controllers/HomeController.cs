using AspNetCoreHero.ToastNotification.Abstractions;
using DocumentFormat.OpenXml.InkML;
using IDEA_Collection.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PagedList.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace IDEA_Collection.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly CollectIdeasContext _context;
        private IHostingEnvironment _IHosting;
        public INotyfService _notyfService { get; }

        public HomeController(ILogger<HomeController> logger, CollectIdeasContext context, INotyfService notyfService, IHostingEnvironment IHosting)
        {
            _context = context;
            _logger = logger;
            _notyfService = notyfService;
            _IHosting = IHosting;

        }

        public IActionResult Index(int page = 1, int filterID = 0)
        {
            var accountID = HttpContext.Session.GetString("AccountId");
            if (accountID != null)
            {
                var staff = _context.Accounts.AsNoTracking().SingleOrDefault(x => x.AccountId == Convert.ToInt32(accountID));
                if (staff != null)
                {
                    var avata = staff.Avatar;
                    var fullname = staff.FullName;
                    ViewBag.avata = avata;
                    ViewBag.fullname = fullname;
                }
            }
            var pageNumber = page;
            var pageSize = 5;
            var itemComments = new List<Comment>();
            var lsIdeas = _context.Ideas.ToList();
            List<Comment> lsComment = _context.Comments.ToList();
            List<Like> lsLike = _context.Likes.ToList();
            List<Unlike> lsUnlike = _context.Unlikes.ToList();
            foreach (var item in lsIdeas)
            {
                item.Likes = lsLike.Count(x => x.PostId == item.PostId);
                item.Unlikes = lsUnlike.Count(x => x.PostId == item.PostId);
                _context.Update(item);
                _context.SaveChanges();
            }
            if (filterID != 0)
            {
                lsIdeas = _context.Ideas
                   .Where(x => x.Published == true)
                   .Include(x => x.Account)
                   .Include(x => x.Comments)
                   .AsNoTracking()
                   .OrderByDescending(x => x.Likes).ToList();
            }
            else
            {
                lsIdeas = _context.Ideas
                         .Where(x => x.Published == true)
                         .Include(x => x.Account)
                         .Include(x => x.Comments)
                         .AsNoTracking()
                         .OrderByDescending(x => x.CreatedDate).ToList();
            }
            PagedList<Idea> models = new PagedList<Idea>(lsIdeas.AsQueryable(), pageNumber, pageSize);

            ViewBag.ListComment = lsComment;
            ViewBag.ListLike = lsLike;
            ViewBag.ListUnlike = lsUnlike;
            ViewBag.CurrentFilterID = filterID;
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }
        public IActionResult Filter(int filterID = 0)
        {
            var url = $"/Home?filterID={filterID}";
            if (filterID == 0)
            {
                url = $"/Home";
            }
            return Json(new { status = "success", redirectUrl = url });
        }

        public async Task<IActionResult> LikePost(int id, [Bind("LikeId,PostId,AccountId")] Like like)
        {
            var taikhoanID = HttpContext.Session.GetString("AccountId");

            if (taikhoanID == null)
            {
                return RedirectToAction("Login", "Accounts");
            }
            var staff = _context.Accounts.AsNoTracking().SingleOrDefault(x => x.AccountId == Convert.ToInt32(taikhoanID));
            var idea = _context.Ideas.AsNoTracking().FirstOrDefault(x => x.PostId == id);
            var likePost = _context.Likes.FirstOrDefault(x => x.PostId == idea.PostId && x.AccountId == staff.AccountId);
            var unlikePost = _context.Unlikes.FirstOrDefault(x => x.PostId == idea.PostId && x.AccountId == staff.AccountId);
            if (likePost != null)
            {
                _context.Remove(likePost);
                await _context.SaveChangesAsync();
                _notyfService.Success("You have unliked the post!");
                return RedirectToAction("Index", "Home");
            }
            if (unlikePost != null)
            {
                _context.Remove(unlikePost);

            }
            like.PostId = idea.PostId;
            like.AccountId = staff.AccountId;
            _context.Add(like);
            await _context.SaveChangesAsync();
            _notyfService.Success("You liked the post!");
            return RedirectToAction("Index", "Home");
        }
        public async Task<IActionResult> UnlikePost(int id, [Bind("UnlikeId,PostId,AccountId")] Unlike unlike)
        {
            var taikhoanID = HttpContext.Session.GetString("AccountId");

            if (taikhoanID == null)
            {
                return RedirectToAction("Login", "Accounts");
            }
            var staff = _context.Accounts.AsNoTracking().SingleOrDefault(x => x.AccountId == Convert.ToInt32(taikhoanID));
            var idea = _context.Ideas.AsNoTracking().FirstOrDefault(x => x.PostId == id);
            var likePost = _context.Likes.FirstOrDefault(x => x.PostId == idea.PostId && x.AccountId == staff.AccountId);
            var unlikePost = _context.Unlikes.FirstOrDefault(x => x.PostId == idea.PostId && x.AccountId == staff.AccountId);
            if (likePost != null)
            {
                _context.Remove(likePost);
            }
            if (unlikePost != null)
            {
                _context.Remove(unlikePost);
                await _context.SaveChangesAsync();
                _notyfService.Success("You remove dislike  this post!");
                return RedirectToAction("Index", "Home");
            }
            unlike.PostId = idea.PostId;
            unlike.AccountId = staff.AccountId;
            _context.Add(unlike);
            await _context.SaveChangesAsync();
            _notyfService.Success("You don't like the post!");
            return RedirectToAction("Index", "Home");
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
