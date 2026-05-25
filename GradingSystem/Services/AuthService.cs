using System.Threading.Tasks;
using GradingSystem.DTOs;

namespace GradingSystem.Services
{
    public class AuthService
    {
        private readonly ApiClient _apiClient;
        private readonly TokenService _tokenService;

        public AuthService(ApiClient apiClient, TokenService tokenService)
        {
            _apiClient = apiClient;
            _tokenService = tokenService;
        }

        public async Task<string> RegisterAsync(RegisterDto registerDto)
        {
            var response = await _apiClient.PostAsync<string>("api/auth/register", registerDto);
            return response ?? "Регистрация выполнена";
        }

        public async Task<LoginResponse?> LoginAsync(LoginDto loginDto)
        {
            var response = await _apiClient.PostAsync<LoginResponse>("api/auth/login", loginDto);
            if (response != null && !string.IsNullOrEmpty(response.Token))
            {
                _apiClient.SetToken(response.Token);
                _tokenService.SetToken(response.Token);
            }
            return response;
        }

        public void Logout()
        {
            _apiClient.ClearToken();
        }

        public bool IsAuthenticated => _apiClient.IsAuthenticated;
    }
}
