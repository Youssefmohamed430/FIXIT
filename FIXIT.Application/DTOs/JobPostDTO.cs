namespace FIXIT.Application.DTOs;

public class JobPostDTO
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public string? ServiceType { get; set; }
    public string? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public JobPostStatus? Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    //public Point? Location { get; set; }
    public List<string>? JobPostImgPaths { get; set; } = new();
}
