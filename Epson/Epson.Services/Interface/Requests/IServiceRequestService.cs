using Epson.Services.DTO.Requests;
using Epson.Core.Domain.Requests;
using Epson.Core.Domain.Users;
using Epson.Services.DTO.SLA;
using Epson.Core.Domain.Products;
using Epson.Services.DTO.Report;
using Epson.Data;

namespace Epson.Services.Interface.Requests
{
    public interface IServiceRequestService
    {
        public ServiceRequestDTO GetRequestById(int id);
        bool InsertServiceRequest(ServiceRequestDTO serviceRequestDTO);
        List<ServiceRequestDTO> GetServiceRequests(string search = null, Func<ServiceRequest, bool> filter = null, int? page = null, int? itemsPerPage = null);
        List<ServiceRequestDTO> GetServiceRequests(out int totalItems, Func<ServiceRequest, bool> filter = null, string search = null, int? page = null, int? itemsPerPage = null);
    }
}
