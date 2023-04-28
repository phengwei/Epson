using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Epson.Services.Interface.Email;
using Epson.Services.DTO.Email;
using Epson.Data;
using Epson.Core.Domain.Email;
using AutoMapper;
using MimeKit;
using Microsoft.IdentityModel.Tokens;

namespace Epson.Services.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IRepository<EmailAccount> _EmailAccountRepository;
        private readonly IRepository<EmailQueue> _EmailQueueRepository;
        public EmailService
            (ILogger logger,
            IMapper mapper,
            IRepository<EmailAccount> EmailAccountRepository,
            IRepository<EmailQueue> EmailQueueRepository)
        {
            _logger = logger;
            _mapper = mapper;
            _EmailAccountRepository = EmailAccountRepository;
            _EmailQueueRepository = EmailQueueRepository;
        }

        public EmailAccountDTO GetEmailAccountById(int id)
        {
            var emailAccount = _EmailAccountRepository.GetById(id);

            return _mapper.Map<EmailAccountDTO>(emailAccount);
        }

        //only get unsent and < 3 sent attempt queues 
        public List<EmailQueueDTO> GetUnsentEmailQueues()
        {
            var emailQueues = _EmailQueueRepository.GetAll().Where(x => x.SendAttempts < 3 && x.SentTime == null);

            return _mapper.Map<List<EmailQueueDTO>>(emailQueues);
        }

        public bool InsertEmailQueue(EmailQueue emailQueue)
        {
            if (emailQueue == null)
                throw new ArgumentNullException(nameof(emailQueue));

            try
            {
                _EmailQueueRepository.Add(emailQueue);
                _logger.Information("Inserting email queue about {subject}", emailQueue.Subject);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error inserting email queue {subject}", emailQueue.Subject);

                return false;
            }
        }

        public void SendEmailBatch()
        {
            var emailQueues = GetUnsentEmailQueues();
            foreach (var emailQueue in emailQueues)
            {
                var emailAccount = GetEmailAccountById(emailQueue.EmailAccountId);

                MailMessage message = new MailMessage(emailQueue.FromEmail, emailQueue.ToEmail, emailQueue.Subject, emailQueue.Body);

                SmtpClient client = new SmtpClient(emailAccount.OutgoingServer, int.Parse(emailAccount.OutgoingPort));

                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(emailAccount.Username, emailAccount.Password);
                client.EnableSsl = true;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;

                if (DateTime.UtcNow > emailQueue.ScheduleTime.ToUniversalTime())
                {
                    try
                    {
                        client.Send(message);
                        _logger.Information("Email of queue {emailQueue} successfully sent!", emailQueue.Id);
                        emailQueue.SentTime = DateTime.UtcNow;
                        _EmailQueueRepository.Update(_mapper.Map<EmailQueue>(emailQueue));
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Failed to send email of queue {emailQueue} ", emailQueue.Id);
                        emailQueue.SendAttempts += 1;
                        _EmailQueueRepository.Update(_mapper.Map<EmailQueue>(emailQueue));
                    }
                }
                else
                {
                    _logger.Information("Email of queue {emailQueue} is scheduled for {scheduledTime}. Skipped sending email", emailQueue.Id, emailQueue.ScheduleTime);
                }
 
            }
        }
    }
}
