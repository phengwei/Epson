using Epson.Core.Domain.Products;
using Epson.Model.Products;
using Epson.Model.Request;
using Epson.Services.DTO.Products;
using Epson.Services.DTO.Requests;

namespace Epson.Factories
{
    public interface IRequestModelFactory
    {
        public RequestModel PrepareRequestModel(RequestDTO request);
        public List<RequestModel> PrepareRequestModels(List<RequestDTO> requests);
    }
}
