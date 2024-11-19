using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    [Column(TypeName = "decimal(18, 2)")]
    [Display(Name = "History of leases")]
    public string history_of_leases {get; set; }
}