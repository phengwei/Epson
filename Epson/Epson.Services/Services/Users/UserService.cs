using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Epson.Services.Interface.Email;
using Epson.Services.DTO.Email;
using Epson.Data;
using Epson.Core.Domain.Email;
using AutoMapper;
using MimeKit;
using Microsoft.IdentityModel.Tokens;
using Epson.Core.Domain.Requests;
using Microsoft.AspNetCore.Identity;
using Epson.Core.Domain.Users;
using Epson.Services.Interface.Products;
using Epson.Services.Interface.Users;
using Epson.Services.DTO.Users;
using Epson.Core.Domain.Enum;

namespace Epson.Services.Services.Users
{
    public class UserService : IUserService
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IRepository<Team> _TeamRepository;
        private readonly IRepository<TeamHierarchy> _TeamHierarchyRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        public UserService
            (ILogger logger,
            IMapper mapper,
            IRepository<Team> TeamRepository,
            IRepository<TeamHierarchy> TeamHierarchyRepository,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _mapper = mapper;
            _TeamRepository = TeamRepository;
            _TeamHierarchyRepository = TeamHierarchyRepository;
            _userManager = userManager;
        }

        public TeamDTO GetTeamById(int id)
        {
            var team = _TeamRepository.GetById(id);

            return _mapper.Map<TeamDTO>(team);
        }

        public List<TeamDTO> GetTeams()
        {
            return _mapper.Map<List<TeamDTO>>(_TeamRepository.GetAll());
        }

        public List<ApplicationUser> GetAllUsers()
        {
            List<ApplicationUser> users = _userManager.Users.ToList();
            return users;
        }

        public List<ApplicationUser> GetGovtUsersWithProductRole()
        {
            var team = _TeamRepository.Table.FirstOrDefault(x => x.Name == TeamEnum.Govt.ToString().ToUpper());
            var govtUsers = GetAllUsers().Where(x => x.TeamId == team.Id).ToList();

            var productRoleUsers = new List<ApplicationUser>();

            foreach (var user in govtUsers)
            {
                var userRoles = _userManager.GetRolesAsync(user).Result;

                if (userRoles.Contains("Product"))
                {
                    productRoleUsers.Add(user);
                }
            }

            return productRoleUsers;
        }

        public string GetCoverplusTeamHierarchyEmail()
        {
            return _TeamHierarchyRepository.GetAll().Where(x => x.ApprovalLevel == 99).First().EmailRecipient;
        }

        public List<TeamHierarchy> GetTeamHierarchy()
        {
            return _TeamHierarchyRepository.GetAll().ToList();
        }

        public Dictionary<string, string> InitializeTeamHierarchy(bool isSalesHead = false, bool multiRole = false)
        {
            List<TeamHierarchy> teamHierarchies = GetTeamHierarchy();

            var filteredHierarchies = teamHierarchies
                .Where(th => th.IsSalesHead == isSalesHead)
                .ToList();

            Dictionary<string, string> teamHierarchyDict = filteredHierarchies
                .ToDictionary(th => th.RequestingTeam, th => th.ApproverTeam);

            return teamHierarchyDict;
        }


        //public Dictionary<string, string> InitializeTeamHierarchy(bool isSalesHead = false, bool multiRole = false)
        //{
        //    List<TeamHierarchy> teamHierarchies = _TeamHierarchyRepository.GetAll().ToList();
        //    if (!isSalesHead)
        //    {
        //        return new Dictionary<string, string>
        //        {
        //            {"Corporate Sales", "Corporate Sales"},
        //            {"Government Sales", "Government Sales"},
        //            {"Corp & Gov", "Corp & Gov"},
        //            {"Inside Sales", "Channel Support"},
        //            {"Sales Support Management", "Channel Support"},
        //            {"Product Marketing", "Product Marketing"},
        //            {"Brand & Comms", "Channel Support"},
        //            {"Vertical Biz (LFP)", "Channel Support"},
        //            {"Area (City)", "Retail"},
        //            {"Area - MDT / Chain Store / E Commerce", "Retail"},
        //            {"Retail", "Retail"},
        //            {"Corporate Sales (West M'sia)", "Corporate Sales (West M'sia)" },
        //            {"Corporate Sales (East M'sia)", "Corporate Sales (East M'sia)" }
        //        };
        //    }
        //    else if (isSalesHead && multiRole)
        //    {
        //        return new Dictionary<string, string>
        //        {
        //            {"Corporate Sales", "Corp & Gov"},
        //            {"Government Sales", "Corp & Gov"},
        //            {"Corp & Gov", "Corp & Gov"},
        //            {"Inside Sales", "Channel Support"},
        //            {"Sales Support Management", "Channel Support"},
        //            {"Product Marketing", "Product Marketing"},
        //            {"Brand & Comms", "Channel Support"},
        //            {"Vertical Biz (LFP)", "Channel Support"},
        //            {"Area (City)", "Retail"},
        //            {"Area - MDT / Chain Store / E Commerce", "Retail"},
        //            {"Retail", "Retail"},
        //            {"Corporate Sales (West M'sia)", "Corp & Gov" },
        //            {"Corporate Sales (East M'sia)", "Corp & Gov" }
        //        };
        //    }
        //    else
        //    {
        //        return new Dictionary<string, string>
        //        {
        //            {"Corporate Sales", "Corp & Gov"},
        //            {"Government Sales", "Corp & Gov"},
        //            {"Corp & Gov", "Corp & Gov"},
        //            {"Inside Sales", "Channel Support"},
        //            {"Sales Support Management", "Channel Support"},
        //            {"Product Marketing", "Product Marketing"},
        //            {"Brand & Comms", "Channel Support"},
        //            {"Vertical Biz (LFP)", "Channel Support"},
        //            {"Area (City)", "Retail"},
        //            {"Area - MDT / Chain Store / E Commerce", "Retail"},
        //            {"Retail", "Retail Head"},
        //            {"Corporate Sales (West M'sia)", "Corp & Gov" },
        //            {"Corporate Sales (East M'sia)", "Corp & Gov" }
        //        };
        //    }
        //}

        public List<int> GetChildTeamIds(Dictionary<string, string> teamHierarchy, int parentTeamId, IRepository<Team> teamRepository)
        {
            var parentTeamName = teamRepository.GetAll().FirstOrDefault(t => t.Id == parentTeamId)?.Name;

            var childTeamNames = teamHierarchy
                .Where(kvp => kvp.Value == parentTeamName)
                .Select(kvp => kvp.Key)
                .ToList();

            var childTeamIds = teamRepository.GetAll()
                .Where(t => childTeamNames.Contains(t.Name))
                .Select(t => t.Id)
                .ToList();

            return childTeamIds;
        }

        public async Task<List<ApplicationUser>> GetUserSalesHead(int teamID, string userId, bool isSalesHead = false)
        {
            string fulfillerTeamName = _TeamRepository.GetAll()
                .Where(x => x.Id == teamID)
                .FirstOrDefault()?.Name;

            if (string.IsNullOrEmpty(fulfillerTeamName))
            {
                _logger.Information($"Team with ID {teamID} not found.");
                return null;
            }

            List<TeamHierarchy> teamHierarchies = GetTeamHierarchy();

            var approverRecord = teamHierarchies
                .Where(th => th.RequestingTeam == fulfillerTeamName && th.IsSalesHead == isSalesHead)
                .OrderBy(th => th.ApprovalLevel)
                .FirstOrDefault();

            if (approverRecord == null)
            {
                _logger.Information($"No approver team found for team {fulfillerTeamName} with IsSalesHead = {isSalesHead}");
                return null;
            }

            List<ApplicationUser> salesHeadUsers = new List<ApplicationUser>();

            salesHeadUsers.Add(await _userManager.FindByEmailAsync(approverRecord.EmailRecipient));

            if (salesHeadUsers.Count > 0)
            {
                if (salesHeadUsers.First() != null && salesHeadUsers.First().Id != userId)
                {
                    var isInRole = await _userManager.IsInRoleAsync(salesHeadUsers.First(), "Sales Section Head");
                    if (isInRole)
                    {
                        return salesHeadUsers;
                    }
                }

            }
            return null;
        }



        //public async Task<List<ApplicationUser>> GetUserSalesHead(int teamID, string userId, bool isSalesHead = false)
        //{
        //    string fulfillerTeamName = _TeamRepository.GetAll().Where(x => x.Id == teamID).FirstOrDefault().Name;
        //    List<TeamHierarchy> teamHierarchies = GetTeamHierarchy();

        //    var teamHierarchy = InitializeTeamHierarchy(isSalesHead);
        //    var parentTeamName = teamHierarchy.ContainsKey(fulfillerTeamName) ? teamHierarchy[fulfillerTeamName] : null;

        //    if (parentTeamName == null)
        //    {
        //        _logger.Information($"No parent team found for team {fulfillerTeamName}");
        //        return new List<ApplicationUser>();
        //    }

        //    var parentTeam = _TeamRepository.Table.FirstOrDefault(t => t.Name == parentTeamName);
        //    if (parentTeam == null)
        //    {
        //        _logger.Information($"Parent team {parentTeamName} not found in the repository.");
        //        return new List<ApplicationUser>();
        //    }

        //    var salesUsers = await _userManager.GetUsersInRoleAsync("Sales Section Head");
        //    var salesHeadUser = salesUsers.Where(x => x.TeamId == parentTeam.Id && x.Id != userId).ToList();

        //    return salesHeadUser;
        //}

    }
}
