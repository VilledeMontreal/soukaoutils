using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ReservationAPI.Models;

namespace ReservationAPI.Data
{
    public class ReservationContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<ItemType> ItemTypes { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        public ReservationContext(DbContextOptions<ReservationContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Item>().ToTable("Items");
            modelBuilder.Entity<ItemType>().ToTable("ItemTypes");
            modelBuilder.Entity<Reservation>().ToTable("Reservations");

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            modelBuilder.Entity<ItemType>()
                .HasData(
                    new ItemType
                    {
                        Id = 1,
                        Name = "Car",
                        Description = "A normal car"
                    },
                    new ItemType
                    {
                        Id = 2,
                        Name = "Bike",
                        Description = "A normal bike"
                    }
                );


            modelBuilder.Entity<User>()
                .HasData(
                    new User
                    {
                        Id = 1,
                        UserName = "user1",
                    },
                    new User
                    {
                        Id = 2,
                        UserName = "user1",
                    }
                );


            modelBuilder.Entity<Item>()
                .HasData(
                    new Item
                    {
                        Id = 1,
                        ItemTypeId = 1,
                        OwnerId = 1,
                        Created = DateTime.Now,
                        Withdrawn = false,
                        Title = "My Ford Focus",
                        Description = "Nice 2016 model",
                        Location = "Pierrefonds"
                    },
                    new Item
                    {
                        Id = 2,
                        ItemTypeId = 1,
                        OwnerId = 2,
                        Created = DateTime.Now,
                        Withdrawn = false,
                        Title = "My Nissan ",
                        Description = "Old 2011 model",
                        Location = "Ile-Bizzard"
                    }
                );
        }

    }
}
