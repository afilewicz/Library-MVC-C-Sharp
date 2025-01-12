using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Library.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Library.Data
{
    public class LibraryContext : IdentityDbContext<IdentityUser>
    {
        public LibraryContext (DbContextOptions<LibraryContext> options)
            : base(options)
        {
        }

        public DbSet<Library.Models.Book> Book { get; set; } = default!;
        public DbSet<Library.Models.Loan> Loan { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Book>()
                .Property(b => b.ConcurrencyToken)
                .IsConcurrencyToken();
            
            modelBuilder.Entity<IdentityUserLogin<string>>()
                .HasKey(login => new { login.LoginProvider, login.ProviderKey });
        }
        
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var modifiedEntries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified && e.Entity is Book);

            foreach (var entry in modifiedEntries)
            {
                ((Book)entry.Entity).ConcurrencyToken = Guid.NewGuid();
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            var modifiedEntries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified && e.Entity is Book);

            foreach (var entry in modifiedEntries)
            {
                ((Book)entry.Entity).ConcurrencyToken = Guid.NewGuid();
            }

            return base.SaveChanges();
        }
    }
}
