using Microsoft.EntityFrameworkCore;

namespace PcfMcpApp.Api.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<Sale> Sales => Set<Sale>();
        public DbSet<Customer> Customers => Set<Customer>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().HasKey(c => c.Id);
            modelBuilder.Entity<Sale>().HasKey(s => s.Id);
            modelBuilder.Entity<Sale>()
                .HasOne<Customer>()
                .WithMany()
                .HasForeignKey(s => s.CustomerId);
        }
    }

    public record Customer(int Id, string Name, string Email);
    public record Sale(int Id, int CustomerId, decimal Amount, DateTime SaleDate);
}