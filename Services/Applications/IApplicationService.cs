using JobApplication.Models;

namespace JobApplication.Services.Applications
{
    public interface IApplicationService
    {
        Task<List<Application>> GetAllAsync(int userId);
        Task<Application?> GetByIdAsync(int id, int userId);
        Task<Application> CreateAsync(Application application);
        Task<Application?> UpdateAsync(int id, Application application, int userId);
        Task<bool> DeleteAsync(int id, int userId);
    }
}
