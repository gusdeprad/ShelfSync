using Microsoft.EntityFrameworkCore;
using ShelfSync.Mvc.Models.Entities;

namespace ShelfSync.Mvc.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books { get; set; } = default!;
        public DbSet<Author> Authors { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Book>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Id)
                    .HasDefaultValueSql("NEWSEQUENTIALID()")
                    .ValueGeneratedOnAdd();

                b.Property(x => x.Title)
                    .IsRequired()
                    .HasMaxLength(200);
            });

            modelBuilder.Entity<Author>(a =>
            {
                a.HasKey(x => x.Id);
                a.Property(x => x.Id)
                    .HasDefaultValueSql("NEWSEQUENTIALID()")
                    .ValueGeneratedOnAdd();

                a.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            // EF Core will create the join table for the many-to-many automatically.
        }
    }
}