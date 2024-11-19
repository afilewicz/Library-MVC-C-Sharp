using Microsoft.AspNetCore.Mvc.Rendering;
namespace Library.Models;

public class BookPublisherViewModel
{
    public List<Book>? Books { get; set; }
    public SelectList? Publishers { get; set; }
    public string? BookPublisher { get; set; }
    public string? SearchString { get; set; }
}