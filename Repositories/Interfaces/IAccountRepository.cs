namespace inventory_server.Repositories.Interfaces;

public interface IAccountRepository
{
    Task<bool> Login(string username, string password);
}