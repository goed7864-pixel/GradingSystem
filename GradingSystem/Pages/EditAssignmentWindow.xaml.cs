using System;
using System.Windows;
using GradingSystem.DTOs;
using GradingSystem.Services;

namespace GradingSystem.Pages
{
    public partial class EditAssignmentWindow : Window
    {
        private readonly AssignmentService _assignmentService;
        private readonly int _assignmentId;
        public bool IsAssignmentUpdated { get; private set; }

        public EditAssignmentWindow(int assignmentId)
        {
            InitializeComponent();
            _assignmentService = ServiceLocator.Instance.AssignmentService;
            _assignmentId = assignmentId;
            Loaded += async (s, e) => await LoadAssignment();
        }

        private async System.Threading.Tasks.Task LoadAssignment()
        {
            try
            {
                var assignment = await _assignmentService.GetAssignmentByIdAsync(_assignmentId);
                if (assignment != null)
                {
                    TitleTextBox.Text = assignment.Title;
                    DescriptionTextBox.Text = assignment.Description ?? "";
                    MaxPointsTextBox.Text = assignment.MaxScore.ToString();
                    DeadlinePicker.SelectedDate = assignment.Deadline;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки задания: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Введите название задания", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
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

            try
            {
                var updateDto = new AssignmentUpdateDto
                {
                    Title = TitleTextBox.Text.Trim(),
                    Description = DescriptionTextBox.Text.Trim(),
                    MaxScore = points,
                    Deadline = DeadlinePicker.SelectedDate.Value
                };

                await _assignmentService.UpdateAssignmentAsync(_assignmentId, updateDto);
                IsAssignmentUpdated = true;
                MessageBox.Show("Задание успешно обновлено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления задания: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
