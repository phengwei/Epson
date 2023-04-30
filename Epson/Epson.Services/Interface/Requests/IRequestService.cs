using Epson.Services.DTO.Requests;
using Epson.Core.Domain.Requests;

namespace Epson.Services.Interface.Requests
{
    public interface IRequestService
    {
        public RequestDTO GetRequestById(int id);

        public List<RequestDTO> GetRequests();

        public bool InsertRequest(Request request, List<RequestProduct> requestProducts);

        public bool UpdateRequest(Request request, List<RequestProduct> requestProducts);
    }
}
