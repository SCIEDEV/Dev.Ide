using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Dev.Ide.Models;
using Dev.Ide.Pseudo;
using System.Text;
using Newtonsoft.Json;
using Microsoft.AspNetCore.SignalR;
using System.Reflection;

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

    [HttpPost]
    public async Task<IActionResult> Evaluate([FromForm] string code, [FromForm] string inputJson)
    {
        var rootPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        code = code.Replace("\\n", "\n");

        var runCode = new RunCode()
        {
            code = code,
            connectionId = RandomString(10)
        };

        while(System.IO.File.Exists(rootPath + $"/Buffer/{runCode.connectionId}.pseudo"))
        {
            runCode.connectionId = RandomString(10);
        }

        //_logger.LogInformation("WRITING TO " + rootPath + $"/Buffer/{runCode.connectionId}.pseudo");

        var worker = new PseudoWorker(runCode);

        var inputs = JsonConvert.DeserializeObject<List<string>>(inputJson);

        foreach(var input in inputs)
        {
            worker.Input(input);
        }

        var evaluate = new CodeEvaluate()
        {
            outputs = new List<string>(),
            errors = new List<string>()
        };

        while (!worker.terminate)
        {
            await Task.Delay(100);
            evaluate.outputs.AddRange(worker.outputs);
            worker.outputs.Clear();

            evaluate.errors.AddRange(worker.errors);
            worker.errors.Clear();
        }

        var evaluateJson = JsonConvert.SerializeObject(evaluate);

        System.IO.File.Delete(rootPath + $"/Buffer/{runCode.connectionId}.pseudo");

        return Content(evaluateJson);
    }

    private static Random random = new Random();

    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}