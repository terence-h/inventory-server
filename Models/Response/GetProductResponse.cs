namespace inventory_server.Models.Response;

public class GetProductResponse
{
    public required string ProductId { get; set; }
    public required string ProductName { get; set; }
    public required string BatchNo { get; set; }
    public required int Quantity { get; set; }
    public required int CategoryId { get; set; }
    public required string CategoryName { get; set; }
    public DateTime? MfgDate { get; set; }
    public DateTime? MfgExpiryDate { get; set; }
    public DateTime? AddedOn { get; set; }
}