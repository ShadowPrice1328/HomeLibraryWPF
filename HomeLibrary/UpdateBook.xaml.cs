using HomeLibrary.Model;
using HomeLibrary.Repositories;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace HomeLibrary
{
    public partial class UpdateBook : Window
    {
        private string userSelectedImagePath;

        public UpdateBook()
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

                if (this.FindName("ClickToUpdateLabel") is Label label)
                {
                    label.Visibility = Visibility.Collapsed;
                }
            }
        }

        private bool UpdateImage(string oldImagePath, string newImagePath)
        {
            if (File.Exists(oldImagePath))
            {
                string oldImageHash = CalculateFileHash(oldImagePath);
                string newImageHash = CalculateFileHash(newImagePath);

                if (oldImageHash == newImageHash)
                {
                    Console.WriteLine("Images are identical. No update needed...");
                    return false;
                }

                File.Delete(oldImagePath);
            }

            return true;
        }

        private string CalculateFileHash(string filePath)
        {
            using (var sha256 = SHA256.Create())
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hash = sha256.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        private string SaveImage(string sourcePath, int bookId)
        {
            if (string.IsNullOrEmpty(sourcePath))
                return null;

            try
            {
                string projectRoot = AppDomain.CurrentDomain.BaseDirectory;
                string serverPath = Path.Combine(projectRoot, "Images");

                if (!Directory.Exists(serverPath))
                {
                    Directory.CreateDirectory(serverPath);
                }

                var bookRepository = new BookRepository();
                var existingBook = bookRepository.ReadBook(bookId);

                if (existingBook == null || string.IsNullOrEmpty(existingBook.Image))
                    throw new FileNotFoundException("Existing book image not found.");

                string fileName = $"image_{Guid.NewGuid()}.jpg";
                string destinationPath = Path.Combine(serverPath, fileName);

                if (!UpdateImage(existingBook.Image, sourcePath))
                {
                    return existingBook.Image;
                }

                File.Copy(sourcePath, destinationPath, true);
                return Path.Combine("Images", fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving image: {ex.Message}");
                return null;
            }
        }

        private void btnRemoveBook_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var bookRepository = new BookRepository();
                string bookTitle = tbBookName.Text;

                try
                {
                    if (bookRepository.DeleteBook(bookTitle))
                    {
                        MessageBox.Show("Book successfully removed!");
                        Close();
                        new ListWindow().Show();
                    }
                    else
                    {
                        MessageBox.Show("Book not found or couldn't be deleted.");
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
            new ListWindow().Show();
        }

        private void btnUpdateBook_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(userSelectedImagePath))
            {
                MessageBox.Show("Please select an image before adding the book.");
                return;
            }

            var repository = new BookRepository();

            try
            {
                int bookId = repository.GetBookId(tbBookName.Text, tbBookDescription.Text);
                string imagePath = SaveImage(userSelectedImagePath, bookId);

                var authors = tbxAuthor.Text
                    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(author =>
                    {
                        var parts = author.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length < 2) throw new FormatException("Each author must have both first and last names.");
                        return new Author { FirstName = parts[0], LastName = parts[1] };
                    }).ToList();

                var genres = tbxGenre.Text
                    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(genre => new Genre { Name = genre }).ToList();

                var newBook = new Book
                {
                    Title = tbBookName.Text,
                    Year = int.TryParse(tbxYear.Text, out int year) ? year : throw new FormatException("Year must be a valid number."),
                    Description = tbBookDescription.Text,
                    Image = imagePath,
                    Source = (BookSource)lbxSource.SelectedItem,
                    IsLent = chbxLent.IsChecked ?? false,
                    Authors = authors,
                    Genres = genres
                };

                if (repository.UpdateBook(newBook))
                {
                    MessageBox.Show("Book updated successfully!");

                    Close();
                    new ListWindow().Show();
                }
                else
                {
                    MessageBox.Show("Failed to update the book. Please try again.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
    }
}
