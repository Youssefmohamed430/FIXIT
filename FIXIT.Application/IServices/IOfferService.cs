namespace FIXIT.Application.IServices;

public interface IOfferService
{
    Task<Result<List<OfferDTO>>> GetOffersByJobPostId(int id);
    Task<Result<List<OfferDTO>>> GetOffersByProviderName(string Name);
    Task<Result<List<OfferDTO>>> GetOffersByStatus(OfferStatus status);
    Task<Result<List<OfferDTO>>> GetOffersByPriceRange(Price start,Price end);

    Task<Result<OfferDTO>> CreateOffer(CreateOfferDTO offer);
    Task<Result<OfferDTO>> UpdateOffer(OfferDTO offer,int offerId);
    Task<Result<Object>> DeleteOffer(int id);
}
