using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GradingSystem.Services;

namespace GradingSystem.Pages
{
    public partial class EnrollGroupToCourseWindow : Window
    {
        private readonly CourseService _courseService;
        private readonly int _groupId;
        private readonly string _groupName;
        private List<CourseViewModel> _allCourses;

        public EnrollGroupToCourseWindow(int groupId, string groupName)
        {
            InitializeComponent();
            _courseService = ServiceLocator.Instance.CourseService;
            _groupId = groupId;
            _groupName = groupName;

            GroupNameText.Text = $"Группа: {groupName}";

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
                        Description = c.Description ?? "Нет описания"
                    }).ToList();

                    CoursesList.ItemsSource = _allCourses;
                    NoCoursesPanel.Visibility = Visibility.Collapsed;
                }
                else
                {
                    _allCourses = new List<CourseViewModel>();
                    CoursesList.ItemsSource = _allCourses;
                    NoCoursesPanel.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки курсов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_allCourses == null) return;

            var searchText = SearchBox.Text.ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                CoursesList.ItemsSource = _allCourses;
            }
            else
            {
                var filtered = _allCourses.Where(c =>
                    c.Name.ToLower().Contains(searchText) ||
                    c.Description.ToLower().Contains(searchText)).ToList();
                CoursesList.ItemsSource = filtered;
            }
        }

        private async void CourseItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is int courseId)
            {
                var course = _allCourses.FirstOrDefault(c => c.Id == courseId);
                if (course != null)
                {
                    var result = MessageBox.Show(
                        $"Записать группу '{_groupName}' на курс '{course.Name}'?\n\nВсе студенты группы будут записаны на этот курс.",
                        "Подтверждение",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            // Получаем всех студентов группы
                            var groupService = ServiceLocator.Instance.GroupService;
                            var studentsResponse = await groupService.GetGroupStudentsAsync(_groupId, 1, 100);

                            if (studentsResponse == null || studentsResponse.Items == null || studentsResponse.Items.Count == 0)
                            {
                                MessageBox.Show("В группе нет студентов для записи на курс", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                                return;
                            }

                            // Записываем каждого студента на курс
                            var enrollmentService = ServiceLocator.Instance.EnrollmentService;
                            int successCount = 0;
                            int failCount = 0;

                            foreach (var student in studentsResponse.Items)
                            {
                                try
                                {
                                    var success = await enrollmentService.EnrollStudentAsync(courseId, student.Id);
                                    if (success)
                                        successCount++;
                                    else
                                        failCount++;
                                }
                                catch
                                {
                                    failCount++;
                                }
                            }

                            if (successCount > 0)
                            {
                                MessageBox.Show(
                                    $"Записано студентов: {successCount}\nОшибок: {failCount}",
                                    "Результат",
                                    MessageBoxButton.OK,
                                    failCount > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);

                                DialogResult = true;
                                Close();
                            }
                            else
                            {
                                MessageBox.Show("Не удалось записать студентов на курс", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка записи на курс: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
    }
}
