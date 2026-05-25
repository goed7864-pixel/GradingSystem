using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GradingSystem.DTOs;
using GradingSystem.Services;

namespace GradingSystem.Pages
{
    public partial class HomePage : Page
    {
        private readonly GroupService _groupService;
        private readonly CourseService _courseService;
        private readonly AssignmentService _assignmentService;
        private readonly DashboardService _dashboardService;
        private readonly TokenService _tokenService;
        private List<GroupViewModel> _allGroups;
        private string _currentUserRole;

        public HomePage()
        {
            InitializeComponent();
            _groupService = ServiceLocator.Instance.GroupService;
            _courseService = ServiceLocator.Instance.CourseService;
            _assignmentService = ServiceLocator.Instance.AssignmentService;
            _dashboardService = ServiceLocator.Instance.DashboardService;
            _tokenService = ServiceLocator.Instance.TokenService;
            Loaded += async (s, e) => await LoadPageContent();
        }

        private async System.Threading.Tasks.Task LoadPageContent()
        {
            try
            {
                _currentUserRole = _tokenService.GetCurrentUserRole();
                var userName = _tokenService.GetCurrentUserName();

                // Отладка: показываем все claims из токена
                var allClaims = _tokenService.GetAllClaims();
                System.Diagnostics.Debug.WriteLine("=== JWT Claims ===");
                System.Diagnostics.Debug.WriteLine(allClaims);
                System.Diagnostics.Debug.WriteLine($"Extracted UserName: {userName}");

                // Обновляем приветствие
                if (WelcomeText != null)
                    WelcomeText.Text = $"Добро пожаловать, {userName}!";

                // Обновляем дату
                if (DateText != null)
                {
                    var culture = new CultureInfo("ru-RU");
                    DateText.Text = DateTime.Now.ToString("dddd, d MMMM yyyy", culture);
                }

                // Адаптируем интерфейс под роль
                AdaptUIForRole();

                // Загружаем данные
                await LoadDashboardData();
                await LoadGroups();
                await LoadUpcomingDeadlines();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AdaptUIForRole()
        {
            if (_currentUserRole == "student")
            {
                // Для студентов скрываем управление группами
                if (MyGroupsPanel != null)
                    MyGroupsPanel.Visibility = Visibility.Collapsed;

                // Меняем заголовки карточек статистики
                if (StudentsCardTitle != null)
                    StudentsCardTitle.Text = "Курсов";

                if (AssignmentsCardTitle != null)
                    AssignmentsCardTitle.Text = "Заданий";
            }
            else if (_currentUserRole == "teacher")
            {
                // Для преподавателей показываем все панели
                if (MyGroupsPanel != null)
                    MyGroupsPanel.Visibility = Visibility.Visible;
            }
        }

        private async System.Threading.Tasks.Task LoadDashboardData()
        {
            try
            {
                if (_currentUserRole == "teacher")
                {
                    var dashboard = await _dashboardService.GetTeacherDashboardAsync();

                    if (dashboard != null)
                    {
                        // Обновляем статистику - используем TotalCourses как количество групп
                        if (TotalGroupsText != null)
                            TotalGroupsText.Text = dashboard.TotalCourses.ToString();

                        if (TotalStudentsText != null)
                            TotalStudentsText.Text = dashboard.TotalStudents.ToString();

                        if (TotalAssignmentsText != null)
                            TotalAssignmentsText.Text = dashboard.TotalAssignments.ToString();

                        // Загружаем активность
                        LoadRecentActivity(dashboard.RecentActivities);

                        // Если API вернул 0 студентов, пробуем загрузить вручную
                        if (dashboard.TotalStudents == 0)
                        {
                            await LoadTeacherStatistics();
                        }
                    }
                    else
                    {
                        // Если API не вернул данные, загружаем статистику вручную
                        await LoadTeacherStatistics();
                    }
                }
                else if (_currentUserRole == "student")
                {
                    var dashboard = await _dashboardService.GetStudentDashboardAsync();

                    if (dashboard != null)
                    {
                        // Обновляем статистику для студента
                        if (TotalGroupsText != null)
                            TotalGroupsText.Text = "1"; // Группа студента

                        if (TotalStudentsText != null)
                            TotalStudentsText.Text = dashboard.EnrolledCourses.ToString();

                        if (TotalAssignmentsText != null)
                            TotalAssignmentsText.Text = dashboard.TotalAssignments.ToString();

                        // Загружаем активность
                        LoadRecentActivity(dashboard.RecentActivities);
                    }
                    else
                    {
                        await LoadStudentStatistics();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                // Загружаем базовую статистику при ошибке
                if (_currentUserRole == "teacher")
                    await LoadTeacherStatistics();
                else
                    await LoadStudentStatistics();
            }
        }

        private async System.Threading.Tasks.Task LoadTeacherStatistics()
        {
            try
            {
                // Загружаем группы
                var groupsResponse = await _groupService.GetGroupsWithCountAsync(1, 100);
                if (groupsResponse != null && TotalGroupsText != null)
                    TotalGroupsText.Text = groupsResponse.TotalCount.ToString();

                // Загружаем задания
                var assignmentsResponse = await _assignmentService.GetAssignmentsAsync(1, 100);
                if (assignmentsResponse != null && TotalAssignmentsText != null)
                    TotalAssignmentsText.Text = assignmentsResponse.TotalCount.ToString();

                // Подсчитываем общее количество студентов из всех групп
                if (groupsResponse != null && groupsResponse.Items != null && TotalStudentsText != null)
                {
                    var totalStudents = groupsResponse.Items.Sum(g => g.StudentCount ?? 0);
                    TotalStudentsText.Text = totalStudents.ToString();
                }

                LoadRecentActivity(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task LoadStudentStatistics()
        {
            try
            {
                // Для студента загружаем его курсы и задания
                var coursesResponse = await _courseService.GetCoursesAsync(1, 100);
                if (coursesResponse != null && TotalStudentsText != null)
                    TotalStudentsText.Text = coursesResponse.TotalCount.ToString();

                var assignmentsResponse = await _assignmentService.GetAssignmentsAsync(1, 100);
                if (assignmentsResponse != null && TotalAssignmentsText != null)
                    TotalAssignmentsText.Text = assignmentsResponse.TotalCount.ToString();

                if (TotalGroupsText != null)
                    TotalGroupsText.Text = "1";

                LoadRecentActivity(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            _ = LoadPageContent();
        }

        private void CreateAssignmentButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция создания задания в разработке", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ViewSubmissionsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция просмотра работ в разработке", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void AddGroupButton_Click(object sender, RoutedEventArgs e)
        {
            var addGroupWindow = new AddGroupWindow { Owner = Window.GetWindow(this) };
            if (addGroupWindow.ShowDialog() == true && addGroupWindow.IsGroupCreated)
            {
                await LoadGroups();
            }
        }

        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
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

        private async void EditGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int groupId)
            {
                var group = _allGroups.FirstOrDefault(g => g.Id == groupId);
                if (group != null)
                {
                    var editWindow = new EditGroupWindow(groupId, group.Name) { Owner = Window.GetWindow(this) };
                    if (editWindow.ShowDialog() == true)
                    {
                        await LoadGroups();
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
                    var result = MessageBox.Show($"Вы уверены, что хотите удалить группу '{group.Name}'?",
                        "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            await _groupService.DeleteGroupAsync(groupId);
                            await LoadGroups();
                            MessageBox.Show("Группа успешно удалена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка удаления группы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        private void LoadRecentActivity(List<RecentActivityItem> activities)
        {
            try
            {
                var activityList = new List<ActivityViewModel>();

                if (activities != null && activities.Count > 0)
                {
                    // Используем данные из API
                    activityList = activities.Select(a => new ActivityViewModel
                    {
                        Icon = GetIconForActivityType(a.Type),
                        IconBackground = GetBackgroundForActivityType(a.Type),
                        Title = a.Type,
                        Description = a.Description,
                        Time = GetRelativeTime(a.Timestamp)
                    }).ToList();
                }
                else
                {
                    // Заглушка, если API не вернул данные
                    if (_currentUserRole == "teacher")
                    {
                        activityList = new List<ActivityViewModel>
                        {
                            new ActivityViewModel
                            {
                                Icon = "✅",
                                IconBackground = "#E8F5E9",
                                Title = "Работа проверена",
                                Description = "Нет новых проверенных работ",
                                Time = "Сегодня"
                            }
                        };
                    }
                    else
                    {
                        activityList = new List<ActivityViewModel>
                        {
                            new ActivityViewModel
                            {
                                Icon = "📝",
                                IconBackground = "#FFF3E0",
                                Title = "Задания",
                                Description = "Нет новых заданий",
                                Time = "Сегодня"
                            }
                        };
                    }
                }

                RecentActivityList.ItemsSource = activityList;

                if (NoActivityMessage != null)
                    NoActivityMessage.Visibility = activityList.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки активности: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetIconForActivityType(string type)
        {
            return type.ToLower() switch
            {
                "grade" => "✅",
                "assignment" => "📝",
                "submission" => "📋",
                "comment" => "💬",
                _ => "📌"
            };
        }

        private string GetBackgroundForActivityType(string type)
        {
            return type.ToLower() switch
            {
                "grade" => "#E8F5E9",
                "assignment" => "#FFF3E0",
                "submission" => "#E3F2FD",
                "comment" => "#F3E5F5",
                _ => "#EEEEEE"
            };
        }

        private string GetRelativeTime(DateTime timestamp)
        {
            var diff = DateTime.Now - timestamp;

            if (diff.TotalMinutes < 1)
                return "Только что";
            if (diff.TotalMinutes < 60)
                return $"{(int)diff.TotalMinutes} мин назад";
            if (diff.TotalHours < 24)
                return $"{(int)diff.TotalHours} ч назад";
            if (diff.TotalDays < 7)
                return $"{(int)diff.TotalDays} дн назад";

            return timestamp.ToString("dd.MM.yyyy");
        }

        private async System.Threading.Tasks.Task LoadUpcomingDeadlines()
        {
            try
            {
                // Загружаем задания с ближайшими дедлайнами
                var assignmentsResponse = await _assignmentService.GetAssignmentsAsync(1, 10);

                if (assignmentsResponse != null && assignmentsResponse.Items != null && assignmentsResponse.Items.Count > 0)
                {
                    var deadlines = assignmentsResponse.Items
                        .Where(a => a.Deadline > DateTime.Now)
                        .OrderBy(a => a.Deadline)
                        .Take(3)
                        .Select(a => new DeadlineViewModel
                        {
                            Title = a.Title,
                            Date = $"📅 {a.Deadline:dd MMMM yyyy, HH:mm}"
                        }).ToList();

                    DeadlinesList.ItemsSource = deadlines;

                    if (NoDeadlinesMessage != null)
                        NoDeadlinesMessage.Visibility = deadlines.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    DeadlinesList.ItemsSource = new List<DeadlineViewModel>();
                    if (NoDeadlinesMessage != null)
                        NoDeadlinesMessage.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки дедлайнов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

                    if (NoGroupsMessage != null)
                        NoGroupsMessage.Visibility = Visibility.Collapsed;
                }
                else
                {
                    _allGroups = new List<GroupViewModel>();
                    GroupsList.ItemsSource = _allGroups;

                    if (NoGroupsMessage != null)
                    {
                        NoGroupsMessage.Visibility = Visibility.Visible;
                        NoGroupsMessage.Text = "У вас пока нет групп";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки групп: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}