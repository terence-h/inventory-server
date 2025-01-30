using inventory_server.Models;
using inventory_server.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace inventory_server.Repositories;

public class AccountRepository(UserManager<Account> userManager, SignInManager<Account> signInManager) : IAccountRepository
{
    public async Task<bool> Login(string username, string password)
    {
        var user = await userManager.FindByNameAsync(username);

        if (user == null)
            return false;
        
        var result = await signInManager.PasswordSignInAsync(user, password, true, false);
        
        return result.Succeeded;
    }
}