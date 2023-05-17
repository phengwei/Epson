using Epson.Core.Domain.Email;
using Epson.Core.Domain.Requests;
using Epson.Services.DTO.Email;
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
        public List<EmailQueueDTO> GetUnsentEmailQueues();
        public bool InsertEmailQueue(EmailQueue emailQueue);
        public void SendEmailBatch();
        public EmailQueue CreateRequestEmailQueue(Request request, List<RequestProduct> requestProducts);
        public EmailQueue CreateFulfillEmailQueue(Request request, RequestProduct requestProduct, bool hasFulfillmentComplete);
        public EmailQueue CreateAmendQuotationEmailQueue(Request request, RequestProduct requestProduct);
    }
}
