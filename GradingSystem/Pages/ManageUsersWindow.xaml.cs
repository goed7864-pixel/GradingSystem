using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GradingSystem.Services;
using GradingSystem.DTOs;

namespace GradingSystem.Pages
{
    public partial class ManageUsersWindow : Window
    {
        private readonly UserService _userService;
        private List<UserViewModel> _allUsers;
        private string _currentRoleFilter = "all";

        public ManageUsersWindow()
        {
            InitializeComponent();
            _userService = ServiceLocator.Instance.UserService;
            Loaded += async (s, e) => await LoadUsers();
        }

        private async System.Threading.Tasks.Task LoadUsers()
        {
            try
            {
                var response = await _userService.GetUsersAsync(1, 1000);

                if (response != null && response.Items != null && response.Items.Count > 0)
                {
                    _allUsers = response.Items.Select(u => new UserViewModel
                    {
                        Id = u.Id,
                        FullName = u.FullName,
                        Email = u.Email,
                        Role = u.Role,
                        RoleText = u.Role.ToLower() == "teacher" ? "Преподаватель" : "Студент",
                        RoleColor = u.Role.ToLower() == "teacher" ? "#FF9800" : "#2196F3",
                        Initials = GetInitials(u.FullName),
                        AvatarColor = GetAvatarColor(u.FullName)
                    }).ToList();

                    ApplyFilters();
                }
                else
                {
                    _allUsers = new List<UserViewModel>();
                    UsersList.ItemsSource = _allUsers;
                    UsersCountText.Text = "0 пользователей";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilters()
        {
            if (_allUsers == null) return;

            var filtered = _allUsers.AsEnumerable();

            // Фильтр по роли
            if (_currentRoleFilter != "all")
            {
                filtered = filtered.Where(u => u.Role.ToLower() == _currentRoleFilter);
            }

            // Фильтр по поиску
            var searchText = SearchBox.Text.ToLower();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filtered = filtered.Where(u =>
                    u.FullName.ToLower().Contains(searchText) ||
                    u.Email.ToLower().Contains(searchText));
            }

            var filteredList = filtered.ToList();
            UsersList.ItemsSource = filteredList;
            UsersCountText.Text = $"{filteredList.Count} пользователей";
        }

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "?";

            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                return $"{parts[0][0]}{parts[1][0]}".ToUpper();
            }
            return parts[0][0].ToString().ToUpper();
        }

        private string GetAvatarColor(string name)
        {
            var colors = new[] { "#E91E63", "#9C27B0", "#673AB7", "#3F51B5", "#2196F3", "#009688", "#4CAF50", "#FF9800", "#FF5722" };
            var hash = Math.Abs(name.GetHashCode());
            return colors[hash % colors.Length];
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void RoleFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RoleFilterComboBox.SelectedItem is ComboBoxItem selected)
            {
                _currentRoleFilter = selected.Tag.ToString();
                ApplyFilters();
            }
        }

        private async void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            var addUserWindow = new AddEditUserWindow { Owner = this };
            if (addUserWindow.ShowDialog() == true && addUserWindow.IsUserSaved)
            {
                await LoadUsers();
            }
        }

        private async void EditUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int userId)
            {
                var user = _allUsers.FirstOrDefault(u => u.Id == userId);
                if (user != null)
                {
                    var editWindow = new AddEditUserWindow(userId, user.FullName, user.Email, user.Role) { Owner = this };
                    if (editWindow.ShowDialog() == true && editWindow.IsUserSaved)
                    {
                        await LoadUsers();
                    }
                }
            }
        }

        private async void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int userId)
            {
                var user = _allUsers.FirstOrDefault(u => u.Id == userId);
                if (user != null)
                {
                    var result = MessageBox.Show(
                        $"Вы уверены, что хотите удалить пользователя '{user.FullName}'?",
                        "Подтверждение",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            var success = await _userService.DeleteUserAsync(userId);
                            if (success)
                            {
                                MessageBox.Show("Пользователь успешно удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                                await LoadUsers();
                            }
                            else
                            {
                                MessageBox.Show("Не удалось удалить пользователя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка удаления пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }
    }

    public class UserViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string RoleText { get; set; }
        public string RoleColor { get; set; }
        public string Initials { get; set; }
        public string AvatarColor { get; set; }
    }
}
