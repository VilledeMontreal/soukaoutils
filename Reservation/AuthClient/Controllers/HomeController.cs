using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AuthClient.Models;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using System.Threading;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Text;
using Newtonsoft.Json.Linq;

namespace AuthClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(IHttpClientFactory httpClientFactory)
            => _httpClientFactory = httpClientFactory;

        [HttpGet("~/")]
        public ActionResult Index() => View("Home");

        [HttpGet]
        [Route("/Account/AccessDenied")]
        public ActionResult AccessDenied()
        {
            return View(new AccessDeniedViewModel
                {
                    APIResourceUrl = "TestURL",
                    Exception = new HttpRequestException("Test exception")
                });
        }
    }
}
