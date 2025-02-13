namespace inventory_server.Models.Requests;

public class AddProductRequest
{
    public required string ProductNo { get; set; }
    public required string ProductName { get; set; }
    public required string Manufacturer { get; set; }
    public required string BatchNo { get; set; }
    public required int Quantity { get; set; }
    public required int CategoryId { get; set; }
    public DateTime? MfgDate { get; set; }
    public DateTime? MfgExpiryDate { get; set; }
    
    // For audit
    public required string Username { get; set; }
    
    public override string ToString()
    {
        return $"ProductNo:{ProductNo},ProductName:{ProductName},Manufacturer:{Manufacturer},BatchNo:{BatchNo},Quantity:{Quantity},CategoryId:{CategoryId},MfgDate:{MfgDate},MfgExpiryDate:{MfgExpiryDate}";
    }
}