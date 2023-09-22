using Epson.Infrastructure;
using Epson.Model.Common;
using Microsoft.AspNetCore.Mvc;
using Epson.Factories;
using Epson.Core.Domain.Requests;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Epson.Services.Interface.Requests;
using Epson.Model.Request;
using Microsoft.AspNetCore.Identity;
using Epson.Core.Domain.Users;
using Epson.Core.Domain.Enum;
using Epson.Services.Interface.Products;
using Epson.Core.Domain.Products;
using Epson.Services.DTO.Requests;
using System.Globalization;
using Epson.Services.DTO.Report;
using Epson.Services.Interface.Users;
using Epson.Services.DTO.Products;
using Epson.Model.Products;
using OfficeOpenXml;

namespace Epson.Controllers.API
{
    [Route("api/export")]
    public class ExportApiController : BaseApiController
    {
        private readonly IRequestService _requestService;
        private readonly IProductService _productService;
        private readonly IUserService _userService;
        private readonly IRequestModelFactory _requestModelFactory;
        private readonly IWorkContext _workContext;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public ExportApiController(
            IRequestService requestService,
            IProductService productService,
            IUserService userService,
            IRequestModelFactory requestModelFactory,
            IWorkContext workContext,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            _requestService = requestService;
            _productService = productService;
            _userService = userService;
            _requestModelFactory = requestModelFactory;
            _workContext = workContext;
            _mapper = mapper;
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpGet("toExcel")]
        public async Task<IActionResult> ExportToExcel([FromQuery] int requestId)
        {
            var request = _requestService.GetRequestById(requestId) as RequestDTO;

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var package = new ExcelPackage();

            await PopulateRequestWorksheet(package.Workbook.Worksheets.Add("Request"), request);
            await PopulateRequestProductWorksheet(package.Workbook.Worksheets.Add("Request Products"), request.RequestProducts);
            PopulateCompetitorInformationWorksheet(package.Workbook.Worksheets.Add("Competitor Informations"), request.CompetitorInformations);
            PopulateRequestSubmissionDetailWorksheet(package.Workbook.Worksheets.Add("Request Submission Detail"), request.RequestSubmissionDetail);
            PopulateProjectInformationWorksheet(package.Workbook.Worksheets.Add("Project Information"), request.ProjectInformation);

            var stream = new MemoryStream();
            await package.SaveAsAsync(stream);

            string fileName = "request.xlsx";
            string fileType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            stream.Position = 0;
            return File(stream, fileType, fileName);
        }

        private async Task PopulateRequestWorksheet(ExcelWorksheet ws, RequestDTO request)
        {
            setBorder(ws.Cells[1, 1, 1, 9]);
            setTitleStyle(ws.Cells[1, 1, 1, 9]);

            ws.Cells[1, 1, 1, 9].Merge = true;
            ws.Cells[1, 1].Value = $"Request Data for Request ID {request.Id}";
            ws.Row(3).Height = 50;

            ws.Cells[3, 1].Value = "Id";
            ws.Cells[3, 2].Value = "Approved Time";
            ws.Cells[3, 3].Value = "Approved By";
            ws.Cells[3, 4].Value = "Total Budget";
            ws.Cells[3, 5].Value = "Approval State";
            ws.Cells[3, 6].Value = "Time To Resolution";
            ws.Cells[3, 7].Value = "Breached";
            ws.Cells[3, 8].Value = "Created On";
            ws.Cells[3, 9].Value = "Comments";

            setBorder(ws.Cells[3, 1, 3, 9]); 

            ws.Cells[4, 1].Value = request.Id;
            ws.Cells[4, 2].Value = request.ApprovedTime.ToString("yyyy-MM-dd HH:mm:ss");
            ws.Cells[4, 3].Value = request.ApprovedBy;
            ws.Cells[4, 3].Value = await _userManager.FindByIdAsync(request.ApprovedBy);
            ws.Cells[4, 4].Value = request.TotalBudget;
            ws.Cells[4, 5].Value = ((ApprovalStateEnum)request.ApprovalState).GetDescription();
            ws.Cells[4, 6].Value = $"{request.TimeToResolution.Hours}h {request.TimeToResolution.Minutes}m {request.TimeToResolution.Seconds}s";
            ws.Cells[4, 7].Value = request.Breached ? "Breached" : "Not Breached";
            ws.Cells[4, 8].Value = request.CreatedOnUTC.ToString("yyyy-MM-dd HH:mm:ss"); ;
            ws.Cells[4, 9].Value = request.Comments;

            ws.Cells.AutoFitColumns(0);
        }


        private async Task PopulateRequestProductWorksheet(ExcelWorksheet ws, List<RequestProductDTO> requestProducts)
        {
            setBorder(ws.Cells[1, 1, 1, 13]); 
            setTitleStyle(ws.Cells[1, 1, 1, 13]);

            ws.Cells[1, 1, 1, 13].Merge = true;
            ws.Cells[1, 1].Value = "Request Products Data";
            ws.Row(3).Height = 50;

            ws.Cells[3, 1].Value = "Id";
            ws.Cells[3, 2].Value = "Product";
            ws.Cells[3, 3].Value = "Quantity";
            ws.Cells[3, 4].Value = "Disty Price";
            ws.Cells[3, 5].Value = "Dealer Price";
            ws.Cells[3, 6].Value = "End User Price";
            ws.Cells[3, 7].Value = "Fulfiller";
            ws.Cells[3, 8].Value = "Has Fulfilled";
            ws.Cells[3, 9].Value = "Fulfilled Date";
            ws.Cells[3, 10].Value = "Time to Resolution";
            ws.Cells[3, 11].Value = "Status";
            ws.Cells[3, 12].Value = "Has Breached";
            ws.Cells[3, 13].Value = "Is Coverplus";

            setBorder(ws.Cells[3, 1, 3, 13]);

            int rowStart = 4;
            foreach (var requestProduct in requestProducts)
            {
                ws.Cells[rowStart, 1].Value = requestProduct.Id;
                ws.Cells[rowStart, 2].Value = _productService.GetProductById(requestProduct.ProductId).Name;
                ws.Cells[rowStart, 3].Value = requestProduct.Quantity;
                ws.Cells[rowStart, 4].Value = requestProduct.DistyPrice;
                ws.Cells[rowStart, 5].Value = requestProduct.DealerPrice;
                ws.Cells[rowStart, 6].Value = requestProduct.EndUserPrice;
                ws.Cells[rowStart, 7].Value = await _userManager.FindByIdAsync(requestProduct.FulfillerId);
                ws.Cells[rowStart, 8].Value = requestProduct.HasFulfilled? "Fulfilled" : "Not Fulfilled";
                ws.Cells[rowStart, 9].Value = requestProduct.FulfilledDate.ToString("yyyy-MM-dd HH:mm:ss");
                ws.Cells[rowStart, 10].Value = $"{requestProduct.TimeToResolution.Hours}h {requestProduct.TimeToResolution.Minutes}m {requestProduct.TimeToResolution.Seconds}s";
                ws.Cells[rowStart, 11].Value = requestProduct.Status;
                ws.Cells[rowStart, 12].Value = requestProduct.Breached ? "Breached" : "Not Breached";
                ws.Cells[rowStart, 13].Value = requestProduct.IsCoverplus ? "Coverplus": "Not Coverplus";
                rowStart++;
            }

            ws.Cells.AutoFitColumns(0);
        }

        private void PopulateCompetitorInformationWorksheet(ExcelWorksheet ws, List<CompetitorInformation> competitorInfos)
        {
            setBorder(ws.Cells[1, 1, 1, 6]); 
            setTitleStyle(ws.Cells[1, 1, 1, 6]);

            ws.Cells[1, 1, 1, 6].Merge = true;
            ws.Cells[1, 1].Value = "Competitor Information Data";
            ws.Row(3).Height = 50;

            ws.Cells[3, 1].Value = "Id";
            ws.Cells[3, 2].Value = "Model";
            ws.Cells[3, 3].Value = "Brand";
            ws.Cells[3, 4].Value = "Disty Price";
            ws.Cells[3, 5].Value = "Dealer Price";
            ws.Cells[3, 6].Value = "End User Price";

            setBorder(ws.Cells[3, 1, 3, 6]);

            int rowStart = 4;
            foreach (var competitor in competitorInfos)
            {
                ws.Cells[rowStart, 1].Value = competitor.Id;
                ws.Cells[rowStart, 2].Value = competitor.Model;
                ws.Cells[rowStart, 3].Value = competitor.Brand;
                ws.Cells[rowStart, 4].Value = competitor.DistyPrice;
                ws.Cells[rowStart, 5].Value = competitor.DealerPrice;
                ws.Cells[rowStart, 6].Value = competitor.EndUserPrice;
                rowStart++;
            }

            ws.Cells.AutoFitColumns(0);
        }


        private void PopulateRequestSubmissionDetailWorksheet(ExcelWorksheet ws, RequestSubmissionDetail submissionDetail)
        {
            setBorder(ws.Cells[1, 1, 1, 6]); 
            setTitleStyle(ws.Cells[1, 1, 1, 6]);

            ws.Cells[1, 1, 1, 6].Merge = true;
            ws.Cells[1, 1].Value = "Request Submission Detail Data";
            ws.Row(3).Height = 50;

            ws.Cells[3, 1].Value = "Distributor Name";
            ws.Cells[3, 2].Value = "Reseller Name";
            ws.Cells[3, 3].Value = "Contact Person Name";
            ws.Cells[3, 4].Value = "Telephone No";
            ws.Cells[3, 5].Value = "Fax No";
            ws.Cells[3, 6].Value = "Email";

            setBorder(ws.Cells[3, 1, 3, 6]);

            ws.Cells[4, 1].Value = submissionDetail.DistributorName;
            ws.Cells[4, 2].Value = submissionDetail.ResellerName;
            ws.Cells[4, 3].Value = submissionDetail.ContactPersonName;
            ws.Cells[4, 4].Value = submissionDetail.TelephoneNo;
            ws.Cells[4, 5].Value = submissionDetail.FaxNo;
            ws.Cells[4, 6].Value = submissionDetail.Email;

            ws.Cells.AutoFitColumns(0);
        }

        private async Task PopulateProjectInformationWorksheet(ExcelWorksheet ws, ProjectInformationDTO projectInfo)
        {
            setBorder(ws.Cells[1, 1, 1, 15]);
            setTitleStyle(ws.Cells[1, 1, 1, 15]);

            ws.Cells[1, 1, 1, 15].Merge = true;
            ws.Cells[1, 1].Value = "Project Information Data";
            ws.Row(3).Height = 50;

            ws.Cells[3, 1].Value = "Project Name";
            ws.Cells[3, 2].Value = "Industry";
            ws.Cells[3, 3].Value = "Type";
            ws.Cells[3, 4].Value = "Closing Date";
            ws.Cells[3, 5].Value = "Delivery Date";
            ws.Cells[3, 6].Value = "Company Address";
            ws.Cells[3, 7].Value = "Contact Person";
            ws.Cells[3, 8].Value = "Telephone No";
            ws.Cells[3, 9].Value = "Email";
            ws.Cells[3, 10].Value = "Requirements";
            ws.Cells[3, 11].Value = "Customer Applications";
            ws.Cells[3, 12].Value = "Budget";
            ws.Cells[3, 13].Value = "Staggered Comments";
            ws.Cells[3, 14].Value = "Staggered Months";
            ws.Cells[3, 15].Value = "Other Information";

            setBorder(ws.Cells[3, 1, 3, 15]);

            ws.Cells[4, 1].Value = projectInfo.ProjectName;
            ws.Cells[4, 2].Value = projectInfo.Industry;
            ws.Cells[4, 3].Value = projectInfo.Type;
            ws.Cells[4, 4].Value = projectInfo.ClosingDate.ToString("yyyy-MM-dd HH:mm:ss");
            ws.Cells[4, 5].Value = projectInfo.DeliveryDate.ToString("yyyy-MM-dd HH:mm:ss");
            ws.Cells[4, 6].Value = projectInfo.CompanyAddress;
            ws.Cells[4, 7].Value = projectInfo.ContactPersonName;
            ws.Cells[4, 8].Value = projectInfo.TelephoneNo;
            ws.Cells[4, 9].Value = projectInfo.Email;
            ws.Cells[4, 10].Value = projectInfo.Requirements;
            ws.Cells[4, 11].Value = projectInfo.CustomerApplications;
            ws.Cells[4, 12].Value = projectInfo.Budget;
            ws.Cells[4, 13].Value = projectInfo.StaggeredComments;
            ws.Cells[4, 14].Value = projectInfo.StaggeredMonth;
            ws.Cells[4, 15].Value = projectInfo.OtherInformation;

            ws.Cells.AutoFitColumns(0);
        }


        private void setTitleStyle(ExcelRange cell)
        {
            cell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            cell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            cell.Style.Font.SetFromFont("Arial", 14);
            cell.Style.Font.Bold = true;
        }

        private void setHeaderStyle(ExcelRange cell)
        {
            cell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            cell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            cell.Style.Font.SetFromFont("Arial", 10);
            cell.Style.Font.Bold = true;
        }

        private void setStandardStyle(ExcelRange cell)
        {
            cell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            cell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            cell.Style.Font.SetFromFont("Arial", 10);
        }

        private void setYHeaderStyle(ExcelRange cell)
        {
            cell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            cell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            cell.Style.Font.SetFromFont("Arial", 10);
            cell.Style.Font.Bold = true;
        }

        private void setBorder(ExcelRange cell)
        {
            cell.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            cell.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            cell.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            cell.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        }
    }
}
