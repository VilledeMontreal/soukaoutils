using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;
using OpenIddict.Server.AspNetCore;
using WebIdentity.Helpers;
using WebIdentity.ViewModels.Authorization;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace WebIdentity.Controllers
{
    public class AuthorizationController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;

        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly IOpenIddictAuthorizationManager _authorizationManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IOpenIddictScopeManager _scopeManager;


        public AuthorizationController(
            IOpenIddictApplicationManager applicationManager,
            IOpenIddictAuthorizationManager authorizationManager,
            IOpenIddictScopeManager scopeManager,
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager)
        {
            _applicationManager = applicationManager;
            _authorizationManager = authorizationManager;
            _scopeManager = scopeManager;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpPost("~/connect/token"), Produces("application/json")]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.GetOpenIddictServerRequest() ??
                          throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            ClaimsPrincipal claimsPrincipal;

            if (request.IsClientCredentialsGrantType())
            {
                // Note: the client credentials are automatically validated by OpenIddict:
                // if client_id or client_secret are invalid, this action won't be invoked.

                var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                // Subject (sub) is a required field, we use the client id as the subject identifier here.
                //identity.AddClaim(OpenIddictConstants.Claims.Subject, request.ClientId ?? throw new InvalidOperationException());
                identity.AddClaim(OpenIddictConstants.Claims.Subject, request.ClientId ?? "mvc");

                // Add some claim, don't forget to add destination otherwise it won't be added to the access token.
                identity.AddClaim("some-claim", "some-value", OpenIddictConstants.Destinations.AccessToken);

                claimsPrincipal = new ClaimsPrincipal(identity);

                claimsPrincipal.SetScopes(request.GetScopes());
            }

            else if (request.IsAuthorizationCodeGrantType())
            {
                // Retrieve the claims principal stored in the authorization code
                claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
            }
            else if (request.IsRefreshTokenGrantType())
            {
                // Retrieve the claims principal stored in the refresh token.
                claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
            }
            else
            {
                throw new InvalidOperationException("The specified grant type is not supported.");
            }

            // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
            return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        [HttpGet("~/connect/authorize")]
        [HttpPost("~/connect/authorize")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Authorize()
        {
            var request = HttpContext.GetOpenIddictServerRequest() ??
                throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            // Retrieve the user principal stored in the authentication cookie.
            //var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);


            // If the user principal can't be extracted, redirect the user to the login page.
            if (result is null || !result.Succeeded)
            {
                // If the client application requested promptless authentication,
                // return an error indicating that the user is not logged in.
                if (request.HasPrompt(Prompts.None))
                {
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties(new Dictionary<string, string>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.LoginRequired,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is not logged in."
                        }));
                }

                return Challenge(
                    authenticationSchemes: CookieAuthenticationDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties
                    {
                        RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                            Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
                    });
            }

            // If a max_age parameter was provided, ensure that the cookie is not too old.
            // If it's too old, automatically redirect the user agent to the login page.
            if (request.MaxAge is not null && result.Properties?.IssuedUtc is not null &&
                DateTimeOffset.UtcNow - result.Properties.IssuedUtc > TimeSpan.FromSeconds(request.MaxAge.Value))
            {
                if (request.HasPrompt(Prompts.None))
                {
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties(new Dictionary<string, string>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.LoginRequired,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is not logged in."
                        }));
                }

                return Challenge(
                    authenticationSchemes: IdentityConstants.ApplicationScheme,
                    properties: new AuthenticationProperties
                    {
                        RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                            Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
                    });
            }


            // Create a new claims principal
            /* var claims = new List<Claim>
            {
                // 'subject' claim which is required
                new Claim(OpenIddictConstants.Claims.Subject, result.Principal.Claims)
                new Claim("some claim", "some value").SetDestinations(OpenIddictConstants.Destinations.AccessToken),
                new Claim("roles", "PaidUser").SetDestinations(OpenIddictConstants.Destinations.AccessToken),
                new Claim("role", "PaidUser").SetDestinations(OpenIddictConstants.Destinations.AccessToken),
                new Claim("role", "FreeUser").SetDestinations(OpenIddictConstants.Destinations.AccessToken),
                new Claim("role", "Admin").SetDestinations(OpenIddictConstants.Destinations.AccessToken),
                new Claim(OpenIddictConstants.Claims.Email, "some@email").SetDestinations(OpenIddictConstants.Destinations.IdentityToken) 
           }; 
           */

            // Retrieve the profile of the logged in user.
            var user = await _userManager.GetUserAsync(result.Principal) ??
                throw new InvalidOperationException("The user details cannot be retrieved.");

            // Retrieve the application details from the database.
            var application = await _applicationManager.FindByClientIdAsync(request.ClientId) ??
                throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

            // Retrieve the permanent authorizations associated with the user and the calling client application.
            var authorizations = await _authorizationManager.FindAsync(
                subject: await _userManager.GetUserIdAsync(user),
                client : await _applicationManager.GetIdAsync(application),
                status : Statuses.Valid,
                type   : AuthorizationTypes.Permanent).ToListAsync();

            OpenIddictEntityFrameworkCoreAuthorization authorization = GetLastAuthorization(authorizations);

            List<string> lstScopes = GetScopes(authorization);

            switch (await _applicationManager.GetConsentTypeAsync(application))
            {
                // If the consent is external (e.g when authorizations are granted by a sysadmin),
                // immediately return an error if no authorization can be found in the database.
                case ConsentTypes.External when !authorizations.Any():
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties(new Dictionary<string, string>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                                "The logged in user is not allowed to access this client application."
                        }));

                // If the consent is implicit or if an authorization was found,
                // return an authorization response without displaying the consent form.
                case ConsentTypes.Implicit:
                case ConsentTypes.External when authorizations.Any():
                case ConsentTypes.Explicit when authorizations.Any() && !request.HasPrompt(Prompts.Consent) : // && false: // Testing
                    var principal = await _signInManager.CreateUserPrincipalAsync(user);

                    // Note: in this sample, the granted scopes match the requested scope
                    // but you may want to allow the user to uncheck specific scopes.
                    // For that, simply restrict the list of scopes before calling SetScopes.
                    
                    //principal.SetScopes(request.GetScopes());
                    //principal.SetResources(await _scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());
                    principal.SetScopes(lstScopes.ToArray());
                    principal.SetResources(await _scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());


                    // Automatically create a permanent authorization to avoid requiring explicit consent
                    // for future authorization or token requests containing the same scopes.
                    
                    principal.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));

                    principal.Claims.Append(new Claim(OpenIddictConstants.Claims.Subject, user.Id));
                    principal.Claims.Append(new Claim(OpenIddictConstants.Claims.Scope, authorization.Scopes));

                    foreach (var claim in principal.Claims)
                    {
                        claim.SetDestinations(GetDestinations(claim, principal));
                    }

                    return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                // At this point, no authorization was found in the database and an error must be returned
                // if the client application specified prompt=none in the authorization request.
                case ConsentTypes.Explicit   when request.HasPrompt(Prompts.None):
                case ConsentTypes.Systematic when request.HasPrompt(Prompts.None):
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties(new Dictionary<string, string>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                                "Interactive user consent is required."
                        }));

                // In every other case, render the consent form.
                default: return View(new AuthorizeViewModel
                {
                    ApplicationName = await _applicationManager.GetLocalizedDisplayNameAsync(application),
                    Scope = request.Scope,
                    GrantedScope = lstScopes
                });
            }

        }

        [Authorize, FormValueRequired("submit.Accept")]
        [HttpPost("~/connect/authorize"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept()
        {
            var request = HttpContext.GetOpenIddictServerRequest() ??
                throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            // Retrieve the profile of the logged in user.
            var user = await _userManager.GetUserAsync(User) ??
                throw new InvalidOperationException("The user details cannot be retrieved.");

            // Retrieve the application details from the database.
            var application = await _applicationManager.FindByClientIdAsync(request.ClientId) ??
                throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

            // Retrieve the permanent authorizations associated with the user and the calling client application.
            var authorizations = await _authorizationManager.FindAsync(
                subject: await _userManager.GetUserIdAsync(user),
                client : await _applicationManager.GetIdAsync(application),
                status : Statuses.Valid,
                type   : AuthorizationTypes.Permanent).ToListAsync();

            // Note: the same check is already made in the other action but is repeated
            // here to ensure a malicious user can't abuse this POST-only endpoint and
            // force it to return a valid response without the external authorization.
            if (!authorizations.Any() && await _applicationManager.HasConsentTypeAsync(application, ConsentTypes.External))
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The logged in user is not allowed to access this client application."
                    }));
            }

            var principal = await _signInManager.CreateUserPrincipalAsync(user);

            // Note: in this sample, the granted scopes match the requested scope
            // but you may want to allow the user to uncheck specific scopes.
            // For that, simply restrict the list of scopes before calling SetScopes.
            //principal.SetScopes(request.GetScopes());
            List<string> lstConsentedScopes = new List<string>();
            foreach(var scp in request.GetParameter("scopeSwitches").GetValueOrDefault().ToString().Split(','))
            {
                lstConsentedScopes.Add(scp.Trim());
            }
            principal.SetScopes(lstConsentedScopes);
            principal.SetResources(await _scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());

            var authorization = await _authorizationManager.CreateAsync(
                principal: principal,
                subject  : await _userManager.GetUserIdAsync(user),
                client   : await _applicationManager.GetIdAsync(application),
                type     : AuthorizationTypes.Permanent,
                scopes   : principal.GetScopes());
            

            principal.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));

            principal.Claims.Append(new Claim(OpenIddictConstants.Claims.Subject, user.Id));
            principal.Claims.Append(new Claim(OpenIddictConstants.Claims.Scope, ((OpenIddictEntityFrameworkCoreAuthorization)authorization).Scopes));

            foreach (var claim in principal.Claims)
            {
                claim.SetDestinations(GetDestinations(claim, principal));
            }

            // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }        

        [Authorize, FormValueRequired("submit.Deny")]
        [HttpPost("~/connect/authorize"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Deny()
        {
            var request = HttpContext.GetOpenIddictServerRequest() ??
                throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            // Retrieve the profile of the logged in user.
            var user = await _userManager.GetUserAsync(User) ??
                throw new InvalidOperationException("The user details cannot be retrieved.");

            // Retrieve the application details from the database.
            var application = await _applicationManager.FindByClientIdAsync(request.ClientId) ??
                throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

            // Retrieve the permanent authorizations associated with the user and the calling client application.
            var authorizations = await _authorizationManager.FindAsync(
                subject: await _userManager.GetUserIdAsync(user),
                client : await _applicationManager.GetIdAsync(application),
                status : Statuses.Valid,
                type   : AuthorizationTypes.Permanent).ToListAsync();
            
            OpenIddictEntityFrameworkCoreAuthorization authorization = GetLastAuthorization(authorizations);

            List<string> lstScopes = GetScopes(authorization);
  
            var principal = await _signInManager.CreateUserPrincipalAsync(user);
            principal.SetScopes(lstScopes.ToArray());
            principal.SetResources(await _scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());


            // Automatically create a permanent authorization to avoid requiring explicit consent
            // for future authorization or token requests containing the same scopes.
            
            principal.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));

            principal.Claims.Append(new Claim(OpenIddictConstants.Claims.Subject, user.Id));
            principal.Claims.Append(new Claim(OpenIddictConstants.Claims.Scope, authorization.Scopes));

            foreach (var claim in principal.Claims)
            {
                claim.SetDestinations(GetDestinations(claim, principal));
            }

            // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }        


        [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("~/connect/userinfo")]
        public async Task<IActionResult> Userinfo()
        {

            var claims = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                // Note: the "sub" claim is a mandatory claim and must be included in the JSON response.
                [Claims.Subject] = User.Claims.FirstOrDefault(x => x.Type == "sub").Value //"alice@solarex.com" //await _userManager.GetUserIdAsync(user)
            };
            /*
                         if (User.HasScope(Scopes.Email))
                        {
                            claims[Claims.Email] = await _userManager.GetEmailAsync(user);
                            claims[Claims.EmailVerified] = await _userManager.IsEmailConfirmedAsync(user);
                        }

                        if (User.HasScope(Scopes.Phone))
                        {
                            claims[Claims.PhoneNumber] = await _userManager.GetPhoneNumberAsync(user);
                            claims[Claims.PhoneNumberVerified] = await _userManager.IsPhoneNumberConfirmedAsync(user);
                        }

                        if (User.HasScope(Scopes.Roles))
                        {
                            claims[Claims.Role] = await _userManager.GetRolesAsync(user);
                        } 
            */
            // Note: the complete list of standard claims supported by the OpenID Connect specification
            // can be found here: http://openid.net/specs/openid-connect-core-1_0.html#StandardClaims

            return Ok(claims);
            var claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;

            return Ok(new
            {
                Name = claimsPrincipal.GetClaim(OpenIddictConstants.Claims.Subject),
                Occupation = "Developer",
                Age = 43
            });
        }

        [HttpGet("~/connect/logout")]
        public IActionResult Logout()
        {
            //return  Redirect("https://localhost:5001/Identity/Account/Logout?returnUrl=%2F");
            //_signInManager.SignOutAsync();
            //return Redirect("/Identity/Account/Logout?returnUrl=%2F");
            return View();
        }

        [ActionName(nameof(Logout)), HttpPost("~/connect/logout"), ValidateAntiForgeryToken]
        public async Task<IActionResult> LogoutPost()
        {
            // Ask ASP.NET Core Identity to delete the local and external cookies created
            // when the user agent is redirected from the external identity provider
            // after a successful authentication flow (e.g Google or Facebook).
            await _signInManager.SignOutAsync();

            // Returning a SignOutResult will ask OpenIddict to redirect the user agent
            // to the post_logout_redirect_uri specified by the client application or to
            // the RedirectUri specified in the authentication properties if none was set.
            return SignOut(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = "/"
                });
        }

        // POST: /Account/ExternalLogin
        [HttpPost("~/Account/ExternalLogin")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { loginProvider = provider, ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        private OpenIddictEntityFrameworkCoreAuthorization GetLastAuthorization(List<object> authorizations)
        {
            authorizations.Sort((a, b) => {
                var a1 = a as OpenIddictEntityFrameworkCoreAuthorization;
                var b1 = b as OpenIddictEntityFrameworkCoreAuthorization;
                return DateTime.Compare(a1.CreationDate.Value, b1.CreationDate.Value);
                });
            var authorization = authorizations.LastOrDefault();
            return authorization as OpenIddictEntityFrameworkCoreAuthorization;
        }

        private List<string> GetScopes(OpenIddictEntityFrameworkCoreAuthorization auth)
        {
            List<string> lstScopes = new List<string>();
            if(auth is not null)
            {
                string[] savedScopes = auth.Scopes.Split(',');
                foreach(string s in savedScopes)
                {
                    lstScopes.Add(s.Trim('[',']','\\', '"'));
                }
            }
            return lstScopes;
        }
        private IEnumerable<string> GetDestinations(Claim claim, ClaimsPrincipal principal)
        {
            // Note: by default, claims are NOT automatically included in the access and identity tokens.
            // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
            // whether they should be included in access tokens, in identity tokens or in both.

            switch (claim.Type)
            {
                case Claims.Name:
                    yield return Destinations.AccessToken;

                    if (principal.HasScope(Scopes.Profile))
                        yield return Destinations.IdentityToken;

                    yield break;

                case Claims.Email:
                    yield return Destinations.AccessToken;

                    if (principal.HasScope(Scopes.Email))
                        yield return Destinations.IdentityToken;

                    yield break;

                case Claims.Role:
                    yield return Destinations.AccessToken;

                    if (principal.HasScope(Scopes.Roles))
                        yield return Destinations.IdentityToken;

                    yield break;

                case "oi_scp":
                    yield return Destinations.AccessToken;
                    yield return Destinations.IdentityToken;

                    yield break;

                // Never include the security stamp in the access and identity tokens, as it's a secret value.
                case "AspNet.Identity.SecurityStamp": yield break;

                default:
                    yield return Destinations.AccessToken;
                    yield break;
            }
        }
    }
}