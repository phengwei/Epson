using Epson.Services.DTO.Requests;
using Epson.Core.Domain.Requests;
using Epson.Core.Domain.Users;
using Epson.Services.DTO.SLA;
using Epson.Core.Domain.Products;
using Epson.Services.DTO.Report;

namespace Epson.Services.Interface.Requests
{
    public interface IRequestService
    {
        public RequestDTO GetRequestById(int id);
        public List<RequestDTO> GetRequests();
        public List<RequestDTO> GetUnfulfilledRequests(bool isCoverplus);
        public List<RequestProductDTO> GetRequestProducts();
        public bool InsertRequest(Request request, List<RequestProduct> requestProducts, List<CompetitorInformation> competitorInformations, RequestSubmissionDetail requestSubmissionDetail);
        public bool UpdateRequest(Request request, List<RequestProduct> requestProducts, List<CompetitorInformation> competitorInformations);
        public bool ApproveRequest(ApplicationUser user, Request request, string comments);
        public bool SetRequestToAmendQuotation(Request request);
        public bool FulfillRequest(ApplicationUser user, Request request, Product product, decimal totalPrice, DateTime deliveryDate, string remarks);
        public List<FulfillmentSummary> GetFulfillmentSummary(DateTime startDate, DateTime endDate, string granularity, string userId);
        public List<SalesSummary> GetRequestSummary(DateTime startDate, DateTime endDate, string granularity, string userId);
        public TimeSpan CalculateResolutionTime(DateTime approvedTime, DateTime ticketCreateTime, List<SLAStaffLeaveDTO> staffLeaves, List<SLAHolidayDTO> holidays);
        public bool CancelRequest(Request request, string remarks);
        public bool RejectRequest(ApplicationUser user, RequestProduct requestProduct, string remarks);
    }
}
