using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using MyProject.Models;
using MyProject.Data;
using Microsoft.AspNetCore.Authorization;

namespace MyProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly CmsContext _context;

        public AccountController(CmsContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        { 
            var user = _context.Members.FirstOrDefault(u => u.StudentId == username && u.Password == password);

            if (user != null)
            {

                string realName = user != null ? user.Name:"";

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim("RealName", realName)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Item");
            }
            else
            {
                ViewBag.ErrorMessage = "帳號或密碼錯誤，請重新輸入！";
                return View();
            }
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        // 顯示註冊畫面
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }


        // 接收並處理註冊資料
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("StudentId,Password,Name,Email,Department")] Member member)
        {
            if (ModelState.IsValid)
            {
                // 檢查資料庫是不是已經有這個學號了
                var existingUser = _context.Members.FirstOrDefault(u => u.StudentId == member.StudentId);

                if (existingUser != null)
                {
                    // 如果學號已經存在，就把錯誤訊息丟回畫面
                    ViewBag.ErrorMessage = "這個學號已經註冊過囉！請直接登入。";
                    return View(member);
                }

                // 將新會員存入資料庫
                _context.Add(member);
                await _context.SaveChangesAsync();

                // 註冊成功後，利用 TempData 傳遞成功訊息給登入頁面
                TempData["SuccessMessage"] = "註冊成功！歡迎加入，請使用新帳號登入。";

                // 導向回登入頁面
                return RedirectToAction(nameof(Login));
            }

            // 如果驗證失敗 (例如必填欄位沒填)，就把原本輸入的資料退回去讓使用者重填
            return View(member);
        }


    }
}