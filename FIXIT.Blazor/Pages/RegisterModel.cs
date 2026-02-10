using System.ComponentModel.DataAnnotations;

namespace FIXIT.Blazor.Pages;

public class RegisterModel
{
    [Required]
    [MinLength(3, ErrorMessage = "At Least three Letters")]
    [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Name must contain only letters")]
    public string? Name { get; set; }
    [Required]
    [MinLength(10, ErrorMessage = "At Least ten Letters")]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Username must contain only letters and numbers")]
    public string? UserName { get; set; }
    [Required]
    [EmailAddress(ErrorMessage = "In Valid Email")]
    public string? Email { get; set; }
    [Required]
    [MinLength(10, ErrorMessage = "At Least ten Letters")]
    [DataType(DataType.Password)]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Password must contain only letters and numbers")]
    public string? Password { get; set; }
    [Required]
    [RegularExpression(@"^\d+$", ErrorMessage = "Phone must contain numbers only")]
    public string? Phone { get; set; }
    [Required]
    public string? Role { get; set; }
    [Required]
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
