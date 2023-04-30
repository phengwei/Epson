using AutoMapper;
using Epson.Core.Domain.Products;
using Epson.Core.Domain.Requests;
using Epson.Core.Domain.Users;
using Epson.Data;
using Epson.Services.DTO.Requests;
using Epson.Services.Interface.Requests;
using Serilog;

namespace Epson.Services.Services.Requests
{
    public class RequestService : IRequestService
    {
        private readonly IMapper _mapper;
        private readonly IRepository<Request> _RequestRepository;
        private readonly IRepository<RequestProduct> _RequestProductRepository;
        private readonly ILogger _logger;

        public RequestService
            (IMapper mapper,
            IRepository<Request> requestRepository,
            IRepository<RequestProduct> requestProductRepository,
            ILogger logger)
        {
            _mapper = mapper;
            _RequestRepository = requestRepository;
            _RequestProductRepository = requestProductRepository;
            _logger = logger;
        }

        public RequestDTO GetRequestById(int id)
        {
            if (id == 0 || id == null)
                return new RequestDTO();

            return _mapper.Map<RequestDTO>(_RequestRepository.GetById(id));
        }

        public List<RequestDTO> GetRequests()
        {
            var requests = _RequestRepository.GetAll();

            var requestDTOs = requests.Select(x => new RequestDTO
            {
                Id = x.Id,
                ApprovedBy = x.ApprovedBy,
                ApprovedTime = x.ApprovedTime,
                CreatedById = x.CreatedById,
                CreatedOnUTC = x.CreatedOnUTC,
                UpdatedById = x.UpdatedById,
                UpdatedOnUTC = x.UpdatedOnUTC,
                Segment = x.Segment,
                ManagerId = x.ManagerId,
                ManagerName = x.ManagerName,
                Quantity = x.Quantity,
                Priority = x.Priority,
                Deadline = x.Deadline,
                TotalPrice = x.TotalPrice,
                TimeToResolution = x.TimeToResolution
            })
            .OrderBy(x => x.CreatedOnUTC)
            .ToList();

            return requestDTOs;
        }

        public bool InsertRequest(Request request, List<RequestProduct> requestProducts)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            try
            {
                request.Id = _RequestRepository.Add(request);
                _logger.Information("Creating request {id}", request.Id);

                foreach (var requestProduct in requestProducts)
                {
                    requestProduct.RequestId = request.Id;

                    InsertRequestProduct(requestProduct);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating request {id}", request.Id);

                return false;
            }
        }

        private bool InsertRequestProduct(RequestProduct requestProduct)
        {
            if (requestProduct == null)
                throw new ArgumentNullException(nameof(requestProduct));

            try
            {
                _RequestProductRepository.Add(requestProduct);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating product request {id}", requestProduct.Id);
                return false;
            }
        }

        public bool UpdateRequest(Request request, List<RequestProduct> requestProducts)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (GetRequestById(request.Id) == null)
                throw new ArgumentNullException(nameof(request));
            try
            {
                _RequestRepository.Update(request);
                _logger.Information("Updating request {id}", request.Id);

                DeleteRequestProductOfRequest(request.Id);

                foreach (var requestProduct in requestProducts)
                {
                    requestProduct.RequestId = request.Id;

                    InsertRequestProduct(requestProduct);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating request {id}", request.Id);

                return false;
            }
        }

        private bool DeleteRequestProductOfRequest(int requestId)
        {
            if (requestId == 0 || requestId == null)
                return false;

            if (GetRequestById(requestId) == null)
                return false;

            var requestProducts = _RequestProductRepository.GetAll().Where(x => x.RequestId == requestId).ToList();
            try
            {
                foreach (var requestProduct in requestProducts)
                    _RequestProductRepository.Delete(requestProduct.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting request products of request{requestid}", requestId);

                return false;
            }
        }

        public bool ApproveRequest(ApplicationUser user, Request request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (GetRequestById(request.Id) == null)
                throw new ArgumentNullException(nameof(request));

            request.ApprovedBy = user.Id;
            request.ApprovedTime = DateTime.UtcNow;
            request.TimeToResolution = CalculateTimeToResolution(request);

            try
            {
                _RequestRepository.Update(request);
                _logger.Information("Approving request {id}", request.Id);

                return true;
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "Error approving request {requestid}", request.Id);
                return false;
            }
        }
        public TimeSpan CalculateTimeToResolution (Request request)
        {
            var timeToResolution = request.ApprovedTime - request.CreatedOnUTC;
            return timeToResolution;
        }
    }
}
