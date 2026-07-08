
using Microsoft.Extensions.Localization;

namespace FIXIT.Application.Servicces;

public class AccountService(IUnitOfWork unitOfWork,ILogger<AccountService> logger, IStringLocalizer<AccountService> _localizer) : IAccountService
{
    public async Task<Result<UserDTO>> GetUserInfo(string Id)
    {
        var user = await unitOfWork.GetRepository<ApplicationUser>()
            .FindWithAttributesAsync<UserDTO>(u => u.Id == Id, u => new UserDTO
            {
                Name = u.Name,
                UserName = u.UserName,
                Phone = u.PhoneNumber,
                Email = u.Email,
                Longitude = u.Location.X,
                Latitude = u.Location.Y,
                ImgPath = u.Img!.Value
            });
        if (user is null)
            return Result<UserDTO>.Failure(new Error("User.NotFound", _localizer["User.NotFound"]));
        return Result<UserDTO>.Success(user);
    }
    public async Task<Result<UserDTO>> GetImg(string Id)
    {
        var user = await unitOfWork.GetRepository<ApplicationUser>()
            .FindWithAttributesAsync<UserDTO>(u => u.Id == Id, u => new UserDTO { ImgPath = u.Img!.Value });

        if (user is null)
            return Result<UserDTO>.Failure(new Error("User.NotFound", _localizer["User.NotFound"]));

        var imgPath = user.ImgPath;

        if (imgPath == null)
            return Result<UserDTO>.Failure(new Error("User.NoImage", _localizer["User.NoImage"]));

        var userdto = new UserDTO { ImgPath = imgPath };

        return Result<UserDTO>.Success(userdto);
    }

    public async Task<Result<UserDTO>> UpdateUserInfo(string Id,UserDTO user)
    {
        var existingUser = await unitOfWork.GetRepository<ApplicationUser>().FindAsync(u => u.Id == Id);

        if (existingUser is null)
            return Result<UserDTO>.Failure(new Error("User.NotFound", _localizer["User.NotFound"]));

        existingUser.UpdateFrom(
            user.Name,
            user.UserName,
            user.Phone,
            user.Email,
            user.Longitude,
            user.Latitude,
            user.ImgPath
        );

        await unitOfWork.GetRepository<ApplicationUser>().UpdateAsync(existingUser);
        await unitOfWork.SaveAsync();

        return Result<UserDTO>.Success(user);
    }

    public async Task<Result<UserDTO>> UploadImg(string Id, IFormFile imgFile)
    {
        var user = await unitOfWork.GetRepository<ApplicationUser>().FindAsync(u => u.Id == Id);

        if (user is null)
            return Result<UserDTO>.Failure(new Error("User.NotFound", _localizer["User.NotFound"]));

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
