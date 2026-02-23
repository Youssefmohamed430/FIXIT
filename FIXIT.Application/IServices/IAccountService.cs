
namespace FIXIT.Application.IServices;

public interface IAccountService
{
    Task<Result<UserDTO>> UpdateUserInfo(string Id,UserDTO user);
    Task<Result<UserDTO>> UploadImg(string Id, IFormFile img);
    Task<Result<UserDTO>> GetImg(string Id);
}
