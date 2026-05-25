using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Win32;
using GradingSystem.DTOs;
using GradingSystem.Services;

namespace GradingSystem.Pages
{
    public partial class StudentAssignmentsPage : Page
    {
        private readonly AssignmentService _assignmentService;
        private readonly CourseService _courseService;
        private readonly TokenService _tokenService;
        private readonly SubmissionService _submissionService;
        private List<StudentAssignmentCardViewModel> _allAssignments;
        private Dictionary<int, int> _assignmentSubmissionMap; // assignmentId -> submissionId

        public StudentAssignmentsPage()
        {
            InitializeComponent();
            _assignmentService = ServiceLocator.Instance.AssignmentService;
            _courseService = ServiceLocator.Instance.CourseService;
            _tokenService = ServiceLocator.Instance.TokenService;
            _submissionService = ServiceLocator.Instance.SubmissionService;
            _allAssignments = new List<StudentAssignmentCardViewModel>();
            _assignmentSubmissionMap = new Dictionary<int, int>();
            Loaded += async (s, e) => await LoadAssignments();
        }

        private async System.Threading.Tasks.Task LoadAssignments()
        {
            try
            {
                // Получаем ID текущего студента
                var studentId = _tokenService.GetCurrentUserId();
                if (!studentId.HasValue)
                {
                    MessageBox.Show("Не удалось определить ID студента. Пожалуйста, войдите заново.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Загружаем все задания
                var assignmentsResponse = await _assignmentService.GetAssignmentsAsync(1, 100);

                // Загружаем submissions студента
                var submissionsResponse = await _submissionService.GetStudentSubmissionsAsync(studentId.Value, 1, 100);
                var submittedAssignmentIds = new HashSet<int>();
                _assignmentSubmissionMap.Clear();

                if (submissionsResponse != null && submissionsResponse.Items != null)
                {
                    foreach (var submission in submissionsResponse.Items)
                    {
                        submittedAssignmentIds.Add(submission.AssignmentId);
                        _assignmentSubmissionMap[submission.AssignmentId] = submission.Id;
                    }
                }

                if (assignmentsResponse != null && assignmentsResponse.Items != null && assignmentsResponse.Items.Count > 0)
                {
                    var now = DateTime.Now;

                    _allAssignments = assignmentsResponse.Items.Select(a =>
                    {
                        var isSubmitted = submittedAssignmentIds.Contains(a.Id);
                        var status = isSubmitted ? "Выполнено" : GetAssignmentStatus(a.Deadline);
                        var statusColor = isSubmitted ? "#4CAF50" : GetStatusColor(a.Deadline);

                        return new StudentAssignmentCardViewModel
                        {
                            Id = a.Id,
                            Title = a.Title,
                            Description = a.Description ?? "Нет описания",
                            CourseName = $"Курс #{a.CourseId}",
                            Deadline = a.Deadline.ToString("dd.MM.yyyy HH:mm"),
                            MaxScore = a.MaxScore,
                            Status = status,
                            StatusColor = statusColor,
                            DeadlineDate = a.Deadline
                        };
                    }).ToList();

                    AssignmentsList.ItemsSource = _allAssignments;
                    NoAssignmentsMessage.Visibility = Visibility.Collapsed;
                }
                else
                {
                    _allAssignments = new List<StudentAssignmentCardViewModel>();
                    AssignmentsList.ItemsSource = _allAssignments;
                    NoAssignmentsMessage.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заданий: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetAssignmentStatus(DateTime deadline)
        {
            var now = DateTime.Now;
            if (deadline < now)
                return "Просрочено";
            else if ((deadline - now).TotalDays <= 3)
                return "Срочно";
            else
                return "К выполнению";
        }

        private string GetStatusColor(DateTime deadline)
        {
            var now = DateTime.Now;
            if (deadline < now)
                return "#F44336"; // Красный
            else if ((deadline - now).TotalDays <= 3)
                return "#FF9800"; // Оранжевый
            else
                return "#4CAF50"; // Зеленый
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void StatusFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_allAssignments == null || AssignmentsList == null)
                return;

            var searchText = SearchBox?.Text?.ToLower() ?? "";
            var selectedStatus = (StatusFilterComboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Все задания";

            var filtered = _allAssignments.AsEnumerable();

            // Фильтр по поиску
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filtered = filtered.Where(a =>
                    a.Title.ToLower().Contains(searchText) ||
                    a.Description.ToLower().Contains(searchText) ||
                    a.CourseName.ToLower().Contains(searchText));
            }

            // Фильтр по статусу
            if (selectedStatus != "Все задания")
            {
                filtered = filtered.Where(a => a.Status == selectedStatus);
            }

            AssignmentsList.ItemsSource = filtered.ToList();
            NoAssignmentsMessage.Visibility = filtered.Any() ? Visibility.Collapsed : Visibility.Visible;
        }

        private async void UploadAssignment_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int assignmentId)
            {
                var assignment = _allAssignments.FirstOrDefault(a => a.Id == assignmentId);
                if (assignment != null)
                {
                    // Проверяем, не загружен ли уже файл для этого задания
                    if (_assignmentSubmissionMap.ContainsKey(assignmentId))
                    {
                        MessageBox.Show(
                            $"Вы уже загрузили работу для задания '{assignment.Title}'.\n\nЧтобы загрузить новый файл, сначала удалите предыдущую работу.",
                            "Работа уже загружена",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }

                    // Открываем диалог выбора файла
                    var openFileDialog = new OpenFileDialog
                    {
                        Title = "Выберите файл для загрузки",
                        Filter = "Документы (*.pdf;*.docx;*.doc)|*.pdf;*.docx;*.doc|Изображения (*.jpg;*.png)|*.jpg;*.png|Архивы (*.zip;*.rar)|*.zip;*.rar|Текстовые файлы (*.txt)|*.txt",
                        FilterIndex = 1
                    };

                    if (openFileDialog.ShowDialog() == true)
                    {
                        var filePath = openFileDialog.FileName;
                        var fileName = System.IO.Path.GetFileName(filePath);
                        var fileExtension = System.IO.Path.GetExtension(filePath).ToLowerInvariant();

                        // Проверяем расширение файла
                        var allowedExtensions = new[] { ".pdf", ".docx", ".doc", ".jpg", ".jpeg", ".png", ".zip", ".rar", ".txt" };
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            MessageBox.Show(
                                $"Недопустимый формат файла: {fileExtension}\n\nРазрешенные форматы:\n• Документы: PDF, DOCX, DOC\n• Изображения: JPG, PNG\n• Архивы: ZIP, RAR\n• Текст: TXT",
                                "Неверный формат файла",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                            return;
                        }

                        // Проверяем размер файла (максимум 50 МБ)
                        var fileInfo = new System.IO.FileInfo(filePath);
                        const long maxFileSize = 50 * 1024 * 1024; // 50 МБ
                        if (fileInfo.Length > maxFileSize)
                        {
                            MessageBox.Show(
                                $"Размер файла ({fileInfo.Length / 1024 / 1024} МБ) превышает максимально допустимый (50 МБ).\n\nПожалуйста, выберите файл меньшего размера.",
                                "Файл слишком большой",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                            return;
                        }

                        // Показываем окно подтверждения
                        var result = MessageBox.Show(
                            $"Вы хотите загрузить файл:\n\n{fileName}\n\nДля задания: {assignment.Title}",
                            "Подтверждение загрузки",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            try
                            {
                                // Получаем ID текущего студента из токена
                                var studentId = _tokenService.GetCurrentUserId();
                                if (!studentId.HasValue)
                                {
                                    MessageBox.Show(
                                        "Не удалось определить ID студента. Пожалуйста, войдите заново.",
                                        "Ошибка",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                                    return;
                                }

                                // Отправляем файл на сервер
                                var response = await _submissionService.CreateSubmissionAsync(
                                    assignmentId,
                                    studentId.Value,
                                    filePath);

                                if (response != null)
                                {
                                    MessageBox.Show(
                                        $"Файл '{fileName}' успешно загружен!\n\n{response.Message}",
                                        "Успех",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Information);

                                    // Перезагружаем список заданий для обновления статуса
                                    await LoadAssignments();
                                }
                                else
                                {
                                    MessageBox.Show(
                                        "Не удалось загрузить файл. Попробуйте еще раз.",
                                        "Ошибка",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(
                                    $"Ошибка при загрузке файла: {ex.Message}",
                                    "Ошибка",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            _ = LoadAssignments();
        }

        private async void DeleteAssignment_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int assignmentId)
            {
                var assignment = _allAssignments.FirstOrDefault(a => a.Id == assignmentId);
                if (assignment != null)
                {
                    // Проверяем, есть ли submission для этого задания
                    if (!_assignmentSubmissionMap.ContainsKey(assignmentId))
                    {
                        MessageBox.Show("Работа не найдена", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var submissionId = _assignmentSubmissionMap[assignmentId];

                    // Показываем окно подтверждения
                    var result = MessageBox.Show(
                        $"Вы уверены, что хотите удалить работу?\n\nЗадание: {assignment.Title}\n\nВнимание: это действие нельзя отменить!",
                        "Подтверждение удаления",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            // Удаляем submission с сервера
                            var success = await _submissionService.DeleteSubmissionAsync(submissionId);

                            if (success)
                            {
                                MessageBox.Show(
                                    "Работа успешно удалена!",
                                    "Успех",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                                // Перезагружаем список заданий
                                await LoadAssignments();
                            }
                            else
                            {
                                MessageBox.Show(
                                    "Не удалось удалить работу. Попробуйте еще раз.",
                                    "Ошибка",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(
                                $"Ошибка при удалении работы: {ex.Message}",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                }
            }
        }
    }

    // Конвертер для видимости кнопок в зависимости от статуса
    public class StatusToVisibilityConverter : IValueConverter
    {
        public string ShowForStatus { get; set; } = string.Empty;
        public bool Invert { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                bool isMatch = status == ShowForStatus;
                bool shouldShow = Invert ? !isMatch : isMatch;
                return shouldShow ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
