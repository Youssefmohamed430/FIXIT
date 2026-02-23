
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

    [HttpPost("UploadImage")]
    public async Task<IActionResult> UploadImage([FromForm] ImageUploadModel model)
    {
        var result = await serviceManager._accountService.UploadImg(model.UserId, model.Image);

        return result.IsSuccess ? Ok("Uploading Success!") : BadRequest(result.Error);
    }

    [HttpPut("UpdateUserInfo/{id}")]
    public async Task<IActionResult> UpdateUserInfo(string id,[FromBody] UserDTO user)
    {
        var result = await serviceManager._accountService.UpdateUserInfo(id, user);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
public class ImageUploadModel
{
    public string UserId { get; set; }
    public IFormFile Image { get; set; }
}
