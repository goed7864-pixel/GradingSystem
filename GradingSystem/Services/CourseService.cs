using System.Threading.Tasks;
using GradingSystem.DTOs;

namespace GradingSystem.Services
{
    public class CourseService
    {
        private readonly ApiClient _apiClient;

        public CourseService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<PagedResponse<CourseDto>?> GetCoursesAsync(int page = 1, int pageSize = 10)
        {
            return await _apiClient.GetAsync<PagedResponse<CourseDto>>($"api/courses?page={page}&pageSize={pageSize}");
        }

        public async Task<CourseDto?> GetCourseByIdAsync(int id)
        {
            return await _apiClient.GetAsync<CourseDto>($"api/courses/{id}");
        }

        public async Task<CourseResponse?> CreateCourseAsync(CourseCreateDto createDto)
        {
            return await _apiClient.PostAsync<CourseResponse>("api/courses", createDto);
        }

        public async Task<CourseResponse?> UpdateCourseAsync(int id, CourseUpdateDto updateDto)
        {
            return await _apiClient.PutAsync<CourseResponse>($"api/courses/{id}", updateDto);
        }

        public async Task<bool> DeleteCourseAsync(int id)
        {
            return await _apiClient.DeleteAsync($"api/courses/{id}");
        }

        public async Task<PagedResponse<UserDto>?> GetCourseStudentsAsync(int courseId, int page = 1, int pageSize = 10)
        {
            return await _apiClient.GetAsync<PagedResponse<UserDto>>($"api/courses/{courseId}/students?page={page}&pageSize={pageSize}");
        }

        public async Task<PagedResponse<AssignmentDto>?> GetCourseAssignmentsAsync(int courseId, int page = 1, int pageSize = 10)
        {
            return await _apiClient.GetAsync<PagedResponse<AssignmentDto>>($"api/courses/{courseId}/assignments?page={page}&pageSize={pageSize}");
        }

        public async Task<PagedResponse<CourseDto>?> GetMyCoursesAsync(int page = 1, int pageSize = 10)
        {
            return await _apiClient.GetAsync<PagedResponse<CourseDto>>($"api/courses/my?page={page}&pageSize={pageSize}");
        }

        public async Task<PagedResponse<CourseDto>?> GetCoursesWithCountsAsync(int page = 1, int pageSize = 10)
        {
            return await _apiClient.GetAsync<PagedResponse<CourseDto>>($"api/courses/with-counts?page={page}&pageSize={pageSize}");
        }
    }
}
