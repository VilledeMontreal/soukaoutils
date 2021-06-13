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

        [Authorize, HttpPost("~/")]
        public async Task<ActionResult> Index(CancellationToken cancellationToken)
        {
            var idtoken = await HttpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectParameterNames.IdToken);
            var isPaidUser = HttpContext.User.IsInRole("PaidUser");
            var token = await HttpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectParameterNames.AccessToken);
            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("The access token cannot be found in the authentication ticket. " +
                                                    "Make sure that SaveTokens is set to true in the OIDC options.");
            }

            using var client = _httpClientFactory.CreateClient();

            using var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:6001/api/items");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            var resp = await response.Content.ReadAsStringAsync();

            return View("Home", model: JArray.Parse(resp));
        }

        [Authorize, HttpPost("Add")]
        public async Task<ActionResult> Add(CancellationToken cancellationToken)
        {
            var token = await HttpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectParameterNames.AccessToken);
            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("The access token cannot be found in the authentication ticket. " +
                                                    "Make sure that SaveTokens is set to true in the OIDC options.");
            }

            using var client = _httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:6001/api/items/add");

            //string newItem = string.Format("{{\"Id\":10,\"OwnerId\":1,\"Title\":\"Title\",\"ItemTypeId\":1,\"Description\":\"{0}\",\"Location\":\"Ile-Bizard\"}}", Guid.NewGuid());
            string newItem = string.Format("{{\"Title\":\"Title\",\"Description\":\"{0}\",\"ItemTypeId\":1,\"Location\":\"Ile-Bizard\"}}", Guid.NewGuid());
            request.Content = new StringContent(newItem);
            //request.Content = new StringContent(newItem,);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content.Headers.Remove("Content-Type");
            request.Content.Headers.Add("Content-Type", "application/json");

            using var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var resp = await response.Content.ReadAsStringAsync();

            return View("Home", model: new JArray{JObject.Parse(resp)});
        }

        [Authorize, HttpPost("Clear")]
        public async Task<ActionResult> Clear(CancellationToken cancellationToken)
        {
            var token = await HttpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectParameterNames.AccessToken);
            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("The access token cannot be found in the authentication ticket. " +
                                                    "Make sure that SaveTokens is set to true in the OIDC options.");
            }

            using var client = _httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:6001/api/items/clear");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var resp = await response.Content.ReadAsStringAsync();

            return View("Home", model: null);
        }        

    }
}
