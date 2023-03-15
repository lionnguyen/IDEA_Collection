﻿using AspNetCoreHero.ToastNotification.Abstractions;
using IDEA_Collection.Extension;
using IDEA_Collection.Helpper;
using IDEA_Collection.Models;
using IDEA_Collection.ModelViews;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IDEA_Collection.Controllers
{
    [Authorize]
    public class AccountsController : Controller
    {
        private readonly CollectIdeasContext _context;
        public INotyfService _notyfService { get; }
        public AccountsController(CollectIdeasContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ValidatePhone(string Phone)
        {
            try
            {
                var khachhang = _context.Accounts.AsNoTracking().SingleOrDefault(x => x.Phone.ToLower() == Phone.ToLower());
                if (khachhang != null)
                    return Json(data: "Phone number : " + Phone + "used");

                return Json(data: true);

            }
            catch
            {
                return Json(data: true);
            }
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ValidateEmail(string Email)
        {
            try
            {
                var khachhang = _context.Accounts.AsNoTracking().SingleOrDefault(x => x.Email.ToLower() == Email.ToLower());
                if (khachhang != null)
                    return Json(data: "Email : " + Email + " used");
                return Json(data: true);
            }
            catch
            {
                return Json(data: true);
            }
        }

        [Route("my-account.html", Name = "MyAccount")]
        public IActionResult MyAccount()
        {
            var taikhoanID = HttpContext.Session.GetString("AccountId");
            if (taikhoanID != null)
            {
                var myAccount = _context.Accounts
                    .Include(x => x.Role)
                    .Include(x => x.Department)
                    .AsNoTracking()
                    .SingleOrDefault(x => x.AccountId == Convert.ToInt32(taikhoanID));
                if (myAccount != null)
                {
                    var lsIdeas = _context.Ideas
                        .Include(x => x.Account)
                        .Include(x => x.Comments)
                        .Include(x => x.Cat)
                        .AsNoTracking()
                        .Where(x => x.AccountId == myAccount.AccountId)
                        .OrderByDescending(x => x.CreatedDate)
                        .ToList();
                    ViewBag.Ideas = lsIdeas;
                    var avata = myAccount.Avatar;
                    ViewBag.avata = avata;
                    ViewBag.fullname = myAccount.FullName;
                    return View(myAccount);
                }

            }
            return RedirectToAction("Login");
        }
        [HttpGet]
        [AllowAnonymous]
        [Route("dang-ky.html", Name = "DangKy")]
        public IActionResult DangkyTaiKhoan()
        {
            ViewData["Departments"] = new SelectList(_context.Departments.Where(x => x.DepartmentId != 1002), "DepartmentId", "DepartmentName");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("dang-ky.html", Name = "DangKy")]
        public async Task<IActionResult> DangkyTaiKhoan(RegisterViewModel taikhoan)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Account khachhang = new Account
                    {
                        FullName = taikhoan.FullName,
                        Phone = taikhoan.Phone.Trim().ToLower(),
                        Email = taikhoan.Email.Trim().ToLower(),
                        Password = taikhoan.Password.Trim().ToMD5(),
                        Address = taikhoan.Address.Trim().ToLower(),
                        Birthday = taikhoan.Birthday,
                        Avatar = "Default.png",
                        Active = true,
                        CreateDate = DateTime.Now,
                        DepartmentId = taikhoan.DepartmentId,
                        RoleId = 2
                    };
                    try
                    {
                        _context.Add(khachhang);
                        await _context.SaveChangesAsync();
                        //Lưu Session MaKh
                        HttpContext.Session.SetString("AccountId", khachhang.AccountId.ToString());
                        var taikhoanID = HttpContext.Session.GetString("AccountId");

                        //Identity
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name,khachhang.FullName),
                            new Claim("AccountId", khachhang.AccountId.ToString())
                        };
                        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "login");
                        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                        await HttpContext.SignInAsync(claimsPrincipal);
                        _notyfService.Success("Sign Up Success");
                        ViewData["Departments"] = new SelectList(_context.Departments.Where(x => x.DepartmentId != 1002), "DepartmentId", "DepartmentName");
                        return RedirectToAction("Login", "Accounts");
                    }
                    catch
                    {
                        return RedirectToAction("DangkyTaiKhoan", "Accounts");
                    }
                }
                else
                {
                    ViewData["Departments"] = new SelectList(_context.Departments.Where(x => x.DepartmentId != 1002), "DepartmentId", "DepartmentName");
                    return View(taikhoan);
                }
            }
            catch
            {
                return View(taikhoan);
            }
        }
        [AllowAnonymous]
        [Route("dang-nhap.html", Name = "DangNhap")]
        public IActionResult Login(string returnUrl = null)
        {
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            if (taikhoanID != null)
            {
                return RedirectToAction("Dashboard", "Accounts");
            }
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("dang-nhap.html", Name = "DangNhap")]
        public async Task<IActionResult> Login(LoginViewModel customer, string returnUrl)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool isEmail = Utilities.IsValidEmail(customer.UserName);
                    if (!isEmail) return View(customer);

                    var khachhang = _context.Accounts.AsNoTracking().SingleOrDefault(x => x.Email.Trim() == customer.UserName);
                    if (khachhang == null) return RedirectToAction("DangkyTaiKhoan");
                    string pass = customer.Password.ToMD5();
                    if (khachhang.Password != pass)
                    {
                        _notyfService.Success("Login information is incorrect");
                        return View(customer);
                    }
                    //kiem tra xem account co bi disable hay khong

                    if (khachhang.Active == false)
                    {
                        return RedirectToAction("ThongBao", "Accounts");
                    }

                    //Luu Session MaKh
                    HttpContext.Session.SetString("AccountId", khachhang.AccountId.ToString());
                    HttpContext.Session.SetString("RoletId", khachhang.RoleId.ToString());
                    HttpContext.Session.SetString("DepartmentId", khachhang.DepartmentId.ToString());

                    //Identity
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, khachhang.FullName),
                        new Claim("AccountId", khachhang.AccountId.ToString()),
                        new Claim(ClaimTypes.Thumbprint, khachhang.Avatar)
                    };
                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "login");
                    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    await HttpContext.SignInAsync(claimsPrincipal);
                    _notyfService.Success("Logged in successfully");
                    if (khachhang.RoleId == 1)
                    {
                        return RedirectToAction("Index", "Home", new { Area = "Admin" });
                    }
                    else if (khachhang.RoleId == 1002)
                    {
                        return RedirectToAction("Index", "Home", new { Area = "QADepartment" });
                    }
                    if (string.IsNullOrEmpty(returnUrl))
                    {

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {

                        return Redirect(returnUrl);
                    }
                }
            }
            catch
            {

                return RedirectToAction("DangkyTaiKhoan", "Accounts");
            }

            return View(customer);
        }
        [HttpGet]
        [Route("dang-xuat.html", Name = "DangXuat")]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            HttpContext.Session.Remove("AccountId");
            HttpContext.Session.Remove("DepartmentId");
            HttpContext.Session.Remove("RoletId");
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        [AllowAnonymous]
        [Route("edit-profile.html", Name = "EditProfile")]
        public IActionResult EditProfile()
        {
            var taikhoanID = HttpContext.Session.GetString("AccountId");
            if (taikhoanID == null)
            {
                return RedirectToAction("Login", "Accounts");
            }
            var taikhoan = _context.Accounts.SingleOrDefault(x => x.AccountId == (Convert.ToInt32(taikhoanID)));
            ViewBag.avata = taikhoan.Avatar;
            ViewData["Departments"] = new SelectList(_context.Departments.Where(x => x.DepartmentId != 1002), "DepartmentId", "DepartmentName");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("edit-profile.html", Name = "EditProfile")]
        public async Task<IActionResult> EditProfile(EditProfileModel model, Microsoft.AspNetCore.Http.IFormFile fThumb)
        {
            try
            {
                var taikhoanID = HttpContext.Session.GetString("AccountId");
                if (taikhoanID == null)
                {
                    return RedirectToAction("Login", "Accounts");
                }
                else
                {
                    var taikhoan = _context.Accounts.SingleOrDefault(x => x.AccountId == (Convert.ToInt32(taikhoanID)));
                    if (taikhoan == null) return RedirectToAction("Login", "Accounts");
                    if (fThumb != null)
                    {
                        string extension = Path.GetExtension(fThumb.FileName);
                        string image = Utilities.SEOUrl(taikhoan.AccountId.ToString()) + extension;
                        model.Avata = await Utilities.UploadFile(fThumb, @"avatas", image.ToLower());
                    }
                    taikhoan.Address = model.Address;
                    taikhoan.Avatar = model.Avata;
                    taikhoan.Birthday = model.Birthdate;
                    taikhoan.Phone = model.Phone;
                    taikhoan.FullName = model.FullName;
                    taikhoan.DepartmentId = model.DepartmentID;
                    _context.Update(taikhoan);
                    await _context.SaveChangesAsync();
                    _notyfService.Success("Edit success!");
                    ViewBag.avata = model.Avata;
                    ViewData["Departments"] = new SelectList(_context.Departments.Where(x => x.DepartmentId != 1002), "DepartmentId", "DepartmentName");
                    return RedirectToAction("MyAccount", "Accounts");
                }
            }
            catch
            {
                ViewBag.avata = model.Avata;
                _notyfService.Success("Edit failed!");
                return RedirectToAction("MyAccount", "Accounts");
            }
            ViewBag.avata = model.Avata;
            _notyfService.Success("Edit failed!");
            return RedirectToAction("MyAccount", "Accounts");
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("doi-mat-khau.html", Name = "DoiMatKhau")]
        public IActionResult ChangePassword()
        {
            var taikhoanID = HttpContext.Session.GetString("AccountId");
            if (taikhoanID == null)
            {
                return RedirectToAction("Login", "Accounts");
            }
            var taikhoan = _context.Accounts.SingleOrDefault(x => x.AccountId == (Convert.ToInt32(taikhoanID)));
            ViewBag.avata = taikhoan.Avatar;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("doi-mat-khau.html", Name = "DoiMatKhau")]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            try
            {
                var taikhoanID = HttpContext.Session.GetString("AccountId");
                if (taikhoanID == null)
                {
                    return RedirectToAction("Login", "Accounts");
                }
                else
                {
                    var taikhoan = _context.Accounts.SingleOrDefault(x => x.AccountId == (Convert.ToInt32(taikhoanID)));
                    if (taikhoan == null) return RedirectToAction("Index", "Home");
                    model.Avata = taikhoan.Avatar;
                    var pass = model.PasswordNow.Trim().ToMD5();
                    if (pass == taikhoan.Password)
                    {
                        string passnew = model.Password.Trim().ToMD5();
                        taikhoan.Password = passnew;
                        _context.Update(taikhoan);
                        _context.SaveChanges();
                        _notyfService.Success("Change password successfully");
                        ViewBag.avata = model.Avata;
                        return RedirectToAction("Index", "Home", new { Area = "" });
                    }
                }
            }
            catch
            {
                ViewBag.avata = model.Avata;
                _notyfService.Success("Password change failed");
                return RedirectToAction("ChangePassword", "Accounts");
            }
            ViewBag.avata = model.Avata;
            _notyfService.Success("Password change failed");
            return RedirectToAction("ChangePassword", "Accounts");
        }
    }
}
