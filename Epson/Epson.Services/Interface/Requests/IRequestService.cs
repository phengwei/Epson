using Epson.Services.DTO.Requests;
using Epson.Core.Domain.Requests;
using Epson.Core.Domain.Users;
using Epson.Services.DTO.SLA;
using Epson.Core.Domain.Products;
using Epson.Services.DTO.Report;
using Epson.Data;

namespace Epson.Services.Interface.Requests
{
    public interface IRequestService
    {
        public RequestDTO GetRequestById(int id);
        List<RequestDTO> GetRequests(string search = null, Func<Request, bool> filter = null, int? page = null, int? itemsPerPage = null);
        List<RequestDTO> GetRequests(out int totalItems, Func<Request, bool> filter = null, string search = null, int? page = null, int? itemsPerPage = null);
        Task<List<RequestDTO>> GetRequestsByIdsAsync(List<int> requestIds);
        RequestDTO GetUnfulfilledRequestProducts(RequestDTO request, ApplicationUser user, bool isCoverplusUser, bool isProductUser, bool isAdminUser);
        PagedResult<RequestDTO> GetUnfulfilledRequests(ApplicationUser user, bool isCoverplusUser, bool isProductUser, bool isAdminUser, string search = null, int? page = null, int? itemsPerPage = null);
        public List<RequestProductDTO> GetRequestProducts(Func<RequestProduct, bool> filter = null, int? page = null, int? itemsPerPage = null);

        public List<RequestProductDTO> GetRequestProducts(out int totalCount, Func<RequestProduct, bool> filter = null, int? page = null, int? itemsPerPage = null);
        public bool InsertRequest(RequestDTO request, List<RequestProductDTO> requestProducts, List<CompetitorInformationDTO> competitorInformations, RequestSubmissionDetailDTO requestSubmissionDetail, ProjectInformationDTO projectInformationDTO);
        public bool UpdateRequest(RequestDTO request, List<RequestProductDTO> requestProducts, List<CompetitorInformationDTO> competitorInformations, RequestSubmissionDetailDTO requestSubmissionDetail, ProjectInformationDTO projectInformationDTO);
        public bool AcceptDeal(ApplicationUser user, Request request, string comments);
        public bool RejectDeal(ApplicationUser user, Request request, string comments);
        public bool ExitDeal(ApplicationUser user, Request request, string comments);
        public bool SetRequestToAmendQuotation(Request request);
        public Task<bool> ApproveFirstLevelRequest(string userId, Request request);
        public bool RejectFirstLevelRequest(Request request);
        public bool ApproveFinalLevelRequest(Request request, bool isApprove);
        public bool FulfillRequest(ApplicationUser user, RequestProduct requestProduct, Product product, decimal totalPrice, string remarks);
        bool FulfillRequests(List<int> requestProductIds, ApplicationUser user);
        public List<FulfillmentSummary> GetFulfillmentSummary(DateTime startDate, DateTime endDate, string granularity, string userId);
        public List<SalesSummary> GetRequestSummary(DateTime startDate, DateTime endDate, string granularity, string userId);
        public List<NoOfRequestSummary> GetTotalRequestSummary(DateTime startDate, DateTime endDate, string granularity, string userId);
        public List<NoOfPendingRequestSummary> GetTotalPendingRequestSummary(DateTime startDate, DateTime endDate, string granularity, string userId);
        public List<NoOfCompletedRequestSummary> GetTotalCompletedRequestSummary(DateTime startDate, DateTime endDate, string granularity, string userId);
        public TimeSpan CalculateResolutionTime(DateTime approvedTime, DateTime ticketCreateTime, List<SLAStaffLeaveDTO> staffLeaves, List<SLAHolidayDTO> holidays);
        public bool CancelRequest(Request request, string remarks);
        public bool RejectRequest(ApplicationUser user, RequestProduct requestProduct, string remarks);
    }
}
