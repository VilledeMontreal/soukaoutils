using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReservationAPI.Data;
using ReservationAPI.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ReservationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        private ReservationContext aContext;


        public ReservationsController(ReservationContext pContext)
        {
            aContext = pContext;
        }

        [HttpGet]
        public IEnumerable<Reservation> Get()
        {
            string username = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            User user = aContext.Users.Where(user => user.UserName.Equals(username)).FirstOrDefault();

            if(user == null)
            {
                aContext.Users.Add(new Models.User{UserName=username});
                aContext.SaveChanges();
                user = aContext.Users.Where(u => u.UserName.Equals(username)).FirstOrDefault();
            }

            int userId = user.Id;

            return aContext.Reservations.Where(reservation => reservation.UserId == userId).ToArray();
        }

        [HttpPost("add")]
        public ActionResult Add([FromBody]Reservation pReservation)
        {
            //HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
            try
            {
                // Use the identity of the user making the request to retieve the users items.
                string username = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

                // Get the Owner's Id from the users store. Create a new user if it is not already there
                var user = aContext.Users.Where(u => u.UserName.Equals(username));
                if(user.FirstOrDefault() == null)
                {
                    aContext.Users.Add(new Models.User{UserName=username});
                    aContext.SaveChanges();
                    user = aContext.Users.Where(u => u.UserName.Equals(username));
                }

                pReservation.UserId = user.FirstOrDefault().Id;

                aContext.Reservations.Add(pReservation);
                aContext.SaveChanges();
                return Ok(pReservation);
            }
            catch (Exception e)
            {
                return Problem(e.InnerException.Message);
            }
        }

        [HttpPost("cancel/{id}")]
        public ActionResult Cancel(int id)
        {
            //HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
            try
            {
                // Use the identity of the user making the request to retieve the users items.
                string username = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

                // Get the Owner's Id from the users store. Create a new user if it is not already there
                var user = aContext.Users.Where(u => u.UserName.Equals(username));
                if(user.FirstOrDefault() == null)
                {
                    aContext.Users.Add(new Models.User{UserName=username});
                    aContext.SaveChanges();
                    user = aContext.Users.Where(u => u.UserName.Equals(username));
                }
                
                
                Reservation reservation = aContext.Reservations.Where(r => r.Id == id).FirstOrDefault();
                aContext.Reservations.Remove(reservation);

                aContext.SaveChanges();
                return Ok(reservation);
            }
            catch (Exception e)
            {
                return Problem(e.InnerException.Message);
            }
        }

        [HttpGet("balance/{UserId}")]
        public float Balance(int UserId)
        {
            float balance = 0;
            IEnumerable<Reservation> reservations = aContext.Reservations.Where(reservation => reservation.UserId == UserId);
            
            foreach (Reservation reservation in reservations)
            {
                // Check if the reservation hasen't yet started
                if (reservation.StartDate > DateTime.Now)
                {
                    continue;
                }

                // Check if the reservation has already ended
                if (reservation.EndDate < DateTime.Now)
                {
                    continue;
                }

                Item item = aContext.Items.Where(item => item.Id == reservation.ItemId).FirstOrDefault();
                balance += item.DailyFee;
            }

            return balance;
        }
    }
}
