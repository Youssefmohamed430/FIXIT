using FIXIT.Domain.Abstractions;
using FIXIT.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace FIXIT.Application.DTOs;

public class JobPostDTO
{
    public int Id { get; set; }
    public string Description { get; set; }
    public string ServiceType { get; set; }
    public string CustomerId { get; set; }
    public string CustomerName { get; set; }
    public List<string> JobPostImgPaths { get; set; } = new();
}
