namespace inventory_server.Models.Response;

public class GetProductsResponse
{
    public IEnumerable<GetProductResponse> Products { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}