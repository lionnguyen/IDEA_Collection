using Microsoft.AspNetCore.Mvc;
using IDEA_Collection.Models;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace IDEA_Collection.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ExportController : Controller
    {
        private readonly CollectIdeasContext _context;
        public ExportController(CollectIdeasContext context)
        {
            _context = context;
        }

        public IActionResult ExportDepartment()
        {
            var lsDepartment = new List<Department>();
            lsDepartment = _context.Departments.AsNoTracking().OrderBy(x => x.DepartmentId).ToList();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("DepartID,DepartName,StartDate,CloseDate");
            foreach (var item in lsDepartment)
            {
                sb.AppendLine($"{item.DepartmentId},{item.DepartmentName},{item.StartDates.Value.ToString("dd. MM. yyyy")},{item.ClosureDates.Value.ToString("dd. MM. yyyy")}");
            }
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "Department.csv");
        }
        public IActionResult ExportAccounts()
        {
            var lsAccount = new List<Account>();
            lsAccount = _context.Accounts.AsNoTracking().Include(x => x.Role).Include(x => x.Department).OrderBy(x => x.RoleId).ToList();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("ID,Full Name,Phone,Email,Birthday,Address,Role Name,Deparment,CreateDate");
            foreach (var item in lsAccount)
            {
                sb.AppendLine($"{item.AccountId},{item.FullName},{item.Phone},{item.Email},{item.Birthday.Value.ToString("dd. MM. yyyy")},{item.Address},{item.Role.RoleName},{item.Department.DepartmentName},{item.CreateDate.Value.ToString("dd. MM. yyyy")}");
            }
            return File(Encoding.Unicode.GetBytes(sb.ToString()), "text/csv", "Accounts.csv");
        }
        public IActionResult ExportCategories()
        {
            var lsCategory = new List<Category>();
            lsCategory = _context.Categories.AsNoTracking().OrderBy(x => x.CatId).ToList();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("CatID,CatName,Description");
            foreach (var item in lsCategory)
            {
                sb.AppendLine($"{item.CatId},{item.CatName},{item.Description}");
            }
            return File(Encoding.Unicode.GetBytes(sb.ToString()), "text/csv", "Categories.csv");
        }
        public IActionResult ExportIdeas()
        {
            var lsIdea = new List<Idea>();
            lsIdea = _context.Ideas.AsNoTracking().Include(x => x.Account).Include(x => x.Cat).OrderByDescending(x => x.CreatedDate).ToList();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("ID,Contents,Category,Full Name,Status,Like,Unlike,Create day");
            foreach (var item in lsIdea)
            {
                if (item.Published == true)
                {
                    sb.AppendLine($"{item.PostId},{item.Contents},{item.Cat.CatName},{item.Account.FullName},Active,{item.Likes},{item.Unlikes},{item.CreatedDate.Value.ToString("dd. MM. yyyy")}");
                }
                else
                {
                    sb.AppendLine($"{item.PostId},{item.Contents},{item.Cat.CatName},{item.Account.FullName},Pending,{item.Likes},{item.Unlikes},{item.CreatedDate.Value.ToString("dd. MM. yyyy")}");
                }

            }
            return File(Encoding.Unicode.GetBytes(sb.ToString()), "text/csv", "Ideas.csv");
        }
        public IActionResult ExportComments()
        {
            var lsComment = new List<Comment>();
            lsComment = _context.Comments.AsNoTracking().Include(x => x.Account).Include(x => x.Post).OrderByDescending(x => x.CreatedDate).ToList();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("ID,Contents,Idea ID,Full Name,Create day");
            foreach (var item in lsComment)
            {
                sb.AppendLine($"{item.PostId},{item.Contents},{item.PostId},{item.Account.FullName},{item.Likes},{item.Unlikes},{item.CreatedDate.Value.ToString("dd. MM. yyyy")}");
            }
            return File(Encoding.Unicode.GetBytes(sb.ToString()), "text/csv", "Comments.csv");
        }
        public IActionResult ExportRoles()
        {
            var lsRoles = new List<Role>();
            lsRoles = _context.Roles.AsNoTracking().OrderBy(x => x.RoleId).ToList();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("ID,Role Name");
            foreach (var item in lsRoles)
            {
                sb.AppendLine($"{item.RoleId},{item.RoleName}");
            }
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "Roles.csv");
        }
    }
}
