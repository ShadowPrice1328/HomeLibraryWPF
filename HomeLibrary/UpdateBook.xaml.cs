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
    /// Interaction logic for UpdateBook.xaml
    /// </summary>
    public partial class UpdateBook : Window
    {
        public UpdateBook()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var repository = new BookRepository();

            var book = new Book
            {
                Id = 1,
                Name = "The Great Gatsby (Updated)",
                Title = "Classic Novel",
                Year = 1925,
                Description = "Famous novel by F. Scott Fitzgerald",
                Price = 24.99m,
                Publisher = "Penguin",
                Image = null,
                IsLent = false,
                Source = BookSource.Purchased,
                AuthorIds = new List<int> { 1 },
                GenreIds = new List<int> { 3, 4 }
            };

            bool updated = repository.UpdateBook(book);
            Console.WriteLine(updated ? "Book updated!" : "Error while updating");

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var repository = new BookRepository();
            var bookToDelete = repository.DeleteBook(repository.ReadBook(1));
        }
    }
}
