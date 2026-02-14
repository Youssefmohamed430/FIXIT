using FIXIT.Application.DTOs;
using FIXIT.Application.IServices;
using FIXIT.Domain.Abstractions;
using FIXIT.Domain.Entities;
using FIXIT.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FIXIT.Application.Servicces;

public class AccountService(IUnitOfWork unitOfWork) : IAccountService
{
    public async Task<Result<UserDTO>> GetImg(string Id)
    {
        var user = await unitOfWork.GetRepository<ApplicationUser>().FindAsync(u => u.Id == Id);

        if (user is null)
            return Result<UserDTO>.Failure(new Error("User.NotFound", "User not found"));

        var imgPath = user.Img?.Value;

        if (imgPath == null)
            return Result<UserDTO>.Failure(new Error("User.NoImage", "User has no image"));

        var userdto = new UserDTO
        {
            ImgPath = imgPath
        };

        return Result<UserDTO>.Success(userdto);
    }

    public async Task<Result<UserDTO>> UpdateUserInfo(string Id,UserDTO user)
    {
        var Olduser = await unitOfWork.GetRepository<ApplicationUser>().FindAsync(u => u.Id == Id);

        if (Olduser is null)
            return Result<UserDTO>.Failure(new Error("User.NotFound","User not found"));

        Olduser.Name = user.Name!;
        Olduser.Email = user.Email!;
        Olduser.PhoneNumber = user.Phone!;
        Olduser.Location = new NetTopologySuite.Geometries.Point(user.Longitude!.Value, user.Latitude!.Value) { SRID = 4326 };

        await unitOfWork.GetRepository<ApplicationUser>().UpdateAsync(Olduser);
        await unitOfWork.SaveAsync();

        return Result<UserDTO>.Success(user);
    }

    public async Task<Result<UserDTO>> UploadImg(string Id,IFormFile imgFile)
    {
        var user = await unitOfWork.GetRepository<ApplicationUser>().FindAsync(u => u.Id == Id);

        if (user is null)
            return Result<UserDTO>.Failure(new Error("User.NotFound", "User not found"));

        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var fileName = Guid.NewGuid() + Path.GetExtension(imgFile.FileName);
        var filePath = Path.Combine(folderPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await imgFile.CopyToAsync(stream);
        }

        var imgPath = ImgPath.Create($"/images/{fileName}");

        user.Img = imgPath;

        await unitOfWork.GetRepository<ApplicationUser>().UpdateAsync(user);

        await unitOfWork.SaveAsync();

        return Result<UserDTO>.Success(null!);
    }
}
