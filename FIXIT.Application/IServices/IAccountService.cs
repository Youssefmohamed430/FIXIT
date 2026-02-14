using FIXIT.Application.DTOs;
using FIXIT.Domain.Abstractions;
using FIXIT.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;

namespace FIXIT.Application.IServices;

public interface IAccountService
{
    Task<Result<UserDTO>> UpdateUserInfo(string Id,UserDTO user);
    Task<Result<UserDTO>> UploadImg(string Id, IFormFile img);
    Task<Result<UserDTO>> GetImg(string Id);
}
