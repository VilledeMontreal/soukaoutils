using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReservationAPI.Models;
using ReservationAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web.Resource;
using System.Security.Claims;
using System.Net.Http;
using System.Text;

namespace ReservationAPI.Controllers
{
    [Authorize(Roles = "Manager,Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private ReservationContext aContext;
        private string[] scopeRequiredByApi = new string[] { "access_as_user" };

        public ItemsController(ReservationContext pContext)
        {
            aContext = pContext;
        }

        [HttpGet]
        public IEnumerable<Item> Get()
        {
            //return View("Home", model: await response.Content.ReadAsStringAsync());
            return aContext.Items.ToArray();
            /* return new[] {  new Item
                    {
                        Id = 1,
                        ItemTypeId = 1,
                        OwnerId = 1,
                        Created = DateTime.Now,
                        Withdrawn = false,
                        Title = "My Ford Focus",
                        Description = "Nice 2016 model",
                        Location = "Pierrefonds"
                    }
                }; */
        }

      [HttpGet("test")]
        public IEnumerable<Item> Test()
        {
            //HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
            return new[] {  new Item
                    {
                        Id = 1,
                        ItemTypeId = 1,
                        OwnerId = 1,
                        Created = DateTime.Now,
                        Withdrawn = false,
                        Title = "My Ford Focus",
                        Description = "Nice 2016 model",
                        Location = "Pierrefonds"
                    }
                };
        }

        [HttpGet("search/{searchString}")]
        public IEnumerable<Item> Search(string searchString)
        {
            HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
            IQueryable<Item> result = aContext.Items;
            searchString = searchString.ToLower();

            result = result.Where(item => item.Title.ToLower().Contains(searchString) || item.Description.ToLower().Contains(searchString));

            return result.ToArray();
        }

        [HttpGet("withdraw/{ItemId}")]
        public ActionResult Withdraw(int ItemId)
        {
            HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
            try
            {
                Item item = aContext.Items.Where(item => item.Id == ItemId).FirstOrDefault();
                item.Withdrawn = true;
                aContext.SaveChanges();
                return Ok(item);
            }
            catch (Exception e)
            {
                return Problem(e.InnerException.Message);
            }
        }

        [HttpPost("quick")]
        public ActionResult Quick([FromBody]Item pItem)
        {
            //HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
       
                return Ok(pItem);
    
        }

        [HttpPost("add")]
        public ActionResult Add([FromBody]Item pItem)
        {
            //HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
            try
            {
                // Use the identity of the user making the request to retieve the users items.
                string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

                // Get the Owner's Id from the users store. Create a new user if it is not already there
                var owner = aContext.Users.Where(u => u.UserName == userId );
                if(owner.FirstOrDefault() == null)
                {
                    aContext.Users.Add(new Models.User{UserName=userId});
                    aContext.SaveChanges();
                    owner = aContext.Users.Where(u => u.UserName == userId);
                }
                pItem.OwnerId = owner.FirstOrDefault().Id;

                aContext.Items.Add(pItem);
                aContext.SaveChanges();
                return Ok(pItem);
            }
            catch (Exception e)
            {
                return Problem(e.InnerException.Message);
            }
        }
        [HttpGet("clear")]
        public ActionResult Clear()
        {
            //HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
            try
            {
                foreach (var item in aContext.Items)
                    aContext.Items.Remove(item);
                aContext.SaveChanges();
                return Ok();
            }
            catch (Exception e)
            {
                return Problem(e.InnerException.Message);
            }
        }        
    }
}
