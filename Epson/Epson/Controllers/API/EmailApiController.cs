using AutoMapper;
using Epson.Core.Domain.Email;
using Epson.Core.Domain.Products;
using Epson.Factories;
using Epson.Infrastructure;
using Epson.Model.Common;
using Epson.Model.Email;
using Epson.Model.Products;
using Epson.Services.Interface.Email;
using Epson.Services.Interface.Products;
using Epson.Services.Services.Products;
using Microsoft.AspNetCore.Mvc;

namespace Epson.Controllers.API
{
    [Route("api/email")]
    public class EmailApiController : BaseApiController
    {
        private readonly IEmailService _emailService;
        private readonly IWorkContext _workContext;
        private readonly IMapper _mapper;

        public EmailApiController(
            IEmailService emailService,
            IWorkContext workContext,
            IMapper mapper)
        {
            _emailService = emailService;
            _workContext = workContext;
            _mapper = mapper;
        }
        [HttpPost("insertemailqueue")]
        public async Task<IActionResult> InsertEmailQueue([FromBody] BaseQueryModel<EmailQueueModel> queryModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var model = queryModel.Data;

            var emailAccount = _emailService.GetEmailAccountByUserName(model.EmailAccountUserName);

            var emailQueue = new EmailQueue
            {
                Priority = model.Priority,
                FromEmail = emailAccount.Username,
                ToEmail = model.ToEmail,
                Cc = model.Cc,
                Bcc = model.Bcc,
                Subject = model.Subject,
                Body = model.Body,
                AttachmentName = model.AttachmentName,
                ScheduleTime = model.ScheduleTime,
                SendAttempts = 0,
                SentTime = null,
                EmailAccountId = emailAccount.Id
            };

            if (_emailService.InsertEmailQueue(emailQueue))
                return Ok();
            else
                return BadRequest("Failed to insert product");
        }
    }
}