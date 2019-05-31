using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebAPI.Middleware.Middlewares
{
    /// <summary>
    /// 权限中间件
    /// </summary>
    public class PermissionMiddleware
    {
        /// <summary>
        /// 管道代理对象
        /// </summary>
        private readonly RequestDelegate _next;

        /// <summary>
        /// 权限中间件构造
        /// </summary>
        /// <param name="next">管道代理对象</param>
        /// <param name="permissionResitory">权限仓储对象</param>
        /// <param name="option">权限中间件配置选项</param>
        public PermissionMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        /// <summary>
        /// 调用管道
        /// </summary>
        /// <param name="context">请求上下文</param>
        /// <returns></returns>
        public Task Invoke(HttpContext context)
        {
            //是否经过验证
            var isAuthenticated = context.User.Identity.IsAuthenticated;
            if (isAuthenticated)
            {
                //请求Url
                var questUrl = context.Request.Path.Value.ToLower();
                //用户名
                var userName = context.User.Claims.SingleOrDefault(s => s.Type == ClaimTypes.Sid).Value;
                if (questUrl == "/")
                {
                    return this._next(context);
                }
                else
                {
                    //无权限跳转到拒绝页面
                    context.Response.WriteAsync("There is no Permission");
                }
            }
            return this._next(context);
        }
    }

    /// <summary>
    /// 扩展权限中间件
    /// </summary>
    public static class PermissionMiddlewareExtensions
    {
        /// <summary>
        /// 引入权限中间件
        /// </summary>
        /// <param name="builder">扩展类型</param>
        /// <param name="option">权限中间件配置选项</param>
        /// <returns></returns>
        public static IApplicationBuilder UsePermission(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PermissionMiddleware>();
        }
    }
}
