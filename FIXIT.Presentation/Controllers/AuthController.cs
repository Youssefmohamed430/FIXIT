using FIXIT.Application.Servicces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FIXIT.Presentation.Controllers;

[Route("[controller]")]
[ApiController]
public class AuthController(IServiceManager serviceManager) : ControllerBase
{
    //[HttpPost]
    //[Route("register")]
    //[AllowAnonymous]
    
    //public async Task<IActionResult> Register([FromBody] RegisterUserDto registerUserDto)
    //{
    //    var result = await serviceManager.AuthService.RegisterUserAsync(registerUserDto);

    //    return StatusCode(result.StatusCode, result);
    //}

}
