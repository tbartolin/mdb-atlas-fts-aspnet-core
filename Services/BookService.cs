using BooksApi.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

using MongoDB.Bson;

namespace BooksApi.Services
{
    public class BookService
    {
        private readonly IMongoCollection<Book> _books;

        public BookService(IBookstoreDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _books = database.GetCollection<Book>(settings.BooksCollectionName);
        }

        public List<Book> Get() =>
            _books.Find(book => true).ToList();

        public Book Get(string id) =>
            _books.Find<Book>(book => book.Id == id).FirstOrDefault();

        public Book Create(Book book)
        {
            _books.InsertOne(book);
            return book;
        }

        public List<Book> Search(string term)
        {

            //_books.Aggregate()

            //var aggregate = _books.Aggregate()
            //    .se
            //                          .Group(new BsonDocument { { "_id", "$token" }, { "count", new BsonDocument("$sum", 1) } })
            //                          .Sort(new BsonDocument { { "count", -1 } })
            //                          .Limit(10);


            //var results = await aggregate.ToListAsync();


            //{ "User", "Tom"},



                var fuzzySearch = new BsonDocument
                {
                    {
                        "$searchBeta", new BsonDocument
                        {
                            { 
                                "term", new BsonDocument
                                {
                                    {"path", "Name"},
                                    { "query", term },
                                    { "fuzzy", new BsonDocument
                                        {
                                            {"maxEdits", 2 },
                                            {"prefixLength", 0 }
                                        }
                                    }
                                }
                            }
                        }
                    }
                };

            var pipeline = new BsonDocument[]
            {
                fuzzySearch
            };

            var result = _books.Aggregate<Book>(pipeline);

            return result.ToList() ?? new List<Book>();

        }

        public void Update(string id, Book bookIn) =>
            _books.ReplaceOne(book => book.Id == id, bookIn);

        public void Remove(Book bookIn) =>
            _books.DeleteOne(book => book.Id == bookIn.Id);

        public void Remove(string id) =>
            _books.DeleteOne(book => book.Id == id);
    }
}