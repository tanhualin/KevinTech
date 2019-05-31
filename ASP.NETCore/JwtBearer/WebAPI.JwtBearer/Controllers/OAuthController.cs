using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NETCore.Encrypt;
using WebAPI.JwtBearer.Common.Mvc;
using WebAPI.JwtBearer.Models.Common;

namespace WebAPI.JwtBearer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OAuthController : ControllerBaseEx
    {
        private Models.JwtSettings setting;
        public OAuthController(IOptions<Models.JwtSettings> options)
        {
            setting = options.Value;
        }

        [HttpPost]
        public IActionResult Authenticate([FromBody]OAuthModel login)
        {
            SmartHttpResult result = new SmartHttpResult();
            try
            {
                var entity = DAL.SmartUser.GetEntityByName(login.UserName);
                if (entity != null)
                {
                    var encrypted = EncryptProvider.AESEncrypt(login.PassWord, entity.Salt);
                    if (entity.PassWord == encrypted)
                    {
                        //重新加密
                        var Saltkey = Guid.NewGuid().ToString("N");
                        var decrypted = EncryptProvider.AESEncrypt(login.PassWord, Saltkey);
                        //替换密码与密钥
                        DAL.SmartUser.utlSmartUserByName(login.UserName, decrypted, Saltkey);
                        var claims = new Claim[] {
                            new Claim(ClaimTypes.Name, login.UserName),
                            new Claim(ClaimTypes.Role, entity.Role)
                        };
                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(setting.SecretKey));
                        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                        var tokenModel = new JwtSecurityToken(
                            setting.Issuer,
                            setting.Audience,
                            claims,
                            DateTime.Now,
                            DateTime.Now.AddMinutes(setting.TokenExpires),
                            creds);

                        var jwtToken = new JwtTokenModel();
                        jwtToken.Token = new JwtSecurityTokenHandler().WriteToken(tokenModel);

                        var tokenUser = new TokenUserModel();
                        tokenUser.UserName = entity.UserName;
                        tokenUser.Email = entity.Email;
                        tokenUser.Phone = entity.Phone;
                        tokenUser.Avatar = entity.Avatar;
                        jwtToken.User = tokenUser;
                        result.Set(true, jwtToken);

                        return new JsonResult(result);
                    }
                    else
                    {
                        result.Set(false, "用户密码不正确！");
                    }
                }
                else
                {
                    result.Set(false, "用户不存在！");
                }
            }
            catch (Exception err)
            {
                result.Set(false, err.Message);
            }
            return new JsonResult(result);
        }

        //[HttpGet("logout")]
        [HttpDelete]
        public IActionResult Logout()
        {
            SmartHttpResult result = new SmartHttpResult();
            HttpContext.SignOutAsync(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme);
            result.Set(false, "用户已注销！");
            return new JsonResult(result); 
        }
    }
}