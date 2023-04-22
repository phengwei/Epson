using AutoMapper;
using Epson.Core.Domain.Products;
using Epson.Services.DTO.Products;
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
            #endregion
        }
    }
}
