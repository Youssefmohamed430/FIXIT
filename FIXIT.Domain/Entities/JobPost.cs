using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIXIT.Domain.Entities;

public enum JobPostStatus { Open = 0, Closed = 1 }

public class JobPost
{
    public int Id { get; set; }
    public string Description { get; set; }
    public string ServiceType { get; set; }
    public JobPostStatus Status { get; set; } = JobPostStatus.Open;   
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public Order? order { get; set; }
}
