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
using Epson.Core.Domain.Requests;
using Microsoft.AspNetCore.Identity;
using Epson.Core.Domain.Users;
using Epson.Services.Interface.Products;
using Epson.Services.Interface.Requests;
using Epson.Core.Domain.Products;

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
                var product = _productService.GetProductById(rp.ProductId);
                return product != null ? product.Name : "Unknown Product";
            });

            if (emailAccount == null)
                return new EmailQueue();

            var subject = "New Request";

            var body = $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <style>
                    body {{
                        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                        margin: 0;
                        padding: 0;
                        background-color: #f4f4f4;
                    }}
                    .email-container {{
                        max-width: 600px;
                        margin: auto;
                        background: #ffffff;
                        padding: 20px;
                        border: 1px solid #dddddd;
                    }}
                    .email-header {{
                        background-color: #004aad;
                        color: white;
                        padding: 10px 20px;
                        text-align: center;
                    }}
                    .email-body {{
                        padding: 20px;
                        line-height: 1.5;
                        color: #333333;
                    }}
                    table {{
                        width: 100%;
                        border-collapse: collapse;
                        margin-top: 20px;
                    }}
                    th, td {{
                        padding: 10px;
                        border: 1px solid #dddddd;
                        text-align: left;
                    }}
                    th {{
                        background-color: #f2f2f2;
                    }}
                </style>
            </head>
            <body>
                <div class='email-container'>
                    <div class='email-header'>
                        <h1>New Request</h1>
                    </div>
                    <div class='email-body'>
                        <p>A new request has been created with the following details:</p>
                        <table>
                            <tr>
                                <th>Requester</th>
                                <td>{requester.Result.UserName}</td>
                            </tr>
                            <tr>
                                <th>Total Budget</th>
                                <td>RM {request.TotalBudget}</td>
                            </tr>
                            <tr>
                                <th>Products</th>
                                <td>{string.Join(", ", productNames)}</td>
                            </tr>
                        </table>
                    </div>
                </div>
            </body>
            </html>";

            var emailQueue = new EmailQueue
            {
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

        public List<EmailQueue> NotifySalesSectionHeadUsers(Request request, List<RequestProduct> requestProducts)
        {
            var emailAccount = _EmailAccountRepository.GetAll().FirstOrDefault();
            var requesterTask = _userManager.FindByIdAsync(request.CreatedById);
            requesterTask.Wait(); 
            var requester = requesterTask;

            var salesSectionHeadUsersTask = _userManager.GetUsersInRoleAsync("Sales Section Head");
            salesSectionHeadUsersTask.Wait();
            var salesSectionHeadUsers = salesSectionHeadUsersTask.Result.ToList();

            var productNames = requestProducts.Select(rp =>
            {
                var product = _productService.GetProductById(rp.ProductId);
                return product != null ? product.Name : "Unknown Product";
            });

            var subject = "New Request";

            var body = $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <style>
                    body {{
                        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                        margin: 0;
                        padding: 0;
                        background-color: #f4f4f4;
                    }}
                    .email-container {{
                        max-width: 600px;
                        margin: auto;
                        background: #ffffff;
                        padding: 20px;
                        border: 1px solid #dddddd;
                    }}
                    .email-header {{
                        background-color: #004aad;
                        color: white;
                        padding: 10px 20px;
                        text-align: center;
                    }}
                    .email-body {{
                        padding: 20px;
                        line-height: 1.5;
                        color: #333333;
                    }}
                    .email-footer {{
                        text-align: center;
                        padding: 10px 20px;
                        background-color: #004aad;
                        color: white;
                    }}
                    table {{
                        width: 100%;
                        border-collapse: collapse;
                        margin-top: 20px;
                    }}
                    th, td {{
                        padding: 10px;
                        border: 1px solid #dddddd;
                        text-align: left;
                    }}
                    th {{
                        background-color: #f2f2f2;
                    }}
                </style>
            </head>
            <body>
                <div class='email-container'>
                    <div class='email-header'>
                        <h1>New Request</h1>
                    </div>
                    <div class='email-body'>
                        <p>A new request has been created with the following details:</p>
                        <table>
                            <tr>
                                <th>Requester</th>
                                <td>{requester.Result.UserName}</td>
                            </tr>
                            <tr>
                                <th>Total Budget</th>
                                <td>RM {request.TotalBudget}</td>
                            </tr>
                            <tr>
                                <th>Products</th>
                                <td>{string.Join(", ", productNames)}</td>
                            </tr>
                        </table>
                    </div>
                    <div class='email-footer'>
                        <p>Thank you for your submission.</p>
                    </div>
                </div>
            </body>
            </html>";


            if (emailAccount == null)
                return new List<EmailQueue>();

            var emailQueues = new List<EmailQueue>();

            foreach (var salesUser in salesSectionHeadUsers)
            {
                var emailQueue = new EmailQueue
                {
                    FromEmail = emailAccount.Username,
                    ToEmail = salesUser.Email,
                    Subject = subject,
                    Body = body,
                    ScheduleTime = DateTime.UtcNow,
                    SendAttempts = 0,
                    SentTime = null,
                    EmailAccountId = emailAccount.Id
                };
                emailQueues.Add(emailQueue);
            }


            return emailQueues;
        }

        public async Task<List<EmailQueue>> CreateReminderEmailQueue(RequestProduct requestProduct)
        {
            var emailAccount = _EmailAccountRepository.GetAll().FirstOrDefault();
            var fulfiller = _userManager.FindByIdAsync(requestProduct.FulfillerId);
            var product = _productService.GetProductById(requestProduct.ProductId);

            var subject = $"Request {requestProduct.Id} due soon!";

            var body = $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <style>
                body {{
                    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                    margin: 0;
                    padding: 0;
                    background-color: #f4f4f4;
                }}
                .email-container {{
                    max-width: 600px;
                    margin: auto;
                    background: #ffffff;
                    padding: 20px;
                    border: 1px solid #dddddd;
                }}
                .email-header {{
                    background-color: #004aad;
                    color: white;
                    padding: 10px 20px;
                    text-align: center;
                }}
                .email-body {{
                    padding: 20px;
                    line-height: 1.5;
                    color: #333333;
                }}
                .email-footer {{
                    text-align: center;
                    padding: 10px 20px;
                    background-color: #004aad;
                    color: white;
                }}
                table {{
                    width: 100%;
                    border-collapse: collapse;
                    margin-top: 20px;
                }}
                th, td {{
                    padding: 10px;
                    border: 1px solid #dddddd;
                    text-align: left;
                }}
                th {{
                    background-color: #f2f2f2;
                }}
            </style>
            </head>
            <body>
                <div class='email-container'>
                    <div class='email-header'>
                        <h1>Reminder</h1>
                    </div>
                    <div class='email-body'>
                        <p><strong>Request {requestProduct.Id}</strong> is due soon with the following details:</p>
                        <table>
                            <tr>
                                <th>Product</th>
                                <td>{product.Name}</td>
                            </tr>
                            <tr>
                                <th>Quantity</th>
                                <td>{requestProduct.Quantity}</td>
                            </tr>
                            <tr>
                                <th>End User Price</th>
                                <td>RM {requestProduct.EndUserPrice}</td>
                            </tr>
                            <tr>
                                <th>Request Created On</th>
                                <td>{requestProduct.CreatedOnUTC:MM/dd/yyyy HH:mm:ss}</td>
                            </tr>
                        </table>
                    </div>
                </div>
            </body>
            </html>";


            if (emailAccount == null)
                return new List<EmailQueue>();

            List<EmailQueue> emailQueues = new List<EmailQueue>();

            if (requestProduct.IsCoverplus == false)
            {
                var emailQueue = new EmailQueue
                {
                    FromEmail = emailAccount.Username,
                    ToEmail = fulfiller.Result.Email,
                    Subject = subject,
                    Body = body,
                    ScheduleTime = DateTime.UtcNow,
                    SendAttempts = 0,
                    SentTime = null,
                    EmailAccountId = emailAccount.Id
                };

                emailQueues.Add(emailQueue);
            }
            else
            {
                var coverplusFulfillers = await _userManager.GetUsersInRoleAsync("Coverplus");

                foreach (var coverplusFulfiller in coverplusFulfillers)
                {
                    var emailQueue = new EmailQueue
                    {
                        FromEmail = emailAccount.Username,
                        ToEmail = coverplusFulfiller.Email,
                        Subject = subject,
                        Body = body,
                        ScheduleTime = DateTime.UtcNow,
                        SendAttempts = 0,
                        SentTime = null,
                        EmailAccountId = emailAccount.Id
                    };

                    emailQueues.Add(emailQueue);
                }
            }

            return emailQueues;
        }


        public EmailQueue CreateFulfillEmailQueue(Request request, RequestProduct requestProduct, bool hasFulfillmentComplete)
        {
            //todo: configure to capture from user / request
            var emailAccount = _EmailAccountRepository.GetAll().FirstOrDefault();
            if (emailAccount == null)
                return new EmailQueue();
            var requester = _userManager.FindByIdAsync(request.CreatedById);
            var fulfiller = _userManager.FindByIdAsync(requestProduct.FulfillerId);
            var product = _productService.GetProductById(requestProduct.ProductId);

            var subject = "";
            if (hasFulfillmentComplete)
                subject = $"Request {request.Id} completed fulfillment";
            else
                subject = $"Request {request.Id} partially fulfillment";

            var body = $@"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <style>
                        body {{
                            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                            margin: 0;
                            padding: 0;
                            background-color: #f4f4f4;
                        }}
                        .email-container {{
                            max-width: 600px;
                            margin: auto;
                            background: #ffffff;
                            padding: 20px;
                            border: 1px solid #dddddd;
                        }}
                        .email-header {{
                            background-color: #004aad;
                            color: white;
                            padding: 10px 20px;
                            text-align: center;
                        }}
                        .email-body {{
                            padding: 20px;
                            line-height: 1.5;
                            color: #333333;
                        }}
                        table {{
                            width: 100%;
                            border-collapse: collapse;
                            margin-top: 20px;
                        }}
                        th, td {{
                            padding: 10px;
                            border: 1px solid #dddddd;
                            text-align: left;
                        }}
                        th {{
                            background-color: #f2f2f2;
                        }}
                    </style>
                </head>
                <body>
                    <div class='email-container'>
                        <div class='email-header'>
                            <h1>Request Fulfillment</h1>
                        </div>
                        <div class='email-body'>
                            <p>Request {request.Id} is fulfilled by {fulfiller.Result.UserName} with the following details:</p>
                            <table>
                                <tr>
                                    <th>Product</th>
                                    <td>{product.Name}</td>
                                </tr>
                                <tr>
                                    <th>Quantity</th>
                                    <td>{requestProduct.Quantity}</td>
                                </tr>
                                <tr>
                                    <th>Price Fulfilled</th>
                                    <td>RM {requestProduct.FulfilledPrice}</td>
                                </tr>
                            </table>
                        </div>
                    </div>
                </body>
                </html>";


            var emailQueue = new EmailQueue
            {
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
            if (emailAccount == null)
                return new EmailQueue();

            var requester = _userManager.FindByIdAsync(request.CreatedById);
            var fulfiller = _userManager.FindByIdAsync(requestProduct.FulfillerId);
            var product = _productService.GetProductById(requestProduct.ProductId);

            var subject = "";
            subject = $"Request {request.Id} amended by {requester.Result.UserName} ";

            var body = $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <style>
                    body {{
                        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                        margin: 0;
                        padding: 0;
                        background-color: #f4f4f4;
                    }}
                    .email-container {{
                        max-width: 600px;
                        margin: auto;
                        background: #ffffff;
                        padding: 20px;
                        border: 1px solid #dddddd;
                    }}
                    .email-header {{
                        background-color: #004aad;
                        color: white;
                        padding: 10px 20px;
                        text-align: center;
                    }}
                    .email-body {{
                        padding: 20px;
                        line-height: 1.5;
                        color: #333333;
                    }}
                    table {{
                        width: 100%;
                        border-collapse: collapse;
                        margin-top: 20px;
                    }}
                    th, td {{
                        padding: 10px;
                        border: 1px solid #dddddd;
                        text-align: left;
                    }}
                    th {{
                        background-color: #f2f2f2;
                    }}
                </style>
            </head>
            <body>
                <div class='email-container'>
                    <div class='email-header'>
                        <h1>Request Amendment</h1>
                    </div>
                    <div class='email-body'>
                        <p>The request is in an amended state by {fulfiller.Result.UserName} with the following old fulfilled details:</p>
                        <table>
                            <tr>
                                <th>Product</th>
                                <td>{product.Name}</td>
                            </tr>
                            <tr>
                                <th>Quantity</th>
                                <td>{requestProduct.Quantity}</td>
                            </tr>
                            <tr>
                                <th>Price Fulfilled</th>
                                <td>RM {requestProduct.FulfilledPrice}</td>
                            </tr>
                        </table>
                    </div>
                </div>
            </body>
            </html>";


            var emailQueue = new EmailQueue
            {
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
            if (emailAccount == null)
                return new EmailQueue();

            var requester = _userManager.FindByIdAsync(request.CreatedById);
            var fulfiller = _userManager.FindByIdAsync(requestProduct.FulfillerId);
            var product = _productService.GetProductById(requestProduct.ProductId);

            var subject = "";
            subject = $"Request {request.Id} amended by {requester.Result.UserName} ";

            var body = $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <style>
                    body {{
                        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                        margin: 0;
                        padding: 0;
                        background-color: #f4f4f4;
                    }}
                    .email-container {{
                        max-width: 600px;
                        margin: auto;
                        background: #ffffff;
                        padding: 20px;
                        border: 1px solid #dddddd;
                    }}
                    .email-header {{
                        background-color: #004aad;
                        color: white;
                        padding: 10px 20px;
                        text-align: center;
                    }}
                    .email-body {{
                        padding: 20px;
                        line-height: 1.5;
                        color: #333333;
                    }}
                    table {{
                        width: 100%;
                        border-collapse: collapse;
                        margin-top: 20px;
                    }}
                    th, td {{
                        padding: 10px;
                        border: 1px solid #dddddd;
                        text-align: left;
                    }}
                    th {{
                        background-color: #f2f2f2;
                    }}
                </style>
            </head>
            <body>
                <div class='email-container'>
                    <div class='email-header'>
                        <h1>Request Cancellation</h1>
                    </div>
                    <div class='email-body'>
                        <p>The request has been cancelled by {fulfiller.Result.UserName} with the following fulfilled details:</p>
                        <table>
                            <tr>
                                <th>Product</th>
                                <td>{product.Name}</td>
                            </tr>
                            <tr>
                                <th>Quantity</th>
                                <td>{requestProduct.Quantity}</td>
                            </tr>
                            <tr>
                                <th>Price Fulfilled</th>
                                <td>RM {requestProduct.FulfilledPrice}</td>
                            </tr>
                        </table>
                    </div>
                </div>
            </body>
            </html>";

            var emailQueue = new EmailQueue
            {
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

                message.IsBodyHtml = true;
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
