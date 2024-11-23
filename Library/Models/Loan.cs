using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Library.Models;

public enum LoanStatus
{
    Free = 0,
    Reserved = 1,
    Loaned = 2
}


public class Loan
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public Guid UserId { get; set; }
    public DateTime? LoanDate { get; set; }
    public DateTime ReturnDate { get; set; }
    public LoanStatus Status { get; set; } = LoanStatus.Free;
}