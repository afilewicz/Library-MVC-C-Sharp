using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Library.Models;

public class Loan
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public Guid UserId { get; set; }
    public DateTime LoanDate { get; set; }
    public DateTime ReturnDate { get; set; }
    public bool Returned { get; set; }
}