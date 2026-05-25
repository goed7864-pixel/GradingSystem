using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GradingSystem.Services;
using GradingSystem.DTOs;

namespace GradingSystem.Pages
{
    public partial class LessonsPage : Page
    {
        private readonly CourseService _courseService;
        private readonly AssignmentService _assignmentService;
        private List<CourseViewModel> _allCourses;
        private List<AssignmentViewModel> _allAssignments;
        private int _selectedCourseId;
        private string _selectedCourseName;

        public LessonsPage()
        {
            InitializeComponent();
            _courseService = ServiceLocator.Instance.CourseService;
            _assignmentService = ServiceLocator.Instance.AssignmentService;
            Loaded += async (s, e) => await LoadCourses();
        }

        private async System.Threading.Tasks.Task LoadCourses()
        {
            try
            {
                var response = await _courseService.GetMyCoursesAsync(1, 100);

                if (response != null && response.Items != null && response.Items.Count > 0)
                {
                    _allCourses = response.Items.Select(c => new CourseViewModel
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        Icon = GetIconForCourse(c.Name),
                        Color = GetColorForCourse(c.Id),
                        AssignmentCount = c.AssignmentCount ?? 0
                    }).ToList();

                    // Принудительно обновляем ItemsSource
                    CoursesList.ItemsSource = null;
                    CoursesList.ItemsSource = _allCourses;
                    CoursesCountText.Text = $"{_allCourses.Count} курсов";
                }
                else
                {
                    _allCourses = new List<CourseViewModel>();
                    CoursesList.ItemsSource = _allCourses;
                    CoursesCountText.Text = "0 курсов";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки курсов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetIconForCourse(string courseName)
        {
            var lowerName = courseName.ToLower();
            if (lowerName.Contains("программ") || lowerName.Contains("c#") || lowerName.Contains("java"))
                return "🎯";
            if (lowerName.Contains("база") || lowerName.Contains("sql"))
                return "🗄️";
            if (lowerName.Contains("веб") || lowerName.Contains("web"))
                return "🌐";
            if (lowerName.Contains("алгоритм"))
                return "🧮";
            if (lowerName.Contains("математ"))
                return "📐";
            return "📚";
        }

        private string GetColorForCourse(int courseId)
        {
            var colors = new[] { "#E8F5E9", "#E3F2FD", "#FFF3E0", "#F3E5F5", "#FCE4EC", "#E0F2F1" };
            return colors[courseId % colors.Length];
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_allCourses == null) return;

            var searchText = SearchBox.Text.ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                CoursesList.ItemsSource = _allCourses;
            }
            else
            {
                var filtered = _allCourses.Where(c => c.Name.ToLower().Contains(searchText)).ToList();
                CoursesList.ItemsSource = filtered;
            }
        }

        private void CourseItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is int courseId)
            {
                var course = _allCourses.FirstOrDefault(c => c.Id == courseId);
                if (course != null)
                {
                    _selectedCourseId = courseId;
                    _selectedCourseName = course.Name;
                    SelectedCourseName.Text = course.Name;
                    SelectedCourseInfo.Text = $"{course.AssignmentCount} заданий";
                    AddAssignmentButton.Visibility = Visibility.Visible;
                    AssignmentSearchPanel.Visibility = Visibility.Visible;
                    _ = LoadAssignmentsForCourse(courseId);
                }
            }
        }

        private async System.Threading.Tasks.Task LoadAssignmentsForCourse(int courseId)
        {
            try
            {
                // Пробуем использовать search endpoint
                PagedResponse<AssignmentDto>? response = null;

                try
                {
                    response = await _assignmentService.SearchAssignmentsAsync(null, courseId, 1, 100);
                }
                catch
                {
                    // Если search не работает, загружаем все задания и фильтруем на клиенте
                    var allAssignments = await _assignmentService.GetAssignmentsAsync(1, 100);
                    if (allAssignments != null && allAssignments.Items != null)
                    {
                        var filtered = allAssignments.Items.Where(a => a.CourseId == courseId).ToList();
                        response = new PagedResponse<AssignmentDto>
                        {
                            Items = filtered,
                            TotalCount = filtered.Count,
                            Page = 1,
                            PageSize = 100
                        };
                    }
                }

                if (response != null && response.Items != null && response.Items.Count > 0)
                {
                    _allAssignments = response.Items.Select(a => new AssignmentViewModel
                    {
                        Id = a.Id,
                        Title = a.Title,
                        Description = a.Description ?? "",
                        Deadline = $"📅 {a.Deadline:dd MMMM yyyy, HH:mm}",
                        Status = GetAssignmentStatus(a.Deadline),
                        StatusColor = GetStatusColor(a.Deadline),
                        Icon = "📝",
                        Color = "#FFF3E0",
                        SubmissionsCount = a.SubmissionsCount ?? 0
                    }).ToList();

                    UpdateAssignmentsDisplay();
                }
                else
                {
                    _allAssignments = new List<AssignmentViewModel>();
                    UpdateAssignmentsDisplay();
                }
            }
            catch (Exception ex)
            {
                // Если ошибка, просто показываем пустой список
                _allAssignments = new List<AssignmentViewModel>();
                UpdateAssignmentsDisplay();
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки заданий: {ex.Message}");
            }
        }

        private string GetAssignmentStatus(DateTime deadline)
        {
            var now = DateTime.Now;
            if (deadline < now)
                return "Закрыто";
            if ((deadline - now).TotalDays <= 3)
                return "Скоро";
            return "Активно";
        }

        private string GetStatusColor(DateTime deadline)
        {
            var now = DateTime.Now;
            if (deadline < now)
                return "#9E9E9E";
            if ((deadline - now).TotalDays <= 3)
                return "#FF9800";
            return "#4CAF50";
        }

        private void UpdateAssignmentsDisplay()
        {
            if (_allAssignments != null && _allAssignments.Count > 0)
            {
                AssignmentsList.ItemsSource = _allAssignments;
                AssignmentsList.Visibility = Visibility.Visible;
                NoAssignmentsPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                AssignmentsList.Visibility = Visibility.Collapsed;
                NoAssignmentsPanel.Visibility = Visibility.Visible;
            }
        }

        private void AssignmentSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_allAssignments == null) return;

            var searchText = AssignmentSearchBox.Text.ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                AssignmentsList.ItemsSource = _allAssignments;
            }
            else
            {
                var filtered = _allAssignments.Where(a => a.Title.ToLower().Contains(searchText) || a.Description.ToLower().Contains(searchText)).ToList();
                AssignmentsList.ItemsSource = filtered;
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadCourses();
            if (_selectedCourseId > 0)
            {
                await LoadAssignmentsForCourse(_selectedCourseId);
            }
        }

        private async void AddCourseButton_Click(object sender, RoutedEventArgs e)
        {
            var addCourseWindow = new AddCourseWindow { Owner = Window.GetWindow(this) };
            if (addCourseWindow.ShowDialog() == true && addCourseWindow.IsCourseCreated)
            {
                await LoadCourses();
            }
        }

        private async void EditCourseButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int courseId)
            {
                var course = _allCourses.FirstOrDefault(c => c.Id == courseId);
                if (course != null)
                {
                    var editWindow = new EditCourseWindow(courseId, course.Name, course.Description ?? "") { Owner = Window.GetWindow(this) };
                    if (editWindow.ShowDialog() == true && editWindow.IsCourseUpdated)
                    {
                        await LoadCourses();
                        if (_selectedCourseId == courseId)
                        {
                            var updatedCourse = _allCourses.FirstOrDefault(c => c.Id == courseId);
                            if (updatedCourse != null)
                            {
                                SelectedCourseName.Text = updatedCourse.Name;
                            }
                        }
                    }
                }
            }
        }

        private async void DeleteCourseButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int courseId)
            {
                var course = _allCourses.FirstOrDefault(c => c.Id == courseId);
                if (course != null)
                {
                    var result = MessageBox.Show($"Вы уверены, что хотите удалить курс '{course.Name}'?\n\nВсе задания этого курса также будут удалены.", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            await _courseService.DeleteCourseAsync(courseId);
                            await LoadCourses();

                            if (_selectedCourseId == courseId)
                            {
                                _selectedCourseId = 0;
                                _selectedCourseName = "";
                                SelectedCourseName.Text = "Выберите курс";
                                SelectedCourseInfo.Text = "Нажмите на курс слева";
                                AddAssignmentButton.Visibility = Visibility.Collapsed;
                                AssignmentSearchPanel.Visibility = Visibility.Collapsed;
                                AssignmentsList.ItemsSource = null;
                                NoAssignmentsPanel.Visibility = Visibility.Collapsed;
                            }

                            MessageBox.Show("Курс успешно удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка удаления курса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        private async void AddAssignmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCourseId <= 0)
            {
                MessageBox.Show("Пожалуйста, выберите курс", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var addAssignmentWindow = new AddAssignmentWindow(_selectedCourseId) { Owner = Window.GetWindow(this) };
            if (addAssignmentWindow.ShowDialog() == true)
            {
                await LoadCourses(); // Обновляем счетчик заданий
                await LoadAssignmentsForCourse(_selectedCourseId);

                // Обновляем информацию о выбранном курсе
                var updatedCourse = _allCourses.FirstOrDefault(c => c.Id == _selectedCourseId);
                if (updatedCourse != null)
                {
                    SelectedCourseInfo.Text = $"{updatedCourse.AssignmentCount} заданий";
                }
            }
        }

        private void EditAssignmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int assignmentId)
            {
                var assignment = _allAssignments.FirstOrDefault(a => a.Id == assignmentId);
                if (assignment != null)
                {
                    var editWindow = new EditAssignmentWindow(assignmentId) { Owner = Window.GetWindow(this) };
                    if (editWindow.ShowDialog() == true)
                    {
                        _ = LoadAssignmentsForCourse(_selectedCourseId);
                    }
                }
            }
        }

        private async void DeleteAssignmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int assignmentId)
            {
                var assignment = _allAssignments.FirstOrDefault(a => a.Id == assignmentId);
                if (assignment != null)
                {
                    var result = MessageBox.Show($"Вы уверены, что хотите удалить задание '{assignment.Title}'?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            await _assignmentService.DeleteAssignmentAsync(assignmentId);
                            await LoadCourses(); // Обновляем счетчик заданий
                            await LoadAssignmentsForCourse(_selectedCourseId);

                            // Обновляем информацию о выбранном курсе
                            var updatedCourse = _allCourses.FirstOrDefault(c => c.Id == _selectedCourseId);
                            if (updatedCourse != null)
                            {
                                SelectedCourseInfo.Text = $"{updatedCourse.AssignmentCount} заданий";
                            }

                            MessageBox.Show("Задание удалено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка удаления задания: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }
    }

    public class CourseViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
        public int AssignmentCount { get; set; }
    }

    public class AssignmentViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Deadline { get; set; }
        public string Status { get; set; }
        public string StatusColor { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
        public int SubmissionsCount { get; set; }
    }
}