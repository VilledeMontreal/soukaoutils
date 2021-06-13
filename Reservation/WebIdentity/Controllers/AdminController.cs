using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebIdentity.Models;

namespace WebIdentity.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;


        public AdminController(SignInManager<IdentityUser> signInManager, 
            ILogger<AdminController> logger,
            UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> CreateRolesandUsers()
        {  
            bool x = await _roleManager.RoleExistsAsync("Admin");
            if (!x)
            {
                // first we create Admin rool    
                var role = new IdentityRole();
                role.Name = "Admin";
                await _roleManager.CreateAsync(role);
            }

            // creating Creating Manager role     
            x = await _roleManager.RoleExistsAsync("Manager");
            if (!x)
            {
                var role = new IdentityRole();
                role.Name = "Manager";
                await _roleManager.CreateAsync(role);
            }

            // creating Creating Employee role     
            x = await _roleManager.RoleExistsAsync("Employee");
            if (!x)
            {
                var role = new IdentityRole();
                role.Name = "Employee";
                await _roleManager.CreateAsync(role);
            }

            var userEmail = "alice@solarex.com";
            var usr = await _userManager.FindByEmailAsync(userEmail);
            if (usr == null)
            {
                //Here we create a Admin super user who will maintain the website                   

                var user = new IdentityUser();
                user.UserName = userEmail;
                user.Email = userEmail;
                user.EmailConfirmed = true;

                string userPWD = "P@ssw0rd";

                IdentityResult chkUser = await _userManager.CreateAsync(user, userPWD);

                //Add default User to Role Admin    
                if (chkUser.Succeeded)
                {
                    var result1 = await _userManager.AddToRoleAsync(user, "Admin");
                }
            }

            userEmail = "joe@solarex.com";
            usr = await _userManager.FindByEmailAsync(userEmail);
            if (usr == null)
            {
                //Here we create a Admin super user who will maintain the website                   

                var user = new IdentityUser();
                user.UserName = userEmail;
                user.Email = userEmail;
                user.EmailConfirmed = true;

                string userPWD = "P@ssw0rd";

                IdentityResult chkUser = await _userManager.CreateAsync(user, userPWD);

                //Add default User to Role Admin    
                if (chkUser.Succeeded)
                {
                    var result1 = await _userManager.AddToRoleAsync(user, "Admin");
                    result1 = await _userManager.AddToRoleAsync(user, "Manager");
                }
            }

            userEmail = "marc@solarex.com";
            usr = await _userManager.FindByEmailAsync(userEmail);
            if (usr == null)
            {
                //Here we create a Admin super user who will maintain the website                   

                var user = new IdentityUser();
                user.UserName = userEmail;
                user.Email = userEmail;
                user.EmailConfirmed = true;

                string userPWD = "P@ssw0rd";

                IdentityResult chkUser = await _userManager.CreateAsync(user, userPWD);

                //Add default User to Role Admin    
                if (chkUser.Succeeded)
                {
                    var result1 = await _userManager.AddToRoleAsync(user, "Employee");
                }
            }

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

    }
}
