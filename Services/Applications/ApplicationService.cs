using JobApplication.Data;
using JobApplication.DTOs.Application;
using JobApplication.Models;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace JobApplication.Services.Applications
{
    public class ApplicationService : IApplicationService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ApplicationService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<ApplicationResponseDto>> GetAllAsync(string userId)
        {
           var applications =  await _context.Applications
                .Where(a => a.UserId == userId)
                .ToListAsync();
            return _mapper.Map<List<ApplicationResponseDto>>(applications);
        }

        public async Task<ApplicationResponseDto?> GetByIdAsync(int id, string userId)
        {
            var application = await _context.Applications
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
            return _mapper.Map<ApplicationResponseDto>(application);
        }

        public async Task<List<string>> GetCompaniesByUserAsync(string userId)
        {
            return await _context.Applications
                .Where(a => a.UserId == userId)
                .Select(a => a.CompanyName)
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<string>> GetPositionsByUserAsync(string userId)
        {
            return await _context.Applications
                .Where(a => a.UserId == userId)
                .Select(a => a.Position)
                .Distinct()
                .ToListAsync();
        }

        public async Task<ApplicationResponseDto> CreateAsync(CreateApplicationDto applicationDto, string userId)
        {
            var application = _mapper.Map<Application>(applicationDto);
            application.UserId = userId;
            _context.Applications.Add(application);
            await _context.SaveChangesAsync();
            return _mapper.Map<ApplicationResponseDto>(application);
        }

        public async Task<ApplicationResponseDto?> UpdateAsync(int id, UpdateApplicationDto applicationDto, string userId)
        {
            var existing = await _context.Applications
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (existing == null) return null;
            _mapper.Map(applicationDto, existing);

            await _context.SaveChangesAsync();
            return _mapper.Map<ApplicationResponseDto>(existing);
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            var existing = await _context.Applications
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (existing == null) return false;
            _context.Applications.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Application?> PatchStatusAsync(int id, ApplicationStatus status, string userId)
        {
            var existing = await _context.Applications
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (existing == null) return null;

            existing.Status = status;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existing;
        }
    }
}