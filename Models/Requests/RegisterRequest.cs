using System.ComponentModel.DataAnnotations;

namespace inventory_server.Models.Requests;

public class RegisterRequest
{
    [MinLength(8)]
    public required string Username { get; set; }
    [MinLength(8)]
    public required string Password { get; set; }
    [MinLength(8)]
    public required string ConfirmPassword { get; set; }
}