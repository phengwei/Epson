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
using Epson.Services.Interface.Users;
using Epson.Services.Interface.Categories;
using Epson.Core.Domain.Categories;
using Epson.Services.DTO.Requests;

namespace Epson.Services.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IRepository<EmailAccount> _EmailAccountRepository;
        private readonly IRepository<EmailQueue> _EmailQueueRepository;
        private readonly IRepository<ProjectInformation> _ProjectInformationRepository;
        private readonly IRepository<Request> _RequestRepository;
        private readonly IRepository<RequestProduct> _RequestProductRepository;
        private readonly IRepository<Team> _TeamRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IUserService _userService;
        public EmailService
            (ILogger logger,
            IMapper mapper,
            IRepository<EmailAccount> EmailAccountRepository,
            IRepository<EmailQueue> EmailQueueRepository,
            IRepository<ProjectInformation> ProjectInformationRepository,
            IRepository<Request> RequestRepository,
            IRepository<RequestProduct> RequestProductRepository,
            IRepository<Team> TeamRepository,
            UserManager<ApplicationUser> userManager,
            IProductService productService,
            ICategoryService categoryService,
            IUserService userService)
        {
            _logger = logger;
            _mapper = mapper;
            _EmailAccountRepository = EmailAccountRepository;
            _EmailQueueRepository = EmailQueueRepository;
            _ProjectInformationRepository = ProjectInformationRepository;
            _RequestRepository = RequestRepository;
            _RequestProductRepository = RequestProductRepository;
            _TeamRepository = TeamRepository;
            _userManager = userManager;
            _productService = productService;
            _categoryService = categoryService;
            _userService = userService;
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

        public EmailQueue CreateRequestEmailQueue(RequestDTO request, List<RequestProductDTO> requestProducts)
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

        public EmailQueue CreateApprovedEmailQueue(Request request, List<RequestProduct> requestProducts)
        {
            var emailAccount = _EmailAccountRepository.GetAll().FirstOrDefault();
            var requester = _userManager.FindByIdAsync(request.CreatedById);
            var productNames = requestProducts.Select(rp =>
            {
                var product = _productService.GetProductById(rp.ProductId);
                return product != null ? product.Name : "Unknown Product";
            });

            if (emailAccount == null)
                return new EmailQueue();

            var subject = $"Request { request.Id } has been approved";

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
                            <p>A new request has been approved with the following details:</p>
                            <table>
                                <tr>
                                    <th>Org</th>
                                    <td>EMSB - {_userService.GetTeamById(requester.Result.TeamId).Name}</td>
                                </tr>
                                <tr>
                                    <th>Requester</th>
                                    <td>{requester.Result.UserName}</td>
                                </tr>
                                <tr>
                                    <th>Total Budget</th>
                                    <td>RM {request.TotalBudget}</td>
                                </tr>
                                <tr>
                                    <th>End User</th>
                                    <td>{_ProjectInformationRepository.GetAll().Where(x => x.RequestId == request.Id).FirstOrDefault().ProjectName}</td>
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

        public List<EmailQueue> NotifySalesSectionHeadUsersOnApprovedRequest(Request request, List<RequestProduct> requestProducts)
        {
            var emailAccount = _EmailAccountRepository.GetAll().FirstOrDefault();
            var requesterTask = _userManager.FindByIdAsync(request.CreatedById);
            requesterTask.Wait();
            var requester = requesterTask.Result;

            var salesHeadUsersTask = _userManager.GetUsersInRoleAsync("Sales Section Head");
            salesHeadUsersTask.Wait();
            var salesHeadUsers = salesHeadUsersTask.Result;

            bool isSalesHead = salesHeadUsers.Any(user => user.Id == requester.Id);

            var approverTask = _userManager.FindByIdAsync(request.ApprovedBy);
            approverTask.Wait();
            var approver = approverTask.Result;

            var salesSectionHeadUsersTask = _userService.GetUserSalesHead(requester.TeamId, request.CreatedById, isSalesHead);
            salesSectionHeadUsersTask.Wait();
            var salesSectionHeadUsers = salesSectionHeadUsersTask.Result.ToList();

            if (salesSectionHeadUsers.Count > 0)
            {
                var productNames = requestProducts.Select(rp =>
                {
                    var product = _productService.GetProductById(rp.ProductId);
                    return product != null ? product.Name : "Unknown Product";
                });

                var subject = $"Request {request.Id} has been approved";

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
                                <h1>Approved Request</h1>
                            </div>
                            <div class='email-body'>
                                <p>A request has been approved with the following details:</p>
                                <table>
                                    <tr>
                                        <th>Org</th>
                                        <td>EMSB - {_userService.GetTeamById(requester.TeamId).Name}</td>
                                    </tr>
                                    <tr>
                                        <th>Requester</th>
                                        <td>{requester.UserName}</td>
                                    </tr>
                                    <tr>
                                        <th>Total Budget</th>
                                        <td>RM {request.TotalBudget}</td>
                                    </tr>
                                    <tr>
                                        <th>End User</th>
                                        <td>{_ProjectInformationRepository.GetAll().FirstOrDefault(x => x.RequestId == request.Id)?.ProjectName}</td>
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

                var salesSectionHeadEmails = string.Join(",", salesSectionHeadUsers.Select(u => u.Email));

                var emailQueues = new List<EmailQueue>();

                if (salesSectionHeadUsers.Any(u => u.Email == approver.Email))
                {
                    var emailQueue = new EmailQueue
                    {
                        FromEmail = emailAccount.Username,
                        ToEmail = salesSectionHeadEmails,
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
                    var emailQueue = new EmailQueue
                    {
                        FromEmail = emailAccount.Username,
                        ToEmail = approver.Email,
                        Subject = subject,
                        Body = body,
                        ScheduleTime = DateTime.UtcNow,
                        SendAttempts = 0,
                        SentTime = null,
                        Cc = salesSectionHeadEmails,
                        EmailAccountId = emailAccount.Id
                    };
                    emailQueues.Add(emailQueue);
                }

                return emailQueues;
            }
            else
            {
                return new List<EmailQueue>();
            }
        }


        public List<EmailQueue> NotifySalesOperationTeamsOnApprovedRequest(Request request, List<RequestProduct> requestProducts)
        {
            var emailAccount = _EmailAccountRepository.GetAll().FirstOrDefault();
            var requesterTask = _userManager.FindByIdAsync(request.CreatedById);
            requesterTask.Wait();
            var requester = requesterTask;

            var salesOperationUsersTask = _userManager.GetUsersInRoleAsync("Sales Operation");
            salesOperationUsersTask.Wait();

            var salesOperationUsers = salesOperationUsersTask;

            if (salesOperationUsers.Result.Count > 0)
            {
                var productNames = requestProducts.Select(rp =>
                {
                    var product = _productService.GetProductById(rp.ProductId);
                    return product != null ? product.Name : "Unknown Product";
                });

                var subject = $"Request {request.Id} has been approved";

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
                            <h1>Approved Request</h1>
                        </div>
                        <div class='email-body'>
                            <p>A request has been approved with the following details:</p>
                            <table>
                                <tr>
                                    <th>Org</th>
                                    <td>EMSB - {_userService.GetTeamById(requester.Result.TeamId).Name}</td>
                                </tr>
                                <tr>
                                    <th>Requester</th>
                                    <td>{requester.Result.UserName}</td>
                                </tr>
                                <tr>
                                    <th>Total Budget</th>
                                    <td>RM {request.TotalBudget}</td>
                                </tr>
                                <tr>
                                    <th>End User</th>
                                    <td>{_ProjectInformationRepository.GetAll().Where(x => x.RequestId == request.Id).FirstOrDefault().ProjectName}</td>
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

                foreach (var salesOpUser in salesOperationUsers.Result)
                {
                    var emailQueue = new EmailQueue
                    {
                        FromEmail = emailAccount.Username,
                        ToEmail = salesOpUser.Email,
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
            else
            {
                return new List<EmailQueue>();
            }
        }

        public async Task<List<EmailQueue>> NotifyFulfillers(Request request)
        {
            var emailAccount = _EmailAccountRepository.GetAll().FirstOrDefault();
            var requesterTask = _userManager.FindByIdAsync(request.CreatedById);
            requesterTask.Wait();
            var requester = requesterTask;

            if (emailAccount == null)
                return new List<EmailQueue>();

            var emailQueues = new List<EmailQueue>();

            List<RequestProduct> requestProducts = _RequestProductRepository.GetAll().Where(x => x.RequestId == request.Id).ToList();
            List<ApplicationUser> fulfillers = new List<ApplicationUser>();

            foreach (var rp in requestProducts)
            {
                var productCategories = _productService.GetCategoryIdsByProductId(rp.ProductId);
                List<string> backupFulfillerEmails = new List<string>();

                foreach (var pc in productCategories)
                {
                    var category = _categoryService.GetCategoryById(pc.CategoryId);

                    var backupFulfiller1 = await _userManager.FindByIdAsync(category.BackupFulfiller1);
                    var backupFulfiller2 = await _userManager.FindByIdAsync(category.BackupFulfiller2);

                    if (backupFulfiller1 != null)
                    {
                        backupFulfillerEmails.Add(backupFulfiller1.Email);
                    }
                    if (backupFulfiller2 != null)
                    {
                        backupFulfillerEmails.Add(backupFulfiller2.Email);
                    }
                }

                ApplicationUser fulfiller = await _userManager.FindByIdAsync(rp.FulfillerId);
                fulfillers.Add(fulfiller);

                var productNames = requestProducts.Where(x => x.FulfillerId == rp.FulfillerId).Select(rp =>
                {
                    var product = _productService.GetProductById(rp.ProductId);
                    return product != null ? product.Name : "Unknown Product";
                });

                var subject = "Request ready to be fulfilled";

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
                        <p>A new request has been approved and is ready to be fulfilled with the following details:</p>
                        <table>
                            <tr>
                                <th>Org</th>
                                <td>EMSB - {_userService.GetTeamById(requester.Result.TeamId).Name}</td>
                            </tr>
                            <tr>
                                <th>Requester</th>
                                <td>{requester.Result.UserName}</td>
                            </tr>
                            <tr>
                                <th>Total Budget</th>
                                <td>RM {request.TotalBudget.ToString("N2")}</td>
                            </tr>
                            <tr>
                                <th>End User</th>
                                <td>{_ProjectInformationRepository.GetAll().Where(x => x.RequestId == request.Id).FirstOrDefault()?.ProjectName}</td>
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

                HashSet<string> uniqueEmails = new HashSet<string>(backupFulfillerEmails);

                if (uniqueEmails.Contains(fulfiller.Email))
                {
                    uniqueEmails.Remove(fulfiller.Email);
                }

                string ccEmails = string.Join(" ", uniqueEmails);

                var emailQueue = new EmailQueue
                {
                    FromEmail = emailAccount.Username,
                    ToEmail = fulfiller.Email,
                    Subject = subject,
                    Body = body,
                    ScheduleTime = DateTime.UtcNow,
                    SendAttempts = 0,
                    SentTime = null,
                    Cc = ccEmails,
                    EmailAccountId = emailAccount.Id
                };
                emailQueues.Add(emailQueue);
            }

            var distinctEmailQueues = emailQueues
                .GroupBy(eq => eq.ToEmail)
                .Select(group => group.First())
                .ToList();

            return distinctEmailQueues;
        }


        public List<EmailQueue> NotifySalesSectionHeadUsers(RequestDTO request, List<RequestProductDTO> requestProducts)
        {
            var emailAccount = _EmailAccountRepository.GetAll().FirstOrDefault();
            var requesterTask = _userManager.FindByIdAsync(request.CreatedById);
            requesterTask.Wait(); 
            var requester = requesterTask;

            var salesHeadUsers = _userManager.GetUsersInRoleAsync("Sales Section Head");
            salesHeadUsers.Wait();

            bool isSalesHead = salesHeadUsers.Result.Any(user => user.Id == requester.Result.Id);

            List<ApplicationUser> salesSectionHeadUsers = _userService.GetUserSalesHead(requester.Result.TeamId, request.CreatedById, isSalesHead).Result.ToList();

            if (salesSectionHeadUsers.Count > 0)
            {
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
                                    <th>Org</th>
                                    <td>EMSB - {_userService.GetTeamById(requester.Result.TeamId).Name}</td>
                                </tr>
                                <tr>
                                    <th>Requester</th>
                                    <td>{requester.Result.UserName}</td>
                                </tr>
                                <tr>
                                    <th>Total Budget</th>
                                    <td>RM {request.TotalBudget}</td>
                                </tr>
                                <tr>
                                    <th>End User</th>
                                    <td>{_ProjectInformationRepository.GetAll().Where(x => x.RequestId == request.Id).FirstOrDefault().ProjectName}</td>
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


                var salesSectionHeadEmails = string.Join(",", salesSectionHeadUsers.Select(u => u.Email));

                if (emailAccount == null)
                    return new List<EmailQueue>();

                var emailQueues = new List<EmailQueue>();

                var emailQueue = new EmailQueue
                {
                    FromEmail = emailAccount.Username,
                    ToEmail = salesSectionHeadEmails,
                    Subject = subject,
                    Body = body,
                    ScheduleTime = DateTime.UtcNow,
                    SendAttempts = 0,
                    SentTime = null,
                    EmailAccountId = emailAccount.Id
                };
                emailQueues.Add(emailQueue);

                return emailQueues;
            }
            else
            {
                return new List<EmailQueue>();
            }
        }

        public async Task<List<EmailQueue>> CreateReminderEmailQueue(RequestProduct requestProduct)
        {
            var emailAccount = _EmailAccountRepository.GetAll().FirstOrDefault();
            var fulfillerTask = _userManager.FindByIdAsync(requestProduct.FulfillerId);
            var product = _productService.GetProductById(requestProduct.ProductId);
            var productCategories = _productService.GetCategoryIdsByProductId(requestProduct.ProductId);
            List<string> backupFulfillerEmails = new List<string>();

            foreach (var pc in productCategories)
            {
                var category = _categoryService.GetCategoryById(pc.CategoryId);

                var backupFulfiller1Task = _userManager.FindByIdAsync(category.BackupFulfiller1);
                var backupFulfiller2Task = _userManager.FindByIdAsync(category.BackupFulfiller2);

                var backupFulfiller1 = await backupFulfiller1Task;
                var backupFulfiller2 = await backupFulfiller2Task;

                if (backupFulfiller1 != null)
                {
                    backupFulfillerEmails.Add(backupFulfiller1.Email);
                }
                if (backupFulfiller2 != null)
                {
                    backupFulfillerEmails.Add(backupFulfiller2.Email);
                }
            }

            var requesterTask = _userManager.FindByIdAsync(_RequestRepository.GetById(requestProduct.RequestId).CreatedById);
            var requester = await requesterTask;

            var subject = $"Request {requestProduct.RequestId} due soon!";

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
                            <p><strong>Request {requestProduct.RequestId}</strong> is due soon with the following details:</p>
                            <table>                    
                                <tr>
                                    <th>Org</th>
                                    <td>EMSB - {_userService.GetTeamById(requester.TeamId).Name}</td>
                                </tr>
                                <tr>
                                    <th>Requester</th>
                                    <td>{requester.UserName}</td>
                                </tr>
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
                                <tr>
                                    <th>End User</th>
                                    <td>{_ProjectInformationRepository.GetAll().FirstOrDefault(x => x.RequestId == requestProduct.RequestId)?.ProjectName}</td>
                                </tr>
                            </table>
                        </div>
                    </div>
                </body>
                </html>";

            if (emailAccount == null)
                return new List<EmailQueue>();

            List<EmailQueue> emailQueues = new List<EmailQueue>();
            List<ApplicationUser> ccSalesHead = await _userService.GetUserSalesHead(fulfillerTask.Result.TeamId, requester.Id);

            HashSet<string> uniqueEmails = new HashSet<string>(backupFulfillerEmails);
            uniqueEmails.Add("hanson.ong@emsb.epson.com.my");

            if (uniqueEmails.Contains(fulfillerTask.Result.Email))
            {
                uniqueEmails.Remove(fulfillerTask.Result.Email);
            }

            foreach (var user in ccSalesHead)
            {
                if (!uniqueEmails.Contains(user.Email))
                {
                    uniqueEmails.Add(user.Email);
                }
            }

            string ccEmails = string.Join(",", uniqueEmails);

            var emailQueue = new EmailQueue
            {
                FromEmail = emailAccount.Username,
                ToEmail = fulfillerTask.Result.Email,
                Subject = subject,
                Body = body,
                ScheduleTime = DateTime.UtcNow,
                SendAttempts = 0,
                SentTime = null,
                Cc = ccEmails,
                EmailAccountId = emailAccount.Id
            };

            emailQueues.Add(emailQueue);

            return emailQueues;
        }



        public EmailQueue CreateFulfillEmailQueue(Request request, RequestProduct requestProduct, bool hasFulfillmentComplete)
        {
            var emailAccount = _EmailAccountRepository.GetAll().FirstOrDefault();
            if (emailAccount == null)
                return new EmailQueue();
            var requester = _userManager.FindByIdAsync(request.CreatedById);
            var fulfiller = _userManager.FindByIdAsync(requestProduct.FulfillerId);
            var product = _productService.GetProductById(requestProduct.ProductId);

            var productCategories = _productService.GetCategoryIdsByProductId(requestProduct.ProductId);
            List<string> backupFulfillerEmails = new List<string>();

            foreach (var pc in productCategories)
            {
                var category = _categoryService.GetCategoryById(pc.CategoryId);

                var backupFulfiller1 = _userManager.FindByIdAsync(category.BackupFulfiller1);
                var backupFulfiller2 = _userManager.FindByIdAsync(category.BackupFulfiller2);

                if (backupFulfiller1 != null)
                {
                    backupFulfillerEmails.Add(backupFulfiller1.Result.Email);
                }
                if (backupFulfiller2 != null)
                {
                    backupFulfillerEmails.Add(backupFulfiller2.Result.Email);
                }
            }

            var subject = "";
            if (hasFulfillmentComplete)
                subject = $"Request {request.Id} completed fulfillment";
            else
                subject = $"Request {request.Id} partial fulfillment";

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
                                    <th>Org</th>
                                    <td>EMSB - {_userService.GetTeamById(requester.Result.TeamId).Name}</td>
                                </tr>
                                <tr>
                                    <th>Requester</th>
                                    <td>{requester.Result.UserName}</td>
                                </tr>
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
                                <tr>
                                    <th>End User</th>
                                    <td>{_ProjectInformationRepository.GetAll().Where(x => x.RequestId == requestProduct.RequestId).FirstOrDefault().ProjectName}</td>
                                </tr>
                            </table>
                        </div>
                    </div>
                </body>
                </html>";

            HashSet<string> uniqueEmails = new HashSet<string>(backupFulfillerEmails);

            string ccEmails = string.Join(" ", uniqueEmails);

            var emailQueue = new EmailQueue
            {
                FromEmail = emailAccount.Username,
                ToEmail = requester.Result.Email,
                Subject = subject,
                Body = body,
                ScheduleTime = DateTime.UtcNow,
                SendAttempts = 0,
                SentTime = null,
                Cc = ccEmails,
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

            var requester = _userManager.FindByIdAsync(request.CreatedById).Result;
            var fulfiller = _userManager.FindByIdAsync(requestProduct.FulfillerId).Result;
            var product = _productService.GetProductById(requestProduct.ProductId);

            var productCategories = _productService.GetCategoryIdsByProductId(requestProduct.ProductId);

            List<string> backupFulfillerEmails = new List<string>();

            foreach (var pc in productCategories)
            {
                var category = _categoryService.GetCategoryById(pc.CategoryId);

                var backupFulfiller1 = _userManager.FindByIdAsync(category.BackupFulfiller1).Result;
                var backupFulfiller2 = _userManager.FindByIdAsync(category.BackupFulfiller2).Result;

                if (backupFulfiller1 != null)
                {
                    backupFulfillerEmails.Add(backupFulfiller1.Email);
                }
                if (backupFulfiller2 != null)
                {
                    backupFulfillerEmails.Add(backupFulfiller2.Email);
                }
            }

            var subject = "";
            subject = $"Request {request.Id} amended by {requester.UserName} ";

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
                            <p>The request is in an amended state by {fulfiller.UserName} with the following old fulfilled details:</p>
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

            HashSet<string> uniqueEmails = new HashSet<string>(backupFulfillerEmails);

            string ccEmails = string.Join(" ", uniqueEmails);

            var emailQueue = new EmailQueue
            {
                FromEmail = emailAccount.Username,
                ToEmail = requester.Email,
                Subject = subject,
                Body = body,
                ScheduleTime = DateTime.UtcNow,
                SendAttempts = 0,
                SentTime = null,
                Cc = ccEmails,
                EmailAccountId = emailAccount.Id
            };

            return emailQueue;
        }

        public EmailQueue CreateCancellationEmailQueue(Request request, RequestProduct requestProduct)
        {
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

                if (!string.IsNullOrEmpty(emailQueue.Cc))
                {
                    var delimiters = new[] { ',', ' ' };
                    foreach (var ccEmail in emailQueue.Cc.Split(delimiters, StringSplitOptions.RemoveEmptyEntries))
                    {
                        message.CC.Add(ccEmail.Trim());
                    }
                }

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
