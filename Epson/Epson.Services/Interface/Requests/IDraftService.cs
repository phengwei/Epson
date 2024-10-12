using Epson.Core.Domain.Requests;
using Epson.Services.DTO.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Services.Interface.Requests
{
    public interface IDraftService
    {
        bool InsertDraft(DraftDTO draft);
        bool UpdateDraft(DraftDTO draft);
        List<DraftDTO> GetDrafts(out int totalItems, Func<Draft, bool> filter, string search = null, int? page = null, int? itemsPerPage = null);
        DraftDTO GetDraftById(int id);
        bool DeleteDraft(DraftDTO draft);
        DraftDTO GetDraftByUserId(string userId);
        List<DraftDTO> GetDraftsByUserId(string userId);
    }

}
