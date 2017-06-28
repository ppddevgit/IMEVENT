using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using IMEVENT.Models;

namespace IMEVENT.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {        
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Group> Groups { get; set; }
        public DbSet<Zone> Zones { get; set;}
        public DbSet<Responsable> Responsables{get;set;}
        public DbSet<SousZone> SousZones  { get; set;}
        public DbSet<UsersZone> UsersZones { get; set;}
        public DbSet<Event> Events { get; set; }
        public DbSet<Dormitory> Dorms { get; set; }
        public DbSet<Hall> Halls { get; set; }
        public DbSet<Refectory> Refectories { get; set; }
        public DbSet<EventAttendee> EventAttendees { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<SharingGroup> SharingGroups { get; set; }
        public DbSet<FreeHallSection> FreeHallSections { get; set; }
        public DbSet<FreeDormitory> FreeDormitories { get; set; }
        public DbSet<FreeRefectory> FreeRefectories { get; set; }
        public DbSet<FreeSharingGroup> FreeSharingGroups { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }

        public static  ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>();
            options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=IMEVENTDB;Trusted_Connection=True;MultipleActiveResultSets=true");         
            return new ApplicationDbContext(options.Options);
        }
    }
}
