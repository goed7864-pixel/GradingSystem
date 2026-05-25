using System;
using System.Windows;
using GradingSystem.DTOs;
using GradingSystem.Services;

namespace GradingSystem.Pages
{
    public partial class AddCourseWindow : Window
    {
        private readonly CourseService _courseService;
        private readonly TokenService _tokenService;
        public bool IsCourseCreated { get; private set; }

        public AddCourseWindow()
        {
            InitializeComponent();
            _courseService = ServiceLocator.Instance.CourseService;
            _tokenService = ServiceLocator.Instance.TokenService;
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            var courseName = CourseNameTextBox.Text.Trim();
            var description = DescriptionTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(courseName))
            {
                MessageBox.Show("Пожалуйста, введите название курса", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var teacherId = _tokenService.GetCurrentUserId();
                if (!teacherId.HasValue)
                {
                    MessageBox.Show("Не удалось определить ID преподавателя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var createDto = new CourseCreateDto
                {
                    Name = courseName,
                    Description = description,
                    TeacherId = teacherId.Value
                };

                await _courseService.CreateCourseAsync(createDto);
                IsCourseCreated = true;
                MessageBox.Show("Курс успешно создан", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания курса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
