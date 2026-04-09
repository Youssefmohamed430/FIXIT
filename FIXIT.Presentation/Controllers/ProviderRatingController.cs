using FIXIT.Application.DTOs;

namespace FIXIT.Presentation.Controllers;

[ApiController]
[Route("[controller]")]
public class ProviderRatingController(IServiceManager serviceManager) : ControllerBase
{
    [Authorize(Roles = "Provider")]
    [HttpGet("GetProviderRatings/{providerId}")]
    public async Task<IActionResult> GetProviderRatings(string providerId)
    {
        var result = await serviceManager.providerRatingService.GetProviderRatings(providerId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [Authorize(Roles = "Customer")]
    [HttpPost("AddProviderRating")]
    public async Task<IActionResult> AddProviderRating([FromBody] ProviderRatingDTO providerRatingDTO)
    {
            var result = await serviceManager.providerRatingService.AddProviderRating(providerRatingDTO);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [Authorize(Roles = "Customer")]
    [HttpPut("UpdateProviderRating/{id}")]
    public async Task<IActionResult> UpdateProviderRating(int id, [FromBody] ProviderRatingDTO providerRatingDTO)
    {
        var result = await serviceManager.providerRatingService.UpdateProviderRating(providerRatingDTO,id);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }


    [HttpDelete("DeleteProviderRating/{id}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> DeleteProviderRating(int id)
    {
        var result = await serviceManager.providerRatingService.DeleteProviderRating(id);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("GetAverageRates/{providerId}")]
    public async Task<IActionResult> GetAverageRates(string providerId)
    {
        var result = await serviceManager.providerRatingService.GetAverageRates(providerId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
