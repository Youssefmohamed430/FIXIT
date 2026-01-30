namespace FIXIT.Domain.Entities;
public class Customer
{
    public required string Id { get; set; }
    public ApplicationUser? User { get; set; }
    public JobPost? post { get; set; }
}
