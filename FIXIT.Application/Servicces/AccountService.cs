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

        var userdto = new UserDTO { ImgPath = imgPath };

        return Result<UserDTO>.Success(userdto);
    }

    public async Task<Result<UserDTO>> UpdateUserInfo(string Id,UserDTO user)
    {
        var Olduser = await unitOfWork.GetRepository<ApplicationUser>().FindAsync(u => u.Id == Id);

        if (Olduser is null)
            return Result<UserDTO>.Failure(new Error("User.NotFound", "User not found"));

        HandleUpdatingUser(user, Olduser);

        await unitOfWork.GetRepository<ApplicationUser>().UpdateAsync(Olduser);
        await unitOfWork.SaveAsync();

        return Result<UserDTO>.Success(user);
    }

    private static void HandleUpdatingUser(UserDTO user, ApplicationUser Olduser)
    {
        Olduser.Name = user.Name! ?? Olduser.Name;
        Olduser.UserName = user.UserName! ?? Olduser.UserName;
        Olduser.Email = user.Email! ?? Olduser.Email;
        Olduser.PhoneNumber = user.Phone! ?? Olduser.PhoneNumber;
        Olduser.Location = user.Longitude.HasValue && user.Latitude.HasValue
             ? new NetTopologySuite.Geometries.Point(user.Longitude!.Value, user.Latitude!.Value) { SRID = 4326 }
             : Olduser.Location;
        Olduser.Img = user.ImgPath != null ? ImgPath.Create(user.ImgPath) : Olduser.Img;
    }

    public async Task<Result<UserDTO>> UploadImg(string Id,IFormFile imgFile)
    {
        var user = await unitOfWork.GetRepository<ApplicationUser>().FindAsync(u => u.Id == Id);

        if (user is null)
            return Result<UserDTO>.Failure(new Error("User.NotFound", "User not found"));

        string fileName = await HandleFoldersandFile(imgFile);

        user.Img = ImgPath.Create(fileName);

        await unitOfWork.GetRepository<ApplicationUser>().UpdateAsync(user);

        await unitOfWork.SaveAsync();

        return Result<UserDTO>.Success(new UserDTO { ImgPath = fileName });
    }

    private static async Task<string> HandleFoldersandFile(IFormFile imgFile)
    {
        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var fileName = Guid.NewGuid() + Path.GetExtension(imgFile.FileName);
        var filePath = Path.Combine(folderPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        { await imgFile.CopyToAsync(stream); }

        return fileName;
    }
}
