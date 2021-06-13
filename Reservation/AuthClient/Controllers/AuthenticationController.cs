using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace AuthClient.Controllers
{
   public class AuthenticationController : Controller
    {
        [HttpGet("~/login")]
        public ActionResult LogIn()
        {
            return Challenge(new AuthenticationProperties { RedirectUri = "/" }, OpenIdConnectDefaults.AuthenticationScheme);
        }


        [HttpGet("~/logout"), HttpPost("~/logout")]
        public ActionResult LogOut()
        {
            HttpContext.SignOutAsync();
            // is redirected from the identity provider after a successful authorization flow and
            // to redirect the user agent to the identity provider to sign out.
            return SignOut(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
            //return SignOut(new AuthenticationProperties {RedirectUri = "~/"}, OpenIdConnectDefaults.AuthenticationScheme);
            return SignOut(
                properties: new AuthenticationProperties
                {
                    RedirectUri = "/"
                },
                authenticationSchemes: OpenIdConnectDefaults.AuthenticationScheme
            );            
        }
    }
}