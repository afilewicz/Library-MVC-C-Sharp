namespace Library.Models
{
    public class LoanViewModel
    {
        public int Id { get; set; }
        public string BookTitle { get; set; }
        public string UserName { get; set; }
        public DateTime LoanDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public bool Returned { get; set; }
    }
}