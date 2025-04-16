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

        public UpdateBook(Book book)
        {
            InitializeComponent();

            userSelectedImagePath = book.Image;


            tbBookName.Text = book.Title;
            chbxLent.IsChecked = book.IsLent;
            lbxSource.SelectedItem = book.Source;
            tbxAuthor.Text = string.Join("; ", book.Authors.Select(a => $"{a.FirstName} {a.LastName}"));
            tbxGenre.Text = string.Join("; ", book.Genres.Select(g => g.Name));
            tbxYear.Text = book.Year.ToString();
            tbBookDescription.Text = book.Description;

            if (!string.IsNullOrEmpty(book.Image) && File.Exists(book.Image))
            {
                var imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, book.Image);
                SelectedImage.Source = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
            }
            else
            {
                MessageBox.Show("Image file not found. No image will be shown.");
            }
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
            }
        }

        private bool UpdateImage(string oldImagePath, string newImagePath)
        {
            // First, we ensure that the old image is not in use.
            if (SelectedImage.Source is BitmapImage oldBitmapImage)
            {
                oldBitmapImage?.StreamSource?.Dispose();
            }

            // Now, attempt to delete the old image if it exists
            if (File.Exists(oldImagePath))
            {
                try
                {
                    // Check if the new image is identical to the old one
                    string oldImageHash = CalculateFileHash(oldImagePath);
                    string newImageHash = CalculateFileHash(newImagePath);

                    // Skip deletion if the images are identical
                    if (oldImageHash == newImageHash)
                    {
                        Console.WriteLine("Images are identical. No update needed...");
                        return false;
                    }

                    // Try to delete the old image file
                    File.Delete(oldImagePath);
                    Console.WriteLine("Old image deleted successfully.");
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Failed to delete the old image. It might be in use by another process: {ex.Message}");
                    return false;
                }
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

                // Retry logic for file copying (e.g., handle if the file is locked by another process)
                const int maxRetries = 3;
                int attempts = 0;
                while (attempts < maxRetries)
                {
                    try
                    {
                        File.Copy(sourcePath, destinationPath, true);
                        return Path.Combine("Images", fileName);
                    }
                    catch (IOException ex)
                    {
                        attempts++;
                        if (attempts >= maxRetries)
                        {
                            MessageBox.Show($"Error copying image: {ex.Message}. Please try again later.");
                            return null;
                        }
                        else
                        {
                            // Wait a short time and try again if the file is locked
                            System.Threading.Thread.Sleep(500);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving image: {ex.Message}");
                return null;
            }

            return null;
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
                        new ListWindow().Show();
                        Close();
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
            new ListWindow().Show();
            Close();
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
                    Id = bookId,
                    Title = tbBookName.Text,
                    Year = int.TryParse(tbxYear.Text, out int year) ? year : throw new FormatException("Year must be a valid number."),
                    Description = tbBookDescription.Text,
                    Image = imagePath,
                    Source = Enum.TryParse<BookSource>(lbxSource.SelectedItem?.ToString(), out var source)
                    ? source
                    : BookSource.Purchased,
                    IsLent = chbxLent.IsChecked ?? false,
                    Authors = authors,
                    Genres = genres
                };

                if (repository.UpdateBook(newBook))
                {
                    MessageBox.Show("Book updated successfully!");

                    new ListWindow().Show();
                    Close();
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
