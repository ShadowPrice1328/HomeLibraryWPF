using HomeLibrary.Model;
using HomeLibrary.Repositories;
using System.IO;
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
        private BookRepository _repository;
        private string _userName = "Anna";
        public MainWindow()
        {
            InitializeComponent();

            _repository = new BookRepository();
            
            lbWelcome.Content += _userName;
            lbTotal.Content += _repository.GetBooksCount().ToString();

            LoadRecentBooks();
        }

        private void LoadRecentBooks()
        {
            var recentBooks = _repository.GetLastAddedBooks(5);

            StackPanelRecentBooks.Children.Clear();

            foreach (var book in recentBooks)
            {
                if (File.Exists(book.Image))
                {
                    var imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, book.Image);
                    var image = new Image
                    {
                        Source = new BitmapImage(new Uri(imagePath, UriKind.Absolute)),
                        Margin = new Thickness(10),
                        Width = 75,
                        Height = 75,
                        Stretch = Stretch.UniformToFill,
                        Tag = book
                    };

                    image.MouseLeftButtonDown += Image_OnClick;

                    StackPanelRecentBooks.Children.Add(image);
                }
                else
                {
                    MessageBox.Show($"Image not found: {book.Image}");
                }
            }
        }

        private void Image_OnClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Image clickedImage && clickedImage.Tag is Book clickedBook)
            {
                new BookInfo(clickedBook).Show();
                Close();
            }
        }

        private void btnShowAll_Click(object sender, RoutedEventArgs e)
        {
            ListWindow listWindow = new();
            listWindow.Show();
            Close();
        }
    }
}