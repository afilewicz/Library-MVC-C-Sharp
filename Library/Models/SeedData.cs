using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Library.Data;
using System;
using System.Linq;

namespace Library.Models;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using (var context = new LibraryContext(
                   serviceProvider.GetRequiredService<
                       DbContextOptions<LibraryContext>>()))
        {
            // Look for any books.
            if (context.Book.Any())
            {
                return;   // DB has been seeded
            }
            context.Book.AddRange(
                new Book
                {
                    author = "Adam Mickiewicz",
                    publisher = "Czytelnik",
                    date_of_publication = DateTime.Parse("2000-1-15"),
                    price = 30.00M,
                    history_of_leases = string.Empty
                },
                new Book
                {
                    author = "Henryk Sienkiewicz",
                    publisher = "Greg",
                    date_of_publication = DateTime.Parse("2005-12-14"),
                    price = 25.00M,
                    history_of_leases = string.Empty
                },
                new Book
                {
                    author = "Juliusz Słowacki",
                    publisher = "GWO",
                    date_of_publication = DateTime.Parse("2005-3-19"),
                    price = 25.00M,
                    history_of_leases = string.Empty
                },
                new Book
                {
                    author = "Władysław Reymont",
                    publisher = "Czytelnik",
                    date_of_publication = DateTime.Parse("2001-10-10"),
                    price = 25.00M,
                    history_of_leases = string.Empty
                }
            );
            context.SaveChanges();
        }
    }
}