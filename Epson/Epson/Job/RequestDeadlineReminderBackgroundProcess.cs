using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epson.Core.Domain.Email;
using Epson.Core.Domain.Enum;
using Epson.Core.Domain.Requests;
using Epson.Data;
using Epson.Data.Context;
using Epson.Extensions;
using Epson.Services.Interface.Email;
using Epson.Services.Interface.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Epson.Job
{
    public class DeadlineReminderService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Serilog.ILogger _logger;
        private Timer _timer;

        public DeadlineReminderService(
            IServiceProvider serviceProvider, 
            Serilog.ILogger logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Information("[{0}] Begin executing process.", "RequestDeadlineReminderBackgroundProcess");
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromHours(24));
            _logger.Information("[{0}] Finished executing process.", "RequestDeadlineReminderBackgroundProcess");
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<EpsonDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var requestProductRepository = scope.ServiceProvider.GetRequiredService<IRepository<RequestProduct>>();

            var requestProducts = await dbContext.RequestProduct
                .Where(rp => !rp.HasFulfilled
                             && !rp.HasReminded
                             && rp.Status != (int)RequestProductStatusEnum.Cancelled
                             && dbContext.Request.Any(r => r.Id == rp.RequestId
                                 && dbContext.ProjectInformation.Any(p => p.RequestId == r.Id)))
                .ToListAsync();

            var groupedRequestProducts = requestProducts
                .GroupBy(rp => rp.RequestId)
                .ToList();

            List<EmailQueue> emailQueues = new List<EmailQueue>();

            foreach (var requestProductGroup in groupedRequestProducts)
            {
                var requestId = requestProductGroup.Key;
                var request = await dbContext.Request.FirstOrDefaultAsync(x => x.Id == requestId);

                if (request != null)
                {
                    var referenceTime = request.AmendQuotationTime ?? request.ApprovedTime;
                    int workingDays = 0;

                    List<RequestProduct> productsToNotify = new List<RequestProduct>();
                    foreach (var rp in requestProductGroup)
                    {
                        var sla = string.IsNullOrWhiteSpace(rp.SLA) ? "Local" : rp.SLA;

                        if (sla == "Local")
                            workingDays = 3;
                        else if (sla == "Regional")
                            workingDays = 5;
                        else if (sla == "SEC")
                            workingDays = 10;
                        else
                            workingDays = 3; // default to Local if SLA is invalid or missing

                        if (referenceTime != DateTime.MinValue && referenceTime.AddWorkingDays(workingDays) <= DateTime.UtcNow)
                        {
                            productsToNotify.Add(rp);
                        }
                    }

                    if (productsToNotify.Any())
                    {
                        var queues = await emailService.CreateReminderEmailQueue(productsToNotify);
                        emailQueues.AddRange(queues);

                        foreach (var rp in productsToNotify)
                        {
                            rp.HasReminded = true;
                            requestProductRepository.Update(rp);
                        }
                    }
                }
            }

            foreach (var emailQueue in emailQueues)
            {
                emailService.InsertEmailQueue(emailQueue);
            }
        }


        public Task StopAsync(CancellationToken cancellationToken)
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
