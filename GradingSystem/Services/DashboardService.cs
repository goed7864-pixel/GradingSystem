using System.Threading.Tasks;
using GradingSystem.DTOs;

namespace GradingSystem.Services
{
    public class DashboardService
    {
        private readonly ApiClient _apiClient;

        public DashboardService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<TeacherDashboardDto?> GetTeacherDashboardAsync()
        {
            return await _apiClient.GetAsync<TeacherDashboardDto>("api/dashboard/teacher");
        }

        public async Task<StudentDashboardDto?> GetStudentDashboardAsync()
        {
            return await _apiClient.GetAsync<StudentDashboardDto>("api/dashboard/student");
        }
    }
}
