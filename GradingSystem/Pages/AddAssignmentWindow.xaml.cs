using System;
using System.Windows;
using GradingSystem.DTOs;
using GradingSystem.Services;

namespace GradingSystem.Pages
{
    public partial class AddAssignmentWindow : Window
    {
        private readonly AssignmentService _assignmentService;
        private readonly int _courseId;
        public bool IsAssignmentCreated { get; private set; }

        public AddAssignmentWindow(int courseId)
        {
            InitializeComponent();
            _assignmentService = ServiceLocator.Instance.AssignmentService;
            _courseId = courseId;
            DeadlinePicker.SelectedDate = DateTime.Now.AddDays(7);
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Введите название задания", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TitleTextBox.Text.Trim().Length < 3)
            {
                MessageBox.Show("Название задания должно содержать минимум 3 символа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(MaxPointsTextBox.Text) || !int.TryParse(MaxPointsTextBox.Text, out int points) || points <= 0)
            {
                MessageBox.Show("Введите корректный максимальный балл", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!DeadlinePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите дедлайн", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DeadlinePicker.SelectedDate.Value.Date < DateTime.Now.Date)
            {
                MessageBox.Show("Дедлайн не может быть в прошлом. Выберите сегодняшнюю или будущую дату.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var createDto = new AssignmentCreateDto
                {
                    CourseId = _courseId,
                    Title = TitleTextBox.Text.Trim(),
                    Description = DescriptionTextBox.Text.Trim(),
                    MaxScore = points,
                    Deadline = DeadlinePicker.SelectedDate.Value
                };

                System.Diagnostics.Debug.WriteLine($"=== Creating assignment ===");
                System.Diagnostics.Debug.WriteLine($"CourseId: {createDto.CourseId}");
                System.Diagnostics.Debug.WriteLine($"Title: '{createDto.Title}' (length: {createDto.Title.Length})");
                System.Diagnostics.Debug.WriteLine($"Description: '{createDto.Description}'");
                System.Diagnostics.Debug.WriteLine($"MaxScore: {createDto.MaxScore}");
                System.Diagnostics.Debug.WriteLine($"Deadline: {createDto.Deadline:yyyy-MM-ddTHH:mm:ss}");
                System.Diagnostics.Debug.WriteLine($"Deadline (UTC): {createDto.Deadline.ToUniversalTime():yyyy-MM-ddTHH:mm:ss}");
                System.Diagnostics.Debug.WriteLine($"Deadline Kind: {createDto.Deadline.Kind}");

                var json = System.Text.Json.JsonSerializer.Serialize(createDto);
                System.Diagnostics.Debug.WriteLine($"JSON to send: {json}");

                await _assignmentService.CreateAssignmentAsync(createDto);
                IsAssignmentCreated = true;
                MessageBox.Show("Задание успешно создано", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Services.ApiException ex) when (ex.Message.Contains("409") || ex.Message.Contains("Conflict"))
            {
                System.Diagnostics.Debug.WriteLine($"409 Conflict error: {ex.Message}");

                // Пытаемся извлечь читаемое сообщение из ответа сервера
                var errorMessage = "Конфликт данных при создании задания.";

                if (ex.Message.Contains("message"))
                {
                    try
                    {
                        // Извлекаем JSON из сообщения об ошибке
                        var jsonStart = ex.Message.IndexOf("{");
                        if (jsonStart >= 0)
                        {
                            var jsonPart = ex.Message.Substring(jsonStart);
                            var jsonEnd = jsonPart.LastIndexOf("}") + 1;
                            if (jsonEnd > 0)
                            {
                                jsonPart = jsonPart.Substring(0, jsonEnd);
                                var errorResponse = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonPart);

                                if (errorResponse.TryGetProperty("message", out var messageProperty))
                                {
                                    errorMessage = messageProperty.GetString() ?? errorMessage;
                                }

                                if (errorResponse.TryGetProperty("details", out var detailsProperty))
                                {
                                    var details = detailsProperty.GetString();
                                    if (!string.IsNullOrEmpty(details))
                                    {
                                        errorMessage += $"\n\nДетали: {details}";
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Если не удалось распарсить, используем общее сообщение
                    }
                }

                MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Services.ApiException ex)
            {
                System.Diagnostics.Debug.WriteLine($"API error: {ex.Message}");
                MessageBox.Show($"Ошибка API: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating assignment: {ex}");
                MessageBox.Show($"Ошибка создания задания: {ex.Message}\n\nПроверьте, что все поля заполнены корректно.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
