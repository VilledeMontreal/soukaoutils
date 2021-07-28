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
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private ReservationContext aContext;
        private string[] scopeRequiredByApi = new string[] { "Reservations" };

        public ItemsController(ReservationContext pContext)
        {
            aContext = pContext;
        }

        [HttpGet]
        public IEnumerable<Item> Get()
        {
            IEnumerable<Item> items = aContext.Items.ToArray();
            return aContext.Items.ToArray();
        }

        [HttpGet("getItemTypes")]
        public IEnumerable<ItemType> GetItemTypes()
        {
            IEnumerable<Item> items = aContext.Items.ToArray();
            return aContext.ItemTypes.ToArray();
        }

      [HttpGet("test")]
        public IEnumerable<Item> Test()
        {
            HttpContext.VerifyUserHasAnyAcceptedScope("Reservations");
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
            //HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
            IQueryable<Item> result = aContext.Items;
            searchString = searchString.ToLower();

            result = result.Where(item => item.Title.ToLower().Contains(searchString) || item.Description.ToLower().Contains(searchString));

            return result.ToArray();
        }

        [HttpGet("withdraw/{ItemId}")]
        public ActionResult Withdraw(int ItemId)
        {
            //HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
            try
            {
                // Get the user
                string username = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                User user = aContext.Users.Where(user => user.UserName.Equals(username)).FirstOrDefault();

                // Create the user if he does not exist
                if (user == null)
                {
                    aContext.Users.Add(new Models.User{UserName=username});
                    aContext.SaveChanges();
                    user = aContext.Users.Where(u => u.UserName.Equals(username)).FirstOrDefault();
                }

                // Get the item
                Item item = aContext.Items.Where(item => item.Id == ItemId).FirstOrDefault();

                // Verify that the user owns this item
                if (item.OwnerId != user.Id)
                {
                    throw new Exception("You do not own this item");
                }

                item.Withdrawn = true;
                aContext.SaveChanges();
                return Ok(item);
            }
            catch (Exception e)
            {
                return Problem(e.InnerException.Message);
            }
        }

        [HttpGet("return/{ItemId}")]
        public ActionResult Return(int ItemId)
        {
            //HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
            try
            {
                // Get the user
                string username = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                User user = aContext.Users.Where(user => user.UserName.Equals(username)).FirstOrDefault();

                // Create the user if he does not exist
                if (user == null)
                {
                    aContext.Users.Add(new Models.User{UserName=username});
                    aContext.SaveChanges();
                    user = aContext.Users.Where(u => u.UserName.Equals(username)).FirstOrDefault();
                }

                // Get the item
                Item item = aContext.Items.Where(item => item.Id == ItemId).FirstOrDefault();

                // Verify that the user owns this item
                if (item.OwnerId != user.Id)
                {
                    throw new Exception("You do not own this item");
                }

                item.Withdrawn = false;
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
                string username = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

                // Get the Owner's Id from the users store. Create a new user if it is not already there
                var owner = aContext.Users.Where(u => u.UserName.Equals(username));
                if(owner.FirstOrDefault() == null)
                {
                    aContext.Users.Add(new Models.User{UserName=username});
                    aContext.SaveChanges();
                    owner = aContext.Users.Where(u => u.UserName.Equals(username));
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
        
        [HttpGet("clear"), Authorize(Roles = "Admin")]
        public ActionResult Clear()
        {
            //HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
            try
            {
                foreach (var reservation in aContext.Reservations)
                    aContext.Reservations.Remove(reservation);
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

        [HttpGet("withdrawall"), Authorize(Roles = "Admin")]
        public ActionResult WithdrawAll()
        {
            //HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
            try
            {
                foreach (var item in aContext.Items)
                {
                    item.Withdrawn = true;
                }
                aContext.SaveChanges();
                return Ok();
            }
            catch (Exception e)
            {
                return Problem(e.InnerException.Message);
            }
        }        

        //[RequiredScope("Reservations")]
        [HttpGet("getMyId")]
        public ActionResult GetMyId()
        {
            try
            {
                //HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
                Claim? scopeClaim = HttpContext.User.FindFirst("scope");
                if (scopeClaim == null || !scopeClaim.Value.Split(' ').Contains<string>("Reservations", StringComparer.OrdinalIgnoreCase))
                {
                    return Forbid();
                }
                string username = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                User user = aContext.Users.Where(user => user.UserName.Equals(username)).FirstOrDefault();

                if(user == null)
                {
                    aContext.Users.Add(new Models.User{UserName=username});
                    aContext.SaveChanges();
                    user = aContext.Users.Where(u => u.UserName.Equals(username)).FirstOrDefault();
                }

                return Ok(user.Id);
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }
        }
    }
}
