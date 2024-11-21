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
using System.Drawing.Imaging;
using System.Drawing.Printing;
using static Epson.Controllers.API.RequestApiController;
using Epson.Data;

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
        private readonly IRepository<Team> _teamRepository;
        private readonly IConfiguration _configuration;

        public ExportApiController(
            IRequestService requestService,
            IProductService productService,
            IUserService userService,
            IRequestModelFactory requestModelFactory,
            IWorkContext workContext,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            IRepository<Team> teamRepository,
            IConfiguration configuration)
        {
            _requestService = requestService;
            _productService = productService;
            _userService = userService;
            _requestModelFactory = requestModelFactory;
            _workContext = workContext;
            _mapper = mapper;
            _userManager = userManager;
            _teamRepository = teamRepository;
            _configuration = configuration;
        }

        [HttpGet("toExcel")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales,Admin,Product,Sales Section Head,Coverplus,Sales Operation,Director")]
        public async Task<IActionResult> ExportToExcel(string search = null, bool breached = false, int month = 0, int approvalState = 0)
        {
            var response = new GenericResponseModel<List<RequestDTO>>();
            var currentUser = _workContext.CurrentUser;
            var currentUserDetail = await _userManager.FindByIdAsync(_workContext.CurrentUser?.Id);

            Func<Request, bool> filter = null;
            int totalItems;

            HashSet<RequestDTO> requestSet = new HashSet<RequestDTO>(new RequestDTOComparer());

            Func<Request, bool> monthFilter = x => month == 0 || (x.CreatedOnUTC.Month == month);
            Func<Request, bool> breachedFilter = x => !breached || x.RequestProducts.Any(rp => rp.Breached);
            Func<Request, bool> approvalStateFilter = x => approvalState == 0 || x.ApprovalState == approvalState;

            if (currentUser.Roles.Contains("Admin") || currentUser.Roles.Contains("Director"))
            {
                Func<Request, bool> adminFilter = x => true;
                requestSet.UnionWith(_requestService.GetRequests(out totalItems, x => adminFilter(x) && monthFilter(x) && breachedFilter(x) && approvalStateFilter(x), search, page: null, itemsPerPage: null));
            }
            else
            {
                if (currentUser.Roles.Contains("Sales Operation"))
                {
                    Func<Request, bool> salesOperationFilter = x => x.ApprovalState == (int)ApprovalStateEnum.Approved;
                    requestSet.UnionWith(_requestService.GetRequests(out totalItems, x => salesOperationFilter(x) && monthFilter(x) && breachedFilter(x), search, page: null, itemsPerPage: null));
                }

                if (currentUser.Roles.Contains("Sales Section Head"))
                {
                    bool multiRoles = currentUser.Roles.Count > 1;

                    var teamHierarchy = _userService.InitializeTeamHierarchyPairs(true, multiRoles);

                    var relevantTeamIds = _userService.GetChildTeamIdsV2(teamHierarchy, currentUserDetail.TeamId, _teamRepository);
                    relevantTeamIds.Add(currentUserDetail.TeamId);

                    var usersInRelevantTeams = _userManager.Users
                                                            .Where(u => relevantTeamIds.Contains(u.TeamId))
                                                            .Select(u => u.Id)
                                                            .ToList();

                    Func<Request, bool> salesSectionHeadFilter = x => usersInRelevantTeams.Contains(x.CreatedById) &&
                                                                x.CreatedById != currentUser.Id;

                    requestSet.UnionWith(_requestService.GetRequests(out totalItems, x => salesSectionHeadFilter(x) && monthFilter(x) && breachedFilter(x), search, page: null, itemsPerPage: null));
                }

                if (currentUser.Roles.Contains("Product") || currentUser.Roles.Contains("Coverplus"))
                {
                    Func<Request, bool> fulfillerFilter = x => x.RequestProducts.Any(rp => rp.FulfillerId == currentUser.Id);

                    requestSet.UnionWith(_requestService.GetRequests(out totalItems, x => fulfillerFilter(x) && monthFilter(x) && breachedFilter(x), search, page: null, itemsPerPage: null));
                }

                if (currentUser.Roles.Contains("Sales"))
                {
                    Func<Request, bool> salesFilter = x => x.CreatedById == currentUser.Id;

                    requestSet.UnionWith(_requestService.GetRequests(out totalItems, x => salesFilter(x) && monthFilter(x) && breachedFilter(x), search, page: null, itemsPerPage: null));
                }
            }

            List<RequestDTO> uniqueRequests = requestSet.ToList().Distinct(new RequestDTOComparer()).ToList();

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var package = new ExcelPackage();

            await PopulateRequestWorksheet(package.Workbook.Worksheets.Add("Request"), uniqueRequests);
            await PopulateRequestProductWorksheet(package.Workbook.Worksheets.Add("Request Products"), uniqueRequests);
            //PopulateCompetitorInformationWorksheet(package.Workbook.Worksheets.Add("Competitor Informations"), request.CompetitorInformations);
            //PopulateRequestSubmissionDetailWorksheet(package.Workbook.Worksheets.Add("Request Submission Detail"), request.RequestSubmissionDetail);
            //PopulateProjectInformationWorksheet(package.Workbook.Worksheets.Add("Project Information"), request.ProjectInformation);

            var stream = new MemoryStream();
            await package.SaveAsAsync(stream);

            string fileName = "request.xlsx";
            string fileType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            stream.Position = 0;
            return File(stream, fileType, fileName);
        }

        private async Task PopulateRequestWorksheet(ExcelWorksheet ws, List<RequestDTO> requests)
        {
            setBorder(ws.Cells[1, 1, 1, 9]);
            setTitleStyle(ws.Cells[1, 1, 1, 9]);

            ws.Cells[1, 1, 1, 9].Merge = true;
            ws.Cells[1, 1].Value = "Request";
            ws.Row(3).Height = 50;

            ws.Cells[3, 1].Value = "Id";
            ws.Cells[3, 2].Value = "Created On";
            ws.Cells[3, 3].Value = "Approved Time";
            ws.Cells[3, 4].Value = "Approved By";
            ws.Cells[3, 5].Value = "Total Budget";
            ws.Cells[3, 6].Value = "Approval State";
            ws.Cells[3, 7].Value = "End User Name";
            ws.Cells[3, 8].Value = "Breached";
            ws.Cells[3, 9].Value = "Resolution Time";

            setBorder(ws.Cells[3, 1, 3, 9]);

            int rowIndex = 4;

            foreach (var request in requests)
            {
                ws.Cells[rowIndex, 1].Value = request.Id;
                ws.Cells[rowIndex, 2].Value = request.CreatedOnUTC.ToString("yyyy-MM-dd HH:mm:ss");
                ws.Cells[rowIndex, 3].Value = request.ApprovedTime.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss");
                ws.Cells[rowIndex, 4].Value = await _userManager.FindByIdAsync(request.ApprovedBy);
                ws.Cells[rowIndex, 5].Value = request.TotalBudget;
                ws.Cells[rowIndex, 6].Value = ((ApprovalStateEnum)request.ApprovalState).GetDescription();
                ws.Cells[rowIndex, 7].Value = request.ProjectInformation.ProjectName;

                if (request.ApprovalState == (int)ApprovalStateEnum.Approved)
                {
                    bool isBreached = request.RequestProducts.Any(rp => rp.Breached);
                    ws.Cells[rowIndex, 8].Value = isBreached ? "Breached" : "Not Breached";

                    var latestUpdatedOnUTC = request.RequestProducts.Max(rp => rp.UpdatedOnUTC);
                    var resolutionTime = CalculateBusinessTimeDifference(request.CreatedOnUTC.AddHours(-8), latestUpdatedOnUTC);
                    ws.Cells[rowIndex, 9].Value = $"{resolutionTime.Days}d {resolutionTime.Hours}h {resolutionTime.Minutes}m {resolutionTime.Seconds}s";
                }
                else
                {
                    ws.Cells[rowIndex, 8].Value = "N/A";
                    ws.Cells[rowIndex, 9].Value = "N/A";
                }

                setBorder(ws.Cells[rowIndex, 1, rowIndex, 9]);

                rowIndex++;
            }

            ws.Cells.AutoFitColumns(0);
        }
        private TimeSpan CalculateBusinessTimeDifference(DateTime start, DateTime end)
        {
            TimeSpan totalDuration = TimeSpan.Zero;
            DateTime current = start;

            while (current < end)
            {
                if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                {
                    var nextDay = current.AddDays(1);
                    if (nextDay > end)
                    {
                        totalDuration += end - current;
                    }
                    else
                    {
                        totalDuration += nextDay - current;
                    }
                }
                current = current.AddDays(1).Date;
            }

            return totalDuration;
        }




        private async Task PopulateRequestProductWorksheet(ExcelWorksheet ws, List<RequestDTO> requests)
        {
            setBorder(ws.Cells[1, 1, 1, 13]); 
            setTitleStyle(ws.Cells[1, 1, 1, 13]);

            ws.Cells[1, 1, 1, 13].Merge = true;
            ws.Cells[1, 1].Value = "Request Products";
            ws.Row(3).Height = 50;

            ws.Cells[3, 1].Value = "Request ID";
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
            foreach (var request in requests)
            {
                foreach (var requestProduct in request.RequestProducts)
                {
                    ws.Cells[rowStart, 1].Value = request.Id;
                    ws.Cells[rowStart, 2].Value = _productService.GetProductById(requestProduct.ProductId).Name;
                    ws.Cells[rowStart, 3].Value = requestProduct.Quantity;
                    ws.Cells[rowStart, 4].Value = requestProduct.DistyPrice;
                    ws.Cells[rowStart, 5].Value = requestProduct.DealerPrice;
                    ws.Cells[rowStart, 6].Value = requestProduct.EndUserPrice;
                    ws.Cells[rowStart, 7].Value = await _userManager.FindByIdAsync(requestProduct.FulfillerId);
                    ws.Cells[rowStart, 8].Value = requestProduct.HasFulfilled ? "Fulfilled" : "Not Fulfilled";
                    ws.Cells[rowStart, 9].Value = requestProduct.FulfilledDate.ToString("yyyy-MM-dd HH:mm:ss");
                    var resolutionTime = CalculateBusinessTimeDifference(request.CreatedOnUTC.AddHours(-8), requestProduct.UpdatedOnUTC);
                    ws.Cells[rowStart, 10].Value = $"{resolutionTime.Days}d {resolutionTime.Hours}h {resolutionTime.Minutes}m {resolutionTime.Seconds}s";
                    ws.Cells[rowStart, 11].Value = ((RequestProductStatusEnum)requestProduct.Status).GetDescription();
                    ws.Cells[rowStart, 12].Value = requestProduct.Breached ? "Breached" : "Not Breached";
                    ws.Cells[rowStart, 13].Value = requestProduct.IsCoverplus ? "Coverplus" : "Not Coverplus";
                    rowStart++;


                    setBorder(ws.Cells[rowStart, 1, rowStart, 13]);
                }
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

        public class HtmlContentModel
        {
            public string Html { get; set; }
        }
    }
}
