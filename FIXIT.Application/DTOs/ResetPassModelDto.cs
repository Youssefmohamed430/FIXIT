
namespace FIXIT.Application.DTOs;

public class ResetPassModelDto
{
    [EmailAddress(ErrorMessage = "In Valid Email")]
    public string Email { get; set; }
    public string token { get; set; }
    [Required]
    [MinLength(10, ErrorMessage = "At Least ten Letters")]
    [DataType(DataType.Password)]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Password must contain only letters and numbers")]
    public string NewPassword { get; set; }
}