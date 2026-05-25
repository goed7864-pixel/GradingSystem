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
    public partial class StudentHomePage : Page
    {
        private readonly DashboardService _dashboardService;
        private readonly AssignmentService _assignmentService;
        private readonly CourseService _courseService;
        private readonly TokenService _tokenService;

        public StudentHomePage()
        {
            InitializeComponent();
            _dashboardService = ServiceLocator.Instance.DashboardService;
            _assignmentService = ServiceLocator.Instance.AssignmentService;
            _courseService = ServiceLocator.Instance.CourseService;
            _tokenService = ServiceLocator.Instance.TokenService;
            Loaded += async (s, e) => await LoadPageContent();
        }

        private async System.Threading.Tasks.Task LoadPageContent()
        {
            try
            {
                var userName = _tokenService.GetCurrentUserName();

                // Обновляем приветствие
                if (WelcomeText != null)
                    WelcomeText.Text = $"Добро пожаловать, {userName}!";

                // Обновляем дату
                if (DateText != null)
                {
                    var culture = new CultureInfo("ru-RU");
                    DateText.Text = DateTime.Now.ToString("dddd, d MMMM yyyy", culture);
                }

                // Загружаем данные
                await LoadDashboardData();
                await LoadUpcomingDeadlines();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task LoadDashboardData()
        {
            try
            {
                var dashboard = await _dashboardService.GetStudentDashboardAsync();

                if (dashboard != null)
                {
                    // Обновляем статистику
                    if (TotalCoursesText != null)
                        TotalCoursesText.Text = dashboard.EnrolledCourses.ToString();

                    if (PendingAssignmentsText != null)
                        PendingAssignmentsText.Text = dashboard.PendingAssignments.ToString();

                    if (CompletedAssignmentsText != null)
                        CompletedAssignmentsText.Text = dashboard.CompletedAssignments.ToString();

                    if (AverageGradeText != null)
                        AverageGradeText.Text = dashboard.AverageGrade.ToString("F1");

                    // Загружаем активность
                    LoadRecentActivity(dashboard.RecentActivities);
                }
                else
                {
                    // Если API не вернул данные, загружаем базовую статистику
                    await LoadBasicStatistics();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                await LoadBasicStatistics();
            }
        }

        private async System.Threading.Tasks.Task LoadBasicStatistics()
        {
            try
            {
                // Загружаем курсы
                var coursesResponse = await _courseService.GetCoursesAsync(1, 100);
                if (coursesResponse != null && TotalCoursesText != null)
                    TotalCoursesText.Text = coursesResponse.TotalCount.ToString();

                // Загружаем задания
                var assignmentsResponse = await _assignmentService.GetAssignmentsAsync(1, 100);
                if (assignmentsResponse != null && assignmentsResponse.Items != null)
                {
                    var now = DateTime.Now;
                    var pendingCount = assignmentsResponse.Items.Count(a => a.Deadline > now);
                    var completedCount = assignmentsResponse.Items.Count(a => a.Deadline <= now);

                    if (PendingAssignmentsText != null)
                        PendingAssignmentsText.Text = pendingCount.ToString();

                    if (CompletedAssignmentsText != null)
                        CompletedAssignmentsText.Text = completedCount.ToString();
                }

                // Средний балл пока ставим 0
                if (AverageGradeText != null)
                    AverageGradeText.Text = "0.0";

                LoadRecentActivity(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки базовой статистики: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    // Заглушка
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
                var assignmentsResponse = await _assignmentService.GetAssignmentsAsync(1, 20);

                if (assignmentsResponse != null && assignmentsResponse.Items != null && assignmentsResponse.Items.Count > 0)
                {
                    var now = DateTime.Now;
                    var deadlines = assignmentsResponse.Items
                        .Where(a => a.Deadline > now)
                        .OrderBy(a => a.Deadline)
                        .Take(5)
                        .Select(a => new DeadlineViewModel
                        {
                            Title = a.Title,
                            CourseName = $"Курс #{a.CourseId}",
                            Date = $"📅 {a.Deadline:dd MMMM yyyy, HH:mm}",
                            DaysLeft = (int)(a.Deadline - now).TotalDays
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

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            _ = LoadPageContent();
        }
    }
}
