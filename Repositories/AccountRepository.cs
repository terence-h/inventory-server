using inventory_server.Entities;
using inventory_server.Models.Requests;
using inventory_server.Models.Response;
using inventory_server.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using LoginRequest = inventory_server.Models.Requests.LoginRequest;

namespace inventory_server.Repositories;

public class AccountRepository(UserManager<Account> userManager, SignInManager<Account> signInManager) : IAccountRepository
{
    public async Task<LoginResponse> Login(LoginRequest request)
    {
        var user = await userManager.FindByNameAsync(request.Username);

        if (user == null)
            return new LoginResponse { Username = "", Message = "Invalid username or password" };
        
        var result = await signInManager.PasswordSignInAsync(user, request.Password, false, false);
        
        return result.Succeeded ? new LoginResponse { Username = request.Username } : new LoginResponse { Username = "", Message = "Invalid username or password" };
    }

    public async Task<RegisterResponse> Register(RegisterRequest request)
    {
        var user = new Account
        {
            UserName = request.Username
        };

        try
        {
            var result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
        
                return new RegisterResponse()
                {
                    Success = false,
                    Message = $"Acccount creation failed! Errors: {errors}",
                };
            }

            return new RegisterResponse { Success = true };
        }
        catch (Exception ex)
        {
            return new RegisterResponse()
            {
                Success = false,
                Message = $"ERROR Register: {ex.Message}",
            };
        }
    }
}