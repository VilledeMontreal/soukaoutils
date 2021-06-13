using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ReservationAPI.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ItemId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime Created { get; set; }
        public DateTime ? Cancelled { get; set; }


        public User User { get; set; }
        public Item Item { get; set; }


        public Reservation()
        {
            Created = DateTime.Now;
        }
    }
}
