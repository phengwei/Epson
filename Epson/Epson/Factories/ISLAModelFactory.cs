using Epson.Core.Domain.SLA;
using Epson.Model.Products;
using Epson.Model.SLA;
using Epson.Services.DTO.SLA;

namespace Epson.Factories
{
    public interface ISLAModelFactory
    {
        public SLAHolidayModel PrepareSLAHolidayModel(SLAHolidayDTO slaHoliday);
        public SLAStaffLeaveModel PrepareSLAStaffLeaveModel(SLAStaffLeaveDTO slaStaffLeave);
        public List<SLAHolidayModel> PrepareSLAHolidayModels(List<SLAHolidayDTO> slaHolidays);
        public List<SLAStaffLeaveModel> PrepareSLAStaffLeaveModels(List<SLAStaffLeaveDTO> slaStaffLeave);
    }
}
