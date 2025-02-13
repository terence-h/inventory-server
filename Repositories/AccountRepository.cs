using inventory_server.Entities;
using inventory_server.Models.Requests;
using inventory_server.Models.Response;
using inventory_server.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using LoginRequest = inventory_server.Models.Requests.LoginRequest;

namespace inventory_server.Repositories;

public class AccountRepository(UserManager<Account> userManager, SignInManager<Account> signInManager, IAuditRepository auditRepository) : IAccountRepository
{
    public async Task<LoginResponse> Login(LoginRequest request)
    {
        var user = await userManager.FindByNameAsync(request.Username);

        if (user == null)
        {
            await auditRepository.CreateAuditLogAsync(new AddAuditLogRequest
            {
                AuditTypeId = Globals.AuditType.Login,
                AuditContent = $"Invalid username - {request.Username}",
                ActionBy = request.Username,
                Date = DateTime.Now
            });
            
            return new LoginResponse { Username = "", Message = "Invalid username or password" };
        }
            
        
        var result = await signInManager.PasswordSignInAsync(user, request.Password, false, false);

        if (!result.Succeeded)
        {
            await auditRepository.CreateAuditLogAsync(new AddAuditLogRequest
            {
                AuditTypeId = Globals.AuditType.Login,
                AuditContent = $"Invalid password - {request.Username}",
                ActionBy = request.Username,
                Date = DateTime.Now
            });

            return new LoginResponse { Username = "", Message = "Invalid username or password" };
        }
        
        await auditRepository.CreateAuditLogAsync(new AddAuditLogRequest
        {
            AuditTypeId = Globals.AuditType.Login,
            AuditContent = $"Login successful",
            ActionBy = request.Username,
            Date = DateTime.Now
        });
        
        return new LoginResponse { Username = request.Username };
    }

    public async Task<bool> Logout(LogoutRequest request)
    {
        await auditRepository.CreateAuditLogAsync(new AddAuditLogRequest
        {
            AuditTypeId = Globals.AuditType.Logout,
            AuditContent = $"Logout successful",
            ActionBy = request.Username,
            Date = DateTime.Now
        });

        return true;
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
                
                await auditRepository.CreateAuditLogAsync(new AddAuditLogRequest
                {
                    AuditTypeId = Globals.AuditType.Register,
                    AuditContent = $"Acccount creation failed! {errors}",
                    ActionBy = "System", // API is not exposed to client side, only available on server
                    Date = DateTime.Now
                });
        
                return new RegisterResponse()
                {
                    Success = false,
                    Message = $"Acccount creation failed! Errors: {errors}",
                };
            }
            
            await auditRepository.CreateAuditLogAsync(new AddAuditLogRequest
            {
                AuditTypeId = Globals.AuditType.Register,
                AuditContent = $"Registration for account {request.Username} successful",
                ActionBy = "System", // API is not exposed to client side, only available on server
                Date = DateTime.Now
            });

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