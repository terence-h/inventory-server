using inventory_server.Models.Requests;
using inventory_server.Models.Response;

namespace inventory_server.Repositories.Interfaces;

public interface IAccountRepository
{
    Task<LoginResponse> Login(LoginRequest request);
    Task<RegisterResponse> Register(RegisterRequest request);
}