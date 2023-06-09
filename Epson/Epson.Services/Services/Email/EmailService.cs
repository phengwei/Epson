﻿using System;
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
using Epson.Core.Domain.Requests;
using Microsoft.AspNetCore.Identity;
using Epson.Core.Domain.Users;
using Epson.Services.Interface.Products;

namespace Epson.Services.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IRepository<EmailAccount> _EmailAccountRepository;
        private readonly IRepository<EmailQueue> _EmailQueueRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProductService _productService;
        public EmailService
            (ILogger logger,
            IMapper mapper,
            IRepository<EmailAccount> EmailAccountRepository,
            IRepository<EmailQueue> EmailQueueRepository,
            UserManager<ApplicationUser> userManager,
            IProductService productService)
        {
            _logger = logger;
            _mapper = mapper;
            _EmailAccountRepository = EmailAccountRepository;
            _EmailQueueRepository = EmailQueueRepository;
            _userManager = userManager;
            _productService = productService;
        }

        public EmailAccountDTO GetEmailAccountById(int id)
        {
            var emailAccount = _EmailAccountRepository.GetById(id);

            return _mapper.Map<EmailAccountDTO>(emailAccount);
        }


        public EmailAccountDTO GetEmailAccountByUserName(string username)
        {
            var emailAccount = _EmailAccountRepository.GetAll().Where(x => x.Username == username).FirstOrDefault();

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

        public EmailQueue CreateRequestEmailQueue(Request request, List<RequestProduct> requestProducts)
        {
            //todo: configure to capture from user / request
            var emailAccount = _EmailAccountRepository.GetAll().FirstOrDefault();
            var requester = _userManager.FindByIdAsync(request.CreatedById);
            var productNames = requestProducts.Select(rp =>
            {
                var product = _productService.GetProductById(rp.Id);
                return product != null ? product.Name : "Unknown Product";
            });

            var subject = "New Request";

            var body = $"New request is created with the following details:\n" +
                       $"Requester: {requester.Result.UserName}\n" +
                       $"Priority: {request.Priority}\n" +
                       $"Total Budget: {request.TotalBudget}\n" +
                       $"Products: {string.Join(", ", productNames)}";

            var emailQueue = new EmailQueue
            {
                Priority = request.Priority,
                FromEmail = emailAccount.Username,
                ToEmail = requester.Result.Email,
                Subject = subject,
                Body = body,
                ScheduleTime = DateTime.UtcNow,
                SendAttempts = 0,
                SentTime = null,
                EmailAccountId = emailAccount.Id
            };

            return emailQueue;
        }

        public EmailQueue CreateFulfillEmailQueue(Request request, RequestProduct requestProduct, bool hasFulfillmentComplete)
        {
            //todo: configure to capture from user / request
            var emailAccount = _EmailAccountRepository.GetAll().FirstOrDefault();
            var requester = _userManager.FindByIdAsync(request.CreatedById);
            var fulfiller = _userManager.FindByIdAsync(requestProduct.FulfillerId);
            var productName = _productService.GetProductById(requestProduct.ProductId);

            var subject = "";
            if (hasFulfillmentComplete)
                subject = $"Request {request.Id} completed fulfillment";
            else
                subject = $"Request {request.Id} partially fulfillment";

            var body = $"Request is fulfilled by {fulfiller.Result.UserName} with the following details:\n" +
                       $"Product: {productName}\n" +
                       $"Quantity: {requestProduct.Quantity}\n" +
                       $"Price fulfilled: {requestProduct.FulfilledPrice}";

            var emailQueue = new EmailQueue
            {
                Priority = request.Priority,
                FromEmail = emailAccount.Username,
                ToEmail = requester.Result.Email,
                Subject = subject,
                Body = body,
                ScheduleTime = DateTime.UtcNow,
                SendAttempts = 0,
                SentTime = null,
                EmailAccountId = emailAccount.Id
            };

            return emailQueue;
        }

        public EmailQueue CreateAmendQuotationEmailQueue(Request request, RequestProduct requestProduct)
        {
            //todo: configure to capture from user / request
            var emailAccount = _EmailAccountRepository.GetAll().FirstOrDefault();
            var requester = _userManager.FindByIdAsync(request.CreatedById);
            var fulfiller = _userManager.FindByIdAsync(requestProduct.FulfillerId);
            var productName = _productService.GetProductById(requestProduct.ProductId);

            var subject = "";
            subject = $"Request {request.Id} amended by {requester.Result.UserName} ";

            var body = $"Request is in amend state by {fulfiller.Result.UserName} with these being the old fulfilled details:\n" +
                       $"Product: {productName}\n" +
                       $"Quantity: {requestProduct.Quantity}\n" +
                       $"Price fulfilled: {requestProduct.FulfilledPrice}";

            var emailQueue = new EmailQueue
            {
                Priority = request.Priority,
                FromEmail = emailAccount.Username,
                ToEmail = requester.Result.Email,
                Subject = subject,
                Body = body,
                ScheduleTime = DateTime.UtcNow,
                SendAttempts = 0,
                SentTime = null,
                EmailAccountId = emailAccount.Id
            };

            return emailQueue;
        }

        public EmailQueue CreateCancellationEmailQueue(Request request, RequestProduct requestProduct)
        {
            //todo: configure to capture from user / request
            var emailAccount = _EmailAccountRepository.GetAll().FirstOrDefault();
            var requester = _userManager.FindByIdAsync(request.CreatedById);
            var fulfiller = _userManager.FindByIdAsync(requestProduct.FulfillerId);
            var productName = _productService.GetProductById(requestProduct.ProductId);

            var subject = "";
            subject = $"Request {request.Id} amended by {requester.Result.UserName} ";

            var body = $"Request is cancelled by {fulfiller.Result.UserName} with these being the fulfilled details:\n" +
                       $"Product: {productName}\n" +
                       $"Quantity: {requestProduct.Quantity}\n" +
                       $"Price fulfilled: {requestProduct.FulfilledPrice}";

            var emailQueue = new EmailQueue
            {
                Priority = request.Priority,
                FromEmail = emailAccount.Username,
                ToEmail = requester.Result.Email,
                Subject = subject,
                Body = body,
                ScheduleTime = DateTime.UtcNow,
                SendAttempts = 0,
                SentTime = null,
                EmailAccountId = emailAccount.Id
            };

            return emailQueue;
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
                        emailQueue.SendAttempts += 1;
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
