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
    public partial class AssignmentsPage : Page
    {
        private readonly GroupService _groupService;
        private readonly CourseService _courseService;
        private readonly AssignmentService _assignmentService;
        private readonly GradeService _gradeService;
        private readonly SubmissionService _submissionService;
        private readonly TokenService _tokenService;

        private List<StudentAssignmentViewModel> _currentAssignments;
        private int _selectedGroupId;
        private int _selectedCourseId;
        private int _selectedAssignmentId;

        public AssignmentsPage()
        {
            InitializeComponent();
            _groupService = ServiceLocator.Instance.GroupService;
            _courseService = ServiceLocator.Instance.CourseService;
            _assignmentService = ServiceLocator.Instance.AssignmentService;
            _gradeService = ServiceLocator.Instance.GradeService;
            _submissionService = ServiceLocator.Instance.SubmissionService;
            _tokenService = ServiceLocator.Instance.TokenService;

            Loaded += async (s, e) => await LoadFilters();
        }

        private async System.Threading.Tasks.Task LoadFilters()
        {
            try
            {
                // Загрузка курсов преподавателя
                var coursesResponse = await _courseService.GetMyCoursesAsync(1, 100);
                if (coursesResponse != null && coursesResponse.Items != null)
                {
                    CourseComboBox.Items.Clear();
                    CourseComboBox.Items.Add(new ComboBoxItem { Content = "Выберите курс" });
                    foreach (var course in coursesResponse.Items)
                    {
                        CourseComboBox.Items.Add(new ComboBoxItem { Content = course.Name, Tag = course.Id });
                    }
                    CourseComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки фильтров: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void GroupComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GroupComboBox.SelectedItem is ComboBoxItem selected && selected.Tag is int groupId)
            {
                _selectedGroupId = groupId;

                // Загружаем задания только если выбран и курс, и группа
                if (_selectedCourseId > 0)
                {
                    await LoadAssignmentsForFilters();
                }
            }
        }

        private async void CourseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CourseComboBox.SelectedItem is ComboBoxItem selected && selected.Tag is int courseId)
            {
                _selectedCourseId = courseId;

                // Загружаем группы для выбранного курса
                await LoadGroupsForCourse(courseId);

                // Сбрасываем выбор группы и задания
                _selectedGroupId = 0;
                _selectedAssignmentId = 0;
                AssignmentComboBox.Items.Clear();
                AssignmentComboBox.Items.Add(new ComboBoxItem { Content = "Выберите задание" });
                AssignmentComboBox.SelectedIndex = 0;
                AssignmentComboBox.IsEnabled = false;

                // Скрываем таблицу студентов
                StudentsGrid.Visibility = Visibility.Collapsed;
                NoDataPanel.Visibility = Visibility.Visible;
            }
        }

        private async System.Threading.Tasks.Task LoadGroupsForCourse(int courseId)
        {
            try
            {
                var groupsResponse = await _groupService.GetGroupsByCourseAsync(courseId, 1, 100);

                GroupComboBox.Items.Clear();
                GroupComboBox.Items.Add(new ComboBoxItem { Content = "Выберите группу" });

                if (groupsResponse != null && groupsResponse.Items != null && groupsResponse.Items.Count > 0)
                {
                    foreach (var group in groupsResponse.Items)
                    {
                        GroupComboBox.Items.Add(new ComboBoxItem { Content = group.Name, Tag = group.Id });
                    }
                    GroupComboBox.IsEnabled = true;
                }
                else
                {
                    GroupComboBox.IsEnabled = false;
                }

                GroupComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки групп: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task LoadAssignmentsForFilters()
        {
            if (_selectedGroupId > 0 && _selectedCourseId > 0)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"Загрузка заданий для курса ID: {_selectedCourseId}");
                    var response = await _courseService.GetCourseAssignmentsAsync(_selectedCourseId, 1, 100);

                    System.Diagnostics.Debug.WriteLine($"Получено заданий: {response?.Items?.Count ?? 0}");

                    AssignmentComboBox.Items.Clear();
                    AssignmentComboBox.Items.Add(new ComboBoxItem { Content = "Выберите задание" });

                    if (response != null && response.Items != null && response.Items.Count > 0)
                    {
                        foreach (var assignment in response.Items)
                        {
                            System.Diagnostics.Debug.WriteLine($"Задание: {assignment.Title} (ID: {assignment.Id}, CourseId: {assignment.CourseId})");
                            AssignmentComboBox.Items.Add(new ComboBoxItem
                            {
                                Content = assignment.Title,
                                Tag = assignment.Id
                            });
                        }
                        AssignmentComboBox.IsEnabled = true;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Задания не найдены или список пуст");
                        AssignmentComboBox.IsEnabled = false;
                    }

                    AssignmentComboBox.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка: {ex.Message}");
                    MessageBox.Show($"Ошибка загрузки заданий: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void AssignmentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AssignmentComboBox.SelectedItem is ComboBoxItem selected && selected.Tag is int assignmentId)
            {
                _selectedAssignmentId = assignmentId;
                await LoadStudentsWithSubmissions();
            }
        }

        private async System.Threading.Tasks.Task LoadStudentsWithSubmissions()
        {
            try
            {
                // Получаем студентов группы
                var studentsResponse = await _groupService.GetGroupStudentsAsync(_selectedGroupId, 1, 100);
                if (studentsResponse == null || studentsResponse.Items == null || studentsResponse.Items.Count == 0)
                {
                    _currentAssignments = new List<StudentAssignmentViewModel>();
                    StudentsGrid.ItemsSource = _currentAssignments;
                    StudentsGrid.Visibility = Visibility.Collapsed;
                    NoDataPanel.Visibility = Visibility.Visible;
                    return;
                }

                // Загружаем все submissions (API вернет только те, к которым есть доступ)
                PagedResponse<SubmissionDto>? allSubmissionsResponse = null;
                try
                {
                    allSubmissionsResponse = await _submissionService.GetSubmissionsAsync(1, 1000);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка загрузки submissions: {ex.Message}");
                    MessageBox.Show($"Не удалось загрузить работы студентов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                // Фильтруем submissions по выбранному заданию
                var submissions = allSubmissionsResponse?.Items?
                    .Where(s => s.AssignmentId == _selectedAssignmentId)
                    .ToList() ?? new List<SubmissionDto>();

                // Получаем оценки для отправок
                var students = new List<StudentAssignmentViewModel>();

                foreach (var student in studentsResponse.Items)
                {
                    var submission = submissions.FirstOrDefault(s => s.StudentId == student.Id);

                    string status;
                    string statusText;
                    string statusColor;
                    int? score = null;

                    if (submission == null)
                    {
                        // Студент не сдал задание
                        status = "red";
                        statusText = "Не сдано";
                        statusColor = "#EF5350";
                    }
                    else
                    {
                        // Проверяем, есть ли оценка
                        try
                        {
                            var gradeResponse = await _gradeService.GetGradeBySubmissionIdAsync(submission.Id);

                            if (gradeResponse != null)
                            {
                                // Задание проверено
                                status = "green";
                                score = gradeResponse.Score;
                                statusText = $"Проверено ({score})";
                                statusColor = "#66BB6A";
                            }
                            else
                            {
                                // Задание на проверке
                                status = "yellow";
                                statusText = "На проверке";
                                statusColor = "#FFB74D";
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки оценки для submission {submission.Id}: {ex.Message}");
                            // Если не удалось загрузить оценку, считаем что на проверке
                            status = "yellow";
                            statusText = "На проверке";
                            statusColor = "#FFB74D";
                        }
                    }

                    students.Add(new StudentAssignmentViewModel
                    {
                        Id = student.Id,
                        FullName = student.FullName,
                        Email = student.Email,
                        Status = status,
                        StatusText = statusText,
                        StatusColor = statusColor,
                        Score = score,
                        SubmissionId = submission?.Id
                    });
                }

                _currentAssignments = students;
                StudentsGrid.ItemsSource = _currentAssignments;
                StudentsGrid.Visibility = Visibility.Visible;
                NoDataPanel.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                _currentAssignments = new List<StudentAssignmentViewModel>();
                StudentsGrid.ItemsSource = _currentAssignments;
                StudentsGrid.Visibility = Visibility.Collapsed;
                NoDataPanel.Visibility = Visibility.Visible;
            }
        }

        private async void AssignmentCell_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is StudentAssignmentViewModel student)
            {
                await OpenGradeWindow(student);
            }
        }

        private async void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is StudentAssignmentViewModel student)
            {
                await OpenGradeWindow(student);
            }
        }

        private async System.Threading.Tasks.Task OpenGradeWindow(StudentAssignmentViewModel student)
        {
            if (student.Status == "yellow" && student.SubmissionId.HasValue)
            {
                try
                {
                    // Получаем данные о задании
                    var assignment = await _assignmentService.GetAssignmentByIdAsync(_selectedAssignmentId);
                    if (assignment == null)
                    {
                        MessageBox.Show("Не удалось загрузить данные задания", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Получаем данные о submission
                    var submission = await _submissionService.GetSubmissionByIdAsync(student.SubmissionId.Value);
                    if (submission == null)
                    {
                        MessageBox.Show("Не удалось загрузить данные отправки", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Получаем ID текущего пользователя (преподавателя)
                    var teacherId = _tokenService.GetCurrentUserId();
                    if (!teacherId.HasValue)
                    {
                        MessageBox.Show("Не удалось определить ID преподавателя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Формируем ответ студента (имя файла)
                    string studentAnswer = submission.OriginalFileName ?? "Файл не найден";

                    // Открываем окно для проверки задания
                    var gradeWindow = new GradeAssignmentWindow(
                        student.SubmissionId.Value,
                        student.FullName,
                        assignment.Title,
                        studentAnswer,
                        assignment.MaxScore,
                        teacherId.Value)
                    {
                        Owner = Window.GetWindow(this)
                    };

                    if (gradeWindow.ShowDialog() == true)
                    {
                        try
                        {
                            // Создаем DTO для оценки
                            var gradeCreateDto = new GradeCreateDto
                            {
                                SubmissionId = gradeWindow.SubmissionId,
                                Score = gradeWindow.Score,
                                Feedback = gradeWindow.Feedback,
                                GradedById = gradeWindow.GradedById
                            };

                            // Отправляем оценку на сервер
                            var gradeResponse = await _gradeService.CreateGradeAsync(gradeCreateDto);

                            if (gradeResponse != null)
                            {
                                MessageBox.Show(
                                    $"Оценка успешно выставлена!\n\nБаллы: {gradeWindow.Score}\nКомментарий: {(string.IsNullOrWhiteSpace(gradeWindow.Feedback) ? "Без комментария" : gradeWindow.Feedback)}",
                                    "Успех",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                                // Перезагружаем данные после выставления оценки
                                await LoadStudentsWithSubmissions();
                            }
                            else
                            {
                                MessageBox.Show("Не удалось сохранить оценку. Попробуйте еще раз.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при сохранении оценки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при открытии окна проверки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (student.Status == "green")
            {
                MessageBox.Show($"Задание уже проверено. Оценка: {student.Score}", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (student.Status == "red")
            {
                MessageBox.Show("Студент еще не сдал задание", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

    public class StudentAssignmentViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public string StatusText { get; set; }
        public string StatusColor { get; set; }
        public int? Score { get; set; }
        public int? SubmissionId { get; set; }
    }
}
