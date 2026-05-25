using System;
using System.Windows;
using GradingSystem.DTOs;
using GradingSystem.Services;

namespace GradingSystem.Pages
{
    public partial class EditCourseWindow : Window
    {
        private readonly CourseService _courseService;
        private readonly int _courseId;
        public bool IsCourseUpdated { get; private set; }

        public EditCourseWindow(int courseId, string courseName, string description = "")
        {
            InitializeComponent();
            _courseService = ServiceLocator.Instance.CourseService;
            _courseId = courseId;

            CourseNameTextBox.Text = courseName;
            DescriptionTextBox.Text = description;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
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
                var updateDto = new CourseUpdateDto
                {
                    Name = courseName,
                    Description = description
                };

                await _courseService.UpdateCourseAsync(_courseId, updateDto);
                IsCourseUpdated = true;
                MessageBox.Show("Курс успешно обновлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления курса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
