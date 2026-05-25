using System;
using System.Windows;
using GradingSystem.DTOs;
using GradingSystem.Services;

namespace GradingSystem.Pages
{
    public partial class EditGroupWindow : Window
    {
        private readonly GroupService _groupService;
        private readonly int _groupId;
        public bool IsGroupUpdated { get; private set; }

        public EditGroupWindow(int groupId, string currentName)
        {
            InitializeComponent();
            _groupService = ServiceLocator.Instance.GroupService;
            _groupId = groupId;
            GroupNameTextBox.Text = currentName;
            GroupNameTextBox.Focus();
            GroupNameTextBox.SelectAll();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var groupName = GroupNameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(groupName))
            {
                MessageBox.Show("Пожалуйста, введите название группы", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                GroupNameTextBox.Focus();
                return;
            }

            if (groupName.Length < 2)
            {
                MessageBox.Show("Название группы должно содержать минимум 2 символа", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                GroupNameTextBox.Focus();
                return;
            }

            try
            {
                SaveButton.IsEnabled = false;
                SaveButton.Content = "Сохранение...";

                var updateDto = new StudentGroupUpdateDto
                {
                    Name = groupName
                };

                var response = await _groupService.UpdateGroupAsync(_groupId, updateDto);

                if (response != null && response.Group != null)
                {
                    IsGroupUpdated = true;
                    MessageBox.Show($"Группа '{response.Group.Name}' успешно обновлена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Не удалось обновить группу. Попробуйте еще раз.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении группы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SaveButton.IsEnabled = true;
                SaveButton.Content = "Сохранить";
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
