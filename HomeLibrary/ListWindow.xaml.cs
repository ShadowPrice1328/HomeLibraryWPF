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
    /// Interaction logic for ListWindow.xaml
    /// </summary>
    public partial class ListWindow : Window
    {
        private BookRepository _repository;
        public ListWindow()
        {
            InitializeComponent();

            _repository = new BookRepository();

            lbTotal.Content += _repository.GetBooksCount().ToString();
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
    }
}
