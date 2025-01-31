using inventory_server.Models.Requests;
using inventory_server.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace inventory_server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController(IAccountRepository accountRepository) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest();
        
        if (request.Password != request.ConfirmPassword)
            return BadRequest("Passwords do not match");
        
        var response = await accountRepository.Register(request);

        return response.Success ? Ok(response) : BadRequest(response);
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var response = await accountRepository.Login(request);
        
        return response.Message == null ? Ok(response) : BadRequest(response);
    }
}