﻿using CarParkingBookingDatabase.DBModel;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace CarParkingBookingDatabase.BookingDBContext
{
    public class CarParkingBookingDBContext : DbContext
    {
        public CarParkingBookingDBContext(DbContextOptions<CarParkingBookingDBContext> options)
        : base(options)
        {
        }


        public DbSet<UserDetails> userDetails {  get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserDetails>().HasKey(b => b.ID);
        }
    }
}
