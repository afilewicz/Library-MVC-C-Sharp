using System.ComponentModel.DataAnnotations;

namespace Library.Models;

public class Book
{
    public int id { get; set; }
    public string author { get; set; }
    public string publisher  {get; set; }
    [DataType(DataType.Date)]
    public DateTime date_of_publication {get; set; }
    public decimal price {get; set; }
    public string history_of_leases {get; set; }
}