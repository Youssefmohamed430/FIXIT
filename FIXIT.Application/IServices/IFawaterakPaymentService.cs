

using static FIXIT.Application.DTOs.EInvoiceResponseModel;

namespace FIXIT.Application.IServices;

public interface IFawaterakPaymentService : IPaymentGateway
{
    // Create EInvoice Link
    Task<EInvoiceResponseDataModel?> CreateEInvoiceAsync(EInvoiceRequestModel eInvoice);

    // WebHook Verification
    //bool VerifyWebhook(WebHookModel webHook);
    //bool VerifyCancelTransaction(CancelTransactionModel cancelTransaction);
    //bool VerifyApiKeyTransaction(string apiKey);

    // HashKey
    string GenerateHashKeyForIFrame(string domain);
}
