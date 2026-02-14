
using FIXIT.Application.IServices;
using Microsoft.AspNetCore.Mvc;

namespace FIXIT.Presentation.Controllers;
[ApiController]
[Route("[controller]")]
public class AccountContrtoller(IServiceManager serviceManager)
{
}
