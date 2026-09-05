using Garimpei.Services;
using Microsoft.AspNetCore.Mvc;

namespace Garimpei.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _service;
    public DashboardController(DashboardService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Obter()
        => Ok(await _service.ObterAsync());
}
