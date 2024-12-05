using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Library.Models;

public class Book
{
    public int id { get; set; }
    public string title { get; set; }
    public string author { get; set; }
    public string publisher  {get; set; }
    
    [Display(Name = "Date of publication")]
    [DataType(DataType.Date)]
    public DateTime date_of_publication {get; set; }
    public decimal price {get; set; }
    
    [Display(Name = "History of leases")]
    [ForeignKey("Loan")]
    public int? history_of_leases { get; set; }
    public bool is_loaned { get; set; } = false;
    public Guid ConcurrencyToken { get; set; } = Guid.NewGuid();
}