using System;
using System.Net.Http;
using System.Windows;

namespace GradingSystem.Pages
{
    public partial class login : Window
    {
        // Добавьте этот метод для тестирования подключения
        private async void TestConnection()
        {
            try
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };

                using var client = new HttpClient(handler)
                {
                    BaseAddress = new Uri("http://localhost:7073"),
                    Timeout = TimeSpan.FromSeconds(10)
                };

                var response = await client.GetAsync("/api/groups");

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show(" Подключение к серверу успешно!", "Тест подключения");
                }
                else
                {
                    MessageBox.Show($" Сервер ответил с кодом: {response.StatusCode}", "Тест подключения");
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($" Ошибка подключения:\n{ex.Message}\n\nУбедитесь что:\n1. API запущен\n2. Используется порт 7073\n3. Swagger доступен: http://localhost:7073/swagger", "Ошибка");
            }
            catch (Exception ex)
            {
                MessageBox.Show($" Неизвестная ошибка:\n{ex.Message}", "Ошибка");
            }
        }
    }
}
