namespace inventory_server.Models.Requests;

public class GetProductsRequest
{
    public string? ProductNo;
    public string? ProductName;
    public string? Manufacturer;
    public string? BatchNo;
    public int? Quantity;
    public int? CategoryId;
    public DateTime? MfgDateFrom;
    public DateTime? MfgDateTo;
    public DateTime? MfgExpiryDateFrom;
    public DateTime? MfgExpiryDateTo;
    public DateTime? AddedOn;
    public int? Page;
}