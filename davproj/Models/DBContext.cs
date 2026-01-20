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
            // ОЧЕНЬ ВАЖНО: вызвать базовую реализацию в первую очередь
            base.OnModelCreating(modelBuilder);

            // --- Отношения 1:1 (Внешний ключ находится в дочерней сущности) ---

            // Workplace <--> Phone (FK находится в Phone: p.WorkplaceId)
            modelBuilder.Entity<Workplace>()
                .HasOne(w => w.Phone)
                .WithOne(p => p.Workplace)
                .HasForeignKey<Phone>(p => p.WorkplaceId);

            // Workplace <--> User (FK находится в User: u.WorkplaceId)
            modelBuilder.Entity<Workplace>()
                .HasOne(w => w.User)
                .WithOne(u => u.Workplace)
                .HasForeignKey<User>(u => u.WorkplaceId);

            // Workplace <--> PC (FK находится в PC: p.WorkplaceId)
            modelBuilder.Entity<Workplace>()
                .HasOne(w => w.PC)
                .WithOne(p => p.Workplace)
                .HasForeignKey<PC>(p => p.WorkplaceId);

            // User <--> ADUser (FK находится в User: u.ADUserId)
            modelBuilder.Entity<User>()
                 .HasOne(u => u.ADUser)
                 .WithOne(p => p.User)
                 .HasForeignKey<User>(u => u.ADUserId);

            // --- Отношения 1:Many (Внешний ключ находится в родительской сущности) ---

            // Workplace <--> Printer (FK находится в Workplace: w.PrinterId)
            // Это ваша проблемная связь. Используем WithMany(), так как 1 принтер может быть у многих рабочих мест
            modelBuilder.Entity<Workplace>()
                .HasOne(w => w.Printer)
                .WithMany(p => p.Workplaces)     // Предполагаем, что у Printer есть ICollection<Workplace> Workplaces
                .HasForeignKey(w => w.PrinterId)
                .IsRequired(false);             // Разрешаем NULL (nullable int?)

            // User <--> Printer (FK находится в User: u.PrinterId)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Printer)
                .WithMany(p => p.Users)         // Предполагаем, что у Printer есть ICollection<User> Users
                .HasForeignKey(u => u.PrinterId)
                .OnDelete(DeleteBehavior.SetNull);

            // Building <--> Location (FK находится в Building: b.LocationId)
            modelBuilder.Entity<Building>()
                .HasOne(b => b.Location)
                .WithMany(l => l.Buildings)
                .HasForeignKey(b => b.LocationId)
                .OnDelete(DeleteBehavior.SetNull);

            // Floor <--> Building (FK находится в Floor: f.BuildingId)
            modelBuilder.Entity<Floor>()
                .HasOne(f => f.Building)
                .WithMany(b => b.Floors)
                .HasForeignKey(f => f.BuildingId)
                .OnDelete(DeleteBehavior.SetNull);

            // Office <--> Floor (FK находится в Office: o.FloorId)
            modelBuilder.Entity<Office>()
                .HasOne(o => o.Floor)
                .WithMany(f => f.Offices)
                .HasForeignKey(o => o.FloorId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}