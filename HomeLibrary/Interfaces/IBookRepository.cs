using HomeLibrary.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace HomeLibrary.Interfaces
{
    interface IBookRepository
    {
        /// <summary>
        /// Adds new Book record to database
        /// </summary>
        /// <param name="book">Book to add</param>
        /// <returns>True if success and False is error has occured</returns>
        bool CreateBook(Book book);

        /// <summary>
        /// Retrieves all books from database
        /// </summary>
        /// <returns>List of all books</returns>
        List<Book> ReadBooks();

        /// <summary>
        /// Retrieves desired _book from database
        /// </summary>
        /// <param name="id">Id of desired _book</param>
        /// <returns>Book item</returns>
        Book ReadBook(int id);

        /// <summary>
        /// Updates _book in database
        /// </summary>
        /// <param name="book">Book to update</param>
        /// <returns>True if success and False is error has occured</returns>
        bool UpdateBook(Book book);

        /// <summary>
        /// Removes _book from database
        /// </summary>
        /// <param name="book">Book to delete</param>
        /// <returns>True if success and False is error has occured</returns>
        bool DeleteBook(string bookTitle);
    }
}
