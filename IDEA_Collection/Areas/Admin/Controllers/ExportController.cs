using Microsoft.AspNetCore.Mvc;
using IDEA_Collection.Models;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System;

namespace IDEA_Collection.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ExportController : Controller
    {
        private readonly CollectIdeasContext _context;
        private IHostingEnvironment _IHosting;
        public ExportController(CollectIdeasContext context, IHostingEnvironment IHosting)
        {
            _context = context;
            _IHosting = IHosting;
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
            sb.AppendLine("ID,Contents,Category,Full Name,Status,Like,Unlike,Comments,Create day");
            foreach (var item in lsIdea)
            {
                var likeCount = _context.Likes.Count(x => x.PostId == item.PostId);
                var unlikeCount = _context.Unlikes.Count(x => x.PostId == item.PostId);
                var commentCount = _context.Comments.Count(x => x.PostId == item.PostId);
                if (item.Published == true)
                {
                    sb.AppendLine($"{item.PostId},{item.Contents},{item.Cat.CatName},{item.Account.FullName},Active,{likeCount},{unlikeCount},{commentCount},{item.CreatedDate.Value.ToString("dd. MM. yyyy")}");
                }
                else
                {
                    sb.AppendLine($"{item.PostId},{item.Contents},{item.Cat.CatName},{item.Account.FullName},Pending,{likeCount},{unlikeCount},{commentCount},{item.CreatedDate.Value.ToString("dd. MM. yyyy")}");
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
        [Route("DownLoadZip.html", Name = "DownLoadZip")]
        public FileResult DownLoadZip()
        {
            var webRoot = _IHosting.WebRootPath;
            var fileName = "MyZip.zip";
            var tempOutput = webRoot + "/images/" + fileName;

            using (ZipOutputStream IzipOutputStream = new ZipOutputStream(System.IO.File.Create(tempOutput)))
            {
                IzipOutputStream.SetLevel(9);
                byte[] buffer = new byte[4096];
                var imageList = new List<string>();
                var ideasURL = _context.Ideas.Where(x => x.Thumb != null).ToList();
                foreach (var item in ideasURL)
                {
                    imageList.Add(webRoot + $"/images/ideas/{item.Thumb}");
                }

                for (int i = 0; i < imageList.Count; i++)
                {
                    ZipEntry entry = new ZipEntry(Path.GetFileName(imageList[i]));
                    entry.DateTime = DateTime.Now;
                    entry.IsUnicodeText = true;
                    IzipOutputStream.PutNextEntry(entry);

                    using (FileStream oFileStream = System.IO.File.OpenRead(imageList[i]))
                    {
                        int sourceBytes;
                        do
                        {
                            sourceBytes = oFileStream.Read(buffer, 0, buffer.Length);
                            IzipOutputStream.Write(buffer, 0, sourceBytes);
                        } while (sourceBytes > 0);
                    }
                }
                IzipOutputStream.Finish();
                IzipOutputStream.Flush();
                IzipOutputStream.Close();
            }

            byte[] finalResult = System.IO.File.ReadAllBytes(tempOutput);
            if (System.IO.File.Exists(tempOutput))
            {
                System.IO.File.Delete(tempOutput);
            }
            if (finalResult == null || !finalResult.Any())
            {
                throw new Exception(String.Format("Nothing found"));

            }

            return File(finalResult, "application/zip", fileName);
        }
    }
}
