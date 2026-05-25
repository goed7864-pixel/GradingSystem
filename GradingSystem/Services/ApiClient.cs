using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GradingSystem.Services
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private string? _token;

        public ApiClient()
        {
            _baseUrl = LoadApiUrl();

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(_baseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private static string LoadApiUrl()
        {
            try
            {
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
                if (File.Exists(configPath))
                {
                    var json = File.ReadAllText(configPath);
                    var config = JsonSerializer.Deserialize<AppSettings>(json);
                    if (config?.ApiSettings?.BaseUrl != null)
                    {
                        return config.ApiSettings.BaseUrl.TrimEnd('/') + "/";
                    }
                }
            }
            catch
            {
                // Если не удалось загрузить конфиг, используем значение по умолчанию
            }

            return "http://localhost:7073/";
        }

        public void SetToken(string token)
        {
            _token = token;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public void ClearToken()
        {
            _token = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public bool IsAuthenticated => !string.IsNullOrEmpty(_token);

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException($"Ошибка подключения к серверу. Убедитесь, что сервер запущен на {_baseUrl}. Детали: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new ApiException("Превышено время ожидания ответа от сервера", ex);
            }
            catch (Exception ex)
            {
                throw new ApiException($"GET запрос не выполнен: {ex.Message}", ex);
            }
        }

        public async Task<T?> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var json = JsonSerializer.Serialize(data, options);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new ApiException($"Сервер вернул ошибку ({response.StatusCode}): {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result;
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException($"Ошибка подключения к серверу. Убедитесь, что сервер запущен на {_baseUrl}. Детали: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new ApiException("Превышено время ожидания ответа от сервера", ex);
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApiException($"POST запрос не выполнен: {ex.Message}", ex);
            }
        }

        public async Task<T?> PutAsync<T>(string endpoint, object data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync(endpoint, content);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                throw new ApiException($"PUT request failed: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();

                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.Message))
                        {
                            throw new ApiException(errorResponse.Message);
                        }
                    }
                    catch (JsonException)
                    {
                        // Если не удалось распарсить JSON, используем текст как есть
                    }

                    throw new ApiException($"Сервер вернул ошибку ({response.StatusCode}): {errorContent}");
                }

                return true;
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException($"Ошибка подключения к серверу. Убедитесь, что сервер запущен на {_baseUrl}. Детали: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new ApiException("Превышено время ожидания ответа от сервера", ex);
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApiException($"DELETE запрос не выполнен: {ex.Message}", ex);
            }
        }

        public async Task<byte[]> DownloadFileAsync(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                throw new ApiException($"File download failed: {ex.Message}", ex);
            }
        }

        public async Task<T?> UploadFileAsync<T>(string endpoint, MultipartFormDataContent content)
        {
            try
            {
                var response = await _httpClient.PostAsync(endpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();

                    // Пытаемся распарсить JSON ошибку
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.Message))
                        {
                            // Обрабатываем специфичные ошибки
                            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                            {
                                throw new ApiException($"Работа уже загружена: {errorResponse.Message}");
                            }
                            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                            {
                                throw new ApiException($"Неверный формат данных: {errorResponse.Message}");
                            }
                            else if (response.StatusCode == System.Net.HttpStatusCode.RequestEntityTooLarge)
                            {
                                throw new ApiException("Файл слишком большой. Максимальный размер: 50 МБ");
                            }
                            else if (response.StatusCode == System.Net.HttpStatusCode.UnsupportedMediaType)
                            {
                                throw new ApiException("Неподдерживаемый формат файла");
                            }

                            throw new ApiException(errorResponse.Message);
                        }
                    }
                    catch (JsonException)
                    {
                        // Если не удалось распарсить JSON, используем текст как есть
                    }

                    throw new ApiException($"Сервер вернул ошибку ({response.StatusCode}): {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException($"Ошибка подключения к серверу. Убедитесь, что сервер запущен на {_baseUrl}. Детали: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new ApiException("Превышено время ожидания ответа от сервера. Возможно, файл слишком большой.", ex);
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApiException($"Ошибка загрузки файла: {ex.Message}", ex);
            }
        }
    }

    public class ApiException : Exception
    {
        public ApiException(string message) : base(message) { }
        public ApiException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class ErrorResponse
    {
        public string? Message { get; set; }
        public string? Error { get; set; }
        public int? StatusCode { get; set; }
    }

    public class AppSettings
    {
        public ApiSettingsConfig? ApiSettings { get; set; }
    }

    public class ApiSettingsConfig
    {
        public string? BaseUrl { get; set; }
    }
}
