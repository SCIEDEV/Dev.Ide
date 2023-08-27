using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Dev.Ide.Models;
using Dev.Ide.Pseudo;
using System.Text;

namespace Dev.Ide.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Terminal()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Run([FromBody] RunCode run)
    {
        if(run == null || run.code == null || run.connectionId == null)
        {
            return BadRequest();
        }
        _logger.LogInformation(run.code);
        PsuedoEngine.ExecuteCode(run);
        return Ok();
    }

    [HttpPost]
    public IActionResult Input([FromBody] TermInput input)
    {
        if (input == null || input.input == null || input.connectionId == null)
        {
            return BadRequest();
        }
        PsuedoEngine.Input(input);
        _logger.LogInformation(input.input);
        return Ok();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

