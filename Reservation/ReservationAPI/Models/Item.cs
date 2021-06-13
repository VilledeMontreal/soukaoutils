using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ReservationAPI.Models
{
    public class Item
    {
        public int Id { get; set; }
        [ForeignKey("User")]
        public int OwnerId { get; set; }
        [Required]
        public string Title { get; set; }
        public int ItemTypeId { get; set; }
        [Required]
        public string Description { get; set; }
        public byte[] Picture { get; set; }
        public bool Withdrawn { get; set; }
        [Required]
        public string Location { get; set; }
        public float DailyFee { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }


        public User Owner { get; set; }
        public ItemType ItemType { get; set; }


        public Item()
        {
            Created = DateTime.Now;
            Modified = DateTime.Now;
        }
    }
}
