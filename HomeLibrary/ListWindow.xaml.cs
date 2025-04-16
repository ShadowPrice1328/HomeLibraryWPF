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
using static System.Reflection.Metadata.BlobBuilder;

namespace HomeLibrary
{
    /// <summary>
    /// Interaction logic for ListWindow.xaml
    /// </summary>
    public partial class ListWindow : Window
    {
        private BookRepository _repository;
        private List<Book> books;
        private bool isInitialized = false;


        public ListWindow()
        {
            InitializeComponent();

            _repository = new BookRepository();

            lbTotal.Content += _repository.GetBooksCount().ToString();

            books = _repository.ReadBooks();

            if (books.Count == 0)
            {
                MessageBox.Show("No books available.");
                return;
            }

            foreach (var book in books)
            {
                if (!string.IsNullOrEmpty(book.Image))
                {
                    string imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, book.Image);
                    if (System.IO.File.Exists(imagePath))
                    {
                        book.Image = imagePath;
                    }
                }
            }

            BooksControl.ItemsSource = books;

            isInitialized = true;
        }


        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }

        private void btnAddBook_Click(object sender, RoutedEventArgs e)
        {
            AddBook addBook = new AddBook();
            addBook.Show();
            Close();
        }

        private void tbSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            tbSearch.Text = tbSearch.Text == "Search..." ? string.Empty : tbSearch.Text;
        }

        private void BookImage_OnClick(object sender, MouseButtonEventArgs e)
        {
            // Get the clicked book object from the DataContext or Tag
            if (sender is Image clickedImage && clickedImage.DataContext is Book clickedBook)
            {
                // Open the BookInfo window and pass the selected book
                BookInfo bookInfoWindow = new BookInfo(clickedBook);
                bookInfoWindow.Show();
                Close();
            }
        }



        private void tbSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            tbSearch.Text = tbSearch.Text == string.Empty ? "Search..." : tbSearch.Text;
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isInitialized) return;

            string searchText = tbSearch.Text.ToLower().Trim();

            // If search is empty, display all books
            if (string.IsNullOrEmpty(searchText))
            {
                BooksControl.ItemsSource = books;
                return;
            }

            // Filter books based on title or description containing the search text
            var filteredBooks = books.Where(book =>
                (book.Title != null && book.Title.ToLower().Contains(searchText)) ||
                (book.Description != null && book.Description.ToLower().Contains(searchText))
            ).ToList();

            // Update the ItemsSource to the filtered list
            BooksControl.ItemsSource = filteredBooks;
        }

        private void SortBooks(string sortBy)
        {
            IEnumerable<Book> sortedBooks = books;

            switch (sortBy)
            {
                case "Title":
                    sortedBooks = books.OrderBy(b => b.Title ?? string.Empty);  // Ensure Title is not null
                    break;

                case "Author":
                    sortedBooks = books.OrderBy(b => string.Join(" ", b.Authors?.Select(a => a.FirstName + " " + a.LastName) ?? new List<string>()));
                    break;

                case "Genre":
                    sortedBooks = books.OrderBy(b => string.Join(" ", b.Genres?.Select(g => g.Name) ?? new List<string>()));
                    break;

                case "Year":
                    sortedBooks = books.OrderBy(b => b.Year);
                    break;

                default:
                    sortedBooks = books;
                    break;
            }

            BooksControl.ItemsSource = null;
            BooksControl.ItemsSource = sortedBooks.ToList();
        }


        private void SortOption_Checked(object sender, RoutedEventArgs e)
        {
            if (!isInitialized) return;

            if (sender is RadioButton radioButton && radioButton.IsChecked == true)
            {
                string sortBy = radioButton.Content?.ToString();
                if (!string.IsNullOrEmpty(sortBy))
                {
                    SortBooks(sortBy);
                }
            }
        }

        private void chbShowLent_Checked(object sender, RoutedEventArgs e)
        {
            if (!isInitialized) return;

            // Перевірка на наявність завантажених книг
            if (books == null || books.Count == 0)
            {
                MessageBox.Show("Books data is not loaded.");
                return;
            }

            // Якщо галочка поставлена, показуємо всі книги, включаючи позичені
            var filteredBooks = books;

            // Якщо галочка активована, не фільтруємо книги (показуємо всі)
            if (chbShowLent.IsChecked == true)
            {
                filteredBooks = books.ToList();
            }
            else
            {
                // Якщо галочка не активована, показуємо лише книги, які не позичені
                filteredBooks = books.Where(book => !book.IsLent).ToList();
            }

            BooksControl.ItemsSource = filteredBooks;
        }

        private void chbShowLent_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!isInitialized) return;

            // Перевірка на наявність завантажених книг
            if (books == null || books.Count == 0)
            {
                MessageBox.Show("Books data is not loaded.");
                return;
            }

            // Якщо галочка знята, фільтруємо книги так, щоб показувати лише ті, що не позичені
            var filteredBooks = books.Where(book => !book.IsLent).ToList();

            BooksControl.ItemsSource = filteredBooks;
        }


        private void expSortBy_Expanded(object sender, RoutedEventArgs e)
        {
            expSortBy.SetValue(Panel.ZIndexProperty, 2);
        }

        private void expSortBy_Collapsed(object sender, RoutedEventArgs e)
        {
            expSortBy.SetValue(Panel.ZIndexProperty, 0);
        }
    }
}
