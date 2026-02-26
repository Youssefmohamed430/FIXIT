
using FIXIT.Domain.Entities;
using System.Xml.Linq;

namespace FIXIT.Application.Servicces;

public class OfferService(IUnitOfWork unitOfWork) : IOfferService
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
        "No offers found for the given Job Post.");

    public Task<Result<List<OfferDTO>>> GetOffersByPriceRange(Price start, Price end) =>
        GetOffersAsync(
            o => o.Price.Amount >= start.Amount &&
                 o.Price.Amount <= end.Amount,
            "Offers.NotFound.Price",
            "No offers found for the given Price Range.");

    public Task<Result<List<OfferDTO>>> GetOffersByProviderName(string name) =>
        GetOffersAsync(
            o => o.ServiceProvider.User.Name == name,
            "Offers.NotFound.ProviderName",
            "No offers found for the given Provider.");

    public Task<Result<List<OfferDTO>>> GetOffersByStatus(OfferStatus status) =>
        GetOffersAsync(
            o => o.status == status,
            "Offers.NotFound.Status",
            "No offers found for the given Status.");
    #endregion

    #region Create, Update, Delete Offers

    public async Task<Result<OfferDTO>> CreateOffer(CreateOfferDTO offer)
    {
        try
        {
            var Newoffer = await unitOfWork.GetRepository<Offer>().AddAsync(offer.Adapt<Offer>());

            await unitOfWork.SaveAsync();

            return Result<OfferDTO>.Success(Newoffer.Adapt<OfferDTO>());
        }
        catch (Exception ex)
        {
            return Result<OfferDTO>.Failure(
                new Error("Offers.CreateFailed", $"Failed to create offer: {ex.Message}"));
        }
    }
    public async Task<Result<OfferDTO>> UpdateOffer(OfferDTO offer,int offerId)
    {
        var offerToUpdate = unitOfWork.GetRepository<Offer>().Find(o => o.Id == offerId);

        if (offerToUpdate is null)
            return Result<OfferDTO>.Failure(
                new Error("Offers.NotFound.Id", "No offer found with the given ID."));

        UpdateHandling(offer, offerToUpdate);

        try
        {
            await unitOfWork.GetRepository<Offer>().UpdateAsync(offerToUpdate);
            await unitOfWork.SaveAsync();
            return Result<OfferDTO>.Success(offerToUpdate.Adapt<OfferDTO>());
        }
        catch (Exception ex)
        {
            return Result<OfferDTO>.Failure(
                new Error("Offers.UpdateFailed", $"Failed to update offer: {ex.Message}"));
        }
    }

    private static void UpdateHandling(OfferDTO offer, Offer offerToUpdate)
    {
        offerToUpdate.Description = offer.Description ?? offerToUpdate.Description;
        offerToUpdate.status = offer.status;
        offerToUpdate.Price = offer.Price ?? offerToUpdate.Price;
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
            return Result<object>.Success(null);
        }
        catch (Exception ex)
        {
            return Result<object>.Failure(
                new Error("Offers.UpdateFailed", $"Failed to delete offer: {ex.Message}"));
        }
    }
    #endregion
}
