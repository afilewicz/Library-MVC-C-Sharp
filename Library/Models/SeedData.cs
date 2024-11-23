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
                    title = "Pan Tadeusz",
                    publisher = "Czytelnik",
                    date_of_publication = DateTime.Parse("2000-1-15"),
                    price = 30.00M,
                },
                new Book
                {
                    author = "Henryk Sienkiewicz",
                    title = "Quo Vadis",
                    publisher = "Greg",
                    date_of_publication = DateTime.Parse("2005-12-14"),
                    price = 25.00M,
                },
                new Book
                {
                    author = "Juliusz Słowacki",
                    title = "Balladyna",
                    publisher = "GWO",
                    date_of_publication = DateTime.Parse("2005-3-19"),
                    price = 25.00M,
                },
                new Book
                {
                    author = "Władysław Reymont",
                    title = "Chłopi",
                    publisher = "Czytelnik",
                    date_of_publication = DateTime.Parse("2001-10-10"),
                    price = 25.00M,
                }
            );
            context.SaveChanges();
        }
    }
}