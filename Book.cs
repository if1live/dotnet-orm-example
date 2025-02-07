using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace DemoDatabase;

[Table("book")]
[PrimaryKey(nameof(Id))]
public class Book
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Id { get; set; }
    
    public string Lang { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    
    // TODO: date
    // TODO: blob
    // TODO: guid

    public static Book Create(Faker faker)
    {
        return new Book()
        {
            Id = faker.Random.Guid().ToString(),
            Lang = "en",
            Title = faker.Lorem.Sentence(),
        };
    }

    public static List<Book> CreateMany(int count)
    {
        var faker = new Faker();
        var books = Enumerable.Range(1, count).Select(x => Book.Create(faker));
        return books.ToList();
    }
}