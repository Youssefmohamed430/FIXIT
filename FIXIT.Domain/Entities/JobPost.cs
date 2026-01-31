using Data_Access_Layer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIXIT.Domain.Entities;

public enum JobPostStatus { Open = 1, Closed = 2 }

public class JobPost
{
    public int Id { get; set; }
    public string Description { get; set; }
    public string ServiceType { get; set; }
    public JobPostStatus Status { get; set; } = JobPostStatus.Open;   
    public DateTime CreatedAt { get; set; } = EgyptTimeHelper.Now;
    public string CustomerId { get; set; }
    public bool IsDeleted { get; set; } = false;
    public Customer? Customer { get; set; }
    public List<Order>? orders { get; set; }
    public List<Offer>? Offers { get; set; }
}
