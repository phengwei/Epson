using Epson.Services.DTO.Requests;
using Epson.Core.Domain.Requests;
using Epson.Core.Domain.Users;
using Epson.Services.DTO.SLA;

namespace Epson.Services.Interface.Requests
{
    public interface IRequestService
    {
        public RequestDTO GetRequestById(int id);
        public List<RequestDTO> GetRequests();
        public bool InsertRequest(Request request, List<RequestProduct> requestProducts);
        public bool UpdateRequest(Request request, List<RequestProduct> requestProducts);
        public bool ApproveRequest(ApplicationUser user, Request request);
        public TimeSpan CalculateResolutionTime(DateTime approvedTime, DateTime ticketCreateTime, List<SLAStaffLeaveDTO> staffLeaves, List<SLAHolidayDTO> holidays);
    }
}
