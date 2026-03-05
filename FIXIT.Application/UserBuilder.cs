
namespace FIXIT.Application;

public class UserBuilder
{
    private ApplicationUser user;
    public UserBuilder(ApplicationUser user)
    {
        this.user = user;
    }

    public UserBuilder SetName(string name)
    {
        user.Name = name;
        return this;
    }
    public UserBuilder SetUserName(string username)
    {
        user.UserName = username;
        return this;
    }
    public UserBuilder SetEmail(string email)
    {
        user.Email = email;
        return this;
    }
    public UserBuilder SetPhone(string phone)
    {
        user.PhoneNumber = phone;
        return this;
    }
    public UserBuilder SetLocation(double longitude, double latitude)
    {
        user.Location = new NetTopologySuite.Geometries.Point(longitude, latitude) { SRID = 4326 };
        return this;
    }
    public UserBuilder SetImg(string imgPath)
    {
        user.Img = ImgPath.Create(imgPath);
        return this;
    }
    public ApplicationUser Build()
    {
        return user;
    }
}
