namespace inventory_server.Models.Response;

public class LoginResponse
{
    public required string Username { get; set; }
    public string? Message { get; set; }
}