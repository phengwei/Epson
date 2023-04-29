using System;
using System.Threading;
using System.Threading.Tasks;
using Epson.Services.Interface.Email;
using Serilog;

public class EmailBackgroundService : IHostedService, IDisposable
{
    private Timer _timer;
    private readonly Serilog.ILogger _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public EmailBackgroundService(
        Serilog.ILogger logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.Information("[{0}] Begin executing process.", "SendEmailBackgroundProcess");
        //_timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(15));
        _logger.Information("[{0}] Finished executing process.", "SendEmailBackgroundProcess");
        return Task.CompletedTask;
    }

    private void DoWork(object state)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            emailService.SendEmailBatch();
        }
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}