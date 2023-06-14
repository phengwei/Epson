﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Services.DTO.Requests
{
    public class CompetitorInformationDTO
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public string Model { get; set; }
        public string Brand { get; set; }
        public Decimal Price { get; set; }
    }
}
