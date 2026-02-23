
namespace FIXIT.Presentation.Controllers;

public class LoginDTO
{
    [Required]
    public string UserName { get; set; }
    [Required]
    public string Password { get; set; }
}