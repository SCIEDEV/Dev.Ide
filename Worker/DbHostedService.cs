using System;
using System.Reflection;
using System.Text;
using Dev.Ide.Hubs;
using Dev.Ide.Pseudo;
using Microsoft.AspNetCore.SignalR;

namespace Dev.Ide.Worker
{
    public class DbHostedService : IHostedService
    {
        private readonly ILogger _logger;
        private Timer _timer;
        IWebHostEnvironment environment;

        public bool isProcessing = false;

        public DbHostedService(IServiceProvider services, IWebHostEnvironment _environment, ILogger<DbHostedService> logger)
        {
            Services = services;
            _logger = logger;
            environment = _environment;
        }

        public IServiceProvider Services { get; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Consume Scoped Service Hosted Service is starting.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromMilliseconds(50));

            return Task.CompletedTask;
        }

        public List<string> ToDelete = new List<string>();

        private async void DoWork(object state)
        {
            if (isProcessing) return;
            isProcessing = true;

            foreach (var path in ToDelete)
            {
                if (File.Exists(path)) File.Delete(path);
            }
            ToDelete.Clear();

            //_logger.LogInformation("Consume Scoped Service Hosted Service is working.");

            try
            {

                using (var client = new HttpClient())
                using (var scope = Services.CreateScope())
                {
                    //var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    //step one, check for all notes if they have the subjects

                    var hub = scope.ServiceProvider.GetRequiredService<IHubContext<TerminalHub>>();

                    List<string> toTerminate = new List<string>();

                    foreach (var w in PsuedoEngine.workers)
                    {
                        try
                        {
                            var worker = w.Value;
                            var _outputs = worker.outputs.ToList();
                            worker.outputs.Clear();
                            foreach (var output in _outputs)
                            {
                                await hub.Clients.Client(w.Key).SendAsync("Output", output);
                            }
                            var _errors = worker.errors.ToList();
                            worker.errors.Clear();
                            foreach (var error in _errors)
                            {
                                await hub.Clients.Client(w.Key).SendAsync("Error", error);
                            }
                            worker.errors.Clear();

                            if (worker.terminate) toTerminate.Add(w.Key);
                        }
                        catch (Exception ex)
                        {

                        }
                    }

                    var rootPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                    foreach (var t in toTerminate)
                    {
                        await hub.Clients.Client(t).SendAsync("Complete");
                        File.Delete(rootPath + $"/Buffer/{t}.pseudo");
                        PsuedoEngine.workers.Remove(t);
                    }
                }
            }
            catch(Exception ex)
            {

            }

            //_logger.LogInformation("WORKER COMPLETE");

            isProcessing = false;
        }

        

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Consume Scoped Service Hosted Service is stopping.");

            return Task.CompletedTask;
        }
    }
}