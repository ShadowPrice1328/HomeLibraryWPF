using HomeLibrary.Model;
using HomeLibrary.Repositories;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HomeLibrary
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _userName = "Anna";
        public MainWindow()
        {
            InitializeComponent();

            lbWelcome.Content += _userName;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var repository = new BookRepository();
            //List<Book> books = repository.ReadBooks();
            //foreach (var book in books)
            //{
            //    Console.WriteLine($"Book: {book.Title}, AuthorIds: {string.Join(",", book.AuthorIds)}");
            //}

        }
    }
}