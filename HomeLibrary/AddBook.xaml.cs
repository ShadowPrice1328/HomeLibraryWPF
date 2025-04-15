using HomeLibrary.Model;
using HomeLibrary.Repositories;
using Microsoft.Win32;
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
    /// Interaction logic for AddBook.xaml
    /// </summary>
    public partial class AddBook : Window
    {
        private string userSelectedImagePath;
        public AddBook()
        {
            InitializeComponent();
        }

        private void ChooseImage_Click(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select an image",
                Filter = "Image files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                userSelectedImagePath = openFileDialog.FileName;

                var bitmap = new BitmapImage(new Uri(userSelectedImagePath));
                SelectedImage.Source = bitmap;

                var label = this.FindName("ClickToUploadLabel") as Label;
                if (label != null)
                {
                    label.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void btnAddBook_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrEmpty(userSelectedImagePath))
            {
                MessageBox.Show("Please select an image before adding the book.");
                return;
            }

            var repository = new BookRepository();

            try
            {
                string imagePath = SaveImage(userSelectedImagePath);
                string[] authors = tbxAuthor.Text.Split(';', StringSplitOptions.TrimEntries);

                List<Author> authorList = [];

                for (int i = 0; i < authors.Length; i++)
                {
                    string[] nameParts = authors[i].Split(' ', StringSplitOptions.TrimEntries);

                    if (nameParts.Length >= 2)
                    {
                        string firstName = nameParts[0];
                        string lastName = nameParts[1];

                        authorList.Add(new Author { FirstName = firstName, LastName = lastName });
                    }
                    else
                    {
                        MessageBox.Show("Author format is incorrect. Make sure to input both first and last name.");
                    }
                }

                string[] genres = tbxGenre.Text.Split(';', StringSplitOptions.TrimEntries);
                List<Genre> genresList = [];

                for (int i = 0; i < genres.Length; i++)
                {
                    genresList.Add(new Genre { Name = genres[i] });
                }

                var newBook = new Book
                {
                    Title = tbBookName.Text,
                    Year = int.TryParse(tbxYear.Text, out int year) ? year : throw new FormatException("Year must be a number."),
                    Description = tbBookDescription.Text,
                    Image = imagePath,
                    Source = (BookSource)lbxSource.SelectedItem,
                    IsLent = (bool)chbxLent.IsChecked,
                    Authors = authorList,
                    Genres = genresList
                };

                if (repository.CreateBook(newBook))
                {
                    MessageBox.Show("Book added successfully!");

                    Close();
                    ListWindow listWindow = new();
                    listWindow.Show();
                }
                else
                {
                    MessageBox.Show("An error occurred! Try again!");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }

        }

        private string SaveImage(string sourcePath)
        {
            if (string.IsNullOrEmpty(sourcePath))
                return null;

            try
            {
                string projectRoot = AppDomain.CurrentDomain.BaseDirectory;

                string serverPath = System.IO.Path.Combine(projectRoot, "Images");

                if (!Directory.Exists(serverPath))
                {
                    Directory.CreateDirectory(serverPath);
                }

                string fileName = $"image_{Guid.NewGuid()}.jpg";

                string destinationPath = System.IO.Path.Combine(serverPath, fileName);

                File.Copy(sourcePath, destinationPath, true);

                return System.IO.Path.Combine("Images", fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving image: {ex.Message}");
                return null;
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            Close();
            ListWindow listWindow = new();
            listWindow.Show();
        }
    }
}
