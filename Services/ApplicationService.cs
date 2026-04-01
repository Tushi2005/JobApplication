using JobApplication.Data;
using JobApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace JobApplication.Services
{
    public class ApplicationService: IApplicationService
    {
        private readonly AppDbContext _context;

        public ApplicationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Application>> GetAllAsync(int userId)
        {
            return await _context.
                Applications.
                Where(a => a.UserId == userId).
                ToListAsync();
        }

        public async Task<Application?> GetByIdAsync(int id, int userId)
        {
            return await _context.Applications.
                FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        }

        public async Task<Application> CreateAsync(Application application)
        {
            _context.Applications.Add(application);
            await _context.SaveChangesAsync();
            return application;
        }

        public async Task<Application?> UpdateAsync(int id, Application application, int userId)
        {
            var existing =  await _context.Applications.
                FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (existing == null) return null;

            existing.CompanyName = application.CompanyName;
            existing.Position = application.Position;
            existing.Status = application.Status;
            existing.AppliedAt = application.AppliedAt;
            existing.InterviewAt = application.InterviewAt;
            existing.JobUrl = application.JobUrl;
            existing.Notes = application.Notes;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id, int userId)
        {
            var existing = await _context.Applications.
                FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (existing == null) return false;
            _context.Applications.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
