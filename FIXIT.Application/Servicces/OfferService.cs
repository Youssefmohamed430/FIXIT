
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

    public Task<Result<OfferDTO>> CreateOffer(CreateOfferDTO offer)
    {
        throw new NotImplementedException();
    }

    public Task<Result<object>> DeleteOffer(int id)
    {
        throw new NotImplementedException();
    }

    

    public Task<Result<OfferDTO>> UpdateOffer(OfferDTO offer)
    {
        throw new NotImplementedException();
    }

    #endregion
}
