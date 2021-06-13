using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReservationAPI.Data;
using ReservationAPI.Models;

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


        [HttpGet("{UserId}")]
        public IEnumerable<Reservation> Get(int UserId)
        {
            return aContext.Reservations.Where(reservation => reservation.UserId == UserId);
        }

        [HttpGet("reserve/{ItemId}/{UserId}/{StartDate}/{EndDate}")]
        public ActionResult Reserve(int ItemId, int UserId, DateTime StartDate, DateTime EndDate)
        {
            try
            {
                Item item = aContext.Items.Where(item => item.Id == ItemId).FirstOrDefault();
                User user = aContext.Users.Where(user => user.Id == UserId).FirstOrDefault();

                Reservation reservation = new Reservation { UserId = UserId, ItemId = ItemId, StartDate = StartDate, EndDate = EndDate, Created = DateTime.Now };
                aContext.Reservations.Add(reservation);
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
