using System.Threading.Tasks;
using GradingSystem.DTOs;

namespace GradingSystem.Services
{
    public class GradeService
    {
        private readonly ApiClient _apiClient;

        public GradeService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<PagedResponse<GradeDto>?> GetGradesAsync(int page = 1, int pageSize = 10)
        {
            return await _apiClient.GetAsync<PagedResponse<GradeDto>>($"api/grades?page={page}&pageSize={pageSize}");
        }

        public async Task<GradeDto?> GetGradeByIdAsync(int id)
        {
            return await _apiClient.GetAsync<GradeDto>($"api/grades/{id}");
        }

        public async Task<GradeResponse?> CreateGradeAsync(GradeCreateDto createDto)
        {
            return await _apiClient.PostAsync<GradeResponse>("api/grades", createDto);
        }

        public async Task<GradeResponse?> UpdateGradeAsync(int id, GradeUpdateDto updateDto)
        {
            return await _apiClient.PutAsync<GradeResponse>($"api/grades/{id}", updateDto);
        }

        public async Task<bool> DeleteGradeAsync(int id)
        {
            return await _apiClient.DeleteAsync($"api/grades/{id}");
        }

        public async Task<PagedResponse<GradeDto>?> GetGradesByStudentAsync(int studentId, int page = 1, int pageSize = 10)
        {
            return await _apiClient.GetAsync<PagedResponse<GradeDto>>($"api/grades/student/{studentId}?page={page}&pageSize={pageSize}");
        }

        public async Task<PagedResponse<GradeDto>?> GetGradesByAssignmentAsync(int assignmentId, int page = 1, int pageSize = 10)
        {
            return await _apiClient.GetAsync<PagedResponse<GradeDto>>($"api/grades/assignment/{assignmentId}?page={page}&pageSize={pageSize}");
        }

        public async Task<GradeDto?> GetGradeBySubmissionIdAsync(int submissionId)
        {
            return await _apiClient.GetAsync<GradeDto>($"api/grades/submission/{submissionId}");
        }
    }
}
