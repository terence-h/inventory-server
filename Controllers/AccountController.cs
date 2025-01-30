using inventory_server.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace inventory_server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController(IAccountRepository accountRepository) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody](string username, string password) credentials)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var response = await accountRepository.Login(credentials.username, credentials.password);
        
        return response ? Ok(response) : BadRequest(response);
    }
}