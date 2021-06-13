using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace WebIdentity.Controllers
{
    public class AccountController : Controller
    {
       private readonly SignInManager<IdentityUser> _signInManager;
        public AccountController(SignInManager<IdentityUser> signInManager)
        {
            _signInManager = signInManager;
        }

        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { loginProvider = provider, ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }
/* 
        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string loginProvider, string returnUrl)
        {
            ClaimsIdentity id = IdentityManager.Authentication.GetExternalIdentityAsync(AuthenticationManager).Result;
            // Sign in this external identity if its already linked
            IdentityResult result = await IdentityManager.Authentication.SignInExternalIdentityAsync(AuthenticationManager, id);
            if (result.Success)
            {
                return RedirectToLocal(returnUrl);
            }
            else if (User.Identity.IsAuthenticated)
            {
                // Try to link if the user is already signed in
                result = await IdentityManager.Authentication.LinkExternalIdentityAsync(id, User.Identity.GetUserId());
                if (result.Success)
                {
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    return View("ExternalLoginFailure");
                }
            }
            else
            {
                // Otherwise prompt to create a local user
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.LoginProvider = loginProvider;
                var emailClaim = id.Claims.SingleOrDefault(cl => cl.Type == ClaimTypes.Email);
                var email = emailClaim != null ? emailClaim.Value : string.Empty;
                return View("ExternalLoginConfirmation",
                    new ExternalLoginConfirmationViewModel { UserName = id.Name, Email = email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                IdentityResult result =
                    await
                        IdentityManager.Authentication.CreateAndSignInExternalUserAsync(AuthenticationManager,
                            new User { UserName = model.UserName, Email = model.Email });
                if (result.Success)
                {
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    AddErrors(result);
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        } */

    }
}