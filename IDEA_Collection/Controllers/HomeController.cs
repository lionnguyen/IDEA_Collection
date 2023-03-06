using AspNetCoreHero.ToastNotification.Abstractions;
using AspNetCoreHero.ToastNotification.Notyf;
using DocumentFormat.OpenXml.InkML;
using IDEA_Collection.Models;
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
        public INotyfService _notyfService { get; }

        public HomeController(ILogger<HomeController> logger, CollectIdeasContext context, INotyfService notyfService)
        {
            _context = context;
            _logger = logger;
            _notyfService = notyfService;

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
            List<Idea> lsIdeas = new List<Idea>();
            List<Comment> lsComment = new List<Comment>();
            if (filterID != 0)
            {
                lsIdeas = _context.Ideas
                   .Where(x => x.Published == true)
                   .Include(x => x.Account)
                   .Include(x => x.Comments)
                   .AsNoTracking()
                   .OrderByDescending(x => x.Likes).ToList();
            }

            lsIdeas = _context.Ideas
                     .Where(x => x.Published == true)
                     .Include(x => x.Account)
                     .Include(x => x.Comments)
                     .AsNoTracking()
                     .OrderByDescending(x => x.CreatedDate).ToList();
            lsComment = _context.Comments.ToList();
            List<Like> lsLike = _context.Likes.ToList();
            List<Unlike> lsUnlike = _context.Unlikes.ToList();

            PagedList<Idea> models = new PagedList<Idea>(lsIdeas.AsQueryable(), pageNumber, pageSize);

            ViewBag.ListComment = lsComment;
            ViewBag.ListLike = lsLike;
            ViewBag.ListUnlike = lsUnlike;
            ViewBag.CurrentFilterID = filterID;
            ViewBag.CurrentPage = pageNumber;
            return View(models);
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
