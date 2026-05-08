using JobApplication.DTOs.Application;
using JobApplication.Models;

namespace JobApplication.Services.Applications
{
    public interface IApplicationService
    {
        Task<List<ResponseApplicationDto>> GetAllAsync(string userId);
        Task<ResponseApplicationDto?> GetByIdAsync(int id, string userId);
        Task<List<string>> GetCompaniesByUserAsync(string userId);
        Task<List<string>> GetPositionsByUserAsync(string userId);
        Task<ResponseApplicationDto> CreateAsync(CreateApplicationDto applicationDto, string userId);
        Task<ResponseApplicationDto?> UpdateAsync(int id, UpdateApplicationDto applicationDto, string userId);
        Task<bool> DeleteAsync(int id, string userId);
        Task<ResponseApplicationDto?> PatchStatusAsync(int id, ResponseApplicationDto patchDto, string userId);
    }
}