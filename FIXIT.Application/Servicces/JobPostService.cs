using FIXIT.Application.DTOs;
using FIXIT.Application.IServices;
using FIXIT.Domain.Abstractions;
using FIXIT.Domain.Entities;
using FIXIT.Domain.ValueObjects;
using Mapster;
using Microsoft.AspNetCore.Http;
using System.Xml.Linq;

namespace FIXIT.Application.Servicces;

public class JobPostService(IUnitOfWork unitOfWork) : IJobPostService
{
    #region Get Posts
    public async Task<Result<List<JobPostDTO>>> GetPostsByCustomerId(string Id)
    {
        var Posts = await unitOfWork.GetRepository<JobPost>()
            .FindAllAsync<JobPostDTO>(j => j.CustomerId == Id
            , new string[] { "Customer.User", "JobPostImgs" });

        if (Posts is null || !Posts.Any())
            return Result<List<JobPostDTO>>.Failure(new Error("Posts.NotFound.Id","No posts found for the given customer ID."));

        return Result<List<JobPostDTO>>.Success(Posts.ToList());
    }

    public async Task<Result<List<JobPostDTO>>> GetPostsByCustomerName(string Name)
    {
        var Posts = await unitOfWork.GetRepository<JobPost>()
            .FindAllAsync<JobPostDTO>(j => j.Customer.User.Name == Name
            , new string[] { "Customer.User", "JobPostImgs" });

        if (Posts is null || !Posts.Any())
            return Result<List<JobPostDTO>>.Failure(new Error("Posts.NotFound.Name", "No posts found for the given customer Name."));

        return Result<List<JobPostDTO>>.Success(Posts.ToList());
    }
    public async Task<Result<List<JobPostDTO>>> GetPostByDateRange(DateTime startDate, DateTime endDate)
    {
        var Posts = await unitOfWork.GetRepository<JobPost>()
            .FindAllAsync<JobPostDTO>(j => j.CreatedAt >= startDate && j.CreatedAt <= endDate
            , new string[] { "Customer.User", "JobPostImgs" });

        if (Posts is null || !Posts.Any())
            return Result<List<JobPostDTO>>.Failure(new Error("Posts.NotFound.DateRange", "No posts found for the given Date Range."));

        return Result<List<JobPostDTO>>.Success(Posts.ToList());
    }

    public async Task<Result<List<JobPostDTO>>> GetPostByServiceType(string type)
    {
        var Posts = await unitOfWork.GetRepository<JobPost>()
            .FindAllAsync<JobPostDTO>(j => j.ServiceType == type
            , new string[] { "Customer.User", "JobPostImgs" });

        if (Posts is null || !Posts.Any())
            return Result<List<JobPostDTO>>.Failure(new Error("Posts.NotFound.ServiceType", "No posts found for the given Servcie Type."));

        return Result<List<JobPostDTO>>.Success(Posts.ToList());
    }

    public async Task<Result<List<JobPostDTO>>> GetPostByStatus(JobPostStatus status)
    {
        var Posts = await unitOfWork.GetRepository<JobPost>()
            .FindAllAsync<JobPostDTO>(j => j.Status == status
            , new string[] { "Customer.User", "JobPostImgs" });

        if (Posts is null || !Posts.Any())
            return Result<List<JobPostDTO>>.Failure(new Error("Posts.NotFound.Status", "No posts found for the given Status."));

        return Result<List<JobPostDTO>>.Success(Posts.ToList());
    }

    #endregion

    #region Create - Update - Delete
    public async Task<Result<JobPostDTO>> CreateJobPost(CreateJobPostDTO jobPostDTO)
    {
        var jobPost = jobPostDTO.Adapt<JobPost>();

        await unitOfWork.GetRepository<JobPost>().AddAsync(jobPost);
        await unitOfWork.SaveAsync();

        await SaveImgs(jobPostDTO, jobPost);

        var resultDTO = jobPost.Adapt<JobPostDTO>();

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
             return Result<Object>.Failure(new Error("Posts.NotFound.Id", "No post found for the given ID."));

        post.Description = jobPostDTO.Description;
        post.ServiceType = jobPostDTO.ServiceType;

        await unitOfWork.GetRepository<JobPost>().UpdateAsync(post);
        await unitOfWork.SaveAsync();

        var resultDTO = post.Adapt<JobPostDTO>();

        return Result<Object>.Success(null!);
    }

    public async Task<Result<Object>> DeleteJobPost(int id)
    {
        var post = await unitOfWork.GetRepository<JobPost>().FindAsync(p => p.Id == id);

        if (post is null)
            return Result<Object>.Failure(new Error("Posts.NotFound.Id", "No post found for the given ID."));
        
        post.IsDeleted = true;

        await unitOfWork.GetRepository<JobPost>().UpdateAsync(post);
        await unitOfWork.SaveAsync();

        return Result<Object>.Success(null!);
    }
    #endregion
}
