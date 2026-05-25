using System;
using System.Net.Http;
using System.Windows;
using GradingSystem.DTOs;
using GradingSystem.Services;
using GradingSystem.Pages;

namespace GradingSystem
{
    public partial class login : Window
    {
        private readonly AuthService _authService;

        public login()
        {
            InitializeComponent();
            _authService = ServiceLocator.Instance.AuthService;
        }

        private async void TestConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            TestConnectionButton.IsEnabled = false;
            TestConnectionButton.Content = "Проверка...";

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

                // Если сервер хоть как-то ответил, значит подключение есть
                MessageBox.Show("Подключение есть",
                    "Тест подключения", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"❌ Ошибка подключения:\n{ex.Message}\n\nУбедитесь что:\n1. API запущен (dotnet run)\n2. Используется порт 7073\n3. Swagger доступен: http://localhost:7073/swagger",
                    "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Неизвестная ошибка:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                TestConnectionButton.IsEnabled = true;
                TestConnectionButton.Content = "Проверить подключение";
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorPanel.Visibility = Visibility.Collapsed;

            var email = EmailBox.Text.Trim();
            var password = PasswordBox.Password;

            if (string.IsNullOrEmpty(email))
            {
                ShowError("Введите email");
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError("Введите пароль");
                return;
            }

            LoginButton.IsEnabled = false;
            LoginButton.Content = "ВХОД...";

            try
            {
                var loginDto = new LoginDto
                {
                    Email = email,
                    Password = password
                };

                var response = await _authService.LoginAsync(loginDto);

                if (response != null && !string.IsNullOrEmpty(response.Token))
                {
                    // Получаем роль пользователя из токена
                    var tokenService = ServiceLocator.Instance.TokenService;
                    var userRole = tokenService.GetCurrentUserRole();

                    // Проверяем роль и перенаправляем на соответствующий экран
                    if (userRole == "teacher")
                    {
                        var mainWindow = new MainWindow();
                        mainWindow.Show();
                        this.Close();
                    }
                    else if (userRole == "student")
                    {
                        // Для студента также открываем главное окно
                        // HomePage адаптируется под роль автоматически
                        var mainWindow = new MainWindow();
                        mainWindow.Show();
                        this.Close();
                    }
                    else
                    {
                        ShowError("Неизвестная роль пользователя");
                    }
                }
                else
                {
                    ShowError("Неверный email или пароль");
                }
            }
            catch (ApiException ex)
            {
                ShowError($"Ошибка подключения к серверу");
            }
            catch (Exception ex)
            {
                ShowError($"Неизвестная ошибка");
            }
            finally
            {
                LoginButton.IsEnabled = true;
                LoginButton.Content = "ВОЙТИ";
            }
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorPanel.Visibility = Visibility.Visible;
        }
    }
}
