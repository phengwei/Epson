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

            PopulateRequestWorksheet(package.Workbook.Worksheets.Add("Request"), request);
            PopulateRequestProductWorksheet(package.Workbook.Worksheets.Add("Request Products"), request.RequestProducts);
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

        private void PopulateRequestWorksheet(ExcelWorksheet ws, RequestDTO request)
        {
            setBorder(ws.Cells[1, 1, 1, 10]);
            setTitleStyle(ws.Cells[1, 1, 1, 10]);

            ws.Cells[1, 1, 1, 10].Merge = true;
            ws.Cells[1, 1].Value = $"Request Data for Request ID {request.Id}";
            ws.Row(3).Height = 50;

            ws.Cells[3, 1].Value = "Id";
            ws.Cells[3, 2].Value = "ApprovedTime";

            setBorder(ws.Cells[3, 1, 3, 10]); 

            ws.Cells[4, 1].Value = request.Id;
            ws.Cells[4, 2].Value = request.ApprovedTime.ToString("yyyy-MM-dd HH:mm:ss");

            ws.Cells.AutoFitColumns(0);
        }


        private void PopulateRequestProductWorksheet(ExcelWorksheet ws, List<RequestProductDTO> requestProducts)
        {
            setBorder(ws.Cells[1, 1, 1, 5]); 
            setTitleStyle(ws.Cells[1, 1, 1, 5]);

            ws.Cells[1, 1, 1, 5].Merge = true;
            ws.Cells[1, 1].Value = "Request Products Data";
            ws.Row(3).Height = 50;

            ws.Cells[3, 1].Value = "Id";
            ws.Cells[3, 2].Value = "RequestId";

            setBorder(ws.Cells[3, 1, 3, 5]);

            int rowStart = 4;
            foreach (var product in requestProducts)
            {
                ws.Cells[rowStart, 1].Value = product.Id;
                ws.Cells[rowStart, 2].Value = product.RequestId;
                rowStart++;
            }

            ws.Cells.AutoFitColumns(0);
        }

        private void PopulateCompetitorInformationWorksheet(ExcelWorksheet ws, List<CompetitorInformation> competitorInfos)
        {
            setBorder(ws.Cells[1, 1, 1, 5]); 
            setTitleStyle(ws.Cells[1, 1, 1, 5]);

            ws.Cells[1, 1, 1, 5].Merge = true;
            ws.Cells[1, 1].Value = "Competitor Information Data";
            ws.Row(3).Height = 50;

            ws.Cells[3, 1].Value = "Id";
            ws.Cells[3, 2].Value = "RequestId";

            setBorder(ws.Cells[3, 1, 3, 5]);

            int rowStart = 4;
            foreach (var competitor in competitorInfos)
            {
                ws.Cells[rowStart, 1].Value = competitor.Id;
                ws.Cells[rowStart, 2].Value = competitor.RequestId;
                rowStart++;
            }

            ws.Cells.AutoFitColumns(0);
        }


        private void PopulateRequestSubmissionDetailWorksheet(ExcelWorksheet ws, RequestSubmissionDetail submissionDetail)
        {
            setBorder(ws.Cells[1, 1, 1, 5]); 
            setTitleStyle(ws.Cells[1, 1, 1, 5]);

            ws.Cells[1, 1, 1, 5].Merge = true;
            ws.Cells[1, 1].Value = "Request Submission Detail Data";
            ws.Row(3).Height = 50;

            ws.Cells[3, 1].Value = "Id";

            setBorder(ws.Cells[3, 1, 3, 5]);

            ws.Cells[4, 1].Value = submissionDetail.Id;

            ws.Cells.AutoFitColumns(0);
        }

        private void PopulateProjectInformationWorksheet(ExcelWorksheet ws, ProjectInformationDTO projectInfo)
        {
            setBorder(ws.Cells[1, 1, 1, 5]);
            setTitleStyle(ws.Cells[1, 1, 1, 5]);

            ws.Cells[1, 1, 1, 5].Merge = true;
            ws.Cells[1, 1].Value = "Project Information Data";
            ws.Row(3).Height = 50;

            ws.Cells[3, 1].Value = "Id";

            setBorder(ws.Cells[3, 1, 3, 5]);

            ws.Cells[4, 1].Value = projectInfo.Id;

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
