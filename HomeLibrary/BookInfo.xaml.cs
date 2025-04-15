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
    /// Interaction logic for BookInfo.xaml
    /// </summary>
    public partial class BookInfo : Window
    {
        public BookInfo()
        {
            InitializeComponent();
        }

        private void btnDeleteBook_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var bookRepository = new BookRepository();
                var book = new Book { Title = lbAuthor.Content.ToString() };

                try
                {
                    if (bookRepository.DeleteBook(book.Title))
                    {
                        Close();

                        MessageBox.Show("Book successfully removed!");

                        Close();
                        ListWindow listWindow = new ListWindow();
                        listWindow.Show();
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
            Close();
            ListWindow listWindow = new();
            listWindow.Show();
        }

        private void btnUpdateBook_Click(object sender, RoutedEventArgs e)
        {
            Close();
            UpdateBook updateBook = new UpdateBook();
            updateBook.Show();
        }
    }
}
