using System;
using System.Threading.Tasks;
using GradingSystem.DTOs;

namespace GradingSystem.Services
{
    public class UserService
    {
        private readonly ApiClient _apiClient;

        public UserService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<PagedResponse<UserDto>?> GetUsersAsync(int page = 1, int pageSize = 10)
        {
            return await _apiClient.GetAsync<PagedResponse<UserDto>>($"api/users?page={page}&pageSize={pageSize}");
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            return await _apiClient.GetAsync<UserDto>($"api/users/{id}");
        }

        public async Task<UserResponse?> UpdateUserAsync(int id, UserUpdateDto updateDto)
        {
            return await _apiClient.PutAsync<UserResponse>($"api/users/{id}", updateDto);
        }

        public async Task<UserResponse?> RegisterUserAsync(RegisterDto registerDto)
        {
            return await _apiClient.PostAsync<UserResponse>("api/auth/register", registerDto);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            return await _apiClient.DeleteAsync($"api/users/{id}");
        }

        public async Task<PagedResponse<UserDto>?> SearchStudentsAsync(string? searchTerm = null, int? groupId = null, int page = 1, int pageSize = 10)
        {
            var queryParams = $"page={page}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(searchTerm))
                queryParams += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
            if (groupId.HasValue)
                queryParams += $"&groupId={groupId.Value}";

            return await _apiClient.GetAsync<PagedResponse<UserDto>>($"api/users/students/search?{queryParams}");
        }
    }
}
