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
        public bool InsertRequest(Request request, List<RequestProduct> requestProducts);
        public bool UpdateRequest(Request request, List<RequestProduct> requestProducts);
        public bool ApproveRequest(ApplicationUser user, Request request);
        public bool SetRequestToAmendQuotation(Request request);
        public bool FulfillRequest(ApplicationUser user, Request request, Product product, decimal totalPrice);
        public List<FulfillmentSummary> GetFulfillmentSummary(DateTime startDate, DateTime endDate, string granularity);
        public List<SalesSummary> GetRequestSummary(DateTime startDate, DateTime endDate, string granularity);
        public TimeSpan CalculateResolutionTime(DateTime approvedTime, DateTime ticketCreateTime, List<SLAStaffLeaveDTO> staffLeaves, List<SLAHolidayDTO> holidays);
    }
}
