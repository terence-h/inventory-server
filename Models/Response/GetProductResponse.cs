namespace inventory_server.Models.Response;

public class GetProductResponse
{
    public int ProductId { get; set; }
    public string ProductNo { get; set; }
    public string ProductName { get; set; }
    public string Manufacturer { get; set; }
    public string BatchNo { get; set; }
    public int Quantity { get; set; }
    public string? MfgDate { get; set; }
    public string? MfgExpiryDate { get; set; }
    // public string? AddedOn { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
}