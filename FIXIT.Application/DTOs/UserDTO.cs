
using FIXIT.Application.Servicces;
using System.ComponentModel.DataAnnotations;

namespace FIXIT.Application.DTOs;

public class UserDTO
{
    [MinLength(3, ErrorMessage = "At Least three Letters")]
    [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Name must contain only letters")]
    public string? Name { get; set; }
    [MinLength(10, ErrorMessage = "At Least ten Letters")]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Username must contain only letters and numbers")]
    public string? UserName { get; set; }
    [EmailAddress(ErrorMessage = "In Valid Email")]
    public string? Email { get; set; }
    [RegularExpression(@"^\d+$", ErrorMessage = "Phone must contain numbers only")]
    public string? Phone { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? ImgPath { get; set; }
}
