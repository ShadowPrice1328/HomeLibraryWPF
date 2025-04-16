using HomeLibrary.Model;
using HomeLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for BookInfo.xaml
    /// </summary>
    public partial class BookInfo : Window
    {
        private Book _book;
        public BookInfo(Book book)
        {
            this._book = book;
            InitializeComponent();

            lbBookTitle.Content = book.Title;
            chbxLent.IsChecked = book.IsLent;
            lbSource.Content = book.Source;
            lbAuthor.Content = string.Join("; ", book.Authors.Select(a => a.FirstName + " " + a.LastName));
            lbGenre.Content = string.Join("; ", book.Genres.Select(g => g.Name));
            lbYear.Content = book.Year;
            txbDescription.Text = book.Description;

            if (!string.IsNullOrEmpty(book.Image) && File.Exists(book.Image))
            {
                var imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, book.Image);
                imgImage.Source = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
            }
            else
            {
                MessageBox.Show("Image file not found. No image will be shown.");
            }
        }

        private void btnDeleteBook_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var bookRepository = new BookRepository();
                var book = new Book { Title = lbBookTitle.Content.ToString() };

                try
                {
                    if (bookRepository.DeleteBook(book.Title))
                    {
                        MessageBox.Show("Book successfully removed!");

                       new ListWindow().Show();
                       Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}");
                }
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            new ListWindow().Show();
            Close();

        }

        private void btnUpdateBook_Click(object sender, RoutedEventArgs e)
        {
            new UpdateBook(_book).Show();
            Close();
        }
    }
}
