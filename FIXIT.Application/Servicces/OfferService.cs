
using FIXIT.Domain.Entities;
using Microsoft.Extensions.Localization;

namespace FIXIT.Application.Servicces;

public class OfferService(IUnitOfWork unitOfWork,IServiceManager serviceManager
    ,ILogger<OfferService> logger,IStringLocalizer<OfferService> _localizer) : IOfferService
{
    #region Gets Offers

    private async Task<Result<List<OfferDTO>>> GetOffersAsync(
    Expression<Func<Offer, bool>> filter,
    string errorCode,
    string errorMessage)
    {
        Expression<Func<Offer, bool>> baseFilter = o => !o.IsDeleted;

        var combinedFilter = baseFilter.And(filter);

        var offers = await unitOfWork.GetRepository<Offer>()
            .FindAllAsync<OfferDTO>(
                combinedFilter,
                new string[] { "ServiceProvider.User" });

        if (offers is null || !offers.Any())
            return Result<List<OfferDTO>>.Failure(
                new Error(errorCode, errorMessage));

        return Result<List<OfferDTO>>.Success(offers.ToList());
    }

    public Task<Result<List<OfferDTO>>> GetOffersByJobPostId(int id) =>
    GetOffersAsync(
        o => o.JobPostId == id,
        "Offers.NotFound.Id",
        _localizer["Offer.NotFound.Id"]);

    public Task<Result<List<OfferDTO>>> GetOffersByPriceRange(decimal start, decimal end,int jobpostid) =>
        GetOffersAsync(
            o => o.Price.Amount >= start &&
                 o.Price.Amount <= end &&
                 o.JobPostId == jobpostid,
            "Offers.NotFound.Price",
            _localizer["Offer.NotFound.Price"]);

    public Task<Result<List<OfferDTO>>> GetOffersByProviderName(string name, int jobPostId) =>
        GetOffersAsync(
            o => o.ServiceProvider.User.Name == name &&
            o.JobPostId == jobPostId,
            "Offers.NotFound.ProviderName",
           _localizer["Offer.NotFound.ProviderName"]);

    public Task<Result<List<OfferDTO>>> GetOffersByStatus(OfferStatus status, int jobPostId) =>
        GetOffersAsync(
            o => o.status == status &&
            o.JobPostId == jobPostId,
            "Offers.NotFound.Status",
            _localizer["Offer.NotFound.Status"]);
    #endregion

    #region Create, Update, Delete Offers

    public async Task<Result<OfferDTO>> CreateOffer(CreateOfferDTO offer)
    {
        try
        {
            var Newoffer = await unitOfWork.GetRepository<Offer>().AddAsync(offer.Adapt<Offer>());

            await unitOfWork.SaveAsync();

            await SendNotification(offer.JobPostId,_localizer["Offer.Notif" , offer.Price]);

            logger.LogInformation("Offer created successfully with ID: {OfferId}", Newoffer.Id);

            return Result<OfferDTO>.Success(Newoffer.Adapt<OfferDTO>());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create offer for JobPostId: {JobPostId} by ProviderId: {ProviderId}", offer.JobPostId, offer.ProviderId);
            return Result<OfferDTO>.Failure(
                new Error("Offers.CreateFailed", _localizer["Offer.CreatedFailed", ex.Message]));
        }
    }

    private async Task SendNotification(int jobpostid,string msg)
    {
        JobPost jobPost = unitOfWork.GetRepository<JobPost>().Find(j => j.Id == jobpostid);

        await serviceManager.notifService.CreateNotif(new NotifDTO
        {
            UserId = jobPost.CustomerId,
            Message = msg,
        });
    }

    public async Task<Result<OfferDTO>> UpdateOffer(OfferDTO offer,int offerId)
    {
        var offerToUpdate = unitOfWork.GetRepository<Offer>().Find(o => o.Id == offerId);

        if (offerToUpdate is null)
            return Result<OfferDTO>.Failure(
                new Error("Offers.NotFound.Id", _localizer["Offer.NotFound"]));

        UpdateHandling(offer, offerToUpdate);

        try
        {
            await unitOfWork.GetRepository<Offer>().UpdateAsync(offerToUpdate);
            await unitOfWork.SaveAsync();

            await SendNotification(offerToUpdate.JobPostId, "An offer on your job post has been updated.");
            logger.LogInformation("Offer with ID: {OfferId} updated successfully.", offerId);
            return Result<OfferDTO>.Success(offerToUpdate.Adapt<OfferDTO>());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update offer with ID: {OfferId}", offerId);
            return Result<OfferDTO>.Failure(
                new Error("Offers.UpdateFailed", _localizer["Offer.UpdatedFailed", ex.Message]));
        }
    }

    private static void UpdateHandling(OfferDTO offer, Offer offerToUpdate)
    {
        offerToUpdate.Description = offer.Description ?? offerToUpdate.Description;
        offerToUpdate.status = offer.status;
        offerToUpdate.Price = Price.Create(Convert.ToDecimal(offer.Price)) ?? offerToUpdate.Price;
    }

    public async Task<Result<object>> DeleteOffer(int id)
    {
        var offerToDelete = unitOfWork.GetRepository<Offer>().Find(o => o.Id == id);

        if (offerToDelete is null)
            return Result<object>.Failure(
                new Error("Offers.NotFound.Id", "No offer found with the given ID."));

        offerToDelete.IsDeleted = true;

        try
        {
            await unitOfWork.GetRepository<Offer>().UpdateAsync(offerToDelete);
            await unitOfWork.SaveAsync();

            await SendNotification(offerToDelete.JobPostId, "An offer on your job post has been deleted.");
            logger.LogInformation("Offer with ID: {OfferId} deleted successfully.", id);
            return Result<object>.Success(null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete offer with ID: {OfferId}", id);
            return Result<object>.Failure(
                new Error("Offers.UpdateFailed", $"Failed to delete offer: {ex.Message}"));
        }
    }
    #endregion
}
