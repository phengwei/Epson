using System;
using System.Threading;
using System.Threading.Tasks;
using Epson.Services.Interface.Email;
using Serilog;

namespace Epson.Job
{

    public class EmailBackgroundService : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly Serilog.ILogger _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private bool _isProcessing;

        public EmailBackgroundService(
            Serilog.ILogger logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _isProcessing = false;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.Information("[{0}] Begin executing process.", "SendEmailBackgroundProcess");
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            if (_isProcessing)
            {
                _logger.Information("Email batch processing is already running. Skipping this execution.");
                return;
            }

            _isProcessing = true;

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                await emailService.SendEmailBatchAsync();
            }

            _isProcessing = false;
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
}