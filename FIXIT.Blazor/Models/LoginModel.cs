using System.ComponentModel.DataAnnotations;

namespace FIXIT.Blazor.Models;

public class LoginModel
{
    [Required(ErrorMessage = "UserName Is Required")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password Is Required")]
    public string Password { get; set; } = string.Empty;
}
