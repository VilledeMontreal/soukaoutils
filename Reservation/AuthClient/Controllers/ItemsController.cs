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
using System.Collections.Generic;
using AuthClient.Models;
using Microsoft.Identity.Web.Resource;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace AuthClient.Controllers
{
    [Authorize]
    public class ItemsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ItemsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // GET: ItemsController
        public async Task<ActionResult> Index()
        {
            try
            {
                return await GetAll();
            }
            catch (System.Exception ex)
            {
                if(ex is HttpRequestException)
                {
                    HttpRequestException httpEx = ex as HttpRequestException;

                    if(httpEx.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        return View("AccessDenied", new AccessDeniedViewModel
                        {
                            Title = "Forbidden",
                            Message = "Consetement d'accès requis",
                            APIResourceUrl = _configuration["Variables:API_URL"] + "/api/items/getMyId",
                            Exception = httpEx
                        });

                    }
                    if(httpEx.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return View("AccessDenied", new AccessDeniedViewModel
                        {
                            Title = "Unauthorized",
                            Message = "Consetement d'accès requis",
                            APIResourceUrl = _configuration["Variables:API_URL"] + "/api/items/getMyId",
                            Exception = httpEx
                        });

                    }
                }
                throw;
            }
        }

        public async Task<ActionResult> GetAll()
        {
            string token = await GetToken();

            // Get the user's id
            int userId = await GetUserId(token);

            // Get the items
            JArray items = await GetItems(token);

            // Mark the items that belong to the user
            for (int i = 0; i < items.Count; i++)
            {
                if (((int)items[i]["ownerId"]) == userId)
                {
                    items[i]["Mine"] = true;
                }
                else
                {
                    items[i]["Mine"] = false;
                }
            }

            // Get the item types
            Dictionary<int, string> itemTypes = await GetItemTypes(token);

            // Prepare the model
            ItemViewModel itemViewModel = new ItemViewModel(items, itemTypes);

            return View("ItemsMenu", model: itemViewModel);
        }

        public async Task<ActionResult> AddOld()
        {
            string token = await GetToken();

            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, _configuration["Variables:API_URL"] + "/api/items/add");

            string newItem = string.Format("{{\"Title\":\"Title\",\"Description\":\"{0}\",\"ItemTypeId\":1,\"Location\":\"Ile-Bizard\"}}", Guid.NewGuid());
            request.Content = new StringContent(newItem);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content.Headers.Remove("Content-Type");
            request.Content.Headers.Add("Content-Type", "application/json");

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var resp = await response.Content.ReadAsStringAsync();

            return await GetAll();
        }

        public async Task<ActionResult> Clear()
        {
            string token = await GetToken();

            using var client = _httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, _configuration["Variables:API_URL"] + "/api/items/clear");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var resp = await response.Content.ReadAsStringAsync();

            return await Index();
        }

        public async Task<ActionResult> WithdrawAll()
        {
            try
            {
                string token = await GetToken();

                using var client = _httpClientFactory.CreateClient();
                using var request = new HttpRequestMessage(HttpMethod.Get, _configuration["Variables:API_URL"] + "/api/items/withdrawall");

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var resp = await response.Content.ReadAsStringAsync();

                return await GetAll();
            }

            catch (System.Exception ex)
            {
                if(ex is HttpRequestException)
                {
                    HttpRequestException httpEx = ex as HttpRequestException;

                    if(httpEx.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        return View("AccessDenied", new AccessDeniedViewModel
                        {
                            Title = "Forbidden",
                            Message = "Rôle d'admin requis",
                            APIResourceUrl = _configuration["Variables:API_URL"] + "/api/items/withdrawall",
                            Exception = httpEx
                        });

                    }
                }
                throw;

            }

        }

        public async Task<ActionResult> Withdraw(int id)
        {
            string token = await GetToken();

            using var client = _httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, _configuration["Variables:API_URL"] + "/api/items/withdraw/" + id);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var resp = await response.Content.ReadAsStringAsync();

            return await GetAll();
        }
        
        public async Task<ActionResult> Return(int id)
        {
            string token = await GetToken();

            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, _configuration["Variables:API_URL"] + "/api/items/return/" + id);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var resp = await response.Content.ReadAsStringAsync();

            return await GetAll();
        }

        public async Task<ActionResult> Add(string title, int itemTypeId, string description, byte[] picture, string location, float dailyFee)
        {
            string token = await GetToken();

            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, _configuration["Variables:API_URL"] + "/api/items/add");

            // Prepare the item in JSON format
            string newItem = string.Format("{{" +
                "\"Title\":\"" + title + "\"," +
                "\"ItemTypeId\":\"" + itemTypeId + "\"," +
                "\"Description\":\"" + description + "\"," +
                "\"Picture\":\"" + picture + "\"," +
                "\"Location\":\"" + location + "\"," +
                "\"DailyFee\":\"" + dailyFee + "\"" +
                "}}", Guid.NewGuid());
            request.Content = new StringContent(newItem);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content.Headers.Remove("Content-Type");
            request.Content.Headers.Add("Content-Type", "application/json");

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var resp = await response.Content.ReadAsStringAsync();

            return await GetAll();
        }
    
        public ActionResult Reserve(int id)
        {            
            return View("../Reservations/Reserve", model: id);
        }

        public async Task<ActionResult> Search(string searchString)
        {
            string token = await GetToken();

            // Get the user's id
            int userId = await GetUserId(token);

            // Get the items
            JArray items = await GetSearchItems(token, searchString);

            // Mark the items that belong to the user
            for (int i = 0; i < items.Count; i++)
            {
                if (((int)items[i]["ownerId"]) == userId)
                {
                    items[i]["Mine"] = true;
                }
                else
                {
                    items[i]["Mine"] = false;
                }
            }

            // Get the item types
            Dictionary<int, String> itemTypes = await GetItemTypes(token);

            // Create the ItemViewModel object
            ItemViewModel itemViewModel = new ItemViewModel(items, itemTypes);

            return View("ItemsMenu", model: itemViewModel);
        } 


        // Helpers
        private async Task<string> GetToken()
        {
            var token = await HttpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectParameterNames.AccessToken);
            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("The access token cannot be found in the authentication ticket. " +
                                                    "Make sure that SaveTokens is set to true in the OIDC options.");
            }

            return token;
        }

        private async Task<JArray> GetItems(string token)
        {
            //HttpContext.VerifyUserHasAnyAcceptedScope("Reservations");
            
            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, _configuration["Variables:API_URL"] + "/api/items/");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var resp = await response.Content.ReadAsStringAsync();
            JArray items = JArray.Parse(resp);

            return items;
        }

        private async Task<JArray> GetSearchItems(string token, string searchString)
        {
            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, _configuration["Variables:API_URL"] + "/api/items/search/" + searchString);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var resp = await response.Content.ReadAsStringAsync();
            JArray items = JArray.Parse(resp);

            return items;
        }

        private async Task<int> GetUserId(string token)
        {
            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, _configuration["Variables:API_URL"] + "/api/items/getMyId");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var resp = await response.Content.ReadAsStringAsync();
            int userId = int.Parse(resp);

            return userId;
        }

        private async Task<Dictionary<int, string>> GetItemTypes(string token)
        {
            // Get the array of item types
            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, _configuration["Variables:API_URL"] + "/api/items/getItemTypes");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var resp = await response.Content.ReadAsStringAsync();
            JArray itemTypes = JArray.Parse(resp);

            // Create the dictionary
            Dictionary<int, string> itemTypesDict = new Dictionary<int, string>();
            foreach (JToken itemType in itemTypes)
            {
                itemTypesDict.Add((int)itemType["id"], (string)itemType["name"] + " - " + (string)itemType["description"]);
            }
            
            return itemTypesDict;
        }

        public ActionResult ChangeConsent(string redirectUrl)
        {
            AuthenticationProperties authProps = new AuthenticationProperties { RedirectUri = redirectUrl};
            authProps.Parameters.Add("prompt", "consent");

            return Challenge(authProps, OpenIdConnectDefaults.AuthenticationScheme);
        }

    }
}
