using FIXIT.Domain.Entities;
using FIXIT.Domain.ValueObjects;

namespace FIXIT.Presentation.Controllers;

[ApiController]
[Route("[controller]")]
public class OfferController(IServiceManager serviceManager) : ControllerBase
{
    [HttpGet("ByJobPostId")]
    [Authorize]
    [Cacheable("Offers.ByJobPostId")]
    public async Task<IActionResult> GetOffersByJobPostId(int id)
    {
        var result = await serviceManager.offerService.GetOffersByJobPostId(id);

        return result.IsSuccess ? Ok(result) : BadRequest(result) ;
    }
    [Authorize]
    [Cacheable("Offers.ByProviderName")]
    [HttpGet("ByProviderName")]
    public async Task<IActionResult> GetOffersByProviderName(string name)
    {
        var result = await serviceManager.offerService.GetOffersByProviderName(name);

        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    [Authorize]
    [Cacheable("Offers.ByStatus")]
    [HttpGet("ByStatus")]
    public async Task<IActionResult> GetOffersByStatus(OfferStatus status)
    {
        var result = await serviceManager.offerService.GetOffersByStatus(status);

        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    [Authorize]
    [Cacheable("Offers.ByPriceRange")]
    [HttpGet("ByPriceRange")]
    public async Task<IActionResult> GetOffersByPriceRange(Price start, Price end)
    {
        var result = await serviceManager.offerService.GetOffersByPriceRange(start, end);

        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
