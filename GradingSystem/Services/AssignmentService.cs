using System;
using System.Threading.Tasks;
using GradingSystem.DTOs;

namespace GradingSystem.Services
{
    public class AssignmentService
    {
        private readonly ApiClient _apiClient;

        public AssignmentService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<PagedResponse<AssignmentDto>?> GetAssignmentsAsync(int page = 1, int pageSize = 10)
        {
            return await _apiClient.GetAsync<PagedResponse<AssignmentDto>>($"api/assignments?page={page}&pageSize={pageSize}");
        }

        public async Task<AssignmentDto?> GetAssignmentByIdAsync(int id)
        {
            return await _apiClient.GetAsync<AssignmentDto>($"api/assignments/{id}");
        }

        public async Task<AssignmentResponse?> CreateAssignmentAsync(AssignmentCreateDto createDto)
        {
            return await _apiClient.PostAsync<AssignmentResponse>("api/assignments", createDto);
        }

        public async Task<AssignmentResponse?> UpdateAssignmentAsync(int id, AssignmentUpdateDto updateDto)
        {
            return await _apiClient.PutAsync<AssignmentResponse>($"api/assignments/{id}", updateDto);
        }

        public async Task<bool> DeleteAssignmentAsync(int id)
        {
            return await _apiClient.DeleteAsync($"api/assignments/{id}");
        }

        public async Task<PagedResponse<AssignmentDto>?> SearchAssignmentsAsync(string? searchTerm = null, int? courseId = null, int page = 1, int pageSize = 10)
        {
            var queryParams = $"page={page}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(searchTerm))
                queryParams += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
            if (courseId.HasValue)
                queryParams += $"&courseId={courseId.Value}";

            return await _apiClient.GetAsync<PagedResponse<AssignmentDto>>($"api/assignments/search?{queryParams}");
        }

        public async Task<PagedResponse<SubmissionDto>?> GetAssignmentSubmissionsAsync(int assignmentId, int? groupId = null, string? status = null, int page = 1, int pageSize = 10)
        {
            var queryParams = $"page={page}&pageSize={pageSize}";
            if (groupId.HasValue)
                queryParams += $"&groupId={groupId.Value}";
            if (!string.IsNullOrEmpty(status))
                queryParams += $"&status={Uri.EscapeDataString(status)}";

            return await _apiClient.GetAsync<PagedResponse<SubmissionDto>>($"api/assignments/{assignmentId}/submissions?{queryParams}");
        }

        public async Task<PagedResponse<AssignmentDto>?> GetAssignmentsWithCountsAsync(int page = 1, int pageSize = 10)
        {
            return await _apiClient.GetAsync<PagedResponse<AssignmentDto>>($"api/assignments/with-counts?page={page}&pageSize={pageSize}");
        }
    }
}
