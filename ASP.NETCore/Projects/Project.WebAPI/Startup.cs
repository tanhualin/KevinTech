using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

namespace Project.WebAPI
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
                o.SecurityTokenValidators.Clear();
                o.SecurityTokenValidators.Add(new Common.Helper.AuthTokenValidataHelper());

                o.Events = new JwtBearerEvents()
                {/* TokenValidated：在Token验证通过后调用。
                    AuthenticationFailed: 认证失败时调用。
                    Challenge: 未授权时调用。
                     
                 */
                    //重写OnMessageReceived
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Query != null && context.Request.Query.ContainsKey("_allow_anonymous")
                            && context.Request.Query["_allow_anonymous"].ToString().ToLower() == "true")
                        {
                            return Task.CompletedTask;
                        }
                        else
                        {
                            if (context.Request.Headers.ContainsKey("token"))
                            {
                                var token = context.Request.Headers["token"];
                                context.Token = token.FirstOrDefault();
                                return Task.CompletedTask;
                            }
                            else if (context.Request.Headers.ContainsKey("Authorization") || context.Request.Headers.ContainsKey("Bearer"))
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
                        return c.Response.WriteAsync(Common.SmartJsonHelper.SerializeJSON<Common.Mvc.SmartHttpResult>(result));
                    }
                };
                //o.TokenValidationParameters = new TokenValidationParameters
                //{
                //    ValidateIssuerSigningKey = true,
                //    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(setting.SecretKey)),
                //    ValidateIssuer = true,
                //    ValidIssuer = setting.Issuer,
                //    ValidateAudience = true,
                //    ValidAudience = setting.Audience,
                //    ValidateLifetime = true,
                //    ClockSkew = TimeSpan.Zero
                //}; ;
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
            // Add MVC to the request pipeline.
            app.UseCors(builder =>
                builder.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod());

            app.UseStaticFiles();
            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
