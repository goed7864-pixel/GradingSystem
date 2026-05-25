using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using GradingSystem.DTOs;

namespace GradingSystem.Services
{
    public class SubmissionService
    {
        private readonly ApiClient _apiClient;

        public SubmissionService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<PagedResponse<SubmissionDto>?> GetSubmissionsAsync(int page = 1, int pageSize = 10)
        {
            return await _apiClient.GetAsync<PagedResponse<SubmissionDto>>($"api/submissions?page={page}&pageSize={pageSize}");
        }

        public async Task<PagedResponse<SubmissionDto>?> GetStudentSubmissionsAsync(int studentId, int page = 1, int pageSize = 100)
        {
            // API автоматически фильтрует по текущему пользователю из JWT токена
            // Параметр studentId используется только для проверки на клиенте
            return await _apiClient.GetAsync<PagedResponse<SubmissionDto>>($"api/submissions?page={page}&pageSize={pageSize}");
        }

        public async Task<SubmissionDto?> GetSubmissionByIdAsync(int id)
        {
            return await _apiClient.GetAsync<SubmissionDto>($"api/submissions/{id}");
        }

        public async Task<SubmissionResponse?> CreateSubmissionAsync(int assignmentId, int studentId, string filePath)
        {
            // Читаем файл в память полностью
            var fileBytes = await File.ReadAllBytesAsync(filePath);
            var fileName = Path.GetFileName(filePath);

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(assignmentId.ToString()), "assignmentId");
            content.Add(new StringContent(studentId.ToString()), "studentId");

            // Создаем ByteArrayContent вместо StreamContent
            var fileContent = new ByteArrayContent(fileBytes);

            // Определяем MIME тип на основе расширения файла
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            var mimeType = extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".txt" => "text/plain",
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };

            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);
            content.Add(fileContent, "file", fileName);

            return await _apiClient.UploadFileAsync<SubmissionResponse>("api/submissions", content);
        }

        public async Task<byte[]> DownloadSubmissionAsync(int id)
        {
            return await _apiClient.DownloadFileAsync($"api/submissions/{id}/download");
        }

        public async Task<bool> DeleteSubmissionAsync(int id)
        {
            return await _apiClient.DeleteAsync($"api/submissions/{id}");
        }

        public async Task<SubmissionDto?> GetSubmissionDetailAsync(int id)
        {
            return await _apiClient.GetAsync<SubmissionDto>($"api/submissions/{id}");
        }
    }
}
