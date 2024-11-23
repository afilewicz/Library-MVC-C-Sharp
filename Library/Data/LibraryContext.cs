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
    }
}
