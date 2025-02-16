using Epson.Services.DTO.Requests;
using Epson.Core.Domain.Requests;
using Epson.Core.Domain.Users;
using Epson.Services.DTO.SLA;
using Epson.Core.Domain.Products;
using Epson.Services.DTO.Report;
using Epson.Data;
using Epson.Core.Domain.Enum;

namespace Epson.Services.Interface.Requests
{
    public interface IServiceRequestService
    {
        public ServiceRequestDTO GetRequestById(int id);
        bool InsertServiceRequest(ServiceRequestDTO serviceRequestDTO);
        List<ServiceRequestDTO> GetServiceRequests(string search = null, Func<ServiceRequest, bool> filter = null, int? page = null, int? itemsPerPage = null);
        List<ServiceRequestDTO> GetServiceRequests(out int totalItems, Func<ServiceRequest, bool> filter = null, string search = null, int? page = null, int? itemsPerPage = null);
        bool AssignServiceRequestMaker(int requestId, ApplicationUser user);
        bool MakerServiceRequest(ServiceRequestDTO serviceRequestDTO);
        bool CheckerServiceRequest(ServiceRequestDTO serviceRequestDTO);
        decimal GetAverageTimeToResolutionInHours(int month);
        int GetTotalTicketCount(int month);
        int GetBreachedTicketCount(int month);
        decimal GetSuccessRateOfTickets(int month);
        Task<List<RequesterSales>> GetMonthlySalesByRequesterByDonut(DateTime fromMonth, DateTime toMonth, bool allRequester = false);
        Task<List<RequesterSales>> GetMonthlySalesByRequester(string requesterId, int month = 0, bool allRequester = false);
    }
}
