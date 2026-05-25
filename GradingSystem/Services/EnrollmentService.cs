using System.Threading.Tasks;
using GradingSystem.DTOs;

namespace GradingSystem.Services
{
    public class EnrollmentService
    {
        private readonly ApiClient _apiClient;

        public EnrollmentService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<PagedResponse<EnrollmentDto>?> GetEnrollmentsAsync(int page = 1, int pageSize = 10)
        {
            return await _apiClient.GetAsync<PagedResponse<EnrollmentDto>>($"api/enrollments?page={page}&pageSize={pageSize}");
        }

        public async Task<EnrollmentDto?> GetEnrollmentByIdAsync(int id)
        {
            return await _apiClient.GetAsync<EnrollmentDto>($"api/enrollments/{id}");
        }

        public async Task<EnrollmentResponse?> CreateEnrollmentAsync(EnrollmentCreateDto createDto)
        {
            return await _apiClient.PostAsync<EnrollmentResponse>("api/enrollments", createDto);
        }

        public async Task<bool> DeleteEnrollmentAsync(int id)
        {
            return await _apiClient.DeleteAsync($"api/enrollments/{id}");
        }

        public async Task<bool> EnrollStudentAsync(int courseId, int studentId)
        {
            try
            {
                var createDto = new EnrollmentCreateDto
                {
                    CourseId = courseId,
                    StudentId = studentId
                };

                var response = await CreateEnrollmentAsync(createDto);
                return response != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
