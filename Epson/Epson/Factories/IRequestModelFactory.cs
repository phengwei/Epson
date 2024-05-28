using Epson.Core.Domain.Products;
using Epson.Model.Products;
using Epson.Model.Request;
using Epson.Services.DTO.Products;
using Epson.Services.DTO.Requests;

namespace Epson.Factories
{
    public interface IRequestModelFactory
    {
        Task<List<RequestModel>> PrepareRequestModelsAsync(List<RequestDTO> requests);
        public List<RequestProductModel> PrepareRequestProductModel(List<RequestProductDTO> requestProducts);
    }
}
