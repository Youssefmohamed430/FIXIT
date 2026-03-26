
namespace FIXIT.Application.Servicces;

public class AccountService(IUnitOfWork unitOfWork,ILogger<AccountService> logger) : IAccountService
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

    private void HandleUpdatingUser(UserDTO user, ApplicationUser Olduser)
    {
        try
        {
            double? longitude = user.Longitude.HasValue ? user.Longitude.Value : Olduser.Location?.X;
            double? latitude = user.Latitude.HasValue ? user.Latitude.Value : Olduser.Location?.Y;

            Olduser = new UserBuilder(Olduser)
                .SetName(user.Name! ?? Olduser.Name)
                .SetUserName(user.UserName! ?? Olduser.UserName)
                .SetPhone(user.Phone! ?? Olduser.PhoneNumber)
                .SetEmail(user.Email! ?? Olduser.Email)
                .SetLocation(Convert.ToDouble(longitude), Convert.ToDouble(latitude))
                .SetImg(user.ImgPath != null ? user.ImgPath : Olduser.Img?.Value)
                .Build();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating user information for user with ID {UserId}", Olduser.Id);
        }
    }

    public async Task<Result<UserDTO>> UploadImg(string Id, IFormFile imgFile)
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
