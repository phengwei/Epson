using Epson.Core.Domain.Products;
using Epson.Model.Products;
using Epson.Model.Request;
using Epson.Services.DTO.Products;
using Epson.Services.DTO.Requests;

namespace Epson.Factories
{
    public interface IRequestModelFactory
    {
        RequestModel PrepareRequestModel(RequestDTO request);
        Task<List<RequestProductModel>> PrepareRequestProductModelAsync(List<RequestProductDTO> requestProducts);
    }
}
