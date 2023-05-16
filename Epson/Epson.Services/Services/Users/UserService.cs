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
        private readonly UserManager<ApplicationUser> _userManager;
        public UserService
            (ILogger logger,
            IMapper mapper,
            IRepository<Team> TeamRepository,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _mapper = mapper;
            _TeamRepository = TeamRepository;
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
    }
}
