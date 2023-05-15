using AutoMapper;
using Epson.Core.Domain.Requests;
using Epson.Data;
using Epson.Model.Request;
using Epson.Services.DTO.Products;
using Epson.Services.DTO.Requests;

namespace Epson.Factories
{
    public class RequestModelFactory : IRequestModelFactory
    {
        private readonly IMapper _mapper;

        public RequestModelFactory
            (IMapper mapper)
        {
            _mapper = mapper;
        }
        public RequestModel PrepareRequestModel(RequestDTO request)
        {
            if (request != null)
            {
                var requestModel = new RequestModel();

                requestModel.Id = request.Id;
                requestModel.ApprovedBy = request.ApprovedBy;
                requestModel.ApprovedTime = request.ApprovedTime;
                requestModel.CreatedById = request.CreatedById;
                requestModel.CreatedOnUTC = request.CreatedOnUTC;
                requestModel.UpdatedById = request.UpdatedById;
                requestModel.UpdatedOnUTC = request.UpdatedOnUTC;
                requestModel.Segment = request.Segment;
                requestModel.TotalBudget = request.TotalBudget;
                requestModel.ApprovalState = request.ApprovalState;
                requestModel.Priority = request.Priority;
                requestModel.Deadline = request.Deadline;
                requestModel.TotalPrice = request.TotalPrice;
                requestModel.TimeToResolution = request.TimeToResolution;
                requestModel.RequestProducts = request.RequestProducts;

                return requestModel;
            }

            return new RequestModel();
        }

        public List<RequestModel> PrepareRequestModels(List<RequestDTO> requests)
        {
            if (requests?.Count == 0 || requests == null)
                return new List<RequestModel>();

            List<RequestModel> requestModels = new List<RequestModel>();
            foreach (var request in requests)
            {
                var requestModel = new RequestModel
                {
                    Id = request.Id,
                    ApprovedBy = request.ApprovedBy,
                    ApprovedTime = request.ApprovedTime,
                    CreatedById = request.CreatedById,
                    CreatedOnUTC = request.CreatedOnUTC,
                    UpdatedById = request.UpdatedById,
                    UpdatedOnUTC = request.UpdatedOnUTC,
                    Segment = request.Segment,
                    TotalBudget = request.TotalBudget,
                    ApprovalState = request.ApprovalState,
                    Priority = request.Priority,
                    Deadline = request.Deadline,
                    TotalPrice = request.TotalPrice,
                    TimeToResolution = request.TimeToResolution,
                    RequestProducts = request.RequestProducts,
                };
                requestModels.Add(requestModel);
            }

            return requestModels;
        }
    }
}
