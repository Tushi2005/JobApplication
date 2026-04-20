using JobApplication.Models;

namespace JobApplication.Services.Applications
{
    public interface IApplicationService
    {
        Task<List<Application>> GetAllAsync(string userId);
        Task<Application?> GetByIdAsync(int id, string userId);
        Task<List<string>> GetCompaniesByUserAsync(string userId);
        Task<List<string>> GetPositionsByUserAsync(string userId);
        Task<Application> CreateAsync(Application application);
        Task<Application?> UpdateAsync(int id, Application application, string userId);
        Task<bool> DeleteAsync(int id, string userId);
        Task<Application?> PatchStatusAsync(int id, ApplicationStatus status, string userId);
    }
}