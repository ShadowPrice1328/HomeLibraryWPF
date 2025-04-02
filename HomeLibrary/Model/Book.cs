using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeLibrary.Model
{
    /// <summary>
    /// Domain model for Book
    /// </summary>
    class Book
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int Year { get; set; }
        public decimal Price { get; set; }
        public string? Publisher { get; set; }
        public string? Image { get; set; }
        public bool IsLent {  get; set; }
        public BookSource Source { get; set; }

        // Many-to-Many
        public List<int>? AuthorIds { get; set; }
        public List<int>? GenreIds { get; set; }
    }
}
