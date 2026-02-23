
namespace FIXIT.Application.DTOs;

public class CreateJobPostDTO
{
    public string Description { get; set; } = default!;
    public string ServiceType { get; set; } = default!;
    public string CustomerId { get; set; } = default!;

    public List<IFormFile>? Images { get; set; }
}
