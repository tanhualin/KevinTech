using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Mvc.JwtBearer.Models;
using Mvc.JwtBearer.Models.Common;
using NETCore.Encrypt;

namespace Mvc.JwtBearer.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
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

        [HttpGet]
        public IActionResult Login(string returnUrl = "")
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost, ActionName("Login")]
        public async Task<IActionResult> LoginPost([FromBody]OAuthModel model)
        {
            var s = HttpContext.Authentication;
            //ViewData["ReturnUrl"] = "/";
            if (ModelState.IsValid)
            {
                var entity = DAL.SmartUser.GetEntityByName(model.UserName);
                if (entity != null)
                {
                    var encrypted = EncryptProvider.AESEncrypt(model.PassWord, entity.Salt);
                    if (entity.PassWord == encrypted)
                    {
                        //重新加密
                        var Saltkey = Guid.NewGuid().ToString("N");
                        var decrypted = EncryptProvider.AESEncrypt(model.PassWord, Saltkey);
                        //替换密码与密钥
                        DAL.SmartUser.utlSmartUserByName(model.UserName, decrypted, Saltkey);
                        //创建用户身份标识
                        var claimsIdentity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                        claimsIdentity.AddClaims(new List<Claim>(){
                            new Claim(ClaimTypes.Sid, model.UserName),
                            new Claim(ClaimTypes.Name, model.UserName),
                            new Claim(ClaimTypes.Role, "admin"),
                        });

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                        return Redirect("/Home/Index");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "帐号或者密码错误。");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "用户不存在。");
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var s = HttpContext.Authentication;
            return Redirect("/Home/Index");
        }
    }
}
