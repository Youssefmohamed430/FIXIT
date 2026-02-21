namespace FIXIT.Domain.Entities;

public class JobPostImg
{
    public int Id { get; set; }
    public ImgPath? ImgPath { get; set; }
    public int JobPostId { get; set; }
    public JobPost? JobPost { get; set; }
}
