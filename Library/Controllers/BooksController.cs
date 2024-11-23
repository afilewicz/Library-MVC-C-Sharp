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

        public async Task<IActionResult> Borrow(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book.FindAsync(id);
            if (book == null || book.is_loaned)
            {
                return NotFound();
            }

            Guid userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            
            var loan = new Loan
            {
                BookId = book.id,
                UserId = userId,
                LoanDate = null,
                ReturnDate = DateTime.Now.AddDays(2), // Example return date
                Status = LoanStatus.Reserved
            };
            
            await _context.SaveChangesAsync();

            _context.Loan.Add(loan);
            await _context.SaveChangesAsync();

            // Update the book's loan status
            book.is_loaned = true;
            _context.Book.Update(book);
            await _context.SaveChangesAsync();
            

            return RedirectToAction(nameof(UserIndex));
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
