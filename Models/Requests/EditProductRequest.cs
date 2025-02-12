namespace inventory_server.Models.Requests;

public class EditProductRequest
{
    public int ProductId { get; set; }
    public required string ProductNo { get; set; }
    public required string ProductName { get; set; }
    public required string Manufacturer { get; set; }
    public required string BatchNo { get; set; }
    public required int Quantity { get; set; }
    public required int CategoryId { get; set; }
    public DateTime? MfgDate { get; set; }
    public DateTime? MfgExpiryDate { get; set; }
}