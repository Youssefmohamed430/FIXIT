
namespace FIXIT.Application.Servicces;

public class JobPostService(IUnitOfWork unitOfWork,ILogger<JobPostService> logger) : IJobPostService
{
    #region Get Posts

    private async Task<Result<List<JobPostDTO>>> GetPostsAsync(
    Expression<Func<JobPost, bool>> filter,
    string errorCode,
    string errorMessage)
    {
        Expression<Func<JobPost, bool>> baseFilter = o => !o.IsDeleted;

        var combinedFilter = baseFilter.And(filter);

        var Posts = await unitOfWork.GetRepository<JobPost>()
            .FindAllAsync<JobPostDTO>(
                combinedFilter,
                new string[] { "Customer.User", "JobPostImgs" });

        if (Posts is null || !Posts.Any())
            return Result<List<JobPostDTO>>.Failure(
                new Error(errorCode, errorMessage));

        return Result<List<JobPostDTO>>.Success(Posts.ToList());
    }

    public async Task<Result<List<JobPostDTO>>> GetPostsByCustomerId(string Id) 
        => await GetPostsAsync(
            j => j.CustomerId == Id,
            "Posts.NotFound.Id",
            "No posts found for the given customer ID.");
    public async Task<Result<List<JobPostDTO>>> GetPostsByCustomerName(string Name)
        => await GetPostsAsync(
            j => j.Customer.User.Name == Name,
            "Posts.NotFound.Name",
            "No posts found for the given customer Name.");
    public async Task<Result<List<JobPostDTO>>> GetPostByDateRange(DateTime startDate, DateTime endDate)
        => await GetPostsAsync(
            j => j.CreatedAt >= startDate && j.CreatedAt <= endDate,
            "Posts.NotFound.DateRange",
            "No posts found for the given Date Range.");
    public async Task<Result<List<JobPostDTO>>> GetPostByServiceType(string type) 
        => await GetPostsAsync(
            j => j.ServiceType == type,
            "Posts.NotFound.ServiceType",
            "No posts found for the given Servcie Type.");

    public async Task<Result<List<JobPostDTO>>> GetPostByStatus(JobPostStatus status) 
        => await GetPostsAsync(
            j => j.Status == status,
            "Posts.NotFound.Status",
            "No posts found for the given Status.");
    #endregion

    #region Create - Update - Delete
    public async Task<Result<JobPostDTO>> CreateJobPost(CreateJobPostDTO jobPostDTO)
    {
        var jobPost = jobPostDTO.Adapt<JobPost>();

        await unitOfWork.GetRepository<JobPost>().AddAsync(jobPost);
        await unitOfWork.SaveAsync();

        await SaveImgs(jobPostDTO, jobPost);

        var resultDTO = jobPost.Adapt<JobPostDTO>();
        logger.LogInformation("JobPost created successfully with ID: {JobPostId}", jobPost.Id);

        return Result<JobPostDTO>.Success(resultDTO);
    }

    private async Task SaveImgs(CreateJobPostDTO jobPostDTO, JobPost jobPost)
    {
        foreach (var img in jobPostDTO.Images ?? new List<IFormFile>())
        {
            var fileName = HandleFoldersandFile(img).Result;

            var jobPostImg = new JobPostImg
            {
                JobPostId = jobPost.Id,
                ImgPath = ImgPath.Create(fileName)
            };

            await unitOfWork.GetRepository<JobPostImg>().AddAsync(jobPostImg);
        }
        await unitOfWork.SaveAsync();
    }

    private static async Task<string> HandleFoldersandFile(IFormFile imgFile)
    {
        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var fileName = Guid.NewGuid() + Path.GetExtension(imgFile.FileName);
        var filePath = Path.Combine(folderPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        { await imgFile.CopyToAsync(stream); }

        return fileName;
    }

    public async Task<Result<Object>> UpdateJobPost(int id, JobPostDTO jobPostDTO)
    {
        var post = await unitOfWork.GetRepository<JobPost>().FindAsync(p => p.Id == id);

        if (post is null)
        {
            logger.LogWarning("Attempted to update JobPost with ID: {JobPostId}, but it was not found", id);
            return Result<Object>.Failure(new Error("Posts.NotFound.Id", "No post found for the given ID."));
        }

        post.Description = jobPostDTO.Description ?? post.Description;
        post.ServiceType = jobPostDTO.ServiceType ?? post.ServiceType;

        await unitOfWork.GetRepository<JobPost>().UpdateAsync(post);
        await unitOfWork.SaveAsync();

        var resultDTO = post.Adapt<JobPostDTO>();
        logger.LogInformation("JobPost with ID: {JobPostId} updated successfully", post.Id);

        return Result<Object>.Success(null!);
    }

    public async Task<Result<Object>> DeleteJobPost(int id)
    {
        var post = await unitOfWork.GetRepository<JobPost>().FindAsync(p => p.Id == id);

        if (post is null)
        {
            logger.LogWarning("Attempted to delete JobPost with ID: {JobPostId}, but it was not found", id);
            return Result<Object>.Failure(new Error("Posts.NotFound.Id", "No post found for the given ID."));
        }
        
        post.IsDeleted = true;

        await unitOfWork.GetRepository<JobPost>().UpdateAsync(post);
        await unitOfWork.SaveAsync();
        logger.LogInformation("JobPost with ID: {JobPostId} deleted successfully", post.Id);
        return Result<Object>.Success(null!);
    }
    #endregion
}
