using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GradingSystem.Services;

namespace GradingSystem.Pages
{
    public partial class AddStudentToGroupWindow : Window
    {
        private readonly GroupService _groupService;
        private readonly UserService _userService;
        private readonly int _groupId;
        private readonly string _groupName;
        private List<StudentViewModel> _allStudents;
        private List<int> _existingStudentIds;

        public bool IsStudentAdded { get; private set; }

        public AddStudentToGroupWindow(int groupId, string groupName, List<int> existingStudentIds)
        {
            InitializeComponent();
            _groupService = ServiceLocator.Instance.GroupService;
            _userService = ServiceLocator.Instance.UserService;
            _groupId = groupId;
            _groupName = groupName;
            _existingStudentIds = existingStudentIds ?? new List<int>();

            GroupNameText.Text = $"Добавление студента в группу \"{groupName}\"";
            Loaded += async (s, e) => await LoadAvailableStudents();
        }

        private async System.Threading.Tasks.Task LoadAvailableStudents()
        {
            try
            {
                var response = await _userService.GetUsersAsync(1, 100);

                if (response != null && response.Items != null && response.Items.Count > 0)
                {
                    // Фильтруем только студентов, которые еще не в группе
                    _allStudents = response.Items
                        .Where(u => u.Role.ToLower() == "student" && !_existingStudentIds.Contains(u.Id))
                        .Select(u => new StudentViewModel
                        {
                            Id = u.Id,
                            FullName = u.FullName,
                            Email = u.Email,
                            AvatarColor = GetRandomColor(),
                            Initials = GetInitials(u.FullName)
                        }).ToList();

                    StudentsList.ItemsSource = _allStudents;

                    if (_allStudents.Count == 0)
                    {
                        MessageBox.Show("Нет доступных студентов для добавления в группу.\n\nВозможные причины:\n- Все студенты уже в группе\n- В системе нет зарегистрированных студентов", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    _allStudents = new List<StudentViewModel>();
                    StudentsList.ItemsSource = _allStudents;
                    MessageBox.Show("В системе нет зарегистрированных студентов.\n\nСоздайте студентов через раздел 'Пользователи'.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки студентов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_allStudents == null) return;

            var searchText = SearchBox.Text.ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                StudentsList.ItemsSource = _allStudents;
            }
            else
            {
                var filtered = _allStudents.Where(s =>
                    s.FullName.ToLower().Contains(searchText) ||
                    s.Email.ToLower().Contains(searchText)
                ).ToList();
                StudentsList.ItemsSource = filtered;
            }
        }

        private async void StudentItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is int studentId)
            {
                var student = _allStudents.FirstOrDefault(s => s.Id == studentId);
                if (student != null)
                {
                    var result = MessageBox.Show(
                        $"Добавить студента '{student.FullName}' в группу \"{_groupName}\"?",
                        "Подтверждение",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question
                    );

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            var success = await _groupService.AddStudentToGroupAsync(_groupId, studentId);

                            if (success)
                            {
                                IsStudentAdded = true;
                                MessageBox.Show("Студент успешно добавлен в группу", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                                DialogResult = true;
                                Close();
                            }
                            else
                            {
                                MessageBox.Show("Не удалось добавить студента в группу.\n\nВозможные причины:\n- Студент уже в группе\n- Нет прав доступа\n- Проблема с сервером", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка добавления студента:\n\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
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
            return fullName.Length >= 2 ? fullName.Substring(0, 2).ToUpper() : fullName.ToUpper();
        }
    }
}
