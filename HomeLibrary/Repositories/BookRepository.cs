using HomeLibrary.Interfaces;
using HomeLibrary.Model;
using Microsoft.Data.SqlClient;

namespace HomeLibrary.Repositories
{
    class BookRepository : IBookRepository
    {
        private readonly DatabaseConnection _dbConnection = new();
        public bool CreateBook(Book book, List<Author>? authors = null, List<Genre>? genres = null)
        {
            using (SqlConnection conn = _dbConnection.GetConnection())
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string query = @"INSERT INTO Books (Name, Year, Description, Price, Publisher, Title, Image, Source, Lent) 
                                         OUTPUT INSERTED.ID_Book
                                         VALUES (@Name, @Year, @Description, @Price, @Publisher, @Title, @Image, @Source, @Lent)";
                        using (SqlCommand cmd = new(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Name", book.Name);
                            cmd.Parameters.AddWithValue("@Year", new DateTime(book.Year, 1, 1));
                            cmd.Parameters.AddWithValue("@Description", book.Description);
                            cmd.Parameters.AddWithValue("@Price", book.Price);
                            cmd.Parameters.AddWithValue("@Publisher", book.Publisher);
                            cmd.Parameters.AddWithValue("@Title", book.Title);
                            cmd.Parameters.AddWithValue("@Image", book.Image);
                            cmd.Parameters.AddWithValue("@Source", book.Source);
                            cmd.Parameters.AddWithValue("@Lent", book.IsLent);

                            book.Id = (int)cmd.ExecuteScalar();
                        }


                        if (book.AuthorIds != null)
                        {
                            for (int i = 0; i < book.AuthorIds.Count; i++)
                            {
                                string checkAuthorQuery = "SELECT COUNT(1) FROM Authors WHERE ID_Author = @AuthorId";
                                using (SqlCommand checkAuthorCmd = new(checkAuthorQuery, conn, transaction))
                                {
                                    checkAuthorCmd.Parameters.AddWithValue("@AuthorId", book.AuthorIds[i]);
                                    int authorExists = (int)checkAuthorCmd.ExecuteScalar();

                                    if (authorExists == 0)
                                    {
                                        string insertAuthorQuery = "SET IDENTITY_INSERT Authors ON INSERT INTO Authors (ID_Author, FirstName, LastName) VALUES (@AuthorId, @FirstName, @LastName)";
                                        using (SqlCommand insertAuthorCmd = new(insertAuthorQuery, conn, transaction))
                                        {
                                            insertAuthorCmd.Parameters.AddWithValue("@AuthorId", book.AuthorIds[i]);
                                            insertAuthorCmd.Parameters.AddWithValue("@FirstName", authors[i].FirstName);
                                            insertAuthorCmd.Parameters.AddWithValue("@LastName", authors[i].LastName);
                                            insertAuthorCmd.ExecuteNonQuery();
                                        }
                                    }
                                }

                                string authorQuery = "INSERT INTO Books_Authors (ID_Book, ID_Author) VALUES (@BookId, @AuthorId)";
                                using (SqlCommand authorCmd = new(authorQuery, conn, transaction))
                                {
                                    authorCmd.Parameters.AddWithValue("@BookId", book.Id);
                                    authorCmd.Parameters.AddWithValue("@AuthorId", book.AuthorIds[i]);
                                    authorCmd.ExecuteNonQuery();
                                }
                            }
                        }

                        if (book.GenreIds != null)
                        {
                            for (int i = 0; i < book.GenreIds.Count; i++)
                            {
                                string checkGenreQuery = "SELECT COUNT(1) FROM Genres WHERE ID_Genre = @GenreId";
                                using (SqlCommand checkGenreCmd = new(checkGenreQuery, conn, transaction))
                                {
                                    checkGenreCmd.Parameters.AddWithValue("@GenreId", book.GenreIds[i]);
                                    int genreExists = (int)checkGenreCmd.ExecuteScalar();

                                    if (genreExists == 0)
                                    {
                                        string insertGenreQuery = "SET IDENTITY_INSERT Genres ON INSERT INTO Genres (ID_Genre, Name) VALUES (@GenreId, @Name)";
                                        using (SqlCommand insertGenreCmd = new(insertGenreQuery, conn, transaction))
                                        {
                                            insertGenreCmd.Parameters.AddWithValue("@GenreId", book.GenreIds[i]);
                                            insertGenreCmd.Parameters.AddWithValue("@Name", genres[i].Name);
                                            insertGenreCmd.ExecuteNonQuery();
                                        }
                                    }
                                }

                                string genreQuery = "INSERT INTO Books_Genres (ID_Book, ID_Genre) VALUES (@BookId, @GenreId)";
                                using (SqlCommand genreCmd = new(genreQuery, conn, transaction))
                                {
                                    genreCmd.Parameters.AddWithValue("@BookId", book.Id);
                                    genreCmd.Parameters.AddWithValue("@GenreId", book.GenreIds[i]);
                                    genreCmd.ExecuteNonQuery();
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

        public bool DeleteBook(Book book)
        {
            throw new NotImplementedException();
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
                                Name = reader.GetString(1),
                                Year = reader.GetDateTime(2).Year,
                                Description = reader.GetString(3),
                                Price = reader.GetDecimal(4),
                                Publisher = reader.GetString(5),
                                Title = reader.GetString(6),
                                Image = reader.GetString(7),
                                Source = (BookSource)reader.GetInt32(8),
                                IsLent = reader.GetBoolean(9)
                            };
                        }
                    }
                }

                if (book != null)
                {
                    book.AuthorIds = new();
                    string authorQuery = "SELECT ID_Author FROM Books_Authors WHERE ID_Book = @BookId";
                    using (SqlCommand authorCmd = new(authorQuery, conn))
                    {
                        authorCmd.Parameters.AddWithValue("@BookId", id);
                        using (SqlDataReader authorReader = authorCmd.ExecuteReader())
                        {
                            while (authorReader.Read())
                            {
                                book.AuthorIds.Add(authorReader.GetInt32(0));
                            }
                        }
                    }

                    book.GenreIds = new();
                    string genreQuery = "SELECT ID_Genre FROM Books_Genres WHERE ID_Book = @BookId";
                    using (SqlCommand genreCmd = new(genreQuery, conn))
                    {
                        genreCmd.Parameters.AddWithValue("@BookId", id);
                        using (SqlDataReader genreReader = genreCmd.ExecuteReader())
                        {
                            while (genreReader.Read())
                            {
                                book.GenreIds.Add(genreReader.GetInt32(0));
                            }
                        }
                    }
                }
            }
            return book;
        }
        public List<Book> ReadBooks()
        {
            List<Book> books = new List<Book>();

            using (SqlConnection conn = _dbConnection.GetConnection())
            {
                conn.Open();
                string query = "SELECT * FROM Books";
                using (SqlCommand cmd = new(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Book book = new Book
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Year = reader.GetDateTime(2).Year,
                            Description = reader.GetString(3),
                            Price = reader.GetDecimal(4),
                            Publisher = reader.GetString(5),
                            Title = reader.GetString(6),
                            Image = reader.GetString(7),
                            Source = (BookSource)reader.GetInt32(8),
                            IsLent = reader.GetBoolean(9),
                            AuthorIds = new List<int>(),
                            GenreIds = new List<int>()
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
                WHERE BA.ID_Author = @BookId";
                    using (SqlCommand authorCmd = new(authorQuery, conn))
                    {
                        authorCmd.Parameters.AddWithValue("@BookId", book.Id);
                        using (SqlDataReader authorReader = authorCmd.ExecuteReader())
                        {
                            while (authorReader.Read())
                            {
                                book.AuthorIds.Add(authorReader.GetInt32(0));
                            }
                        }
                    }

                    string genreQuery = @"
                SELECT G.Id, G.Name 
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
                                book.GenreIds.Add(genreReader.GetInt32(0));
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
                                 SET Name = @Name, Year = @Year, Description = @Description, 
                                     Price = @Price, Publisher = @Publisher, Title = @Title, 
                                     Image = @Image, Source = @Source, Lent = @Lent
                                 WHERE Id = @Id";
                        using (SqlCommand cmd = new(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Id", book.Id);
                            cmd.Parameters.AddWithValue("@Name", book.Name);
                            cmd.Parameters.AddWithValue("@Year", book.Year);
                            cmd.Parameters.AddWithValue("@Description", book.Description);
                            cmd.Parameters.AddWithValue("@Price", book.Price);
                            cmd.Parameters.AddWithValue("@Publisher", book.Publisher);
                            cmd.Parameters.AddWithValue("@Title", book.Title);
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

                        if (book.AuthorIds != null)
                        {
                            foreach (int authorId in book.AuthorIds)
                            {
                                string authorQuery = "INSERT INTO Books_Authors (ID_Book, ID_Author) VALUES (@BookId, @AuthorId)";
                                using (SqlCommand authorCmd = new(authorQuery, conn, transaction))
                                {
                                    authorCmd.Parameters.AddWithValue("@BookId", book.Id);
                                    authorCmd.Parameters.AddWithValue("@AuthorId", authorId);
                                    authorCmd.ExecuteNonQuery();
                                }
                            }
                        }

                        if (book.GenreIds != null)
                        {
                            foreach (int genreId in book.GenreIds)
                            {
                                string genreQuery = "INSERT INTO Books_Genres (ID_Book, ID_Genre) VALUES (@BookId, @GenreId)";
                                using (SqlCommand genreCmd = new(genreQuery, conn, transaction))
                                {
                                    genreCmd.Parameters.AddWithValue("@BookId", book.Id);
                                    genreCmd.Parameters.AddWithValue("@GenreId", genreId);
                                    genreCmd.ExecuteNonQuery();
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
    }
}
