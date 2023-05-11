using AutoMapper;
using Epson.Core.Domain.SLA;
using Epson.Data;
using Epson.Model.SLA;
using Epson.Model.SLA;
using Epson.Services.DTO.SLA;

namespace Epson.Factories
{
    public class SLAModelFactory : ISLAModelFactory
    {
        private readonly IMapper _mapper;

        public SLAModelFactory
            (IMapper mapper)
        {
            _mapper = mapper;
        }
        public SLAHolidayModel PrepareSLAHolidayModel(SLAHolidayDTO slaHoliday)
        {
            if (slaHoliday != null)
            {
                var slaHolidayModel = new SLAHolidayModel();
                slaHolidayModel.Id = slaHoliday.Id;
                slaHolidayModel.Date = slaHoliday.Date;
                slaHolidayModel.Description = slaHoliday.Description;
                slaHolidayModel.IsAdhoc = slaHoliday.IsAdhoc;

                return slaHolidayModel;
            }

            return new SLAHolidayModel();
        }

        public SLAStaffLeaveModel PrepareSLAStaffLeaveModel(SLAStaffLeaveDTO slaStaffLeave)
        {
            if (slaStaffLeave != null)
            {
                var slaStaffLeaveModel = new SLAStaffLeaveModel();
                slaStaffLeaveModel.Id = slaStaffLeave.Id;
                slaStaffLeaveModel.StaffId = slaStaffLeave.StaffId;
                slaStaffLeaveModel.StartDate = slaStaffLeave.StartDate;
                slaStaffLeaveModel.EndDate = slaStaffLeave.EndDate;
                slaStaffLeaveModel.Reason = slaStaffLeave.Reason;

                return slaStaffLeaveModel;
            }

            return new SLAStaffLeaveModel();
        }

        public List<SLAHolidayModel> PrepareSLAHolidayModels(List<SLAHolidayDTO> slaHolidays)
        {
            if (slaHolidays?.Count == 0 || slaHolidays == null)
                return new List<SLAHolidayModel>();

            List<SLAHolidayModel> slaHolidayModels = new List<SLAHolidayModel>();
            foreach (var slaHoliday in slaHolidays)
            {
                var slaHolidayModel = new SLAHolidayModel
                {
                    Id = slaHoliday.Id,
                    Date = slaHoliday.Date,
                    Description = slaHoliday.Description,
                    IsAdhoc = slaHoliday.IsAdhoc
                };
                slaHolidayModels.Add(slaHolidayModel);
            }

            return slaHolidayModels;
        }

        public List<SLAStaffLeaveModel> PrepareSLAStaffLeaveModels(List<SLAStaffLeaveDTO> slaStaffLeaves)
        {
            if (slaStaffLeaves?.Count == 0 || slaStaffLeaves == null)
                return new List<SLAStaffLeaveModel>();

            List<SLAStaffLeaveModel> slaStaffLeaveModels = new List<SLAStaffLeaveModel>();
            foreach (var slaStaffLeave in slaStaffLeaves)
            {
                var slaStaffLeaveModel = new SLAStaffLeaveModel
                {
                    Id = slaStaffLeave.Id,
                    StaffId = slaStaffLeave.StaffId,
                    StartDate = slaStaffLeave.StartDate,
                    EndDate = slaStaffLeave.EndDate,
                    Reason = slaStaffLeave.Reason
                };
                slaStaffLeaveModels.Add(slaStaffLeaveModel);
            }

            return slaStaffLeaveModels;
        }

        public SLASettingModel PrepareSLASettingModel(SLASettingDTO slaSetting)
        {
            if (slaSetting != null)
            {
                var slaSettingModel = new SLASettingModel();
                slaSettingModel.IncludeHoliday = slaSetting.IncludeHoliday;
                slaSettingModel.IncludeStaffLeaves = slaSetting.IncludeStaffLeaves;
                slaSettingModel.IncludeWorkingHours = slaSetting.IncludeWorkingHours;
                slaSettingModel.WorkingStartHour = slaSetting.WorkingStartHour;
                slaSettingModel.WorkingStartMinute = slaSetting.WorkingStartMinute;
                slaSettingModel.WorkingEndHour = slaSetting.WorkingEndHour;
                slaSettingModel.WorkingEndMinute = slaSetting.WorkingEndMinute;
                slaSettingModel.DeadlineInHours = slaSetting.DeadlineInHours;

                return slaSettingModel;
            }

            return new SLASettingModel();
        }
    }
}
