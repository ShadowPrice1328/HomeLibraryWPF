using HomeLibrary.Model;
using HomeLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HomeLibrary
{
    /// <summary>
    /// Interaction logic for AddBook.xaml
    /// </summary>
    public partial class AddBook : Window
    {
        public AddBook()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var repository = new BookRepository();

            var newBook = new Book
            {
                Name = "The Great Gatsby",
                Title = "Novel",
                Year = 1925,
                Description = "Classic novel",
                Price = 19.99m,
                Publisher = "Scribner",
                Image = "book1.png",
                IsLent = false,
                Source = BookSource.Purchased,
                AuthorIds = new List<int> { 1 }, // TODO: CONVERT AUTHOR NAME TO ID !!
                GenreIds = new List<int> { 3 }     // TODO: CONVERT NAME OF GENRE TO GENRE ID !!
            };

            List<Author> authors = new List<Author> { new Author() { FirstName = "Francis Scott", LastName = " Key Fitzgerald" } };
            List<Genre> genres = new List<Genre> { new Genre() { Name = "Novel" } };

            repository.CreateBook(newBook, authors, genres);
        }
    }
}
