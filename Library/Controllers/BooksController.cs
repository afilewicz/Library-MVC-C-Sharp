using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Library.Data;
using Library.Models;
using Microsoft.AspNetCore.Authorization;
using System;
using Humanizer;

namespace Library.Controllers
{
    [Authorize]
    public class BooksController : Controller
    {
        private readonly LibraryContext _context;

        public BooksController(LibraryContext context)
        {
            _context = context;
        }

        // GET: Books
        public async Task<IActionResult> Index(string bookPublisher, string searchString)
        {
            await ReleaseReservedBooks();
            if (_context.Book == null)
            {
                return Problem("Entity set 'LibraryContext.Library'  is null.");
            }

            // Use LINQ to get list of genres.
            IQueryable<string> genreQuery = from b in _context.Book
                orderby b.publisher
                select b.publisher;
            
            var books = from b in _context.Book
                select b;
            
            if (!string.IsNullOrEmpty(searchString))
            {
                books = books.Where(s => s.author!.ToUpper().Contains(searchString.ToUpper()) ||
                                         s.title!.ToUpper().Contains(searchString.ToUpper()));
            }
            if (!string.IsNullOrEmpty(bookPublisher))
            {
                books = books.Where(x => x.publisher == bookPublisher);
            }
            
            var bookPublisherVM = new BookPublisherViewModel
            {
                Publishers = new SelectList(await genreQuery.Distinct().ToListAsync()),
                Books = await books.ToListAsync()
            };
            
            // ViewData["LoanDates"] = loanDates;

            if (User.IsInRole("SuperAdmin"))
            {
                return View("Index", bookPublisherVM);
            }
            else
            {
                return View("UserIndex", bookPublisherVM);
            }
        }

        public async Task<IActionResult> UserIndex(string bookPublisher, string searchString)
        {
            await ReleaseReservedBooks();
            if (_context.Book == null)
            {
                return Problem("Entity set 'LibraryContext.Library' is null.");
            }

            IQueryable<string> genreQuery = from b in _context.Book
                orderby b.publisher
                select b.publisher;

            var books = from b in _context.Book
                select b;

            if (!string.IsNullOrEmpty(searchString))
            {
                books = books.Where(s => s.author!.ToUpper().Contains(searchString.ToUpper()));
            }

            if (!string.IsNullOrEmpty(bookPublisher))
            {
                books = books.Where(x => x.publisher == bookPublisher);
            }

            var bookPublisherVM = new BookPublisherViewModel
            {
                Publishers = new SelectList(await genreQuery.Distinct().ToListAsync()),
                Books = await books.ToListAsync()
            };

            return View(bookPublisherVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Borrow(int? id,  Guid concurrencyToken)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var book= await _context.Book.FirstOrDefaultAsync(b => b.id == id);
            
            // if (book.ConcurrencyToken != concurrencyToken)
            // {
            //     TempData["ConcurrencyError"] = "Ktoś wypożyczył już tę książkę.";
            //     TempData["BorrowFailed"] = book.id;
            //     return RedirectToAction(nameof(Index));
            // }
            if (book == null)
            {
                ModelState.AddModelError(string.Empty,
                    "Unable to save changes. The book was deleted by another user.");
                return RedirectToAction(nameof(Index));
            }
            
            Guid userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            var loan = new Loan
            {
                BookId = book.id,
                UserId = userId,
                LoanDate = null,
                ReturnDate = DateTime.Today.AddDays(1).AddSeconds(-1),
                Status = LoanStatus.Reserved
            };

            _context.Loan.Add(loan);
                    
            book.is_loaned = true;

            // book.ConcurrencyToken = Guid.NewGuid();
            _context.Entry(book).Property(b => b.ConcurrencyToken)
                .OriginalValue = concurrencyToken;
            
            if (await TryUpdateModelAsync<Book>(book, "", b=> b.is_loaned))
            {
                try
                {
                    // book.ConcurrencyToken = Guid.NewGuid();
                    _context.Book.Update(book);
                    await _context.SaveChangesAsync();
                    TempData["BorrowFailed"] = false;
                    return RedirectToAction(nameof(UserIndex));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    // Obsługa konfliktu współbieżności
                    var exceptionEntry = ex.Entries.Single();
                    var clientValues = (Book)exceptionEntry.Entity;
                    var databaseEntry = exceptionEntry.GetDatabaseValues();

                    if (databaseEntry == null)
                    {
                        // Książka została usunięta w międzyczasie
                        ModelState.AddModelError(string.Empty,
                            "Unable to borrow the book. It was deleted by another user.");
                    }
                    else
                    {
                        var databaseValues = (Book)databaseEntry.ToObject();

                        // Porównanie pól i dodanie komunikatów błędów
                        if (databaseValues.is_loaned != clientValues.is_loaned)
                        {
                            ModelState.AddModelError("is_loaned",
                                $"Current value in the database: {databaseValues.is_loaned}");
                        }

                        ModelState.AddModelError(string.Empty,
                            "The record you attempted to borrow was modified by another user after you got the original value. "
                            + "The borrow operation was canceled. Current values in the database have been displayed. "
                            + "If you still want to borrow this book, try again.");

                        // Aktualizacja tokena w modelu klienta
                        book.ConcurrencyToken = databaseValues.ConcurrencyToken;
                        TempData["BorrowFailed"] = true;
                        TempData["ConcurrencyError"] = true;
                    }

                    // Powrót do widoku z komunikatem o błędzie

                    return RedirectToAction(nameof(Index));
                }
            }

            return RedirectToAction(nameof(Index));
        }
        
        public async Task<IActionResult> ReleaseReservedBooks()
        {
            var expiredLoans = await _context.Loan
                .Where(loan => loan.Status == LoanStatus.Reserved && loan.ReturnDate < DateTime.Now)
                .ToListAsync();

            foreach (var loan in expiredLoans)
            {
                var book = await _context.Book.FindAsync(loan.BookId);
                if (book != null)
                {
                    book.is_loaned = false;
                    _context.Book.Update(book);
                }
                _context.Loan.Remove(loan);
            }

            await _context.SaveChangesAsync();
            return Ok("Expired reservations have been released and loans deleted.");
        }
     
        [HttpPost]
        public string Index(string searchString, bool notUsed)
        {
            return "From [HttpPost]Index: filter on " + searchString;
        }
        
        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .FirstOrDefaultAsync(m => m.id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,author,publisher,date_of_publication,price,history_of_leases")] Book book)
        {
            if (ModelState.IsValid)
            {
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,title,author,publisher,date_of_publication,price,history_of_leases")] Book book)
        {
            if (id != book.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .FirstOrDefaultAsync(m => m.id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Book.FindAsync(id);
            if (book != null)
            {
                _context.Book.Remove(book);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Book.Any(e => e.id == id);
        }
    }
}
