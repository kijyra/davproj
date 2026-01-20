using Microsoft.EntityFrameworkCore;
using davproj.Models;
using System.Text.Json;

namespace davproj.Models
{
    public class DBContext : DbContext
    {
        public DbSet<ADUser> ADUsers { get; set; } = null!;
        public DbSet<Building> Buildings { get; set; } = null!;
        public DbSet<Cartridge> Cartridges { get; set; } = null!;
        public DbSet<Floor> Floors { get; set; } = null!;
        public DbSet<Location> Locations { get; set; } = null!;
        public DbSet<Manufactor> Manufactors { get; set; } = null!;
        public DbSet<Office> Offices { get; set; } = null!;
        public DbSet<PC> PCs { get; set; } = null!;
        public DbSet<Phone> Phones { get; set; } = null!;
        public DbSet<Printer> Printers { get; set; } = null!;
        public DbSet<PrinterModel> PrinterModels { get; set; } = null;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Workplace> Workplaces { get; set; } = null!;

        public DBContext(DbContextOptions<DBContext> options)
    : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Связь с телефоном
            modelBuilder.Entity<Workplace>()
                .HasOne(w => w.Phone)
                .WithOne(p => p.Workplace)
                .HasForeignKey<Workplace>(w => w.PhoneId)
                .IsRequired(false);
            modelBuilder.Entity<Workplace>()
                .HasOne(w => w.PC)
                .WithOne(p => p.Workplace)
                .HasForeignKey<Workplace>(w => w.PCId)
                .IsRequired(false);
            modelBuilder.Entity<Workplace>()
                .HasOne(w => w.User)
                .WithOne(u => u.Workplace)
                .HasForeignKey<Workplace>(w => w.UserId)
                .IsRequired(false);
            modelBuilder.Entity<User>()
                 .HasOne(u => u.ADUser)
                 .WithOne(p => p.User)
                 .HasForeignKey<User>(u => u.ADUserId);
            modelBuilder.Entity<Workplace>()
                .HasOne(w => w.Printer)
                .WithMany(p => p.Workplaces)    
                .HasForeignKey(w => w.PrinterId)
                .IsRequired(false);           
            modelBuilder.Entity<User>()
                .HasOne(u => u.Printer)
                .WithMany(p => p.Users)         
                .HasForeignKey(u => u.PrinterId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Building>()
                .HasOne(b => b.Location)
                .WithMany(l => l.Buildings)
                .HasForeignKey(b => b.LocationId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Floor>()
                .HasOne(f => f.Building)
                .WithMany(b => b.Floors)
                .HasForeignKey(f => f.BuildingId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Office>()
                .HasOne(o => o.Floor)
                .WithMany(f => f.Offices)
                .HasForeignKey(o => o.FloorId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}