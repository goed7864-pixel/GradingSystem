using System.Threading.Tasks;
using GradingSystem.DTOs;

namespace GradingSystem.Services
{
    public class GroupService
    {
        private readonly ApiClient _apiClient;

        public GroupService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<PagedResponse<StudentGroupDto>?> GetGroupsAsync(int page = 1, int pageSize = 10)
        {
            return await _apiClient.GetAsync<PagedResponse<StudentGroupDto>>($"api/groups?page={page}&pageSize={pageSize}");
        }

        public async Task<StudentGroupDto?> GetGroupByIdAsync(int id)
        {
            return await _apiClient.GetAsync<StudentGroupDto>($"api/groups/{id}");
        }

        public async Task<StudentGroupResponse?> CreateGroupAsync(StudentGroupCreateDto createDto)
        {
            return await _apiClient.PostAsync<StudentGroupResponse>("api/groups", createDto);
        }

        public async Task<StudentGroupResponse?> UpdateGroupAsync(int id, StudentGroupUpdateDto updateDto)
        {
            return await _apiClient.PutAsync<StudentGroupResponse>($"api/groups/{id}", updateDto);
        }

        public async Task<bool> DeleteGroupAsync(int id)
        {
            return await _apiClient.DeleteAsync($"api/groups/{id}");
        }

        public async Task<PagedResponse<UserDto>?> GetGroupStudentsAsync(int groupId, int page = 1, int pageSize = 10)
        {
            return await _apiClient.GetAsync<PagedResponse<UserDto>>($"api/groups/{groupId}/students?page={page}&pageSize={pageSize}");
        }

        public async Task<bool> AddStudentToGroupAsync(int groupId, int studentId)
        {
            try
            {
                await _apiClient.PostAsync<object>($"api/groups/{groupId}/students/{studentId}", null);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveStudentFromGroupAsync(int groupId, int studentId)
        {
            return await _apiClient.DeleteAsync($"api/groups/{groupId}/students/{studentId}");
        }

        public async Task<PagedResponse<StudentGroupDto>?> GetGroupsWithCountAsync(int page = 1, int pageSize = 10)
        {
            return await _apiClient.GetAsync<PagedResponse<StudentGroupDto>>($"api/groups/with-count?page={page}&pageSize={pageSize}");
        }

        public async Task<PagedResponse<StudentGroupDto>?> GetGroupsByCourseAsync(int courseId, int page = 1, int pageSize = 10)
        {
            return await _apiClient.GetAsync<PagedResponse<StudentGroupDto>>($"api/courses/{courseId}/groups?page={page}&pageSize={pageSize}");
        }
    }
}
