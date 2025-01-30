using System.ComponentModel.DataAnnotations.Schema;

namespace inventory_server.Models;

[Table("categories")]
public class Category
{
    public required int CategoryId { get; set; }
    public required string CategoryName { get; set; }
    public ICollection<Product> Products { get; set; }
}