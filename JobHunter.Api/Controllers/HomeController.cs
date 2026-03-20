using Microsoft.AspNetCore.Mvc;

namespace JobHunter.Api.Controllers;

[ApiController]
[Route("")]
public class HomeController : ControllerBase
{
    [HttpGet]
    public IActionResult Index()
    {
        // Redirect to Swagger UI
        return Redirect("/swagger/index.html");
    }
}
