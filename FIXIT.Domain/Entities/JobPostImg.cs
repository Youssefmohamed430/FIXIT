using FIXIT.Domain.ValueObjects;
using System;
namespace FIXIT.Domain.Entities;

public class JobPostImg
{
    public int Id { get; set; }
    public ImgPath? ImgPath { get; private set; }
    public int JobPostId { get; set; }
    public JobPost? JobPost { get; set; }
}
