
using FIXIT.Application.IServices;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using FIXIT.Application.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace FIXIT.Presentation.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class AccountController(IServiceManager serviceManager) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetImage(string id)
    {
        var result = await serviceManager._accountService.GetImg(id);

        return result.IsSuccess ? Ok(result.Value.ImgPath) : NotFound(result.Error);
    }

    [HttpPost("UploadImage/{id}")]
    public async Task<IActionResult> UploadImage(string id, [FromForm(Name = "image")] IFormFile image)
    {
        var result = await serviceManager._accountService.UploadImg(id, image);

        return result.IsSuccess ? Ok(result.Value.ImgPath) : BadRequest(result.Error);
    }

    [HttpPut("UpdateUserInfo/{id}")]
    public async Task<IActionResult> UpdateUserInfo(string id,[FromBody] UserDTO user)
    {
        var result = await serviceManager._accountService.UpdateUserInfo(id, user);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
