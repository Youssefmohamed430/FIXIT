

namespace FIXIT.Application.Servicces;

public class ProviderRatingService(IUnitOfWork unitOfWork) : IProviderRatingService
{
    public async Task<Result<List<ProviderRatingDTO>>> GetProviderRatings(string providerId)
    {
        var providerRatings = await unitOfWork.GetRepository<ProviderRates>()
            .FindAllAsync<ProviderRatingDTO>(p => p.ProviderId == providerId && !p.IsDeleted,new string[] {"Provider.User","Customer.User"});

        return Result<List<ProviderRatingDTO>>.Success(providerRatings.ToList());
    }
    public async Task<Result<ProviderRatingDTO>> AddProviderRating(ProviderRatingDTO providerRating)
    {
        var newProviderRating = providerRating.Adapt<ProviderRates>();

        var addedProviderRating = await unitOfWork.GetRepository<ProviderRates>().AddAsync(newProviderRating);

        await unitOfWork.SaveAsync();

        return Result<ProviderRatingDTO>.Success(addedProviderRating.Adapt<ProviderRatingDTO>());   
    }

    public async Task<Result<bool>> DeleteProviderRating(int id)
    {
        var existingProviderRating = await unitOfWork.GetRepository<ProviderRates>()
                .FindAsync(p => p.Id == id);

        if (existingProviderRating == null)
            return Result<bool>.Failure(new Error("Provider rating not found"));

        existingProviderRating.IsDeleted = true;

        await unitOfWork.SaveAsync();
        return Result<bool>.Success(true);
    }

    public async Task<Result<ProviderRatingDTO>> UpdateProviderRating(ProviderRatingDTO UpdatedproviderRating, int id)
    {
        var existingProviderRating = await unitOfWork.GetRepository<ProviderRates>()
                .FindAsync(p => p.Id == id);

        var updatedProviderRating = UpdatedproviderRating.Adapt(existingProviderRating);

        await unitOfWork.SaveAsync();

        return Result<ProviderRatingDTO>.Success(updatedProviderRating.Adapt<ProviderRatingDTO>());
    }

    public Task<Result<decimal>> GetAverageRates(string providerId)
    {
        var AverageRates =  GetProviderRatings(providerId)
            .Result.Value.Average(p => p.Rate);

        return Task.FromResult(Result<decimal>.Success(AverageRates));
    }
}
