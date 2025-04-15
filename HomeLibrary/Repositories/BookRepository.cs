using HomeLibrary.Interfaces;
using HomeLibrary.Model;
using Microsoft.Data.SqlClient;

namespace HomeLibrary.Repositories
{
    class BookRepository : IBookRepository
    {
        private readonly DatabaseConnection _dbConnection = new();
        public bool CreateBook(Book book)
        {
            using (SqlConnection conn = _dbConnection.GetConnection())
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int bookExists;

                        string checkBookQuery = "SELECT COUNT(1) FROM Books WHERE Name = @Name";
                        using (SqlCommand checkBookCmd = new(checkBookQuery, conn, transaction))
                        {
                            checkBookCmd.Parameters.AddWithValue("@Name", book.Title);
                            bookExists = (int)checkBookCmd.ExecuteScalar();
                        }

                        if (bookExists == 0)
                        {
                            throw new Exception("Book already exists.");
                        }

                        string query = @"INSERT INTO Books (Year, Description, Title, Image, Source, Lent) 
                                         OUTPUT INSERTED.ID_Book
                                         VALUES (@Year, @Description, @Title, @Image, @Source, @Lent)";
                        using (SqlCommand cmd = new(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Year", new DateTime(book.Year, 1, 1));
                            cmd.Parameters.AddWithValue("@Description", book.Description);
                            cmd.Parameters.AddWithValue("@Title", book.Title);
                            cmd.Parameters.AddWithValue("@Image", book.Image); // Path to image
                            cmd.Parameters.AddWithValue("@Source", book.Source);
                            cmd.Parameters.AddWithValue("@Lent", book.IsLent);

                            book.Id = (int)cmd.ExecuteScalar();
                        }


                        if (book.Authors != null)
                        {
                            for (int i = 0; i < book.Authors.Count; i++)
                            {
                                string checkAuthorQuery = "SELECT COUNT(1) FROM Authors WHERE FirstName = @AuthorId AND LastName = @LastName";
                                using (SqlCommand checkAuthorCmd = new(checkAuthorQuery, conn, transaction))
                                {
                                    checkAuthorCmd.Parameters.AddWithValue("@FirstName", book.Authors[i].FirstName);
                                    checkAuthorCmd.Parameters.AddWithValue("@LastName", book.Authors[i].LastName);

                                    int authorExists = (int)checkAuthorCmd.ExecuteScalar();
                                    int authorId;

                                    if (authorExists == 0)
                                    {
                                        string insertAuthorQuery = "SET IDENTITY_INSERT Authors ON INSERT INTO Authors (FirstName, LastName) " +
                                            "OUTPUT INSERTED.ID_Author" +
                                            "VALUES (@FirstName, @LastName)";

                                        using (SqlCommand insertAuthorCmd = new(insertAuthorQuery, conn, transaction))
                                        {
                                            insertAuthorCmd.Parameters.AddWithValue("@FirstName", book.Authors[i].FirstName);
                                            insertAuthorCmd.Parameters.AddWithValue("@LastName", book.Authors[i].LastName);

                                            authorId = (int)insertAuthorCmd.ExecuteScalar();                                      
                                        }
                                    }
                                    else
                                    {
                                        string getAuthorIdQuery = "SELECT ID_Author FROM Authors WHERE FirstName = @FirstName AND LastName = @LastName";

                                        using (SqlCommand getAuthorIdCmd = new(getAuthorIdQuery, conn, transaction))
                                        {
                                            getAuthorIdCmd.Parameters.AddWithValue("@FirstName", book.Authors[i].FirstName);
                                            getAuthorIdCmd.Parameters.AddWithValue("@LastName", book.Authors[i].LastName);

                                            authorId = (int)getAuthorIdCmd.ExecuteScalar();
                                        }
                                    }

                                    string bookAuthorQuery = "INSERT INTO Books_Authors (ID_Book, ID_Author) VALUES (@BookId, @AuthorId)";

                                    using (SqlCommand bookAuthorCmd = new(bookAuthorQuery, conn, transaction))
                                    {
                                        bookAuthorCmd.Parameters.AddWithValue("@BookId", book.Id);
                                        bookAuthorCmd.Parameters.AddWithValue("@AuthorId", authorId);

                                        bookAuthorCmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }

                        if (book.Genres != null)
                        {
                            for (int i = 0; i < book.Genres.Count; i++)
                            {
                                string checkGenreQuery = "SELECT COUNT(1) FROM Genre WHERE Name = @Name";
                                using (SqlCommand checkGenreCmd = new(checkGenreQuery, conn, transaction))
                                {
                                    checkGenreCmd.Parameters.AddWithValue("@Name", book.Genres[i].Name);

                                    int genreExists = (int)checkGenreCmd.ExecuteScalar();
                                    int genreId;

                                    if (genreExists == 0)
                                    {
                                        string insertGenreQuery = "SET IDENTITY_INSERT Genres ON INSERT INTO Genres (Name) " +
                                            "OUTPUT INSERTED.ID_Genre" +
                                            "VALUES (@Name)";

                                        using (SqlCommand insertGenreCmd = new(insertGenreQuery, conn, transaction))
                                        {
                                            insertGenreCmd.Parameters.AddWithValue("@Name", book.Genres[i].Name);

                                            genreId = (int)insertGenreCmd.ExecuteScalar();
                                        }
                                    }
                                    else
                                    {
                                        string getGenreIdQuery = "SELECT ID_Genre FROM Genres WHERE Name = @Name";

                                        using (SqlCommand getGenreIdCmd = new(getGenreIdQuery, conn, transaction))
                                        {
                                            getGenreIdCmd.Parameters.AddWithValue("@Name", book.Genres[i].Name);

                                            genreId = (int)getGenreIdCmd.ExecuteScalar();
                                        }
                                    }

                                    string bookGenreQuery = "INSERT INTO Books_Genres (ID_Book, ID_Genre) VALUES (@ID_Book, @ID_Genre)";

                                    using (SqlCommand bookGenreCmd = new(bookGenreQuery, conn, transaction))
                                    {
                                        bookGenreCmd.Parameters.AddWithValue("@ID_Book", book.Id);
                                        bookGenreCmd.Parameters.AddWithValue("@ID_Genre", genreId);

                                        bookGenreCmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }
        public bool DeleteBook(string bookTitle)
        {
            using (SqlConnection conn = _dbConnection.GetConnection())
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int bookId;

                        string getBookIdQuery = "SELECT ID_Book FROM Books WHERE Name = @Name";
                        using (SqlCommand getBookIdCmd = new(getBookIdQuery, conn, transaction))
                        {
                            getBookIdCmd.Parameters.AddWithValue("@Name", bookTitle);
                            bookId = (int)getBookIdCmd.ExecuteScalar();
                        }

                        string deleteBookAuthorsQuery = "DELETE FROM Books_Authors WHERE ID_Book = @BookId";
                        using (SqlCommand deleteBookAuthorsCmd = new(deleteBookAuthorsQuery, conn, transaction))
                        {
                            deleteBookAuthorsCmd.Parameters.AddWithValue("@BookId", bookId);
                            deleteBookAuthorsCmd.ExecuteNonQuery();
                        }

                        string deleteBookGenresQuery = "DELETE FROM Books_Genres WHERE ID_Book = @BookId";
                        using (SqlCommand deleteBookGenresCmd = new(deleteBookGenresQuery, conn, transaction))
                        {
                            deleteBookGenresCmd.Parameters.AddWithValue("@BookId", bookId);
                            deleteBookGenresCmd.ExecuteNonQuery();
                        }

                        string deleteBookQuery = "DELETE FROM Books WHERE ID_Book = @BookId";
                        using (SqlCommand deleteBookCmd = new(deleteBookQuery, conn, transaction))
                        {
                            deleteBookCmd.Parameters.AddWithValue("@BookId", bookId);
                            deleteBookCmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        public int GetBookId(string bookTitle, string description)
        {
            int bookId;

            using (SqlConnection conn = _dbConnection.GetConnection())
            {
                conn.Open();
                string query = "SELECT ID_Book FROM Books WHERE Name = @Title AND Description = @Description";
                using (SqlCommand cmd = new(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Title", bookTitle);
                    cmd.Parameters.AddWithValue("@Description", description);

                    bookId = (int)cmd.ExecuteScalar();
                }
            }

            return bookId;
        }
        public Book ReadBook(int id)
        {
            Book? book = null;
            using (SqlConnection conn = _dbConnection.GetConnection())
            {
                conn.Open();
                string query = "SELECT * FROM Books WHERE ID_Book = @Id";
                using (SqlCommand cmd = new(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            book = new Book
                            {
                                Id = reader.GetInt32(0),
                                Year = reader.GetDateTime(1).Year,
                                Description = reader.GetString(2),
                                Title = reader.GetString(3),
                                Image = reader.GetString(4),
                                Source = (BookSource)reader.GetInt32(5),
                                IsLent = reader.GetBoolean(6)
                            };
                        }
                    }
                }

                if (book != null)
                {
                    book.Authors = new List<Author>();
                    string authorQuery = @"
                            SELECT a.FirstName, a.LastName
                            FROM Books_Authors ba
                            INNER JOIN Authors a ON ba.ID_Author = a.ID_Author
                            WHERE ba.ID_Book = @BookId";

                    using (SqlCommand authorCmd = new(authorQuery, conn))
                    {
                        authorCmd.Parameters.AddWithValue("@BookId", id);

                        using (SqlDataReader authorReader = authorCmd.ExecuteReader())
                        {
                            while (authorReader.Read())
                            {
                                string firstName = authorReader.GetString(0);
                                string lastName = authorReader.GetString(1);

                                book.Authors.Add(new Author
                                {
                                    FirstName = firstName,
                                    LastName = lastName
                                });
                            }
                        }
                    }

                    book.Genres = new List<Genre>();
                    string genreQuery = @"
                        SELECT g.Name
                        FROM Books_Genres bg
                        INNER JOIN Genres g ON bg.ID_Genre = g.ID_Genre
                        WHERE bg.ID_Book = @BookId";

                    using (SqlCommand genreCmd = new(genreQuery, conn))
                    {
                        genreCmd.Parameters.AddWithValue("@BookId", id);

                        using (SqlDataReader genreReader = genreCmd.ExecuteReader())
                        {
                            while (genreReader.Read())
                            {
                                string genreName = genreReader.GetString(0);

                                book.Genres.Add(new Genre
                                {
                                    Name = genreName
                                });
                            }
                        }
                    }

                }
            }
            return book;
        }
        public List<Book> ReadBooks()
        {
            List<Book> books = new();

            using (SqlConnection conn = _dbConnection.GetConnection())
            {
                conn.Open();

                string query = "SELECT * FROM Books";
                using (SqlCommand cmd = new(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Book book = new()
                        {
                            Id = reader.GetInt32(0),
                            Year = reader.GetDateTime(1).Year,
                            Description = reader.GetString(2),
                            Title = reader.GetString(3),
                            Image = reader.GetString(4),
                            Source = (BookSource)reader.GetInt32(5),
                            IsLent = reader.GetBoolean(6),
                            Authors = [],
                            Genres = []
                        };
                        books.Add(book);
                    }
                }

                foreach (var book in books)
                {
                    string authorQuery = @"
                        SELECT A.ID_Author, A.FirstName, A.LastName 
                        FROM Authors A
                        JOIN Books_Authors BA ON A.ID_Author = BA.ID_Author
                        WHERE BA.ID_Book = @BookId";

                    using (SqlCommand authorCmd = new(authorQuery, conn))
                    {
                        authorCmd.Parameters.AddWithValue("@BookId", book.Id);
                        using (SqlDataReader authorReader = authorCmd.ExecuteReader())
                        {
                            while (authorReader.Read())
                            {
                                book.Authors.Add(new Author
                                {
                                    Id = authorReader.GetInt32(0),
                                    FirstName = authorReader.GetString(1),
                                    LastName = authorReader.GetString(2)
                                });
                            }
                        }
                    }

                    string genreQuery = @"
                        SELECT G.ID_Genre, G.Name 
                        FROM Genres G
                        JOIN Books_Genres BG ON G.ID_Genre = BG.ID_Genre
                        WHERE BG.ID_Book = @BookId";

                    using (SqlCommand genreCmd = new(genreQuery, conn))
                    {
                        genreCmd.Parameters.AddWithValue("@BookId", book.Id);
                        using (SqlDataReader genreReader = genreCmd.ExecuteReader())
                        {
                            while (genreReader.Read())
                            {
                                book.Genres.Add(new Genre
                                {
                                    Id = genreReader.GetInt32(0),
                                    Name = genreReader.GetString(1)
                                });
                            }
                        }
                    }
                }
            }

            return books;
        }

        public bool UpdateBook(Book book)
        {
            using (SqlConnection conn = _dbConnection.GetConnection())
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string query = @"UPDATE Books 
                                 SET Title = @Title, Year = @Year, Description = @Description,  
                                     Image = @Image, Source = @Source, Lent = @Lent
                                 WHERE ID_Book = @Id";
                        using (SqlCommand cmd = new(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Id", book.Id);
                            cmd.Parameters.AddWithValue("@Title", book.Title);
                            cmd.Parameters.AddWithValue("@Year", book.Year);
                            cmd.Parameters.AddWithValue("@Description", book.Description);
                            cmd.Parameters.AddWithValue("@Image", book.Image);
                            cmd.Parameters.AddWithValue("@Source", book.Source);
                            cmd.Parameters.AddWithValue("@Lent", book.IsLent);

                            cmd.ExecuteNonQuery();
                        }

                        string deleteAuthorsQuery = "DELETE FROM Books_Authors WHERE ID_Book = @BookId";
                        using (SqlCommand deleteAuthorCmd = new(deleteAuthorsQuery, conn, transaction))
                        {
                            deleteAuthorCmd.Parameters.AddWithValue("@BookId", book.Id);
                            deleteAuthorCmd.ExecuteNonQuery();
                        }

                        string deleteGenresQuery = "DELETE FROM Books_Genres WHERE ID_Book = @BookId";
                        using (SqlCommand deleteGenreCmd = new(deleteGenresQuery, conn, transaction))
                        {
                            deleteGenreCmd.Parameters.AddWithValue("@BookId", book.Id);
                            deleteGenreCmd.ExecuteNonQuery();
                        }

                        if (book.Authors != null && book.Authors.Count > 0)
                        {
                            foreach (var author in book.Authors)
                            {
                                string authorQuery = "INSERT INTO Books_Authors (ID_Book, ID_Author) VALUES (@BookId, @AuthorId)";
                                using (SqlCommand authorCmd = new(authorQuery, conn, transaction))
                                {
                                    authorCmd.Parameters.AddWithValue("@BookId", book.Id);
                                    authorCmd.Parameters.AddWithValue("@AuthorId", author.Id);
                                    authorCmd.ExecuteNonQuery();
                                }
                            }
                        }

                        if (book.Genres != null && book.Genres.Count > 0)
                        {
                            foreach (var genre in book.Genres)
                            {
                                string genreQuery = "INSERT INTO Books_Genres (ID_Book, ID_Genre) VALUES (@BookId, @GenreId)";
                                using (SqlCommand genreCmd = new(genreQuery, conn, transaction))
                                {
                                    genreCmd.Parameters.AddWithValue("@BookId", book.Id);
                                    genreCmd.Parameters.AddWithValue("@GenreId", genre.Id);
                                    genreCmd.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }
    }
}
