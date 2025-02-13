namespace inventory_server.Models.Requests;

public class DeleteProductRequest
{
    // for audit
    public required string Username { get; set; }
}