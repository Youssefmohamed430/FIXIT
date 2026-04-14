using FIXIT.Domain.Entities;
using FIXIT.Domain.ValueObjects;

namespace FIXIT.Presentation.Controllers;

[ApiController]
[Route("[controller]")]
public class OfferController(IServiceManager serviceManager) : ControllerBase
{
    #region Gets Offers
    [Authorize]
    [HttpGet("ByJobPostId/{id}")]
    [Cacheable("Offers.ByJobPostId")]
    public async Task<IActionResult> GetOffersByJobPostId(int id)
    {
        var result = await serviceManager.offerService.GetOffersByJobPostId(id);

        return result.IsSuccess ? Ok(result) : BadRequest(result) ;
    }
    [Authorize]
    [Cacheable("Offers.ByProviderName")]
    [HttpGet("ByProviderName/{name}/{jobPostId}")]
    public async Task<IActionResult> GetOffersByProviderName(string name, int jobPostId)
    {
        var result = await serviceManager.offerService.GetOffersByProviderName(name,jobPostId);

        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    [Authorize]
    [Cacheable("Offers.ByStatus")]
    [HttpGet("ByStatus/{status}/{jobPostId}")]
    public async Task<IActionResult> GetOffersByStatus(OfferStatus status, int jobPostId)
    {
        var result = await serviceManager.offerService.GetOffersByStatus(status, jobPostId);

        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    [Authorize]
    [Cacheable("Offers.ByPriceRange")]
    [HttpGet("ByPriceRange/{start}/{end}/{jobPostId}")]
    public async Task<IActionResult> GetOffersByPriceRange(decimal start, decimal end, int jobPostId)
    {
        var result = await serviceManager.offerService.GetOffersByPriceRange(start, end,jobPostId);

        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    #endregion

    #region Update - Accept - Reject
    [Authorize(Roles = "Provider")]
    [HttpPost]
    public async Task<IActionResult> CreateOffer(CreateOfferDTO offer)
    {
        var result = await serviceManager.offerService.CreateOffer(offer);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [Authorize(Roles = "Provider")]
    [HttpPut("UpdateOffer/{offerid}")]
    public async Task<IActionResult> UpdateOffer(OfferDTO offer,int offerid)
    {
        var result = await serviceManager.offerService.UpdateOffer(offer,offerid);

        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [Authorize(Roles = "Provider")]
    [HttpDelete("DeleteOffer/{offerid}")]
    public async Task<IActionResult> DeleteOffer(int offerid)
    {
        var result = await serviceManager.offerService.DeleteOffer(offerid);

        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    #endregion
}
