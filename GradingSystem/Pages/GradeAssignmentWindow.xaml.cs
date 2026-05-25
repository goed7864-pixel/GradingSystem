using System;
using System.IO;
using System.Windows;
using GradingSystem.Services;
using Microsoft.Win32;

namespace GradingSystem.Pages
{
    public partial class GradeAssignmentWindow : Window
    {
        public int SubmissionId { get; private set; }
        public int Score { get; private set; }
        public string Feedback { get; private set; }
        public int GradedById { get; private set; }

        private int _maxScore;
        private readonly SubmissionService _submissionService;
        private string _originalFileName;

        public GradeAssignmentWindow(int submissionId, string studentName, string assignmentTitle, string studentAnswer, int maxScore, int gradedById, int? currentScore = null, string currentFeedback = null)
        {
            InitializeComponent();

            SubmissionId = submissionId;
            GradedById = gradedById;
            _maxScore = maxScore;
            _submissionService = ServiceLocator.Instance.SubmissionService;
            _originalFileName = studentAnswer; // studentAnswer теперь содержит имя файла

            StudentNameText.Text = $"Студент: {studentName}";
            AssignmentTitleText.Text = $"Задание: {assignmentTitle}";

            // Отображаем информацию о файле
            if (!string.IsNullOrWhiteSpace(studentAnswer))
            {
                FileNameText.Text = studentAnswer;
                FileSizeText.Text = "Нажмите 'Скачать' для просмотра файла";
                DownloadButton.IsEnabled = true;
            }
            else
            {
                FileNameText.Text = "Файл не загружен";
                FileSizeText.Text = "";
                DownloadButton.IsEnabled = false;
            }

            MaxGradeText.Text = $"/ {maxScore}";

            if (currentScore.HasValue)
            {
                GradeTextBox.Text = currentScore.Value.ToString();
            }

            if (!string.IsNullOrWhiteSpace(currentFeedback))
            {
                CommentTextBox.Text = currentFeedback;
            }
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DownloadButton.IsEnabled = false;
                DownloadButton.Content = "⏳ Загрузка...";

                // Скачиваем файл с сервера
                var fileBytes = await _submissionService.DownloadSubmissionAsync(SubmissionId);

                if (fileBytes == null || fileBytes.Length == 0)
                {
                    MessageBox.Show("Файл пустой или не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Предлагаем пользователю выбрать место для сохранения
                var saveFileDialog = new SaveFileDialog
                {
                    FileName = _originalFileName,
                    Filter = "Все файлы (*.*)|*.*",
                    Title = "Сохранить файл работы"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Сохраняем файл
                    await File.WriteAllBytesAsync(saveFileDialog.FileName, fileBytes);

                    var result = MessageBox.Show(
                        $"Файл успешно сохранен!\n\nОткрыть файл?",
                        "Успех",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Открываем файл в программе по умолчанию
                        try
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = saveFileDialog.FileName,
                                UseShellExecute = true
                            });
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Не удалось открыть файл: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при скачивании файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                DownloadButton.IsEnabled = true;
                DownloadButton.Content = "📥 Скачать";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(GradeTextBox.Text) || !int.TryParse(GradeTextBox.Text, out int score))
            {
                MessageBox.Show("Введите корректную оценку", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (score < 0 || score > _maxScore)
            {
                MessageBox.Show($"Оценка должна быть от 0 до {_maxScore}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Score = score;
            Feedback = CommentTextBox.Text.Trim();

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
