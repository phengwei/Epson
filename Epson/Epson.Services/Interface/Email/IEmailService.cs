using Epson.Core.Domain.Email;
using Epson.Core.Domain.Requests;
using Epson.Services.DTO.Email;
using Epson.Services.DTO.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Services.Interface.Email
{
    public interface IEmailService
    {
        public EmailAccountDTO GetEmailAccountById(int id);
        public EmailAccountDTO GetEmailAccountByUserName(string username);
        public List<EmailQueue> NotifySalesSectionHeadUsers(RequestDTO request, List<RequestProductDTO> requestProducts);
        public Task<List<EmailQueue>> CreateReminderEmailQueue(RequestProduct requestProduct);
        public Task<List<EmailQueue>> NotifyFulfillers(Request request);
        public List<EmailQueueDTO> GetUnsentEmailQueues();
        public bool InsertEmailQueue(EmailQueue emailQueue);
        public void SendEmailBatch();
        public EmailQueue CreateRequestEmailQueue(RequestDTO request, List<RequestProductDTO> requestProducts);
        public List<EmailQueue> CreateFulfillEmailQueue(Request request, List<RequestProduct> requestProduct, bool hasFulfillmentComplete);
        public List<EmailQueue> NotifySalesSectionHeadUsersOnApprovedRequest(Request request, List<RequestProduct> requestProducts);
        public List<EmailQueue> NotifySalesOperationTeamsOnApprovedRequest(Request request, List<RequestProduct> requestProducts);
        public EmailQueue CreateApprovedEmailQueue(Request request, List<RequestProduct> requestProducts);
        public EmailQueue CreateAmendQuotationEmailQueue(Request request, RequestProduct requestProduct);
        public EmailQueue CreateCancellationEmailQueue(Request request, RequestProduct requestProduct);
    }
}
