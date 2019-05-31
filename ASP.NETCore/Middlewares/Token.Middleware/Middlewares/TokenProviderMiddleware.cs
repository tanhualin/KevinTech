using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Token.Middleware.Middlewares
{
    public class TokenProviderMiddleware
    {
        private readonly RequestDelegate _next;
        private Models.JwtSettings _options;
        public IAuthenticationSchemeProvider Schemes { get; set; }
        public TokenProviderMiddleware(RequestDelegate next,IOptions<Models.JwtSettings> options,IAuthenticationSchemeProvider schemes)
        {
            _next = next;
            _options = options.Value;
            Schemes = schemes;
        }

        /// <summary>
        /// invoke the middleware
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            //
            context.Features.Set<IAuthenticationFeature>(new AuthenticationFeature
            {
                OriginalPath = context.Request.Path,
                OriginalPathBase = context.Request.PathBase
            });
            //获取默认Scheme（或者AuthorizeAttribute指定的Scheme）的AuthenticationHandler
            var handlers = context.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();
            foreach (var scheme in await Schemes.GetRequestHandlerSchemesAsync())
            {
                var handler = await handlers.GetHandlerAsync(context, scheme.Name) as IAuthenticationRequestHandler;
                if (handler != null && await handler.HandleRequestAsync())
                {
                    return;
                }
            }
            var defaultAuthenticate = await Schemes.GetDefaultAuthenticateSchemeAsync();
            if (defaultAuthenticate != null)
            {
                var result = await context.AuthenticateAsync(defaultAuthenticate.Name);
                if (result?.Principal != null)
                {
                    context.User = result.Principal;
                }
            }
            //if (!context.Request.Path.Equals(_options.Path.ToLower(), StringComparison.Ordinal))
            //{
            //    await _next(context);
            //    return;
            //}
            // Request must be POST with Content-Type: application/x-www-form-urlencoded
            if (!context.Request.Method.Equals("POST")
               || !context.Request.HasFormContentType)
            {
                await ReturnBadRequest(context);
                return;
            }
            //await GenerateAuthorizedResult(context);
        }

        private async Task ReturnBadRequest(HttpContext context)
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync(JsonConvert.SerializeObject(new
            {
                Status = false,
                Message = "认证失败"
            }));
        }
    }

    public static class TokenProviderExtensions
    {
        public static IApplicationBuilder UseAuthentication(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            return app.UseMiddleware<TokenProviderMiddleware>();
        }
    }
}
