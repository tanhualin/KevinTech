using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace WebAPI.JwtBearer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            DAL.BaseDAL.SqlConnString = Configuration.GetConnectionString("DefaultConnection");

            services.Configure<Models.JwtSettings>(Configuration.GetSection("JwtSettings"));
            Models.JwtSettings setting = new Models.JwtSettings();
            Configuration.Bind("JwtSettings", setting);

            services.AddAuthentication(options =>
            {
                //options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme; // 使用 Cookie 认证方式
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.Events = new JwtBearerEvents()
                {/* TokenValidated：在Token验证通过后调用。
                    AuthenticationFailed: 认证失败时调用。
                    Challenge: 未授权时调用。
                 */
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Query != null && context.Request.Query.ContainsKey("_allow_anonymous")
                            && context.Request.Query["_allow_anonymous"].ToString().ToLower() == "true")
                        {
                            return Task.CompletedTask;
                        }
                        else
                        {
                            if (context.Request.Headers.ContainsKey("Authorization") || context.Request.Headers.ContainsKey("Bearer"))
                            {
                                return Task.CompletedTask;
                            }
                            else
                            {
                                context.NoResult();
                                context.Response.StatusCode = 401;
                                Common.Mvc.SmartHttpResult result = new Common.Mvc.SmartHttpResult();
                                result.status = false;
                                result.msg = "There is no Token";
                                return context.Response.WriteAsync(Common.SmartJsonHelper.SerializeJSON<Common.Mvc.SmartHttpResult>(result));
                            }
                        }
                    },
                    OnAuthenticationFailed = c =>
                    {
                        c.NoResult();
                        c.Response.StatusCode = 401;

                        Common.Mvc.SmartHttpResult result = new Common.Mvc.SmartHttpResult();
                        result.status = false;
                        result.msg = "Invalid Token Failure";
                        return  c.Response.WriteAsync(Common.SmartJsonHelper.SerializeJSON<Common.Mvc.SmartHttpResult>(result));
                    }
                };
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(setting.SecretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = setting.Issuer,
                    ValidateAudience = true,
                    ValidAudience = setting.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero

                    /***********************************TokenValidationParameters的参数默认值***********************************/
                    // RequireSignedTokens = true,
                    // SaveSigninToken = false,
                    // ValidateActor = false,
                    // 将下面两个参数设置为false，可以不验证Issuer和Audience，但是不建议这样做。
                    // ValidateAudience = true,
                    // ValidateIssuer = true, 
                    // ValidateIssuerSigningKey = false,
                    // 是否要求Token的Claims中必须包含Expires
                    // RequireExpirationTime = true,
                    // 允许的服务器时间偏移量
                    // ClockSkew = TimeSpan.FromSeconds(300),
                    // 是否验证Token有效期，使用当前时间与Token的Claims中的NotBefore和Expires对比
                    // ValidateLifetime = true
                }; ;
            });
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
