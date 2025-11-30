using Microsoft.EntityFrameworkCore;
using TuitionApi.Models;

namespace TuitionApi.Data;

public class TuitionDbContext : DbContext
{
    public TuitionDbContext(DbContextOptions<TuitionDbContext> options) : base(options) { }

    public DbSet<Student> Students { get; set; }
    public DbSet<Tuition> Tuitions { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>()
            .HasIndex(s => s.StudentNo)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();
    }
}
