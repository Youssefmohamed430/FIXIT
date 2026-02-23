
namespace FIXIT.Application.IServices;

public interface IJobPostService
{
    Task<Result<List<JobPostDTO>>> GetPostsByCustomerId(string Id);
    Task<Result<List<JobPostDTO>>> GetPostsByCustomerName(string Name);
    Task<Result<List<JobPostDTO>>> GetPostByDateRange(DateTime startDate, DateTime endDate);
    Task<Result<List<JobPostDTO>>> GetPostByStatus(JobPostStatus status);
    Task<Result<List<JobPostDTO>>> GetPostByServiceType(string type);
    Task<Result<JobPostDTO>> CreateJobPost(CreateJobPostDTO jobPostDTO);
    Task<Result<Object>> UpdateJobPost(int id, JobPostDTO jobPostDTO);
    Task<Result<Object>> DeleteJobPost(int id);
}
