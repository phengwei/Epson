using AutoMapper;
using Epson.Core.Domain.Requests;
using Epson.Data.Context;
using Epson.Services.DTO.Requests;
using Epson.Services.Interface.Requests;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Services.Services.Requests
{
    public class DraftService : IDraftService
    {
        private readonly EpsonDbContext _context;
        private readonly IMapper _mapper; 
        private readonly ILogger<DraftService> _logger;

        public DraftService(EpsonDbContext context, IMapper mapper, ILogger<DraftService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public bool InsertDraft(DraftDTO draft)
        {
            if (draft == null)
                throw new ArgumentNullException(nameof(draft));

            try
            {
                var draftEntity = _mapper.Map<Draft>(draft);
                draftEntity.CreatedOnUTC = DateTime.UtcNow;
                draftEntity.UpdatedOnUTC = DateTime.UtcNow;

                _context.Draft.Add(draftEntity);
                _context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting draft");
                return false;
            }
        }

        public bool UpdateDraft(DraftDTO draft)
        {
            if (draft == null)
                throw new ArgumentNullException(nameof(draft));

            try
            {
                var existingDraft = _context.Draft.Find(draft.Id);
                if (existingDraft == null)
                    return false;

                existingDraft.UpdatedOnUTC = DateTime.UtcNow;
                _mapper.Map(draft, existingDraft); 

                _context.Draft.Update(existingDraft);
                _context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating draft");
                return false;
            }
        }

        public List<DraftDTO> GetDrafts(out int totalItems, Func<Draft, bool> filter, string search = null, int? page = null, int? itemsPerPage = null)
        {
            try
            {
                var draftsQuery = _context.Draft.AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    draftsQuery = draftsQuery.Where(d => d.Comments.Contains(search) || d.CustomerName.Contains(search));
                }

                var filteredDrafts = draftsQuery.Where(filter).ToList();
                totalItems = filteredDrafts.Count;

                if (page.HasValue && itemsPerPage.HasValue && itemsPerPage.Value != -1)
                {
                    filteredDrafts = filteredDrafts
                        .Skip((page.Value - 1) * itemsPerPage.Value)
                        .Take(itemsPerPage.Value)
                        .ToList();
                }

                return _mapper.Map<List<DraftDTO>>(filteredDrafts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving drafts");
                totalItems = 0;
                return new List<DraftDTO>();
            }
        }
        public DraftDTO GetDraftByUserId(string userId)
        {
            var draft = _context.Draft
                .FirstOrDefault(d => d.UserId == userId);

            return draft != null ? _mapper.Map<DraftDTO>(draft) : null;
        }


        public DraftDTO GetDraftById(int id)
        {
            try
            {
                var draft = _context.Draft
                    .FirstOrDefault(d => d.Id == id);

                if (draft == null)
                    return null;

                return _mapper.Map<DraftDTO>(draft);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving draft by ID");
                return null;
            }
        }

        public bool DeleteDraft(DraftDTO draft)
        {
            if (draft == null)
                throw new ArgumentNullException(nameof(draft));

            try
            {
                var draftEntity = _context.Draft.Find(draft.Id);
                if (draftEntity == null)
                    return false;

                _context.Draft.Remove(draftEntity);
                _context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting draft");
                return false;
            }
        }
    }

}
