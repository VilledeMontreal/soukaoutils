using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;

namespace AuthClient.Controllers
{
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ReservationsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // GET: ItemsController
        public async Task<ActionResult> Index()
        {
            return await GetAll();
        }

        public async Task<ActionResult> GetAll()
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

            using var request = new HttpRequestMessage(HttpMethod.Get, _configuration["Variables:API_URL"] + "/api/Reservations");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var resp = await response.Content.ReadAsStringAsync();

            return View("ReservationsMenu", model: JArray.Parse(resp));
        }

        public async Task<ActionResult> Add(int itemId, string startDate, string endDate)
        {
            var token = await HttpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectParameterNames.AccessToken);
            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("The access token cannot be found in the authentication ticket. " +
                                                    "Make sure that SaveTokens is set to true in the OIDC options.");
            }

            using var client = _httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, _configuration["Variables:API_URL"] + "/api/reservations/add");

            string newItem = string.Format("{{" +
                "\"ItemId\":\"" + itemId + "\"," +
                "\"StartDate\":\"" + DateTime.Parse(startDate).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'") + "\"," +       // Start Date
                "\"EndDate\":\"" + DateTime.Parse(endDate).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'") + "\"" +            // End Date
                "}}", Guid.NewGuid());
            request.Content = new StringContent(newItem);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content.Headers.Remove("Content-Type");
            request.Content.Headers.Add("Content-Type", "application/json");

            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var resp = await response.Content.ReadAsStringAsync();

            return await GetAll();
        }

        public async Task<ActionResult> Cancel(int id)
        {
            var token = await HttpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectParameterNames.AccessToken);
            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("The access token cannot be found in the authentication ticket. " +
                                                    "Make sure that SaveTokens is set to true in the OIDC options.");
            }

            using var client = _httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, _configuration["Variables:API_URL"] + "/api/reservations/cancel/" + id);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var resp = await response.Content.ReadAsStringAsync();

            return await GetAll();
        }
    }
}
