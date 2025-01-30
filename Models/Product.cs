using System.ComponentModel.DataAnnotations.Schema;

namespace inventory_server.Models;

[Table("products")]
public class Product
{
    public required string ProductId { get; set; }
    public required string BatchNo { get; set; }
    public required int Quantity { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; }
    public required DateTime MfgDate { get; set; }
    public DateTime? MfgExpiryDate { get; set; }
    public int? RowVersion { get; set; }
}