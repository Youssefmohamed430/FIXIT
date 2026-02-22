using FIXIT.Application.DTOs;
using FIXIT.Application.IServices;
using FIXIT.Presentation.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FIXIT.Presentation.Controllers;
[ApiController]
[Route("[controller]")]
public class JobPostController(IServiceManager serviceManger) : ControllerBase
{
    #region Get All Posts
    [HttpGet("ById/{Id}")]
    [Cacheable("posts")]
    [Authorize]

    public IActionResult GetPostsByCustomerId(string Id)
    {
        var result = serviceManger.jobPostService.GetPostsByCustomerId(Id).Result;

        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
    [HttpGet("ByName/{Name}")]
    [Authorize]
    public IActionResult GetPostsByCustomerName(string Name)
    {
        var result = serviceManger.jobPostService.GetPostsByCustomerName(Name).Result;

        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
    [HttpGet("ByDateRange")]
    [Authorize]
    public IActionResult GetPostsByDateRange(DateTime start, DateTime end)
    {
        var result = serviceManger.jobPostService.GetPostByDateRange(start, end).Result;
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
    [HttpGet("ByServiceType/{type}")]
    [Authorize]
    public IActionResult GetPostsByServiceType(string type)
    {
        var result = serviceManger.jobPostService.GetPostByServiceType(type).Result;
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
    #endregion

    #region Create - Update - Delete
    [HttpPost]
    [Authorize(Roles = "Customer")]

    public IActionResult CreatePost([FromForm] CreateJobPostDTO jobPost)
    {
        var result = serviceManger.jobPostService.CreateJobPost(jobPost).Result;
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
    [HttpPut("{id}")]
    [Authorize(Roles = "Customer")]

    public IActionResult UpdatePost(int id, [FromBody] JobPostDTO jobPost)
    {
        var result = serviceManger.jobPostService.UpdateJobPost(id, jobPost).Result;
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
    [HttpDelete("{id}")]
    [Authorize(Roles = "Customer")]

    public IActionResult DeletePost(int id)
    {
        var result = serviceManger.jobPostService.DeleteJobPost(id).Result;
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
    #endregion
}
