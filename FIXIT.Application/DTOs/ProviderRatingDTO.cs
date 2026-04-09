namespace FIXIT.Application.DTOs;

public class ProviderRatingDTO
{
    public int Id { get; set; }
    public required string ProviderId { get; set; }
    public string? ProviderName { get; set; }
    public required Rate Rate { get; set; }
    public string? Comment { get; set; }
    public string? CustomerName { get; set; }
    public required string CustomerID { get; set; }
}
