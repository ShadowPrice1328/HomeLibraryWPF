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
    public class Book
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int Year { get; set; }
        public string? Image { get; set; } // Path to image
        public bool IsLent {  get; set; }
        public BookSource Source { get; set; }

        // Many-to-Many
        public List<Author>? Authors { get; set; }
        public List<Genre>? Genres { get; set; }
    }
}
