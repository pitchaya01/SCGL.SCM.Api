
using Lazarus.Common.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Lazarus.Common.Authentication
{
    public class BasicAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<BasicAuthMiddleware> _logger;
        private readonly IConfiguration _configuration;
        private readonly List<BasicAuthSetting> _basicAuthSettings;

        public BasicAuthMiddleware(RequestDelegate next, ILogger<BasicAuthMiddleware> logger, IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;
            _basicAuthSettings = new List<BasicAuthSetting>();
            _configuration.Bind("BasicAuthens", _basicAuthSettings);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestPath = "";
            try
            {
                var isUnauthen = false;
                var isProcess = true;
                string authHeaderBasic = context.Request.Headers["Authorization"];

                requestPath = (context.Request.Path.HasValue) ? (context.Request.Path.Value.ToLower().Split("/api")?.Last() ?? "") : "";
                if (requestPath.Contains("swagger"))
                {
                    await _next(context);
                    return;
                }
                if (
                    !context.User.Identity.IsAuthenticated &&
                    authHeaderBasic != null &&
                    authHeaderBasic.StartsWith("Basic") &&
                    _basicAuthSettings != null &&
                    _basicAuthSettings.Any(a => a.Endpoint.ToLower() == requestPath)
                    )
                {
                    var basicAuth = _basicAuthSettings.First(a => a.Endpoint.ToLower() == requestPath);
                    var authHeader = AuthenticationHeaderValue.Parse(authHeaderBasic);
                    var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                    var reqCredentials = Encoding.UTF8.GetString(credentialBytes).Split(':');
                    if (reqCredentials[0] == basicAuth.Username && reqCredentials[1] == basicAuth.Password)
                    {
                        var claims = new[] {
                            new Claim("name", reqCredentials[0]),
                            new Claim(ClaimTypes.Role, "INTERFACE"),
                            new Claim(ClaimTypes.Name, reqCredentials[0])
                        };
                        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Basic"));
                    }
                    else
                    {
                        isUnauthen = true;
                        isProcess = false;
                        _logger.LogError($"Request '{requestPath}' by Username:'{reqCredentials[0]}' is NOT Authorize!!");
                    }
                }
                else
                {
                    isUnauthen = true;
                }

                if (isProcess)
                {
                    await _next(context);
                    if (context.Response.StatusCode != (int)HttpStatusCode.OK)
                    {
                        _logger.LogWarning("{Method} {RequestPath} is Error status {status} Res:{response}",
                            context.Request.Method,
                            requestPath,
                            ((HttpStatusCode)context.Response.StatusCode).ToString(),
                            context.Response.Body.ReadStram()
                            );
                    }
                }
                else
                {
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "text";
                    await context.Response.BodyWriter.WriteAsync(Encoding.ASCII.GetBytes("Username or Password is incorrect!"));
                    await context.Response.Body.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"BasicAuthMiddleware Path:{requestPath}, Error:{ex.GetBaseMessage()}");
                await _next(context);
            }
        }
    }

    public class BasicAuthSetting
    {
        public string Endpoint { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
