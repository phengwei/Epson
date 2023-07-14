using AutoMapper;
using Epson.Core.Domain.Categories;
using Epson.Core.Domain.Email;
using Epson.Core.Domain.Products;
using Epson.Core.Domain.Requests;
using Epson.Core.Domain.SLA;
using Epson.Core.Domain.Users;
using Epson.Services.DTO.Categories;
using Epson.Services.DTO.Email;
using Epson.Services.DTO.Products;
using Epson.Services.DTO.Requests;
using Epson.Services.DTO.SLA;
using Epson.Services.DTO.Users;
using Epson.Services.Interface.SLA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Services.DTO
{
    public class AutoMappingProfile : Profile
    {
        public AutoMappingProfile() 
        {
            #region Product
            CreateMap<ProductDTO, Product>();
            CreateMap<Product, ProductDTO>();
            CreateMap<Category, CategoryDTO>();
            CreateMap<CategoryDTO, Category>();
            #endregion

            #region Email
            CreateMap<EmailAccountDTO, EmailAccount>();
            CreateMap<EmailAccount, EmailAccountDTO>();
            CreateMap<EmailQueueDTO, EmailQueue>();
            CreateMap<EmailQueue, EmailQueueDTO>();
            #endregion

            #region Request
            CreateMap<RequestDTO, Request>();
            CreateMap<Request, RequestDTO>();
            CreateMap<RequestProductDTO, RequestProduct>();
            CreateMap<RequestProduct, RequestProductDTO>();
            CreateMap<RequestSubmissionDetail, RequestSubmissionDetailDTO>();
            CreateMap<RequestSubmissionDetailDTO, RequestSubmissionDetail>();
            #endregion

            #region SLA
            CreateMap<SLAHoliday, SLAHolidayDTO>();
            CreateMap<SLAHolidayDTO, SLAHoliday>();
            CreateMap<SLAStaffLeave, SLAStaffLeaveDTO>();
            CreateMap<SLAStaffLeaveDTO, SLAStaffLeave>();
            CreateMap<SLASettingDTO, SLASetting>();
            CreateMap<SLASetting, SLASettingDTO>();
            #endregion

            #region User
            CreateMap<TeamDTO, Team>();
            CreateMap<Team, TeamDTO>();
            #endregion
        }
    }
}
