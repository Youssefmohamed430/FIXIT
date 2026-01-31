namespace FIXIT.Domain.Entities;
public class Customer
{
    public required string Id { get; set; }
    public ApplicationUser? User { get; set; }
    public List<JobPost>? posts { get; set; }
}
