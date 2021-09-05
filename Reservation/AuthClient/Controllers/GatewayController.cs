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

namespace AuthClient.Controllers
{
    [Authorize]
    public class GatewayController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly string _authorization_endpoint;
        private readonly string _token_endpoint;
        private readonly string _client_id;
        private readonly string _client_secret;
        private readonly string _redirect_uri;
        private string _access_token;
        private string _id_token;

/*         string authorization_endpoint = "https://accounts.google.com/o/oauth2/v2/auth";
        string token_endpoint = "https://oauth2.googleapis.com/token";

        string client_id = "479429437363-3os2h43sd5pbrgjiu1c8jfn84ifdbv6n.apps.googleusercontent.com";
        string client_secret = "9jEhYZya-Z8kKnb_qP2n3D6Q";
 */        

        public GatewayController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;

            _client_id = _configuration["GatewayApp:client_id"];
            _client_secret = _configuration["GatewayApp:client_secret"];
            _redirect_uri = _configuration["GatewayApp:redirect_uri"];

            string wellKnownUrl = _configuration["GatewayApp:issuer"] + "/.well-known/openid-configuration";
            // Bypass https validation
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
            {
                return true;
            };
            HttpClient tokenClient = new HttpClient(httpClientHandler);
            var response = tokenClient.GetAsync(wellKnownUrl).Result;
            //get access token from response body
            var responseJson = response.Content.ReadAsStringAsync().Result;
            var jObject = JObject.Parse(responseJson);
            _authorization_endpoint = jObject.GetValue("authorization_endpoint").ToString();
            _token_endpoint = jObject.GetValue("token_endpoint").ToString();

        }

        // GET: ItemsController
        public async Task<ActionResult> Index()
        {
            string code = HttpContext.Request.Query["code"].ToString();
            if(string.IsNullOrEmpty(code))
            {
                return Redirect(GetAuthnReqUrl());
                //return await GetAllGluu();
            }
            else
            {
                //GetToken(code);
                return await GetAllGluu(code);
            }
            //return await GetAll();
        }

        public String GetGoogleAuthnReqUrl()
        {

            return _authorization_endpoint + "?client_id="
                + _client_id + "&response_type=code"
                + "&scope=openid" + "&redirect_uri=" + _redirect_uri;
        }

        public String GetAuthnReqUrl()
        {

            return _authorization_endpoint + "?client_id="
                + _client_id + "&response_type=code"
                + "&scope=openid" + "&redirect_uri=" + _redirect_uri;
        }

        public void GetTokens(String code)
        {
            var values = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "client_id", _client_id},
                { "client_secret", _client_secret },
                { "code" , code },
                { "redirect_uri", _redirect_uri}
            };

            // Bypass https validation
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
            {
                return true;
            };
            HttpClient tokenClient = new HttpClient(httpClientHandler);
            var content = new FormUrlEncodedContent(values);
            var response = tokenClient.PostAsync(_token_endpoint, content).Result;
            //get access token from response body
            var responseJson = response.Content.ReadAsStringAsync().Result;
            var jObject = JObject.Parse(responseJson);
            _access_token = jObject.GetValue("access_token").ToString();
            _id_token = jObject.GetValue("id_token").ToString();
        }

        public async Task<ActionResult> GetAllGluu(String code)
        {

            GetTokens(code);
            return View("Output", new OutputViewModel
            {
                Output = string.Format("<h3>id_token</h3><p>{0}</p><h3>access_token</h3><p>{1}</p>", _id_token, _access_token)
            });

        }

    }
}
