using System;
using System.Windows;
using System.Windows.Controls;
using GradingSystem.Services;
using GradingSystem.DTOs;

namespace GradingSystem.Pages
{
    public partial class AddEditUserWindow : Window
    {
        private readonly UserService _userService;
        private int? _userId;
        public bool IsUserSaved { get; private set; }

        // Конструктор для создания нового пользователя
        public AddEditUserWindow()
        {
            InitializeComponent();
            _userService = ServiceLocator.Instance.UserService;
            _userId = null;
            TitleText.Text = "Создание пользователя";
            RoleComboBox.SelectedIndex = 0; // По умолчанию студент
        }

        // Конструктор для редактирования существующего пользователя
        public AddEditUserWindow(int userId, string fullName, string email, string role) : this()
        {
            _userId = userId;
            TitleText.Text = "Редактирование пользователя";
            FullNameTextBox.Text = fullName;
            EmailTextBox.Text = email;

            // Устанавливаем роль
            if (role.ToLower() == "teacher")
            {
                RoleComboBox.SelectedIndex = 1;
            }
            else
            {
                RoleComboBox.SelectedIndex = 0;
            }

            // При редактировании пароль не обязателен
            PasswordBox.Password = "";
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(FullNameTextBox.Text))
            {
                MessageBox.Show("Введите полное имя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                MessageBox.Show("Введите email", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка email формата
            if (!IsValidEmail(EmailTextBox.Text))
            {
                MessageBox.Show("Введите корректный email", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (RoleComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите роль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Пароль обязателен только при создании нового пользователя
            if (_userId == null && string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Введите пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!string.IsNullOrWhiteSpace(PasswordBox.Password) && PasswordBox.Password.Length < 6)
            {
                MessageBox.Show("Пароль должен содержать минимум 6 символов", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var selectedRole = ((ComboBoxItem)RoleComboBox.SelectedItem).Tag.ToString();

                if (_userId == null)
                {
                    // Создание нового пользователя
                    var registerDto = new RegisterDto
                    {
                        FullName = FullNameTextBox.Text.Trim(),
                        Email = EmailTextBox.Text.Trim(),
                        Password = PasswordBox.Password,
                        Role = selectedRole
                    };

                    var response = await _userService.RegisterUserAsync(registerDto);

                    if (response != null && !string.IsNullOrEmpty(response.Message))
                    {
                        MessageBox.Show("Пользователь успешно создан", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        IsUserSaved = true;
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось создать пользователя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    // Редактирование существующего пользователя
                    var updateDto = new UserUpdateDto
                    {
                        FullName = FullNameTextBox.Text.Trim(),
                        Email = EmailTextBox.Text.Trim(),
                        Role = selectedRole
                    };

                    // Если пароль введен, обновляем его
                    if (!string.IsNullOrWhiteSpace(PasswordBox.Password))
                    {
                        updateDto.Password = PasswordBox.Password;
                    }

                    var response = await _userService.UpdateUserAsync(_userId.Value, updateDto);

                    if (response != null && !string.IsNullOrEmpty(response.Message))
                    {
                        MessageBox.Show("Пользователь успешно обновлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        IsUserSaved = true;
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось обновить пользователя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
