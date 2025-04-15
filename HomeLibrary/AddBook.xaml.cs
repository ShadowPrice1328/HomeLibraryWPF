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

            if (string.IsNullOrEmpty(tbBookName.Text))
            {
                MessageBox.Show("Please enter the book title.");
                return;
            }

            if (!int.TryParse(tbxYear.Text, out int year) || year < 1753 || year > DateTime.Now.Year)
            {
                MessageBox.Show("Please enter a valid year (between 1753 and the current year).");
                return;
            }

            var authors = tbxAuthor.Text.Split(';', StringSplitOptions.TrimEntries);
            if (authors.Length == 0 || authors.Any(a => a.Split(' ').Length < 2))
            {
                MessageBox.Show("Please enter authors in the correct format: 'FirstName LastName'. Separate multiple authors with ';'.");
                return;
            }

            var genres = tbxGenre.Text.Split(';', StringSplitOptions.TrimEntries);
            if (genres.Length == 0 || genres.All(g => string.IsNullOrEmpty(g)))
            {
                MessageBox.Show("Please enter at least one genre.");
                return;
            }

            if (string.IsNullOrEmpty(tbBookDescription.Text.Trim()))
            {
                MessageBox.Show("Please enter a book description.");
                return;
            }

            if (lbxSource.SelectedItem == null)
            {
                MessageBox.Show("Please select a source.");
                return;
            }

            var repository = new BookRepository();

            try
            {
                string imagePath = SaveImage(userSelectedImagePath);

                List<Author> authorList = new List<Author>();
                foreach (var author in authors)
                {
                    string[] nameParts = author.Split(' ', StringSplitOptions.TrimEntries);
                    if (nameParts.Length >= 2)
                    {
                        string firstName = nameParts[0];
                        string lastName = nameParts[1];
                        authorList.Add(new Author { FirstName = firstName, LastName = lastName });
                    }
                    else
                    {
                        MessageBox.Show("Author format is incorrect. Make sure to input both first and last name.");
                        return;
                    }
                }

                List<Genre> genresList = new List<Genre>();
                foreach (var genre in genres)
                {
                    if (!string.IsNullOrEmpty(genre))
                    {
                        genresList.Add(new Genre { Name = genre });
                    }
                }

                var selectedItem = lbxSource.SelectedItem as ListBoxItem;
                if (selectedItem != null)
                {
                    string selectedSource = selectedItem.Content.ToString();
                    if (Enum.TryParse(selectedSource, out BookSource source))
                    {
                        var newBook = new Book
                        {
                            Title = tbBookName.Text,
                            Year = year,
                            Description = tbBookDescription.Text,
                            Image = imagePath,
                            Source = source,
                            IsLent = (bool)chbxLent.IsChecked,
                            Authors = authorList,
                            Genres = genresList
                        };

                        if (repository.CreateBook(newBook))
                        {
                            MessageBox.Show("Book added successfully!");
                            new ListWindow().Show();
                            Close();
                        }
                        else
                        {
                            MessageBox.Show("An error occurred! Book already exists or there was an error while saving the data.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid source selected.");
                    }
                }
                else
                {
                    MessageBox.Show("Please select a source.");
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
            new ListWindow().Show();
            Close();
        }

        private void tbBookName_LostFocus(object sender, RoutedEventArgs e)
        {
            tbBookName.Text = tbBookName.Text == string.Empty ? "Book name..." : tbBookName.Text;
        }

        private void tbBookName_GotFocus(object sender, RoutedEventArgs e)
        {
            tbBookName.Text = tbBookName.Text == "Book name..." ? string.Empty : tbBookName.Text;
        }
    }
}
