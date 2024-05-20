using Epson.Core.Domain.Users;
using Epson.Data;
using Epson.Services.DTO.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Services.Interface.Users
{
    public interface IUserService
    {
        public TeamDTO GetTeamById(int id);
        public List<TeamDTO> GetTeams();
        public List<ApplicationUser> GetAllUsers();
        public List<ApplicationUser> GetGovtUsersWithProductRole();
        Dictionary<string, string> InitializeTeamHierarchy(bool isSalesHead = false, bool multiRole = false);
        List<int> GetChildTeamIds(Dictionary<string, string> teamHierarchy, int parentTeamId, IRepository<Team> teamRepository);
        Task<List<ApplicationUser>> GetUserSalesHead(int teamID, string createdById, bool isSalesHead = false);
    }
}
