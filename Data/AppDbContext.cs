using Microsoft.EntityFrameworkCore;
using AbsensiApi.Models;

namespace AbsensiApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<User> Users { get; set; } // <-- TAMBAHKAN BARIS INI
    }
}