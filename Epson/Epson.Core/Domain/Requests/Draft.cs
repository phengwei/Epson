using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Core.Domain.Requests
{
    public class Draft
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedOnUTC { get; set; }
        public DateTime UpdatedOnUTC { get; set; }
        public string? SelectedCategories { get; set; } 
        public string? ProductsToShow { get; set; }    
        public string? CompetitorsToShow { get; set; }   
        public string? CoverplusesToShow { get; set; } 
        public string? SubmissionDetail { get; set; }   
        public string? ProjectInformation { get; set; }  
        public string? Reasons { get; set; }             
        public string? Priority { get; set; }           
        public string? Comments { get; set; }
        public string? CustomerName { get; set; }
        public string? DealJustification { get; set; }
        public DateTime? Deadline { get; set; }
        public string? SLA { get; set; }
    }

}
