namespace FIXIT.Application.IServices;

public interface IProviderRatingService
{
    Task<Result<List<ProviderRatingDTO>>> GetProviderRatings(string providerId);
    Task<Result<decimal>> GetAverageRates(string providerId);
    Task<Result<ProviderRatingDTO>> AddProviderRating(ProviderRatingDTO providerRating);
    Task<Result<ProviderRatingDTO>> UpdateProviderRating(ProviderRatingDTO UpdatedproviderRating,int id);
    Task<Result<bool>> DeleteProviderRating(int id);
}
