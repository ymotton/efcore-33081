
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

Seed();

using var context = new MyDbContext();
var product = context.Products
    .Include(x => x.Tags)
    .Include(x => x.Categories)
    .Single();

// Expecting 1, Actual 1
Console.WriteLine(product.Tags.Count);
// Expecting 1, Actual 0
Console.WriteLine(product.Categories.Count);

void Seed()
{
    using var context = new MyDbContext();
    context.Database.EnsureDeleted();
    context.Database.EnsureCreated();
    var code1 = new Code { Id = Guid.NewGuid(), Value = "test1" };
    var code2 = new Code { Id = Guid.NewGuid(), Value = "test2" };
    context.Codes.AddRange(
        code1,
        code2
    );
    context.Products.AddRange(
        new Product
        {
            Id = Guid.NewGuid(),
            Name = "Product 1",
            Tags = new List<Code>
            {
                code1
            },
            Categories = new List<Code>
            {
                code2
            }
        }
    );
    context.SaveChanges();
}


class Product
{
    [Key]
    public Guid Id { get; set; }
    public string Name { get; set; }
    public List<Code> Tags { get; set; }
    public List<Code> Categories { get; set; }
}

class Code
{
    [Key]
    public Guid Id { get; set; }
    public string Value { get; set; }
    public List<Product> Products { get; set; } // TODO: Comment this out to fix
}
class MyDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=efcore-33081;Trusted_Connection=True;Encrypt=False");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Product>();

        entity
            .HasMany(x => x.Tags)
            // .WithMany() // TODO: Uncomment this to fix
            .WithMany(x => x.Products) // TODO: Comment this out to fix
            .UsingEntity<Dictionary<string, object>>(
                "ProductTags",
                x => x.HasOne<Code>().WithMany().HasForeignKey("Code_Id").OnDelete(DeleteBehavior.NoAction),
                x => x.HasOne<Product>().WithMany().HasForeignKey("Product_Id").OnDelete(DeleteBehavior.NoAction),
                x => x.ToTable("ProductTags")
            );
        
        entity
            .HasMany(x => x.Categories)
            // .WithMany() // TODO: Uncomment this to fix
            .WithMany(x => x.Products)  // TODO: Comment this out to fix
            .UsingEntity<Dictionary<string, object>>(
                "ProductCategories",
                x => x.HasOne<Code>().WithMany().HasForeignKey("Code_Id").OnDelete(DeleteBehavior.NoAction),
                x => x.HasOne<Product>().WithMany().HasForeignKey("Product_Id").OnDelete(DeleteBehavior.NoAction),
                x => x.ToTable("ProductCategories")
            );
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Code> Codes { get; set; }
}

