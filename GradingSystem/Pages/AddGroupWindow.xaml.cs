using System;
using System.Windows;
using GradingSystem.DTOs;
using GradingSystem.Services;

namespace GradingSystem.Pages
{
    public partial class AddGroupWindow : Window
    {
        private readonly GroupService _groupService;
        public bool IsGroupCreated { get; private set; }

        public AddGroupWindow()
        {
            InitializeComponent();
            _groupService = ServiceLocator.Instance.GroupService;
            GroupNameTextBox.Focus();
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
                SaveButton.Content = "Создание...";

                var createDto = new StudentGroupCreateDto
                {
                    Name = groupName
                };

                var response = await _groupService.CreateGroupAsync(createDto);

                if (response != null && response.Group != null)
                {
                    IsGroupCreated = true;
                    MessageBox.Show($"Группа '{response.Group.Name}' успешно создана", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Не удалось создать группу. Попробуйте еще раз.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании группы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SaveButton.IsEnabled = true;
                SaveButton.Content = "Создать";
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
