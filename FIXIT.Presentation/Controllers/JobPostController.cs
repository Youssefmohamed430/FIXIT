
using System.Security.Claims;

namespace FIXIT.Presentation.Controllers;
[ApiController]
[Route("[controller]")]
[EnableRateLimiting("GeneralPolicy")]

public class JobPostController(IServiceManager serviceManger) : ControllerBase
{
    #region Get All Posts
    [HttpGet("ById/{Id}")]
    [Cacheable("posts.CustomerId")]
    [Authorize(Roles = "Customer")]

    public IActionResult GetPostsByCustomerId(string Id)
    {
        var result = serviceManger.jobPostService.GetPostsByCustomerId(Id).Result;

        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    [HttpGet("ByName/{Name}")]
    [Cacheable("posts.CustomerName")]
    [Authorize]
    public IActionResult GetPostsByCustomerName(string Name)
    {
        var result = serviceManger.jobPostService.GetPostsByCustomerName(Name).Result;

        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    [HttpGet("ByDateRange")]
    [Cacheable("posts.DateRange")]
    [Authorize]
    public IActionResult GetPostsByDateRange(DateTime start, DateTime end)
    {
        var result = serviceManger.jobPostService.GetPostByDateRange(start, end).Result;
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    [HttpGet("ByServiceType/{type}")]
    [Cacheable("posts.ServiceType")]
    [Authorize]
    public IActionResult GetPostsByServiceType(string type)
    {
        var result = serviceManger.jobPostService.GetPostByServiceType(type).Result;
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    #endregion

    #region Create - Update - Delete
    [HttpPost]
    [Authorize(Roles = "Customer")]
    [InvalidatesCache("posts.CustomerId", "/JobPost/ById")]
    public IActionResult CreatePost([FromForm] CreateJobPostDTO jobPost)
    {
        var result = serviceManger.jobPostService.CreateJobPost(jobPost).Result;
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    [HttpPut("{id}")]
    [Authorize(Roles = "Customer")]
    [InvalidatesCache("posts.CustomerId", "/JobPost/ById")]

    public IActionResult UpdatePost(int id, [FromBody] JobPostDTO jobPost)
    {
        var userId = User.FindFirst("uid")?.Value;
        var result = serviceManger.jobPostService.UpdateJobPost(id, userId,jobPost).Result;
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    [HttpDelete("{id}")]
    [Authorize(Roles = "Customer")]
    [InvalidatesCache("posts.CustomerId", "/JobPost/ById")]

    public IActionResult DeletePost(int id)
    {
        var userId = User.FindFirst("uid")?.Value;
        var result = serviceManger.jobPostService.DeleteJobPost(id,userId).Result;
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    #endregion
}
