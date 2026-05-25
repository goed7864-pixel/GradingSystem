using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GradingSystem.Services;

namespace GradingSystem.Pages
{
    public partial class GroupsPage : Page
    {
        private readonly GroupService _groupService;
        private List<GroupViewModel> _allGroups;
        private List<StudentViewModel> _allStudents;
        private int _selectedGroupId;
        private string _selectedGroupName;

        public GroupsPage()
        {
            InitializeComponent();
            _groupService = ServiceLocator.Instance.GroupService;
            Loaded += async (s, e) => await LoadGroups();
        }

        private async System.Threading.Tasks.Task LoadGroups()
        {
            try
            {
                var response = await _groupService.GetGroupsWithCountAsync(1, 100);

                if (response != null && response.Items != null && response.Items.Count > 0)
                {
                    _allGroups = response.Items.Select(g => new GroupViewModel
                    {
                        Id = g.Id,
                        Name = g.Name,
                        StudentCount = g.StudentCount ?? 0
                    }).ToList();

                    GroupsList.ItemsSource = _allGroups;
                    GroupsCountText.Text = $"{_allGroups.Count} групп";
                }
                else
                {
                    _allGroups = new List<GroupViewModel>();
                    GroupsList.ItemsSource = _allGroups;
                    GroupsCountText.Text = "0 групп";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки групп: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_allGroups == null) return;

            var searchText = SearchBox.Text.ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                GroupsList.ItemsSource = _allGroups;
            }
            else
            {
                var filtered = _allGroups.Where(g => g.Name.ToLower().Contains(searchText)).ToList();
                GroupsList.ItemsSource = filtered;
            }
        }

        private void GroupItem_Click(object sender, MouseButtonEventArgs e)
        {
            int groupId = 0;

            // Проверяем, откуда пришел клик - от Border или от StackPanel
            if (sender is Border border && border.Tag is int borderId)
            {
                groupId = borderId;
            }
            else if (sender is StackPanel stackPanel && stackPanel.Tag is int stackPanelId)
            {
                groupId = stackPanelId;
            }

            if (groupId > 0)
            {
                _selectedGroupId = groupId;
                var group = _allGroups.FirstOrDefault(g => g.Id == groupId);

                if (group != null)
                {
                    _selectedGroupName = group.Name;
                    SelectedGroupName.Text = group.Name;
                    SelectedGroupInfo.Text = $"{group.StudentCount} студентов";
                    AddStudentButton.Visibility = Visibility.Visible;
                    EnrollGroupButton.Visibility = Visibility.Visible;
                    StudentSearchPanel.Visibility = Visibility.Visible;

                    LoadStudentsForGroupAsync(groupId);
                }
            }
        }

        private async System.Threading.Tasks.Task LoadStudentsForGroupAsync(int groupId)
        {
            try
            {
                var response = await _groupService.GetGroupStudentsAsync(groupId, 1, 100);

                if (response != null && response.Items != null && response.Items.Count > 0)
                {
                    _allStudents = response.Items.Select(s => new StudentViewModel
                    {
                        Id = s.Id,
                        FullName = s.FullName,
                        Email = s.Email,
                        Status = "Активен",
                        StatusColor = "#4CAF50",
                        AvatarColor = GetRandomColor(),
                        Initials = GetInitials(s.FullName)
                    }).ToList();
                }
                else
                {
                    _allStudents = new List<StudentViewModel>();
                }

                UpdateStudentsDisplay();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки студентов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                _allStudents = new List<StudentViewModel>();
                UpdateStudentsDisplay();
            }
        }

        private void UpdateStudentsDisplay()
        {
            if (_allStudents != null && _allStudents.Count > 0)
            {
                StudentsList.ItemsSource = _allStudents;
                StudentsList.Visibility = Visibility.Visible;
                NoStudentsPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                StudentsList.Visibility = Visibility.Collapsed;
                NoStudentsPanel.Visibility = Visibility.Visible;
            }
        }

        private void StudentSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_allStudents == null) return;

            var searchText = StudentSearchBox.Text.ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                StudentsList.ItemsSource = _allStudents;
            }
            else
            {
                var filtered = _allStudents.Where(s => s.FullName.ToLower().Contains(searchText) || s.Email.ToLower().Contains(searchText)).ToList();
                StudentsList.ItemsSource = filtered;
            }
        }

        private string GetRandomColor()
        {
            var colors = new[] { "#5ED0BA", "#5677D8", "#FF9800", "#D91842", "#9C27B0", "#3F51B5" };
            return colors[new Random().Next(colors.Length)];
        }

        private string GetInitials(string fullName)
        {
            var parts = fullName.Split(' ');
            if (parts.Length >= 2)
            {
                return $"{parts[0][0]}{parts[1][0]}";
            }
            return fullName.Substring(0, 2).ToUpper();
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RefreshButton.IsEnabled = false;
                await LoadGroups();
                MessageBox.Show("Данные обновлены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                RefreshButton.IsEnabled = true;
            }
        }

        private void ManageUsersButton_Click(object sender, RoutedEventArgs e)
        {
            var manageUsersWindow = new ManageUsersWindow { Owner = Window.GetWindow(this) };
            manageUsersWindow.ShowDialog();
        }

        private async void AddGroupButton_Click(object sender, RoutedEventArgs e)
        {
            var addGroupWindow = new AddGroupWindow { Owner = Window.GetWindow(this) };
            if (addGroupWindow.ShowDialog() == true && addGroupWindow.IsGroupCreated)
            {
                await LoadGroups();
            }
        }

        private async void EditGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int groupId)
            {
                var group = _allGroups.FirstOrDefault(g => g.Id == groupId);
                if (group != null)
                {
                    var editGroupWindow = new EditGroupWindow(groupId, group.Name) { Owner = Window.GetWindow(this) };
                    if (editGroupWindow.ShowDialog() == true && editGroupWindow.IsGroupUpdated)
                    {
                        await LoadGroups();

                        // Если редактируемая группа была выбрана, обновляем её название
                        if (_selectedGroupId == groupId)
                        {
                            var updatedGroup = _allGroups.FirstOrDefault(g => g.Id == groupId);
                            if (updatedGroup != null)
                            {
                                SelectedGroupName.Text = updatedGroup.Name;
                                _selectedGroupName = updatedGroup.Name;
                            }
                        }
                    }
                }
            }
        }

        private async void DeleteGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int groupId)
            {
                var group = _allGroups.FirstOrDefault(g => g.Id == groupId);
                if (group != null)
                {
                    var result = MessageBox.Show(
                        $"Вы уверены, что хотите удалить группу '{group.Name}'?\n\nВсе студенты будут удалены из этой группы.",
                        "Подтверждение",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            var success = await _groupService.DeleteGroupAsync(groupId);
                            if (success)
                            {
                                MessageBox.Show("Группа успешно удалена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                                await LoadGroups();

                                // Если удаленная группа была выбрана, сбрасываем выбор
                                if (_selectedGroupId == groupId)
                                {
                                    _selectedGroupId = 0;
                                    _selectedGroupName = "";
                                    SelectedGroupName.Text = "Выберите группу";
                                    SelectedGroupInfo.Text = "Нажмите на группу слева";
                                    AddStudentButton.Visibility = Visibility.Collapsed;
                                    StudentSearchPanel.Visibility = Visibility.Collapsed;
                                    StudentsList.ItemsSource = null;
                                }
                            }
                            else
                            {
                                MessageBox.Show("Не удалось удалить группу", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка удаления группы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        private async void AddStudentButton_Click(object sender, RoutedEventArgs e)
        {
            var existingStudentIds = _allStudents?.Select(s => s.Id).ToList() ?? new List<int>();
            var addStudentWindow = new AddStudentToGroupWindow(_selectedGroupId, _selectedGroupName, existingStudentIds)
            {
                Owner = Window.GetWindow(this)
            };

            if (addStudentWindow.ShowDialog() == true && addStudentWindow.IsStudentAdded)
            {
                // Перезагружаем список студентов группы
                await LoadStudentsForGroupAsync(_selectedGroupId);

                // Обновляем счетчик студентов в группе
                var group = _allGroups.FirstOrDefault(g => g.Id == _selectedGroupId);
                if (group != null)
                {
                    group.StudentCount = _allStudents?.Count ?? 0;
                    SelectedGroupInfo.Text = $"{group.StudentCount} студентов";
                    GroupsList.ItemsSource = null;
                    GroupsList.ItemsSource = _allGroups;
                }
            }
        }

        private async void RemoveStudentButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int studentId)
            {
                var student = _allStudents.FirstOrDefault(s => s.Id == studentId);
                if (student != null)
                {
                    var result = MessageBox.Show($"Вы уверены, что хотите удалить студента '{student.FullName}' из группы?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            var success = await _groupService.RemoveStudentFromGroupAsync(_selectedGroupId, studentId);

                            if (success)
                            {
                                _allStudents.Remove(student);
                                UpdateStudentsDisplay();

                                var group = _allGroups.FirstOrDefault(g => g.Id == _selectedGroupId);
                                if (group != null)
                                {
                                    group.StudentCount--;
                                    SelectedGroupInfo.Text = $"{group.StudentCount} студентов";
                                    GroupsList.ItemsSource = null;
                                    GroupsList.ItemsSource = _allGroups;
                                }

                                MessageBox.Show("Студент удален из группы", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show("Не удалось удалить студента из группы", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка удаления студента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        private void EnrollGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedGroupId > 0)
            {
                var enrollWindow = new EnrollGroupToCourseWindow(_selectedGroupId, _selectedGroupName)
                {
                    Owner = Window.GetWindow(this)
                };

                if (enrollWindow.ShowDialog() == true)
                {
                    MessageBox.Show("Группа успешно записана на курс", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }
}